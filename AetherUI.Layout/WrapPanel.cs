using System;
using System.Collections.Generic;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 自动换行面板，当空间不足时自动换行或换列
    /// </summary>
    public class WrapPanel : Panel
    {
        #region 依赖属性

        /// <summary>
        /// 方向依赖属性
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation), typeof(Orientation), typeof(WrapPanel),
            new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        /// <summary>
        /// 项目宽度依赖属性
        /// </summary>
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            nameof(ItemWidth), typeof(double), typeof(WrapPanel),
            new PropertyMetadata(double.NaN, OnItemSizeChanged));

        /// <summary>
        /// 项目高度依赖属性
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            nameof(ItemHeight), typeof(double), typeof(WrapPanel),
            new PropertyMetadata(double.NaN, OnItemSizeChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 换行方向
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)(GetValue(OrientationProperty) ?? Orientation.Horizontal);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// 统一项目宽度
        /// </summary>
        public double ItemWidth
        {
            get => (double)(GetValue(ItemWidthProperty) ?? double.NaN);
            set => SetValue(ItemWidthProperty, value);
        }

        /// <summary>
        /// 统一项目高度
        /// </summary>
        public double ItemHeight
        {
            get => (double)(GetValue(ItemHeightProperty) ?? double.NaN);
            set => SetValue(ItemHeightProperty, value);
        }

        #endregion

        #region 布局重写

        /// <summary>
        /// 测量子元素
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected override Size MeasureChildren(Size availableSize)
        {
Size childAvailableSize = new Size(
                double.IsNaN(ItemWidth) ? double.PositiveInfinity : ItemWidth,
                double.IsNaN(ItemHeight) ? double.PositiveInfinity : ItemHeight);

            // 测量所有子元素
            foreach (UIElement child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                child.Measure(childAvailableSize);
            }

            // 计算换行布局
            return CalculateWrapLayout(availableSize, true);
        }

        /// <summary>
        /// 排列子元素
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeChildren(Size finalSize)
        {
            return CalculateWrapLayout(finalSize, false);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 计算换行布局
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <param name="measureOnly">是否仅测量</param>
        /// <returns>计算后的尺寸</returns>
        private Size CalculateWrapLayout(Size availableSize, bool measureOnly)
        {
            List<UIElement> visibleChildren = new List<UIElement>();
            foreach (UIElement child in Children)
            {
                if (child.Visibility != Visibility.Collapsed)
                    visibleChildren.Add(child);
            }

            if (visibleChildren.Count == 0)
                return Size.Empty;

            bool isHorizontal = Orientation == Orientation.Horizontal;
            double lineSize = 0; // 当前行/列的主方向尺寸
            double lineThickness = 0; // 当前行/列的次方向尺寸
            double totalSize = 0; // 总的主方向尺寸
            double totalThickness = 0; // 总的次方向尺寸

            double currentPosition = 0; // 当前行/列的主方向位置
            double currentLinePosition = 0; // 当前行/列的次方向位置

            List<UIElement> currentLine = new List<UIElement>();
            List<double> currentLineSizes = new List<double>();
            List<double> currentLineThicknesses = new List<double>();

            for (int i = 0; i < visibleChildren.Count; i++)
            {
                UIElement child = visibleChildren[i];
                Size childDesiredSize = child.DesiredSize;

                // 应用统一尺寸
                double childWidth = double.IsNaN(ItemWidth) ? childDesiredSize.Width : ItemWidth;
                double childHeight = double.IsNaN(ItemHeight) ? childDesiredSize.Height : ItemHeight;

                double childMainSize = isHorizontal ? childWidth : childHeight;
                double childCrossSize = isHorizontal ? childHeight : childWidth;

                // 检查是否需要换行/换列
                bool needNewLine = false;
                if (currentLine.Count > 0)
                {
                    double availableMainSize = isHorizontal ? availableSize.Width : availableSize.Height;
                    if (!double.IsInfinity(availableMainSize) && lineSize + childMainSize > availableMainSize)
                    {
                        needNewLine = true;
                    }
                }

                if (needNewLine)
                {
                    // 排列当前行/列的元素
                    if (!measureOnly)
                    {
                        ArrangeLine(currentLine, currentLineSizes, currentLineThicknesses, 
                                  currentLinePosition, lineThickness, isHorizontal);
                    }

                    // 开始新行/列
                    totalThickness += lineThickness;
                    currentLinePosition += lineThickness;
                    totalSize = Math.Max(totalSize, lineSize);

                    currentLine.Clear();
                    currentLineSizes.Clear();
                    currentLineThicknesses.Clear();
                    lineSize = 0;
                    lineThickness = 0;
                    currentPosition = 0;
                }

                // 添加到当前行/列
                currentLine.Add(child);
                currentLineSizes.Add(childMainSize);
                currentLineThicknesses.Add(childCrossSize);

                lineSize += childMainSize;
                lineThickness = Math.Max(lineThickness, childCrossSize);
                currentPosition += childMainSize;
            }

            // 排列最后一行/列
            if (currentLine.Count > 0)
            {
                if (!measureOnly)
                {
                    ArrangeLine(currentLine, currentLineSizes, currentLineThicknesses,
                              currentLinePosition, lineThickness, isHorizontal);
                }

                totalThickness += lineThickness;
                totalSize = Math.Max(totalSize, lineSize);
            }

            Size result = isHorizontal ? new Size(totalSize, totalThickness) : new Size(totalThickness, totalSize);
return result;
        }

        /// <summary>
        /// 排列一行/列的元素
        /// </summary>
        /// <param name="line">行/列中的元素</param>
        /// <param name="sizes">元素的主方向尺寸</param>
        /// <param name="thicknesses">元素的次方向尺寸</param>
        /// <param name="linePosition">行/列的次方向位置</param>
        /// <param name="lineThickness">行/列的次方向厚度</param>
        /// <param name="isHorizontal">是否为水平方向</param>
        private void ArrangeLine(List<UIElement> line, List<double> sizes, List<double> thicknesses,
                                double linePosition, double lineThickness, bool isHorizontal)
        {
            double currentPosition = 0;

            for (int i = 0; i < line.Count; i++)
            {
                UIElement child = line[i];
                double size = sizes[i];
                double thickness = thicknesses[i];

                Rect childRect;
                if (isHorizontal)
                {
                    childRect = new Rect(currentPosition, linePosition, size, lineThickness);
                }
                else
                {
                    childRect = new Rect(linePosition, currentPosition, lineThickness, size);
                }

                child.Arrange(childRect);
currentPosition += size;
            }
        }

        #endregion

        #region 属性更改回调

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WrapPanel wrapPanel)
            {
                wrapPanel.InvalidateMeasure();
}
        }

        private static void OnItemSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WrapPanel wrapPanel)
            {
                wrapPanel.InvalidateMeasure();
}
        }

        #endregion
    }
}
