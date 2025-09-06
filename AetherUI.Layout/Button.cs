using System;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 按钮控件
    /// </summary>
    public class Button : FrameworkElement
    {
        #region 依赖属性

        /// <summary>
        /// 内容依赖属性
        /// </summary>
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), typeof(object), typeof(Button),
            new PropertyMetadata(null, OnContentChanged));

        /// <summary>
        /// 命令依赖属性
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command), typeof(AetherUI.Core.ICommand), typeof(Button),
            new PropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// 命令参数依赖属性
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            nameof(CommandParameter), typeof(object), typeof(Button),
            new PropertyMetadata(null));

        /// <summary>
        /// 内边距依赖属性
        /// </summary>
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            nameof(Padding), typeof(Thickness), typeof(Button),
            new PropertyMetadata(new Thickness(10), OnPaddingChanged));

        /// <summary>
        /// 背景颜色依赖属性
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background), typeof(string), typeof(Button),
            new PropertyMetadata("#3498DB", OnBackgroundChanged));

        /// <summary>
        /// 前景颜色（文本颜色）依赖属性
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            nameof(Foreground), typeof(string), typeof(Button),
            new PropertyMetadata("#FFFFFF", OnForegroundChanged));

        /// <summary>
        /// 边框颜色依赖属性
        /// </summary>
        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            nameof(BorderBrush), typeof(string), typeof(Button),
            new PropertyMetadata("#2980B9", OnBorderBrushChanged));

        /// <summary>
        /// 圆角弧度依赖属性
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(double), typeof(Button),
            new PropertyMetadata(7.0, OnCornerRadiusChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 按钮内容
        /// </summary>
        public object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>
        /// 命令
        /// </summary>
        public AetherUI.Core.ICommand? Command
        {
            get => (AetherUI.Core.ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// 命令参数
        /// </summary>
        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
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
        /// 背景颜色（支持十六进制颜色值，如 #3498DB）
        /// </summary>
        public string Background
        {
            get => (string)(GetValue(BackgroundProperty) ?? "#3498DB");
            set => SetValue(BackgroundProperty, value);
        }

        /// <summary>
        /// 前景颜色（文本颜色，支持十六进制颜色值，如 #FFFFFF）
        /// </summary>
        public string Foreground
        {
            get => (string)(GetValue(ForegroundProperty) ?? "#FFFFFF");
            set => SetValue(ForegroundProperty, value);
        }

        /// <summary>
        /// 边框颜色（支持十六进制颜色值，如 #2980B9）
        /// </summary>
        public string BorderBrush
        {
            get => (string)(GetValue(BorderBrushProperty) ?? "#2980B9");
            set => SetValue(BorderBrushProperty, value);
        }

        /// <summary>
        /// 圆角弧度（像素值，默认为 7 像素）
        /// </summary>
        public double CornerRadius
        {
            get => (double)(GetValue(CornerRadiusProperty) ?? 7.0);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region 布局重写

        /// <summary>
        /// 测量按钮尺寸
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Thickness padding = Padding;

            // 计算内容区域的可用尺寸
            Size contentAvailableSize = new(
                Math.Max(0, availableSize.Width - padding.Horizontal),
                Math.Max(0, availableSize.Height - padding.Vertical));

            Size contentSize = MeasureContent(contentAvailableSize);

            // 添加内边距
            Size totalSize = new(
                contentSize.Width + padding.Horizontal,
                contentSize.Height + padding.Vertical);
            return totalSize;
        }

        /// <summary>
        /// 排列按钮
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        /// <summary>
        /// 测量内容尺寸
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>内容尺寸</returns>
        private Size MeasureContent(Size availableSize)
        {
            if (Content == null)
            {
                return Size.Empty;
            }

            // 如果内容是字符串，估算文本尺寸
            if (Content is string text)
            {
                double fontSize = 12.0; // 默认字体大小
                double estimatedWidth = text.Length * fontSize * 0.6;
                double estimatedHeight = fontSize * 1.2;

                return new Size(
                    Math.Min(estimatedWidth, availableSize.Width),
                    Math.Min(estimatedHeight, availableSize.Height));
            }

            // 如果内容是UIElement，测量它
            if (Content is UIElement element)
            {
                element.Measure(availableSize);
                return element.DesiredSize;
            }

            // 其他类型的内容，使用默认尺寸
            return new Size(80, 24);
        }

        #endregion

        #region 可视树

        /// <summary>
        /// 获取可视子元素（如果Content是UIElement）
        /// </summary>
        public override System.Collections.Generic.IEnumerable<UIElement> GetVisualChildren()
        {
            if (Content is UIElement element)
            {
                yield return element;
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 点击事件
        /// </summary>
        public event EventHandler? Click;

        /// <summary>
        /// 触发点击事件
        /// </summary>
        protected virtual void OnClick()
        {
            Click?.Invoke(this, EventArgs.Empty);

            // 执行命令
            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }
        }

        #endregion

        #region 属性更改回调

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                button.InvalidateMeasure();
            }
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button)
            {
            }
        }

        private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                button.InvalidateMeasure();
            }
        }

        /// <summary>
        /// 背景颜色属性更改回调
        /// </summary>
        /// <param name="d">依赖对象</param>
        /// <param name="e">属性更改事件参数</param>
        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button)
            {
                // 背景颜色更改时需要重新渲染，但不需要重新测量
            }
        }

        /// <summary>
        /// 前景颜色属性更改回调
        /// </summary>
        /// <param name="d">依赖对象</param>
        /// <param name="e">属性更改事件参数</param>
        private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button)
            {
                // 前景颜色更改时需要重新渲染，但不需要重新测量
            }
        }

        /// <summary>
        /// 边框颜色属性更改回调
        /// </summary>
        /// <param name="d">依赖对象</param>
        /// <param name="e">属性更改事件参数</param>
        private static void OnBorderBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button)
            {
                // 边框颜色更改时需要重新渲染，但不需要重新测量
            }
        }

        /// <summary>
        /// 圆角弧度属性更改回调
        /// </summary>
        /// <param name="d">依赖对象</param>
        /// <param name="e">属性更改事件参数</param>
        private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button)
            {
                // 圆角弧度更改时需要重新渲染，但不需要重新测量
            }
        }

        #endregion

        #region 重写方法

        /// <summary>
        /// 返回按钮的字符串表示
        /// </summary>
        /// <returns>按钮的字符串表示</returns>
        public override string ToString()
        {
            return $"Button: {Content}";
        }

        #endregion
    }
}
