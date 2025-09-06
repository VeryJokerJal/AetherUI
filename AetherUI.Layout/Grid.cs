using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 网格布局容器，支持行列定义和跨行跨列
    /// </summary>
    public class Grid : Panel
    {
        private readonly RowDefinitionCollection _rowDefinitions;
        private readonly ColumnDefinitionCollection _columnDefinitions;

        /// <summary>
        /// 初始化Grid
        /// </summary>
        public Grid()
        {
            _rowDefinitions = new RowDefinitionCollection(this);
            _columnDefinitions = new ColumnDefinitionCollection(this);
        }

        #region 附加属性

        /// <summary>
        /// 行附加属性
        /// </summary>
        public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
            "Row", typeof(int), typeof(Grid),
            new PropertyMetadata(0, OnCellAttachedPropertyChanged));

        /// <summary>
        /// 列附加属性
        /// </summary>
        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
            "Column", typeof(int), typeof(Grid),
            new PropertyMetadata(0, OnCellAttachedPropertyChanged));

        /// <summary>
        /// 行跨度附加属性
        /// </summary>
        public static readonly DependencyProperty RowSpanProperty = DependencyProperty.Register(
            "RowSpan", typeof(int), typeof(Grid),
            new PropertyMetadata(1, OnCellAttachedPropertyChanged));

        /// <summary>
        /// 列跨度附加属性
        /// </summary>
        public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.Register(
            "ColumnSpan", typeof(int), typeof(Grid),
            new PropertyMetadata(1, OnCellAttachedPropertyChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 行定义集合
        /// </summary>
        public RowDefinitionCollection RowDefinitions => _rowDefinitions;

        /// <summary>
        /// 列定义集合
        /// </summary>
        public ColumnDefinitionCollection ColumnDefinitions => _columnDefinitions;

        #endregion

        #region 附加属性访问器

        /// <summary>
        /// 获取元素的行索引
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>行索引</returns>
        public static int GetRow(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (int)(element.GetValue(RowProperty) ?? 0);
        }

        /// <summary>
        /// 设置元素的行索引
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="value">行索引</param>
        public static void SetRow(UIElement element, int value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(RowProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 获取元素的列索引
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>列索引</returns>
        public static int GetColumn(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (int)(element.GetValue(ColumnProperty) ?? 0);
        }

        /// <summary>
        /// 设置元素的列索引
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="value">列索引</param>
        public static void SetColumn(UIElement element, int value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(ColumnProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 获取元素的行跨度
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>行跨度</returns>
        public static int GetRowSpan(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (int)(element.GetValue(RowSpanProperty) ?? 1);
        }

        /// <summary>
        /// 设置元素的行跨度
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="value">行跨度</param>
        public static void SetRowSpan(UIElement element, int value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(RowSpanProperty, Math.Max(1, value));
        }

        /// <summary>
        /// 获取元素的列跨度
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>列跨度</returns>
        public static int GetColumnSpan(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (int)(element.GetValue(ColumnSpanProperty) ?? 1);
        }

        /// <summary>
        /// 设置元素的列跨度
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="value">列跨度</param>
        public static void SetColumnSpan(UIElement element, int value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(ColumnSpanProperty, Math.Max(1, value));
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
            Debug.WriteLine($"Grid measuring {Children.Count} children, Available size: {availableSize}");

            // 确保至少有一行一列
            int rowCount = Math.Max(1, _rowDefinitions.Count);
            int columnCount = Math.Max(1, _columnDefinitions.Count);

            // 计算行列尺寸
            double[] rowSizes = CalculateRowSizes(availableSize.Height, rowCount);
            double[] columnSizes = CalculateColumnSizes(availableSize.Width, columnCount);

            // 测量每个子元素
            foreach (UIElement child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                int row = Math.Min(GetRow(child), rowCount - 1);
                int column = Math.Min(GetColumn(child), columnCount - 1);
                int rowSpan = Math.Min(GetRowSpan(child), rowCount - row);
                int columnSpan = Math.Min(GetColumnSpan(child), columnCount - column);

                // 计算子元素的可用尺寸
                double childWidth = 0;
                double childHeight = 0;

                for (int i = 0; i < columnSpan; i++)
                {
                    childWidth += columnSizes[column + i];
                }

                for (int i = 0; i < rowSpan; i++)
                {
                    childHeight += rowSizes[row + i];
                }

                Size childAvailableSize = new Size(childWidth, childHeight);
                child.Measure(childAvailableSize);

                Debug.WriteLine($"Child at ({row},{column}) span ({rowSpan},{columnSpan}) measured: {child.DesiredSize}");
            }

            // 计算Grid的期望尺寸
            double totalWidth = columnSizes.Sum();
            double totalHeight = rowSizes.Sum();

            Size desiredSize = new Size(totalWidth, totalHeight);
            Debug.WriteLine($"Grid desired size: {desiredSize}");

            return desiredSize;
        }

        /// <summary>
        /// 排列子元素
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeChildren(Size finalSize)
        {
            Debug.WriteLine($"Grid arranging {Children.Count} children, Final size: {finalSize}");

            // 确保至少有一行一列
            int rowCount = Math.Max(1, _rowDefinitions.Count);
            int columnCount = Math.Max(1, _columnDefinitions.Count);

            // 重新计算最终的行列尺寸
            double[] rowSizes = CalculateRowSizes(finalSize.Height, rowCount);
            double[] columnSizes = CalculateColumnSizes(finalSize.Width, columnCount);

            // 计算行列位置
            double[] rowPositions = new double[rowCount];
            double[] columnPositions = new double[columnCount];

            double currentY = 0;
            for (int i = 0; i < rowCount; i++)
            {
                rowPositions[i] = currentY;
                currentY += rowSizes[i];
            }

            double currentX = 0;
            for (int i = 0; i < columnCount; i++)
            {
                columnPositions[i] = currentX;
                currentX += columnSizes[i];
            }

            // 排列每个子元素
            foreach (UIElement child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                int row = Math.Min(GetRow(child), rowCount - 1);
                int column = Math.Min(GetColumn(child), columnCount - 1);
                int rowSpan = Math.Min(GetRowSpan(child), rowCount - row);
                int columnSpan = Math.Min(GetColumnSpan(child), columnCount - column);

                // 计算子元素的位置和尺寸
                double x = columnPositions[column];
                double y = rowPositions[row];
                double width = 0;
                double height = 0;

                for (int i = 0; i < columnSpan; i++)
                {
                    width += columnSizes[column + i];
                }

                for (int i = 0; i < rowSpan; i++)
                {
                    height += rowSizes[row + i];
                }

                Rect childRect = new Rect(x, y, width, height);
                child.Arrange(childRect);

                Debug.WriteLine($"Child at ({row},{column}) arranged to: {childRect}");
            }

            return finalSize;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 计算行尺寸
        /// </summary>
        /// <param name="availableHeight">可用高度</param>
        /// <param name="rowCount">行数</param>
        /// <returns>行尺寸数组</returns>
        private double[] CalculateRowSizes(double availableHeight, int rowCount)
        {
            double[] sizes = new double[rowCount];

            if (_rowDefinitions.Count == 0)
            {
                // 没有行定义，平均分配
                double averageHeight = availableHeight / rowCount;
                for (int i = 0; i < rowCount; i++)
                {
                    sizes[i] = averageHeight;
                }
            }
            else
            {
                // 使用行定义计算尺寸
                sizes = CalculateSizes(_rowDefinitions.Cast<DefinitionBase>().ToArray(), availableHeight);
            }

            return sizes;
        }

        /// <summary>
        /// 计算列尺寸
        /// </summary>
        /// <param name="availableWidth">可用宽度</param>
        /// <param name="columnCount">列数</param>
        /// <returns>列尺寸数组</returns>
        private double[] CalculateColumnSizes(double availableWidth, int columnCount)
        {
            double[] sizes = new double[columnCount];

            if (_columnDefinitions.Count == 0)
            {
                // 没有列定义，平均分配
                double averageWidth = availableWidth / columnCount;
                for (int i = 0; i < columnCount; i++)
                {
                    sizes[i] = averageWidth;
                }
            }
            else
            {
                // 使用列定义计算尺寸
                sizes = CalculateSizes(_columnDefinitions.Cast<DefinitionBase>().ToArray(), availableWidth);
            }

            return sizes;
        }

        #endregion

        /// <summary>
        /// 计算行列尺寸的核心算法
        /// </summary>
        /// <param name="definitions">定义数组</param>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>计算后的尺寸数组</returns>
        private double[] CalculateSizes(DefinitionBase[] definitions, double availableSize)
        {
            int count = definitions.Length;
            double[] sizes = new double[count];

            // 第一阶段：处理固定尺寸（像素）
            double usedSize = 0;
            double totalStarWeight = 0;
            List<int> autoIndices = new List<int>();
            List<int> starIndices = new List<int>();

            for (int i = 0; i < count; i++)
            {
                DefinitionBase def = definitions[i];
                GridLength length;

                if (def is RowDefinition row)
                    length = row.Height;
                else if (def is ColumnDefinition column)
                    length = column.Width;
                else
                    length = GridLength.Star();

                if (length.IsPixel)
                {
                    sizes[i] = Math.Max(def.MinSize, Math.Min(def.MaxSize, length.Value));
                    usedSize += sizes[i];
                }
                else if (length.IsAuto)
                {
                    autoIndices.Add(i);
                    sizes[i] = def.MinSize; // 先设置为最小尺寸
                    usedSize += sizes[i];
                }
                else if (length.IsStar)
                {
                    starIndices.Add(i);
                    totalStarWeight += length.Value;
                    def.StarWeight = length.Value;
                }
            }

            // 第二阶段：处理自动尺寸（Auto）
            // 这里简化处理，实际应该根据内容计算
            foreach (int index in autoIndices)
            {
                DefinitionBase def = definitions[index];
                // 简化：给Auto一个默认尺寸
                double autoSize = Math.Max(def.MinSize, Math.Min(def.MaxSize, 50.0));
                sizes[index] = autoSize;
                usedSize += autoSize - def.MinSize; // 减去之前加的MinSize
            }

            // 第三阶段：处理星号尺寸（Star）
            double remainingSize = Math.Max(0, availableSize - usedSize);
            if (starIndices.Count > 0 && totalStarWeight > 0)
            {
                double unitSize = remainingSize / totalStarWeight;

                foreach (int index in starIndices)
                {
                    DefinitionBase def = definitions[index];
                    double starSize = unitSize * def.StarWeight;
                    sizes[index] = Math.Max(def.MinSize, Math.Min(def.MaxSize, starSize));
                }
            }

            return sizes;
        }

        #region 属性更改回调

        private static void OnCellAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 当附加属性更改时，需要重新布局父Grid
            if (d is UIElement element)
            {
                // 查找父Grid并使其布局无效
                // 这里简化处理，实际应该遍历可视树查找父Grid
                Debug.WriteLine($"Grid attached property {e.Property.Name} changed to: {e.NewValue}");
            }
        }

        #endregion
    }
}
