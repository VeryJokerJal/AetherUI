using System;

namespace AetherUI.Core
{
    /// <summary>
    /// 字体粗细枚举
    /// </summary>
    public enum FontWeight
    {
        /// <summary>
        /// 细体
        /// </summary>
        Thin = 100,

        /// <summary>
        /// 超细体
        /// </summary>
        ExtraLight = 200,

        /// <summary>
        /// 细体
        /// </summary>
        Light = 300,

        /// <summary>
        /// 正常
        /// </summary>
        Normal = 400,

        /// <summary>
        /// 中等
        /// </summary>
        Medium = 500,

        /// <summary>
        /// 半粗体
        /// </summary>
        SemiBold = 600,

        /// <summary>
        /// 粗体
        /// </summary>
        Bold = 700,

        /// <summary>
        /// 超粗体
        /// </summary>
        ExtraBold = 800,

        /// <summary>
        /// 黑体
        /// </summary>
        Black = 900
    }

    /// <summary>
    /// 字体样式枚举
    /// </summary>
    public enum FontStyle
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal,

        /// <summary>
        /// 斜体
        /// </summary>
        Italic,

        /// <summary>
        /// 倾斜
        /// </summary>
        Oblique
    }

    /// <summary>
    /// 字体信息类
    /// </summary>
    public class FontInfo
    {
        /// <summary>
        /// 字体族名称
        /// </summary>
        public string Family { get; set; } = "Microsoft YaHei";

        /// <summary>
        /// 字体大小
        /// </summary>
        public double Size { get; set; } = 14.0;

        /// <summary>
        /// 字体粗细
        /// </summary>
        public FontWeight Weight { get; set; } = FontWeight.Normal;

        /// <summary>
        /// 字体样式
        /// </summary>
        public FontStyle Style { get; set; } = FontStyle.Normal;

        /// <summary>
        /// 前景色
        /// </summary>
        public string Color { get; set; } = "#000000";

        /// <summary>
        /// 构造函数
        /// </summary>
        public FontInfo()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="family">字体族</param>
        /// <param name="size">字体大小</param>
        /// <param name="weight">字体粗细</param>
        /// <param name="style">字体样式</param>
        /// <param name="color">前景色</param>
        public FontInfo(string family, double size, FontWeight weight = FontWeight.Normal, 
                       FontStyle style = FontStyle.Normal, string color = "#000000")
        {
            Family = family;
            Size = size;
            Weight = weight;
            Style = style;
            Color = color;
        }

        /// <summary>
        /// 创建默认字体信息
        /// </summary>
        /// <returns>默认字体信息</returns>
        public static FontInfo Default => new FontInfo("Microsoft YaHei", 14.0);

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns>字体信息的字符串表示</returns>
        public override string ToString()
        {
            return $"{Family} {Size}px {Weight} {Style} {Color}";
        }

        /// <summary>
        /// 重写Equals方法
        /// </summary>
        /// <param name="obj">比较对象</param>
        /// <returns>是否相等</returns>
        public override bool Equals(object? obj)
        {
            if (obj is FontInfo other)
            {
                return Family == other.Family &&
                       Math.Abs(Size - other.Size) < 0.001 &&
                       Weight == other.Weight &&
                       Style == other.Style &&
                       Color == other.Color;
            }
            return false;
        }

        /// <summary>
        /// 重写GetHashCode方法
        /// </summary>
        /// <returns>哈希码</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Family, Size, Weight, Style, Color);
        }
    }

    /// <summary>
    /// 文本测量结果
    /// </summary>
    public struct TextMetrics
    {
        /// <summary>
        /// 文本宽度
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 文本高度
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 基线位置
        /// </summary>
        public double Baseline { get; set; }

        /// <summary>
        /// 行高
        /// </summary>
        public double LineHeight { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="baseline">基线</param>
        /// <param name="lineHeight">行高</param>
        public TextMetrics(double width, double height, double baseline, double lineHeight)
        {
            Width = width;
            Height = height;
            Baseline = baseline;
            LineHeight = lineHeight;
        }

        /// <summary>
        /// 转换为Size
        /// </summary>
        /// <returns>Size对象</returns>
        public Size ToSize()
        {
            return new Size(Width, Height);
        }

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString()
        {
            return $"TextMetrics(W:{Width:F1}, H:{Height:F1}, B:{Baseline:F1}, LH:{LineHeight:F1})";
        }
    }
}
