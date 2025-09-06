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
            new PropertyMetadata(14.0, OnFontChanged));

        /// <summary>
        /// 字体族依赖属性
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            nameof(FontFamily), typeof(string), typeof(TextBlock),
            new PropertyMetadata("Microsoft YaHei", OnFontChanged));

        /// <summary>
        /// 前景色依赖属性
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            nameof(Foreground), typeof(string), typeof(TextBlock),
            new PropertyMetadata("#000000", OnFontChanged));

        /// <summary>
        /// 字体粗细依赖属性
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            nameof(FontWeight), typeof(FontWeight), typeof(TextBlock),
            new PropertyMetadata(FontWeight.Normal, OnFontChanged));

        /// <summary>
        /// 字体样式依赖属性
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
            nameof(FontStyle), typeof(FontStyle), typeof(TextBlock),
            new PropertyMetadata(FontStyle.Normal, OnFontChanged));

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
            get => (double)(GetValue(FontSizeProperty) ?? 14.0);
            set => SetValue(FontSizeProperty, value);
        }

        /// <summary>
        /// 字体族
        /// </summary>
        public string FontFamily
        {
            get => (string)(GetValue(FontFamilyProperty) ?? "Microsoft YaHei");
            set => SetValue(FontFamilyProperty, value);
        }

        /// <summary>
        /// 前景色
        /// </summary>
        public string Foreground
        {
            get => (string)(GetValue(ForegroundProperty) ?? "#000000");
            set => SetValue(ForegroundProperty, value);
        }

        /// <summary>
        /// 字体粗细
        /// </summary>
        public FontWeight FontWeight
        {
            get => (FontWeight)(GetValue(FontWeightProperty) ?? FontWeight.Normal);
            set => SetValue(FontWeightProperty, value);
        }

        /// <summary>
        /// 字体样式
        /// </summary>
        public FontStyle FontStyle
        {
            get => (FontStyle)(GetValue(FontStyleProperty) ?? FontStyle.Normal);
            set => SetValue(FontStyleProperty, value);
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
            string text = Text ?? string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                return new Size(0, FontSize * 1.2);
            }

            try
            {
                // 创建字体信息
                var fontInfo = new FontInfo(FontFamily, FontSize, FontWeight, FontStyle, Foreground);

                // 使用简化的文本测量（避免在布局阶段依赖渲染器）
                // 这里使用估算方法，真实测量在渲染时进行
                double estimatedWidth = MeasureTextWidth(text, fontInfo);
                double estimatedHeight = fontInfo.Size * 1.2; // 行高约为字体大小的1.2倍

                // 应用可用尺寸约束
                double width = Math.Min(estimatedWidth, availableSize.Width);
                double height = Math.Min(estimatedHeight, availableSize.Height);

                Size measuredSize = new Size(width, height);
                Debug.WriteLine($"TextBlock '{text}' measured size: {measuredSize} (Font: {fontInfo.Family} {fontInfo.Size}px)");

                return measuredSize;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error measuring TextBlock '{text}': {ex.Message}");

                // 降级到简单估算
                double estimatedWidth = text.Length * FontSize * 0.6;
                double estimatedHeight = FontSize * 1.2;
                double width = Math.Min(estimatedWidth, availableSize.Width);
                double height = Math.Min(estimatedHeight, availableSize.Height);

                return new Size(width, height);
            }
        }

        /// <summary>
        /// 简化的文本宽度测量
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="fontInfo">字体信息</param>
        /// <returns>估算宽度</returns>
        private double MeasureTextWidth(string text, FontInfo fontInfo)
        {
            // 简化的文本宽度计算
            // 中文字符约为字体大小的1倍宽度，英文字符约为0.6倍
            double width = 0;
            foreach (char c in text)
            {
                if (c >= 0x4E00 && c <= 0x9FFF) // 中文字符范围
                {
                    width += fontInfo.Size;
                }
                else if (char.IsWhiteSpace(c))
                {
                    width += fontInfo.Size * 0.3;
                }
                else
                {
                    width += fontInfo.Size * 0.6;
                }
            }
            return width;
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
