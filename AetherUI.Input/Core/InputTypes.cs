using System;
using System.Numerics;

namespace AetherUI.Input.Core
{
    /// <summary>
    /// 输入设备类型
    /// </summary>
    public enum InputDeviceType
    {
        /// <summary>
        /// 鼠标
        /// </summary>
        Mouse,

        /// <summary>
        /// 触摸屏
        /// </summary>
        Touch,

        /// <summary>
        /// 手写笔
        /// </summary>
        Pen,

        /// <summary>
        /// 键盘
        /// </summary>
        Keyboard,

        /// <summary>
        /// 游戏手柄
        /// </summary>
        Gamepad
    }

    /// <summary>
    /// 指针ID，用于区分不同的指针（多点触控）
    /// </summary>
    public readonly struct PointerId : IEquatable<PointerId>
    {
        /// <summary>
        /// 指针ID值
        /// </summary>
        public uint Value { get; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public InputDeviceType DeviceType { get; }

        /// <summary>
        /// 初始化指针ID
        /// </summary>
        /// <param name="value">ID值</param>
        /// <param name="deviceType">设备类型</param>
        public PointerId(uint value, InputDeviceType deviceType)
        {
            Value = value;
            DeviceType = deviceType;
        }

        /// <summary>
        /// 鼠标指针ID
        /// </summary>
        public static PointerId Mouse => new(0, InputDeviceType.Mouse);

        /// <summary>
        /// 创建触摸指针ID
        /// </summary>
        /// <param name="touchId">触摸ID</param>
        /// <returns>触摸指针ID</returns>
        public static PointerId Touch(uint touchId) => new(touchId, InputDeviceType.Touch);

        /// <summary>
        /// 创建手写笔指针ID
        /// </summary>
        /// <param name="penId">手写笔ID</param>
        /// <returns>手写笔指针ID</returns>
        public static PointerId Pen(uint penId) => new(penId, InputDeviceType.Pen);

        public bool Equals(PointerId other) => Value == other.Value && DeviceType == other.DeviceType;
        public override bool Equals(object? obj) => obj is PointerId other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Value, DeviceType);
        public static bool operator ==(PointerId left, PointerId right) => left.Equals(right);
        public static bool operator !=(PointerId left, PointerId right) => !left.Equals(right);
        public override string ToString() => $"{DeviceType}:{Value}";
    }

    /// <summary>
    /// 指针事件类型
    /// </summary>
    public enum PointerEventType
    {
        /// <summary>
        /// 指针移动
        /// </summary>
        Moved,

        /// <summary>
        /// 指针按下
        /// </summary>
        Pressed,

        /// <summary>
        /// 指针释放
        /// </summary>
        Released,

        /// <summary>
        /// 指针进入
        /// </summary>
        Entered,

        /// <summary>
        /// 指针离开
        /// </summary>
        Exited,

        /// <summary>
        /// 指针取消
        /// </summary>
        Cancelled,

        /// <summary>
        /// 滚轮滚动
        /// </summary>
        Wheel
    }

    /// <summary>
    /// 指针按钮
    /// </summary>
    [Flags]
    public enum PointerButton
    {
        /// <summary>
        /// 无按钮
        /// </summary>
        None = 0,

        /// <summary>
        /// 左键/主按钮
        /// </summary>
        Primary = 1 << 0,

        /// <summary>
        /// 右键/次按钮
        /// </summary>
        Secondary = 1 << 1,

        /// <summary>
        /// 中键/滚轮按钮
        /// </summary>
        Middle = 1 << 2,

        /// <summary>
        /// X1按钮
        /// </summary>
        X1 = 1 << 3,

        /// <summary>
        /// X2按钮
        /// </summary>
        X2 = 1 << 4
    }

    /// <summary>
    /// 修饰键
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        /// <summary>
        /// 无修饰键
        /// </summary>
        None = 0,

        /// <summary>
        /// Ctrl键
        /// </summary>
        Control = 1 << 0,

        /// <summary>
        /// Shift键
        /// </summary>
        Shift = 1 << 1,

        /// <summary>
        /// Alt键
        /// </summary>
        Alt = 1 << 2,

