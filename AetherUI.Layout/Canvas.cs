using System;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 画布布局容器，支持绝对定位
    /// </summary>
    public class Canvas : Panel
    {
        #region 附加属性

        /// <summary>
        /// 左边距附加属性
        /// </summary>
        public static readonly DependencyProperty LeftProperty = DependencyProperty.Register(
            "Left", typeof(double), typeof(Canvas),
            new PropertyMetadata(double.NaN, OnPositionChanged));

        /// <summary>
        /// 上边距附加属性
        /// </summary>
        public static readonly DependencyProperty TopProperty = DependencyProperty.Register(
            "Top", typeof(double), typeof(Canvas),
            new PropertyMetadata(double.NaN, OnPositionChanged));

        /// <summary>
        /// 右边距附加属性
        /// </summary>
        public static readonly DependencyProperty RightProperty = DependencyProperty.Register(
            "Right", typeof(double), typeof(Canvas),
            new PropertyMetadata(double.NaN, OnPositionChanged));

        /// <summary>
        /// 下边距附加属性
        /// </summary>
        public static readonly DependencyProperty BottomProperty = DependencyProperty.Register(
            "Bottom", typeof(double), typeof(Canvas),
            new PropertyMetadata(double.NaN, OnPositionChanged));

        /// <summary>
        /// Z轴顺序附加属性
        /// </summary>
        public static readonly DependencyProperty ZIndexProperty = DependencyProperty.Register(
            "ZIndex", typeof(int), typeof(Canvas),
            new PropertyMetadata(0, OnZIndexChanged));

        #endregion

        #region 附加属性访问器

        /// <summary>
        /// 获取元素的左边距
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>左边距</returns>
        public static double GetLeft(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (double)(element.GetValue(LeftProperty) ?? double.NaN);
        }

        /// <summary>
        /// 设置元素的左边距
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="value">左边距</param>
        public static void SetLeft(UIElement element, double value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(LeftProperty, value);
        }

        /// <summary>
        /// 获取元素的上边距
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>上边距</returns>
        public static double GetTop(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (double)(element.GetValue(TopProperty) ?? double.NaN);
        }

        /// <summary>
        /// 设置元素的上边距
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="value">上边距</param>
        public static void SetTop(UIElement element, double value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(TopProperty, value);
        }

        /// <summary>
        /// 获取元素的右边距
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>右边距</returns>
        public static double GetRight(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (double)(element.GetValue(RightProperty) ?? double.NaN);
        }

        /// <summary>
        /// 设置元素的右边距
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="value">右边距</param>
        public static void SetRight(UIElement element, double value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(RightProperty, value);
        }

        /// <summary>
        /// 获取元素的下边距
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>下边距</returns>
        public static double GetBottom(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (double)(element.GetValue(BottomProperty) ?? double.NaN);
        }

        /// <summary>
        /// 设置元素的下边距
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="value">下边距</param>
        public static void SetBottom(UIElement element, double value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(BottomProperty, value);
        }

        /// <summary>
        /// 获取元素的Z轴顺序
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>Z轴顺序</returns>
        public static int GetZIndex(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (int)(element.GetValue(ZIndexProperty) ?? 0);
        }

        /// <summary>
        /// 设置元素的Z轴顺序
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="value">Z轴顺序</param>
        public static void SetZIndex(UIElement element, int value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(ZIndexProperty, value);
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
            Debug.WriteLine($"Canvas measuring {Children.Count} children, Available size: {availableSize}");

            // Canvas给每个子元素无限的空间进行测量
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            double maxRight = 0;
            double maxBottom = 0;

            foreach (UIElement child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                // 测量子元素
                child.Measure(infiniteSize);
                Size childDesiredSize = child.DesiredSize;

                Debug.WriteLine($"Child {child.GetType().Name} desired size: {childDesiredSize}");

                // 计算子元素的位置和边界
                double left = GetLeft(child);
                double top = GetTop(child);
                double right = GetRight(child);
                double bottom = GetBottom(child);

                // 计算子元素的实际位置和尺寸
                double childLeft = double.IsNaN(left) ? 0 : left;
                double childTop = double.IsNaN(top) ? 0 : top;
                double childWidth = childDesiredSize.Width;
                double childHeight = childDesiredSize.Height;

                // 如果设置了Right但没有设置Left，从右边定位
                if (!double.IsNaN(right) && double.IsNaN(left))
                {
                    childLeft = availableSize.Width - right - childWidth;
                }

                // 如果设置了Bottom但没有设置Top，从下边定位
                if (!double.IsNaN(bottom) && double.IsNaN(top))
                {
                    childTop = availableSize.Height - bottom - childHeight;
                }

                // 更新Canvas的边界
                maxRight = Math.Max(maxRight, childLeft + childWidth);
                maxBottom = Math.Max(maxBottom, childTop + childHeight);

                Debug.WriteLine($"Child positioned at ({childLeft}, {childTop}) with size ({childWidth}, {childHeight})");
            }

            // Canvas的期望尺寸是包含所有子元素的最小尺寸
            Size desiredSize = new Size(maxRight, maxBottom);
            Debug.WriteLine($"Canvas desired size: {desiredSize}");

            return desiredSize;
        }

        /// <summary>
        /// 排列子元素
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeChildren(Size finalSize)
        {
            Debug.WriteLine($"Canvas arranging {Children.Count} children, Final size: {finalSize}");

            foreach (UIElement child in Children)
            {
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                Size childDesiredSize = child.DesiredSize;

                // 获取定位属性
                double left = GetLeft(child);
                double top = GetTop(child);
                double right = GetRight(child);
                double bottom = GetBottom(child);

                // 计算子元素的位置
                double x = 0;
                double y = 0;
                double width = childDesiredSize.Width;
                double height = childDesiredSize.Height;

                // 水平定位
                if (!double.IsNaN(left))
                {
                    x = left;
                    
                    // 如果同时设置了Left和Right，计算宽度
                    if (!double.IsNaN(right))
                    {
                        width = Math.Max(0, finalSize.Width - left - right);
                    }
                }
                else if (!double.IsNaN(right))
                {
                    // 只设置了Right，从右边定位
                    x = finalSize.Width - right - width;
                }

                // 垂直定位
                if (!double.IsNaN(top))
                {
                    y = top;
                    
                    // 如果同时设置了Top和Bottom，计算高度
                    if (!double.IsNaN(bottom))
                    {
                        height = Math.Max(0, finalSize.Height - top - bottom);
                    }
                }
                else if (!double.IsNaN(bottom))
                {
                    // 只设置了Bottom，从下边定位
                    y = finalSize.Height - bottom - height;
                }

                // 排列子元素
                Rect childRect = new Rect(x, y, width, height);
                child.Arrange(childRect);

                Debug.WriteLine($"Child arranged to: {childRect}");
            }

            return finalSize;
        }

        #endregion

        #region 属性更改回调

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 当位置属性更改时，需要重新布局父Canvas
            if (d is UIElement element)
            {
                // 查找父Canvas并使其布局无效
                // 这里简化处理，实际应该遍历可视树查找父Canvas
                Debug.WriteLine($"Canvas position property {e.Property.Name} changed to: {e.NewValue}");
            }
        }

        private static void OnZIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 当Z轴顺序更改时，需要重新排序渲染
            if (d is UIElement element)
            {
                Debug.WriteLine($"Canvas ZIndex changed to: {e.NewValue}");
                // 这里应该触发重新排序和重绘
            }
        }

        #endregion
    }
}
