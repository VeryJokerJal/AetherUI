using System;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 边框装饰容器，为单个子元素提供边框和背景
    /// </summary>
    public class Border : FrameworkElement
    {
        private UIElement? _child;

        #region 依赖属性

        /// <summary>
        /// 边框厚度依赖属性
        /// </summary>
        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
            nameof(BorderThickness), typeof(Thickness), typeof(Border),
            new PropertyMetadata(Thickness.Zero, OnBorderChanged));

        /// <summary>
        /// 边框画刷依赖属性
        /// </summary>
        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            nameof(BorderBrush), typeof(object), typeof(Border),
            new PropertyMetadata(null, OnBorderChanged));

        /// <summary>
        /// 背景画刷依赖属性
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background), typeof(object), typeof(Border),
            new PropertyMetadata(null, OnBackgroundChanged));

        /// <summary>
        /// 内边距依赖属性
        /// </summary>
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            nameof(Padding), typeof(Thickness), typeof(Border),
            new PropertyMetadata(new Thickness(10), OnPaddingChanged));

        /// <summary>
        /// 圆角半径依赖属性
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(double), typeof(Border),
            new PropertyMetadata(0.0, OnCornerRadiusChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 边框厚度
        /// </summary>
        public Thickness BorderThickness
        {
            get => (Thickness)(GetValue(BorderThicknessProperty) ?? Thickness.Zero);
            set => SetValue(BorderThicknessProperty, value);
        }

        /// <summary>
        /// 边框画刷
        /// </summary>
        public object? BorderBrush
        {
            get => GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        /// <summary>
        /// 背景画刷
        /// </summary>
        public object? Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        /// <summary>
        /// 内边距
        /// </summary>
        public Thickness Padding
        {
            get => (Thickness)(GetValue(PaddingProperty) ?? new Thickness(10));
            set => SetValue(PaddingProperty, value);
        }

        /// <summary>
        /// 圆角半径
        /// </summary>
        public double CornerRadius
        {
            get => (double)(GetValue(CornerRadiusProperty) ?? 0.0);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// 子元素
        /// </summary>
        public UIElement? Child
        {
            get => _child;
            set
            {
                if (_child != value)
                {
                    if (_child != null && _child.Parent == this)
                    {
                        _child.Parent = null;
                    }
                    _child = value;
                    if (_child != null)
                    {
                        _child.Parent = this;
                    }
                    InvalidateMeasure();
}
            }
        }

        #endregion

        #region 可视树

        /// <summary>
        /// 获取可视子元素
        /// </summary>
        public override System.Collections.Generic.IEnumerable<UIElement> GetVisualChildren()
        {
            if (_child != null)
            {
                yield return _child;
            }
        }

        #endregion

        #region 布局重写

        /// <summary>
        /// 测量Border和子元素
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
Thickness borderThickness = BorderThickness;
            Thickness padding = Padding;

            // 计算边框和内边距占用的空间
            Thickness combinedThickness = new Thickness(
                borderThickness.Left + padding.Left,
                borderThickness.Top + padding.Top,
                borderThickness.Right + padding.Right,
                borderThickness.Bottom + padding.Bottom);

            // 计算子元素的可用尺寸
            Size childAvailableSize = new Size(
                Math.Max(0, availableSize.Width - combinedThickness.Horizontal),
                Math.Max(0, availableSize.Height - combinedThickness.Vertical));

            Size childDesiredSize = Size.Empty;

            if (_child != null && _child.Visibility != Visibility.Collapsed)
            {
                _child.Measure(childAvailableSize);
                childDesiredSize = _child.DesiredSize;
}

            // Border的期望尺寸是子元素尺寸加上边框和内边距
            Size desiredSize = new Size(
                childDesiredSize.Width + combinedThickness.Horizontal,
                childDesiredSize.Height + combinedThickness.Vertical);
return desiredSize;
        }

        /// <summary>
        /// 排列Border和子元素
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
if (_child != null && _child.Visibility != Visibility.Collapsed)
            {
                Thickness borderThickness = BorderThickness;
                Thickness padding = Padding;

                // 计算子元素的排列区域
                double childX = borderThickness.Left + padding.Left;
                double childY = borderThickness.Top + padding.Top;
                double childWidth = Math.Max(0, finalSize.Width - borderThickness.Horizontal - padding.Horizontal);
                double childHeight = Math.Max(0, finalSize.Height - borderThickness.Vertical - padding.Vertical);

                Rect childRect = new Rect(childX, childY, childWidth, childHeight);
                _child.Arrange(childRect);
}

            return finalSize;
        }

        #endregion

        #region 属性更改回调

        private static void OnBorderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
                border.InvalidateMeasure();
}
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
            }
        }

        private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
                border.InvalidateMeasure();
}
        }

        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
            }
        }

        #endregion
    }
}
