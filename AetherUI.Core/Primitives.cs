using System;
using System.Diagnostics;

namespace AetherUI.Core
{
    /// <summary>
    /// 尺寸结构，表示宽度和高度
    /// </summary>
    public struct Size
    {
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// 高度
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// 初始化尺寸
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public Size(double width, double height)
        {
            Width = Math.Max(0, width);
            Height = Math.Max(0, height);
        }

        /// <summary>
        /// 空尺寸
        /// </summary>
        public static Size Empty => new Size(0, 0);

        /// <summary>
        /// 无限尺寸
        /// </summary>
        public static Size Infinity => new Size(double.PositiveInfinity, double.PositiveInfinity);

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Width == 0 && Height == 0;

        /// <summary>
        /// 是否包含无限值
        /// </summary>
        public bool IsInfinity => double.IsInfinity(Width) || double.IsInfinity(Height);

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }

        public override bool Equals(object? obj)
        {
            return obj is Size size && Width == size.Width && Height == size.Height;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Width, Height);
        }

        public static bool operator ==(Size left, Size right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Size left, Size right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// 矩形结构，表示位置和尺寸
    /// </summary>
    public struct Rect
    {
        /// <summary>
        /// X坐标
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Y坐标
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// 高度
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// 初始化矩形
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public Rect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = Math.Max(0, width);
            Height = Math.Max(0, height);
        }

        /// <summary>
        /// 初始化矩形
        /// </summary>
        /// <param name="location">位置</param>
        /// <param name="size">尺寸</param>
        public Rect(Point location, Size size)
            : this(location.X, location.Y, size.Width, size.Height)
        {
        }

        /// <summary>
        /// 左边界
        /// </summary>
        public double Left => X;

        /// <summary>
        /// 上边界
        /// </summary>
        public double Top => Y;

        /// <summary>
        /// 右边界
        /// </summary>
        public double Right => X + Width;

        /// <summary>
        /// 下边界
        /// </summary>
        public double Bottom => Y + Height;

        /// <summary>
        /// 位置
        /// </summary>
        public Point Location => new Point(X, Y);

        /// <summary>
        /// 尺寸
        /// </summary>
        public Size Size => new Size(Width, Height);

        /// <summary>
        /// 空矩形
        /// </summary>
        public static Rect Empty => new Rect(0, 0, 0, 0);

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Width == 0 || Height == 0;

        /// <summary>
        /// 是否包含指定点
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>如果包含则返回true</returns>
        public bool Contains(Point point)
        {
            return point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;
        }

        /// <summary>
        /// 是否与另一个矩形相交
        /// </summary>
        /// <param name="rect">另一个矩形</param>
        /// <returns>如果相交则返回true</returns>
        public bool IntersectsWith(Rect rect)
        {
            return Left < rect.Right && Right > rect.Left && Top < rect.Bottom && Bottom > rect.Top;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Width}, {Height})";
        }

        public override bool Equals(object? obj)
        {
            return obj is Rect rect && X == rect.X && Y == rect.Y && Width == rect.Width && Height == rect.Height;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rect left, Rect right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// 点结构，表示二维坐标
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// X坐标
        /// </summary>
        public double X { get; }

        /// <summary>
        /// Y坐标
        /// </summary>
        public double Y { get; }

        /// <summary>
        /// 初始化点
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// 零点
        /// </summary>
        public static Point Zero => new Point(0, 0);

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public override bool Equals(object? obj)
        {
            return obj is Point point && X == point.X && Y == point.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }
    }

    /// <summary>
    /// 厚度结构，表示四边的厚度
    /// </summary>
    public struct Thickness
    {
        /// <summary>
        /// 左边厚度
        /// </summary>
        public double Left { get; }

        /// <summary>
        /// 上边厚度
        /// </summary>
        public double Top { get; }

        /// <summary>
        /// 右边厚度
        /// </summary>
        public double Right { get; }

        /// <summary>
        /// 下边厚度
        /// </summary>
        public double Bottom { get; }

        /// <summary>
        /// 初始化厚度
        /// </summary>
        /// <param name="uniformLength">统一厚度</param>
        public Thickness(double uniformLength)
            : this(uniformLength, uniformLength, uniformLength, uniformLength)
        {
        }

        /// <summary>
        /// 初始化厚度
        /// </summary>
        /// <param name="left">左边厚度</param>
        /// <param name="top">上边厚度</param>
        /// <param name="right">右边厚度</param>
        /// <param name="bottom">下边厚度</param>
        public Thickness(double left, double top, double right, double bottom)
        {
            Left = Math.Max(0, left);
            Top = Math.Max(0, top);
            Right = Math.Max(0, right);
            Bottom = Math.Max(0, bottom);
        }

        /// <summary>
        /// 零厚度
        /// </summary>
        public static Thickness Zero => new Thickness(0);

        /// <summary>
        /// 水平厚度总和
        /// </summary>
        public double Horizontal => Left + Right;

        /// <summary>
        /// 垂直厚度总和
        /// </summary>
        public double Vertical => Top + Bottom;

        public override string ToString()
        {
            return $"({Left}, {Top}, {Right}, {Bottom})";
        }

        public override bool Equals(object? obj)
        {
            return obj is Thickness thickness && Left == thickness.Left && Top == thickness.Top && Right == thickness.Right && Bottom == thickness.Bottom;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }

        public static bool operator ==(Thickness left, Thickness right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Thickness left, Thickness right)
        {
            return !(left == right);
        }
    }
}
