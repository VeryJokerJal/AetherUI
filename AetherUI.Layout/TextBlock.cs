using System;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 文本块控件，用于显示文本
    /// </summary>
    public class TextBlock : FrameworkElement
    {
        #region 依赖属性

        /// <summary>
        /// 文本依赖属性
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(TextBlock),
            new PropertyMetadata(string.Empty, OnTextChanged));

        /// <summary>
        /// 字体大小依赖属性
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            nameof(FontSize), typeof(double), typeof(TextBlock),
            new PropertyMetadata(12.0, OnFontChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text
        {
            get => (string)(GetValue(TextProperty) ?? string.Empty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// 字体大小
        /// </summary>
        public double FontSize
        {
            get => (double)(GetValue(FontSizeProperty) ?? 12.0);
            set => SetValue(FontSizeProperty, value);
        }

        #endregion

        #region 布局重写

        /// <summary>
        /// 测量文本尺寸
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // 简单的文本尺寸计算（实际实现中应该使用真实的文本测量）
            string text = Text ?? string.Empty;
            double fontSize = FontSize;

            // 估算文本尺寸：每个字符约为字体大小的0.6倍宽度
            double estimatedWidth = text.Length * fontSize * 0.6;
            double estimatedHeight = fontSize * 1.2; // 行高约为字体大小的1.2倍

            // 应用可用尺寸约束
            double width = Math.Min(estimatedWidth, availableSize.Width);
            double height = Math.Min(estimatedHeight, availableSize.Height);

            Size measuredSize = new Size(width, height);
            Debug.WriteLine($"TextBlock '{text}' measured size: {measuredSize}");

            return measuredSize;
        }

        /// <summary>
        /// 排列文本
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine($"TextBlock '{Text}' arranged to size: {finalSize}");
            return finalSize;
        }

        #endregion

        #region 属性更改回调

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                textBlock.InvalidateMeasure();
                Debug.WriteLine($"TextBlock text changed to: '{e.NewValue}'");
            }
        }

        private static void OnFontChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                textBlock.InvalidateMeasure();
                Debug.WriteLine($"TextBlock font size changed to: {e.NewValue}");
            }
        }

        #endregion

        #region 重写方法

        /// <summary>
        /// 返回文本块的字符串表示
        /// </summary>
        /// <returns>文本块的字符串表示</returns>
        public override string ToString()
        {
            return $"TextBlock: '{Text}'";
        }

        #endregion
    }
}
