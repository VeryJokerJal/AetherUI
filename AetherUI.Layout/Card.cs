using System;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 卡片容器，提供现代化的卡片样式布局
    /// </summary>
    public class Card : FrameworkElement
    {
        private UIElement? _header;
        private UIElement? _content;
        private UIElement? _footer;

        #region 依赖属性

        /// <summary>
        /// 卡片内边距依赖属性
        /// </summary>
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            nameof(Padding), typeof(Thickness), typeof(Card),
            new PropertyMetadata(new Thickness(16), OnPaddingChanged));

        /// <summary>
        /// 头部内边距依赖属性
        /// </summary>
        public static readonly DependencyProperty HeaderPaddingProperty = DependencyProperty.Register(
            nameof(HeaderPadding), typeof(Thickness), typeof(Card),
            new PropertyMetadata(new Thickness(16, 16, 16, 8), OnPaddingChanged));

        /// <summary>
        /// 内容内边距依赖属性
        /// </summary>
        public static readonly DependencyProperty ContentPaddingProperty = DependencyProperty.Register(
            nameof(ContentPadding), typeof(Thickness), typeof(Card),
            new PropertyMetadata(new Thickness(16, 8, 16, 8), OnPaddingChanged));

        /// <summary>
        /// 底部内边距依赖属性
        /// </summary>
        public static readonly DependencyProperty FooterPaddingProperty = DependencyProperty.Register(
            nameof(FooterPadding), typeof(Thickness), typeof(Card),
            new PropertyMetadata(new Thickness(16, 8, 16, 16), OnPaddingChanged));

        /// <summary>
        /// 阴影深度依赖属性
        /// </summary>
        public static readonly DependencyProperty ElevationProperty = DependencyProperty.Register(
            nameof(Elevation), typeof(double), typeof(Card),
            new PropertyMetadata(2.0, OnElevationChanged));

        /// <summary>
        /// 圆角半径依赖属性
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(double), typeof(Card),
            new PropertyMetadata(8.0, OnCornerRadiusChanged));

        /// <summary>
        /// 背景依赖属性
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background), typeof(object), typeof(Card),
            new PropertyMetadata(null, OnBackgroundChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 卡片内边距
        /// </summary>
        public Thickness Padding
        {
            get => (Thickness)(GetValue(PaddingProperty) ?? new Thickness(16));
            set => SetValue(PaddingProperty, value);
        }

        /// <summary>
        /// 头部内边距
        /// </summary>
        public Thickness HeaderPadding
        {
            get => (Thickness)(GetValue(HeaderPaddingProperty) ?? new Thickness(16, 16, 16, 8));
            set => SetValue(HeaderPaddingProperty, value);
        }

        /// <summary>
        /// 内容内边距
        /// </summary>
        public Thickness ContentPadding
        {
            get => (Thickness)(GetValue(ContentPaddingProperty) ?? new Thickness(16, 8, 16, 8));
            set => SetValue(ContentPaddingProperty, value);
        }

        /// <summary>
        /// 底部内边距
        /// </summary>
        public Thickness FooterPadding
        {
            get => (Thickness)(GetValue(FooterPaddingProperty) ?? new Thickness(16, 8, 16, 16));
            set => SetValue(FooterPaddingProperty, value);
        }

        /// <summary>
        /// 阴影深度
        /// </summary>
        public double Elevation
        {
            get => (double)(GetValue(ElevationProperty) ?? 2.0);
            set => SetValue(ElevationProperty, value);
        }

        /// <summary>
        /// 圆角半径
        /// </summary>
        public double CornerRadius
        {
            get => (double)(GetValue(CornerRadiusProperty) ?? 8.0);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// 背景
        /// </summary>
        public object? Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        /// <summary>
        /// 头部内容
        /// </summary>
        public UIElement? Header
        {
            get => _header;
            set
            {
                if (_header != value)
                {
                    _header = value;
                    InvalidateMeasure();
                    Debug.WriteLine($"Card header changed to: {value?.GetType().Name ?? "null"}");
                }
            }
        }

        /// <summary>
        /// 主要内容
        /// </summary>
        public UIElement? Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    InvalidateMeasure();
                    Debug.WriteLine($"Card content changed to: {value?.GetType().Name ?? "null"}");
                }
            }
        }

        /// <summary>
        /// 底部内容
        /// </summary>
        public UIElement? Footer
        {
            get => _footer;
            set
            {
                if (_footer != value)
                {
                    _footer = value;
                    InvalidateMeasure();
                    Debug.WriteLine($"Card footer changed to: {value?.GetType().Name ?? "null"}");
                }
            }
        }

        #endregion

        #region 布局重写

        /// <summary>
        /// 测量Card和子元素
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Debug.WriteLine($"Card measuring, Available size: {availableSize}");

            double totalWidth = 0;
            double totalHeight = 0;

            // 测量头部
            if (_header != null && _header.Visibility != Visibility.Collapsed)
            {
                Thickness headerPadding = HeaderPadding;
                Size headerAvailableSize = new Size(
                    Math.Max(0, availableSize.Width - headerPadding.Horizontal),
                    double.PositiveInfinity);

                _header.Measure(headerAvailableSize);
                Size headerDesiredSize = _header.DesiredSize;

                totalWidth = Math.Max(totalWidth, headerDesiredSize.Width + headerPadding.Horizontal);
                totalHeight += headerDesiredSize.Height + headerPadding.Vertical;

                Debug.WriteLine($"Card header desired size: {headerDesiredSize}");
            }

            // 测量内容
            if (_content != null && _content.Visibility != Visibility.Collapsed)
            {
                Thickness contentPadding = ContentPadding;
                Size contentAvailableSize = new Size(
                    Math.Max(0, availableSize.Width - contentPadding.Horizontal),
                    Math.Max(0, availableSize.Height - totalHeight - contentPadding.Vertical));

                _content.Measure(contentAvailableSize);
                Size contentDesiredSize = _content.DesiredSize;

                totalWidth = Math.Max(totalWidth, contentDesiredSize.Width + contentPadding.Horizontal);
                totalHeight += contentDesiredSize.Height + contentPadding.Vertical;

                Debug.WriteLine($"Card content desired size: {contentDesiredSize}");
            }

            // 测量底部
            if (_footer != null && _footer.Visibility != Visibility.Collapsed)
            {
                Thickness footerPadding = FooterPadding;
                Size footerAvailableSize = new Size(
                    Math.Max(0, availableSize.Width - footerPadding.Horizontal),
                    double.PositiveInfinity);

                _footer.Measure(footerAvailableSize);
                Size footerDesiredSize = _footer.DesiredSize;

                totalWidth = Math.Max(totalWidth, footerDesiredSize.Width + footerPadding.Horizontal);
                totalHeight += footerDesiredSize.Height + footerPadding.Vertical;

                Debug.WriteLine($"Card footer desired size: {footerDesiredSize}");
            }

            Size desiredSize = new Size(totalWidth, totalHeight);
            Debug.WriteLine($"Card desired size: {desiredSize}");

            return desiredSize;
        }

        /// <summary>
        /// 排列Card和子元素
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine($"Card arranging, Final size: {finalSize}");

            double currentY = 0;

            // 排列头部
            if (_header != null && _header.Visibility != Visibility.Collapsed)
            {
                Thickness headerPadding = HeaderPadding;
                double headerWidth = Math.Max(0, finalSize.Width - headerPadding.Horizontal);
                double headerHeight = _header.DesiredSize.Height;

                Rect headerRect = new Rect(
                    headerPadding.Left,
                    currentY + headerPadding.Top,
                    headerWidth,
                    headerHeight);

                _header.Arrange(headerRect);
                currentY += headerHeight + headerPadding.Vertical;

                Debug.WriteLine($"Card header arranged to: {headerRect}");
            }

            // 排列内容
            if (_content != null && _content.Visibility != Visibility.Collapsed)
            {
                Thickness contentPadding = ContentPadding;
                double contentWidth = Math.Max(0, finalSize.Width - contentPadding.Horizontal);
                double contentHeight = Math.Max(0, finalSize.Height - currentY - contentPadding.Vertical);

                // 如果有底部，需要为底部预留空间
                if (_footer != null && _footer.Visibility != Visibility.Collapsed)
                {
                    Thickness footerPadding = FooterPadding;
                    double footerHeight = _footer.DesiredSize.Height + footerPadding.Vertical;
                    contentHeight = Math.Max(0, contentHeight - footerHeight);
                }

                Rect contentRect = new Rect(
                    contentPadding.Left,
                    currentY + contentPadding.Top,
                    contentWidth,
                    contentHeight);

                _content.Arrange(contentRect);
                currentY += contentHeight + contentPadding.Vertical;

                Debug.WriteLine($"Card content arranged to: {contentRect}");
            }

            // 排列底部
            if (_footer != null && _footer.Visibility != Visibility.Collapsed)
            {
                Thickness footerPadding = FooterPadding;
                double footerWidth = Math.Max(0, finalSize.Width - footerPadding.Horizontal);
                double footerHeight = _footer.DesiredSize.Height;

                Rect footerRect = new Rect(
                    footerPadding.Left,
                    finalSize.Height - footerHeight - footerPadding.Bottom,
                    footerWidth,
                    footerHeight);

                _footer.Arrange(footerRect);

                Debug.WriteLine($"Card footer arranged to: {footerRect}");
            }

            return finalSize;
        }

        #endregion

        #region 属性更改回调

        private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Card card)
            {
                card.InvalidateMeasure();
                Debug.WriteLine($"Card {e.Property.Name} changed to: {e.NewValue}");
            }
        }

        private static void OnElevationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Card card)
            {
                Debug.WriteLine($"Card elevation changed to: {e.NewValue}");
                // 阴影更改不影响布局，只需要重绘
            }
        }

        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Card card)
            {
                Debug.WriteLine($"Card corner radius changed to: {e.NewValue}");
                // 圆角更改不影响布局，只需要重绘
            }
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Card card)
            {
                Debug.WriteLine($"Card background changed to: {e.NewValue}");
                // 背景更改不影响布局，只需要重绘
            }
        }

        #endregion
    }
}