        /// <summary>
        /// Windows键/Cmd键
        /// </summary>
        Meta = 1 << 3
    }

    /// <summary>
    /// 键盘事件类型
    /// </summary>
    public enum KeyEventType
    {
        /// <summary>
        /// 按键按下
        /// </summary>
        Down,

        /// <summary>
        /// 按键释放
        /// </summary>
        Up,

        /// <summary>
        /// 字符输入
        /// </summary>
        Char
    }

    /// <summary>
    /// 虚拟键码
    /// </summary>
    public enum VirtualKey
    {
        None = 0,
        
        // 字母键
        A = 65, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        
        // 数字键
        D0 = 48, D1, D2, D3, D4, D5, D6, D7, D8, D9,
        
        // 功能键
        F1 = 112, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
        
        // 控制键
        Escape = 27,
        Tab = 9,
        CapsLock = 20,
        Shift = 16,
        Control = 17,
        Alt = 18,
        Space = 32,
        Enter = 13,
        Backspace = 8,
        Delete = 46,
        Insert = 45,
        Home = 36,
        End = 35,
        PageUp = 33,
        PageDown = 34,
        
        // 方向键
        Left = 37,
        Up = 38,
        Right = 39,
        Down = 40,
        
        // 数字键盘
        NumPad0 = 96, NumPad1, NumPad2, NumPad3, NumPad4, NumPad5, NumPad6, NumPad7, NumPad8, NumPad9,
        NumPadMultiply = 106,
        NumPadAdd = 107,
        NumPadSubtract = 109,
        NumPadDecimal = 110,
        NumPadDivide = 111
    }

    /// <summary>
    /// 输入设备信息
    /// </summary>
    public readonly struct InputDevice
    {
        /// <summary>
        /// 设备类型
        /// </summary>
        public InputDeviceType DeviceType { get; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public uint DeviceId { get; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 初始化输入设备
        /// </summary>
        /// <param name="deviceType">设备类型</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="name">设备名称</param>
        public InputDevice(InputDeviceType deviceType, uint deviceId, string name)
        {
            DeviceType = deviceType;
            DeviceId = deviceId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => $"{DeviceType} ({Name})";
    }

    /// <summary>
    /// 二维点结构
    /// </summary>
    public readonly struct Point : IEquatable<Point>
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
        public static Point Zero => new(0, 0);

        /// <summary>
        /// 转换为Vector2
        /// </summary>
        /// <returns>Vector2</returns>
        public Vector2 ToVector2() => new((float)X, (float)Y);

        /// <summary>
        /// 从Vector2创建点
        /// </summary>
        /// <param name="vector">Vector2</param>
        /// <returns>点</returns>
        public static Point FromVector2(Vector2 vector) => new(vector.X, vector.Y);

        public bool Equals(Point other) => X.Equals(other.X) && Y.Equals(other.Y);
        public override bool Equals(object? obj) => obj is Point other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public static bool operator ==(Point left, Point right) => left.Equals(right);
        public static bool operator !=(Point left, Point right) => !left.Equals(right);
        public override string ToString() => $"({X}, {Y})";

        public static Point operator +(Point left, Point right) => new(left.X + right.X, left.Y + right.Y);
        public static Point operator -(Point left, Point right) => new(left.X - right.X, left.Y - right.Y);
        public static Point operator *(Point point, double scalar) => new(point.X * scalar, point.Y * scalar);
        public static Point operator /(Point point, double scalar) => new(point.X / scalar, point.Y / scalar);
    }

    /// <summary>
    /// 矩形结构
    /// </summary>
    public readonly struct Rect : IEquatable<Rect>
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
            Width = width;
            Height = height;
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
        /// 中心点
        /// </summary>
        public Point Center => new(X + Width / 2, Y + Height / 2);

        /// <summary>
        /// 空矩形
        /// </summary>
        public static Rect Empty => new(0, 0, 0, 0);

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => Width <= 0 || Height <= 0;

        /// <summary>
        /// 检查点是否在矩形内
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>是否包含</returns>
        public bool Contains(Point point) => 
            point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;

        /// <summary>
        /// 检查矩形是否相交
        /// </summary>
        /// <param name="other">另一个矩形</param>
        /// <returns>是否相交</returns>
        public bool IntersectsWith(Rect other) =>
            Left <= other.Right && Right >= other.Left && Top <= other.Bottom && Bottom >= other.Top;

        public bool Equals(Rect other) => X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height);
        public override bool Equals(object? obj) => obj is Rect other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
        public static bool operator ==(Rect left, Rect right) => left.Equals(right);
        public static bool operator !=(Rect left, Rect right) => !left.Equals(right);
        public override string ToString() => $"({X}, {Y}, {Width}, {Height})";
    }
}
