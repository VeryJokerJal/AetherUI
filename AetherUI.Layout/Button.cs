using System;
using System.Diagnostics;
using System.Windows.Input;
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
            nameof(Command), typeof(ICommand), typeof(Button),
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
            new PropertyMetadata(new Thickness(8, 4, 8, 4), OnPaddingChanged));

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
        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
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
            get => (Thickness)(GetValue(PaddingProperty) ?? new Thickness(8, 4, 8, 4));
            set => SetValue(PaddingProperty, value);
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
            Size contentAvailableSize = new Size(
                Math.Max(0, availableSize.Width - padding.Horizontal),
                Math.Max(0, availableSize.Height - padding.Vertical));

            Size contentSize = MeasureContent(contentAvailableSize);

            // 添加内边距
            Size totalSize = new Size(
                contentSize.Width + padding.Horizontal,
                contentSize.Height + padding.Vertical);

            Debug.WriteLine($"Button measured size: {totalSize}");
            return totalSize;
        }

        /// <summary>
        /// 排列按钮
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine($"Button arranged to size: {finalSize}");
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
                return Size.Empty;

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
            Debug.WriteLine($"Button clicked: {Content}");
            
            // 触发点击事件
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
                Debug.WriteLine($"Button content changed to: {e.NewValue}");
            }
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                Debug.WriteLine($"Button command changed to: {e.NewValue}");
            }
        }

        private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                button.InvalidateMeasure();
                Debug.WriteLine($"Button padding changed to: {e.NewValue}");
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
