using System;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 统一网格，所有单元格大小相同
    /// </summary>
    public class UniformGrid : Panel
    {
        #region 依赖属性

        /// <summary>
        /// 行数依赖属性
        /// </summary>
        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
            nameof(Rows), typeof(int), typeof(UniformGrid),
            new PropertyMetadata(0, OnGridSizeChanged));

        /// <summary>
        /// 列数依赖属性
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
            nameof(Columns), typeof(int), typeof(UniformGrid),
            new PropertyMetadata(0, OnGridSizeChanged));

        /// <summary>
        /// 第一列依赖属性
        /// </summary>
        public static readonly DependencyProperty FirstColumnProperty = DependencyProperty.Register(
            nameof(FirstColumn), typeof(int), typeof(UniformGrid),
            new PropertyMetadata(0, OnFirstColumnChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 行数（0表示自动计算）
        /// </summary>
        public int Rows
        {
            get => (int)(GetValue(RowsProperty) ?? 0);
            set => SetValue(RowsProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 列数（0表示自动计算）
        /// </summary>
        public int Columns
        {
            get => (int)(GetValue(ColumnsProperty) ?? 0);
            set => SetValue(ColumnsProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 第一个元素所在的列（用于创建不规则布局）
        /// </summary>
        public int FirstColumn
        {
            get => (int)(GetValue(FirstColumnProperty) ?? 0);
            set => SetValue(FirstColumnProperty, Math.Max(0, value));
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
            (int rows, int columns) = CalculateGridSize();
if (rows == 0 || columns == 0)
                return Size.Empty;

            // 计算每个单元格的尺寸
            double cellWidth = availableSize.Width / columns;
            double cellHeight = availableSize.Height / rows;
            Size cellSize = new Size(cellWidth, cellHeight);
            foreach (UIElement child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                child.Measure(cellSize);
}

            // UniformGrid的期望尺寸是网格的总尺寸
            Size desiredSize = new Size(columns * cellWidth, rows * cellHeight);
return desiredSize;
        }

        /// <summary>
        /// 排列子元素
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeChildren(Size finalSize)
        {
            (int rows, int columns) = CalculateGridSize();

            if (rows == 0 || columns == 0)
                return finalSize;

            // 计算每个单元格的实际尺寸
            double cellWidth = finalSize.Width / columns;
            double cellHeight = finalSize.Height / rows;
int childIndex = 0;
            int currentColumn = FirstColumn;
            int currentRow = 0;

            foreach (UIElement child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                // 计算子元素的位置
                double x = currentColumn * cellWidth;
                double y = currentRow * cellHeight;

                Rect childRect = new Rect(x, y, cellWidth, cellHeight);
                child.Arrange(childRect);
                currentColumn++;
                if (currentColumn >= columns)
                {
                    currentColumn = 0;
                    currentRow++;
                }

                childIndex++;

                // 如果超出网格范围，停止排列
                if (currentRow >= rows)
                    break;
            }

            return finalSize;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 计算网格尺寸
        /// </summary>
        /// <returns>行数和列数</returns>
        private (int rows, int columns) CalculateGridSize()
        {
            // 计算可见子元素数量
            int visibleChildCount = 0;
            foreach (UIElement child in Children)
            {
                if (child.Visibility != Visibility.Collapsed)
                    visibleChildCount++;
            }

            if (visibleChildCount == 0)
                return (0, 0);

            int rows = Rows;
            int columns = Columns;

            // 如果都没有指定，创建一个正方形网格
            if (rows == 0 && columns == 0)
            {
                // 考虑FirstColumn的影响
                int totalCells = visibleChildCount + FirstColumn;
                rows = (int)Math.Ceiling(Math.Sqrt(totalCells));
                columns = rows;
            }
            // 如果只指定了列数，计算行数
            else if (rows == 0 && columns > 0)
            {
                int totalCells = visibleChildCount + FirstColumn;
                rows = (int)Math.Ceiling((double)totalCells / columns);
            }
            // 如果只指定了行数，计算列数
            else if (rows > 0 && columns == 0)
            {
                int totalCells = visibleChildCount + FirstColumn;
                columns = (int)Math.Ceiling((double)totalCells / rows);
            }
            // 如果都指定了，直接使用

            return (Math.Max(1, rows), Math.Max(1, columns));
        }

        #endregion

        #region 属性更改回调

        private static void OnGridSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UniformGrid uniformGrid)
            {
                uniformGrid.InvalidateMeasure();
}
        }

        private static void OnFirstColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UniformGrid uniformGrid)
            {
                uniformGrid.InvalidateArrange();
}
        }

        #endregion
    }
}
