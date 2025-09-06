using System;
using System.Diagnostics;

namespace AetherUI.Core
{
    /// <summary>
    /// 水平对齐方式
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// 左对齐
        /// </summary>
        Left,

        /// <summary>
        /// 居中对齐
        /// </summary>
        Center,

        /// <summary>
        /// 右对齐
        /// </summary>
        Right,

        /// <summary>
        /// 拉伸填充
        /// </summary>
        Stretch
    }

    /// <summary>
    /// 垂直对齐方式
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// 顶部对齐
        /// </summary>
        Top,

        /// <summary>
        /// 居中对齐
        /// </summary>
        Center,

        /// <summary>
        /// 底部对齐
        /// </summary>
        Bottom,

        /// <summary>
        /// 拉伸填充
        /// </summary>
        Stretch
    }

    /// <summary>
    /// 框架元素基类，提供布局、样式和数据绑定的高级功能
    /// </summary>
    public abstract class FrameworkElement : UIElement
    {
        #region 依赖属性

        /// <summary>
        /// 宽度依赖属性
        /// </summary>
        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            nameof(Width), typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(double.NaN, OnSizeChanged));

        /// <summary>
        /// 高度依赖属性
        /// </summary>
        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
            nameof(Height), typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(double.NaN, OnSizeChanged));

        /// <summary>
        /// 最小宽度依赖属性
        /// </summary>
        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register(
            nameof(MinWidth), typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(0.0, OnSizeChanged));

        /// <summary>
        /// 最小高度依赖属性
        /// </summary>
        public static readonly DependencyProperty MinHeightProperty = DependencyProperty.Register(
            nameof(MinHeight), typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(0.0, OnSizeChanged));

        /// <summary>
        /// 最大宽度依赖属性
        /// </summary>
        public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register(
            nameof(MaxWidth), typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(double.PositiveInfinity, OnSizeChanged));

        /// <summary>
        /// 最大高度依赖属性
        /// </summary>
        public static readonly DependencyProperty MaxHeightProperty = DependencyProperty.Register(
            nameof(MaxHeight), typeof(double), typeof(FrameworkElement),
            new PropertyMetadata(double.PositiveInfinity, OnSizeChanged));

        /// <summary>
        /// 外边距依赖属性
        /// </summary>
        public static readonly DependencyProperty MarginProperty = DependencyProperty.Register(
            nameof(Margin), typeof(Thickness), typeof(FrameworkElement),
            new PropertyMetadata(new Thickness(10), OnMarginChanged));

        /// <summary>
        /// 水平对齐依赖属性
        /// </summary>
        public static readonly DependencyProperty HorizontalAlignmentProperty = DependencyProperty.Register(
            nameof(HorizontalAlignment), typeof(HorizontalAlignment), typeof(FrameworkElement),
            new PropertyMetadata(HorizontalAlignment.Stretch, OnAlignmentChanged));

        /// <summary>
        /// 垂直对齐依赖属性
        /// </summary>
        public static readonly DependencyProperty VerticalAlignmentProperty = DependencyProperty.Register(
            nameof(VerticalAlignment), typeof(VerticalAlignment), typeof(FrameworkElement),
            new PropertyMetadata(VerticalAlignment.Stretch, OnAlignmentChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 宽度
        /// </summary>
        public double Width
        {
            get => (double)GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        /// <summary>
        /// 高度
        /// </summary>
        public double Height
        {
            get => (double)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        /// <summary>
        /// 最小宽度
        /// </summary>
        public double MinWidth
        {
            get => (double)GetValue(MinWidthProperty);
            set => SetValue(MinWidthProperty, value);
        }

        /// <summary>
        /// 最小高度
        /// </summary>
        public double MinHeight
        {
            get => (double)GetValue(MinHeightProperty);
            set => SetValue(MinHeightProperty, value);
        }

        /// <summary>
        /// 最大宽度
        /// </summary>
        public double MaxWidth
        {
            get => (double)GetValue(MaxWidthProperty);
            set => SetValue(MaxWidthProperty, value);
        }

        /// <summary>
        /// 最大高度
        /// </summary>
        public double MaxHeight
        {
            get => (double)GetValue(MaxHeightProperty);
            set => SetValue(MaxHeightProperty, value);
        }

        /// <summary>
        /// 外边距
        /// </summary>
        public Thickness Margin
        {
            get => (Thickness)GetValue(MarginProperty);
            set => SetValue(MarginProperty, value);
        }

        /// <summary>
        /// 水平对齐
        /// </summary>
        public HorizontalAlignment HorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(HorizontalAlignmentProperty);
            set => SetValue(HorizontalAlignmentProperty, value);
        }

        /// <summary>
        /// 垂直对齐
        /// </summary>
        public VerticalAlignment VerticalAlignment
        {
            get => (VerticalAlignment)GetValue(VerticalAlignmentProperty);
            set => SetValue(VerticalAlignmentProperty, value);
        }

        #endregion

        #region 布局重写

        /// <summary>
        /// 重写测量核心方法，应用尺寸约束
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected override Size MeasureCore(Size availableSize)
        {
            // 应用外边距
            Thickness margin = Margin;
            Size marginSize = new Size(margin.Horizontal, margin.Vertical);
            Size constrainedAvailableSize = new Size(
                Math.Max(0, availableSize.Width - marginSize.Width),
                Math.Max(0, availableSize.Height - marginSize.Height));

            // 应用尺寸约束
            constrainedAvailableSize = ApplySizeConstraints(constrainedAvailableSize);

            // 调用子类的测量方法
            Size desiredSize = MeasureOverride(constrainedAvailableSize);

            // 应用尺寸约束到期望尺寸
            desiredSize = ApplySizeConstraints(desiredSize);

            // 添加外边距
            return new Size(
                desiredSize.Width + marginSize.Width,
                desiredSize.Height + marginSize.Height);
        }

        /// <summary>
        /// 重写排列核心方法，应用对齐和外边距
        /// </summary>
        /// <param name="finalRect">最终矩形</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeCore(Rect finalRect)
        {
            // 应用外边距
            Thickness margin = Margin;
            Rect arrangeRect = new Rect(
                finalRect.X + margin.Left,
                finalRect.Y + margin.Top,
                Math.Max(0, finalRect.Width - margin.Horizontal),
                Math.Max(0, finalRect.Height - margin.Vertical));

            // 应用对齐
            Size desiredSizeWithoutMargin = new Size(
                Math.Max(0, DesiredSize.Width - margin.Horizontal),
                Math.Max(0, DesiredSize.Height - margin.Vertical));

            arrangeRect = ApplyAlignment(arrangeRect, desiredSizeWithoutMargin);

            // 调用子类的排列方法
            Size arrangedSize = ArrangeOverride(arrangeRect.Size);

            // 返回包含外边距的总尺寸
            return new Size(
                arrangedSize.Width + margin.Horizontal,
                arrangedSize.Height + margin.Vertical);
        }

        /// <summary>
        /// 子类重写以提供自定义测量逻辑
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected virtual Size MeasureOverride(Size availableSize)
        {
            return Size.Empty;
        }

        /// <summary>
        /// 子类重写以提供自定义排列逻辑
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected virtual Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 应用尺寸约束
        /// </summary>
        /// <param name="size">原始尺寸</param>
        /// <returns>约束后的尺寸</returns>
        private Size ApplySizeConstraints(Size size)
        {
            double width = size.Width;
            double height = size.Height;

            // 应用显式宽度
            if (!double.IsNaN(Width))
                width = Width;

            // 应用显式高度
            if (!double.IsNaN(Height))
                height = Height;

            // 应用最小/最大约束
            width = Math.Max(MinWidth, Math.Min(MaxWidth, width));
            height = Math.Max(MinHeight, Math.Min(MaxHeight, height));

            return new Size(width, height);
        }

        /// <summary>
        /// 应用对齐
        /// </summary>
        /// <param name="arrangeRect">排列矩形</param>
        /// <param name="desiredSize">期望尺寸</param>
        /// <returns>对齐后的矩形</returns>
        private Rect ApplyAlignment(Rect arrangeRect, Size desiredSize)
        {
            double x = arrangeRect.X;
            double y = arrangeRect.Y;
            double width = arrangeRect.Width;
            double height = arrangeRect.Height;

            // 水平对齐
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    width = Math.Min(desiredSize.Width, arrangeRect.Width);
                    break;
                case HorizontalAlignment.Center:
                    width = Math.Min(desiredSize.Width, arrangeRect.Width);
                    x += (arrangeRect.Width - width) / 2;
                    break;
                case HorizontalAlignment.Right:
                    width = Math.Min(desiredSize.Width, arrangeRect.Width);
                    x += arrangeRect.Width - width;
                    break;
                case HorizontalAlignment.Stretch:
                    // 保持原始宽度
                    break;
            }

            // 垂直对齐
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    height = Math.Min(desiredSize.Height, arrangeRect.Height);
                    break;
                case VerticalAlignment.Center:
                    height = Math.Min(desiredSize.Height, arrangeRect.Height);
                    y += (arrangeRect.Height - height) / 2;
                    break;
                case VerticalAlignment.Bottom:
                    height = Math.Min(desiredSize.Height, arrangeRect.Height);
                    y += arrangeRect.Height - height;
                    break;
                case VerticalAlignment.Stretch:
                    // 保持原始高度
                    break;
            }

            return new Rect(x, y, width, height);
        }

        #endregion

        #region 属性更改回调

        private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.InvalidateMeasure();
}
        }

        private static void OnMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.InvalidateMeasure();
}
        }

        private static void OnAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.InvalidateArrange();
}
        }

        #endregion
    }
}
