using System;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 堆叠面板，按指定方向排列子元素
    /// </summary>
    public class StackPanel : Panel
    {
        #region 依赖属性

        /// <summary>
        /// 方向依赖属性
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation), typeof(Orientation), typeof(StackPanel),
            new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 堆叠方向
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)(GetValue(OrientationProperty) ?? Orientation.Vertical);
            set => SetValue(OrientationProperty, value);
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
            Debug.WriteLine($"StackPanel measuring {Children.Count} children, Orientation: {Orientation}");

            double totalWidth = 0;
            double totalHeight = 0;
            double maxWidth = 0;
            double maxHeight = 0;

            Size childAvailableSize = availableSize;

            // 根据方向调整可用尺寸
            if (Orientation == Orientation.Horizontal)
            {
                // 水平堆叠：宽度无限制，高度受限
                childAvailableSize = new Size(double.PositiveInfinity, availableSize.Height);
            }
            else
            {
                // 垂直堆叠：高度无限制，宽度受限
                childAvailableSize = new Size(availableSize.Width, double.PositiveInfinity);
            }

            // 测量每个可见的子元素
            foreach (UIElement child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                child.Measure(childAvailableSize);
                Size childDesiredSize = child.DesiredSize;

                Debug.WriteLine($"Child {child.GetType().Name} desired size: {childDesiredSize}");

                if (Orientation == Orientation.Horizontal)
                {
                    // 水平堆叠：累加宽度，取最大高度
                    totalWidth += childDesiredSize.Width;
                    maxHeight = Math.Max(maxHeight, childDesiredSize.Height);
                }
                else
                {
                    // 垂直堆叠：累加高度，取最大宽度
                    totalHeight += childDesiredSize.Height;
                    maxWidth = Math.Max(maxWidth, childDesiredSize.Width);
                }
            }

            Size desiredSize;
            if (Orientation == Orientation.Horizontal)
            {
                desiredSize = new Size(totalWidth, maxHeight);
            }
            else
            {
                desiredSize = new Size(maxWidth, totalHeight);
            }

            Debug.WriteLine($"StackPanel desired size: {desiredSize}");
            return desiredSize;
        }

        /// <summary>
        /// 排列子元素
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeChildren(Size finalSize)
        {
            Debug.WriteLine($"StackPanel arranging {Children.Count} children, Final size: {finalSize}");

            double currentPosition = 0;

            foreach (UIElement child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                Size childDesiredSize = child.DesiredSize;
                Rect childRect;

                if (Orientation == Orientation.Horizontal)
                {
                    // 水平堆叠：从左到右排列
                    childRect = new Rect(
                        currentPosition, 0,
                        childDesiredSize.Width, finalSize.Height);
                    currentPosition += childDesiredSize.Width;
                }
                else
                {
                    // 垂直堆叠：从上到下排列
                    childRect = new Rect(
                        0, currentPosition,
                        finalSize.Width, childDesiredSize.Height);
                    currentPosition += childDesiredSize.Height;
                }

                Debug.WriteLine($"Arranging child {child.GetType().Name} to rect: {childRect}");
                child.Arrange(childRect);
            }

            return finalSize;
        }

        #endregion

        #region 属性更改回调

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StackPanel stackPanel)
            {
                stackPanel.InvalidateMeasure();
                Debug.WriteLine($"StackPanel orientation changed to: {e.NewValue}");
            }
        }

        #endregion
    }
}
