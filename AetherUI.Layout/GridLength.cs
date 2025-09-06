using System;

namespace AetherUI.Layout
{
    /// <summary>
    /// 网格长度单位类型
    /// </summary>
    public enum GridUnitType
    {
        /// <summary>
        /// 自动调整大小
        /// </summary>
        Auto,

        /// <summary>
        /// 像素值
        /// </summary>
        Pixel,

        /// <summary>
        /// 星号（比例）
        /// </summary>
        Star
    }

    /// <summary>
    /// 网格长度，用于定义行高和列宽
    /// </summary>
    public struct GridLength
    {
        private readonly double _value;
        private readonly GridUnitType _unitType;

        /// <summary>
        /// 初始化网格长度
        /// </summary>
        /// <param name="value">数值</param>
        /// <param name="unitType">单位类型</param>
        public GridLength(double value, GridUnitType unitType)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
                throw new ArgumentException("Invalid value for GridLength", nameof(value));

            _value = value;
            _unitType = unitType;
        }

        /// <summary>
        /// 初始化像素网格长度
        /// </summary>
        /// <param name="pixels">像素值</param>
        public GridLength(double pixels) : this(pixels, GridUnitType.Pixel)
        {
        }

        /// <summary>
        /// 数值
        /// </summary>
        public double Value => _value;

        /// <summary>
        /// 单位类型
        /// </summary>
        public GridUnitType GridUnitType => _unitType;

        /// <summary>
        /// 是否为自动
        /// </summary>
        public bool IsAuto => _unitType == GridUnitType.Auto;

        /// <summary>
        /// 是否为像素
        /// </summary>
        public bool IsPixel => _unitType == GridUnitType.Pixel;

        /// <summary>
        /// 是否为星号
        /// </summary>
        public bool IsStar => _unitType == GridUnitType.Star;

        /// <summary>
        /// 自动网格长度
        /// </summary>
        public static GridLength Auto => new GridLength(0, GridUnitType.Auto);

        /// <summary>
        /// 创建星号网格长度
        /// </summary>
        /// <param name="value">星号值</param>
        /// <returns>星号网格长度</returns>
        public static GridLength Star(double value = 1.0)
        {
            return new GridLength(value, GridUnitType.Star);
        }

        /// <summary>
        /// 创建像素网格长度
        /// </summary>
        /// <param name="pixels">像素值</param>
        /// <returns>像素网格长度</returns>
        public static GridLength Pixel(double pixels)
        {
            return new GridLength(pixels, GridUnitType.Pixel);
        }

        public override string ToString()
        {
            return _unitType switch
            {
                GridUnitType.Auto => "Auto",
                GridUnitType.Pixel => $"{_value}px",
                GridUnitType.Star => _value == 1.0 ? "*" : $"{_value}*",
                _ => "Unknown"
            };
        }

        public override bool Equals(object? obj)
        {
            return obj is GridLength length && 
                   _value == length._value && 
                   _unitType == length._unitType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_value, _unitType);
        }

        public static bool operator ==(GridLength left, GridLength right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridLength left, GridLength right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 隐式转换从double到GridLength（像素）
        /// </summary>
        /// <param name="pixels">像素值</param>
        public static implicit operator GridLength(double pixels)
        {
            return new GridLength(pixels, GridUnitType.Pixel);
        }
    }
}
