using System;
using System.Diagnostics;

namespace AetherUI.Events
{
    /// <summary>
    /// 鼠标按钮枚举
    /// </summary>
    public enum MouseButton
    {
        Left,
        Right,
        Middle,
        XButton1,
        XButton2
    }

    /// <summary>
    /// 鼠标按钮状态枚举
    /// </summary>
    public enum MouseButtonState
    {
        Released,
        Pressed
    }

    /// <summary>
    /// 键盘按键状态枚举
    /// </summary>
    public enum KeyState
    {
        Released,
        Pressed
    }

    /// <summary>
    /// 鼠标事件参数基类
    /// </summary>
    public class MouseEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 鼠标位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// 初始化鼠标事件参数
        /// </summary>
        /// <param name="position">鼠标位置</param>
        /// <param name="timestamp">时间戳</param>
        public MouseEventArgs(Point position, uint timestamp)
        {
            Position = position;
            Timestamp = timestamp;
        }

        /// <summary>
        /// 获取相对于指定元素的鼠标位置
        /// </summary>
        /// <param name="relativeTo">相对元素</param>
        /// <returns>相对位置</returns>
        public Point GetPosition(object relativeTo)
        {
            // 这里需要根据实际的UI元素坐标系统来实现
            // 暂时返回绝对位置
            return Position;
        }
    }

    /// <summary>
    /// 鼠标按钮事件参数
    /// </summary>
    public class MouseButtonEventArgs : MouseEventArgs
    {
        /// <summary>
        /// 鼠标按钮
        /// </summary>
        public MouseButton Button { get; }

        /// <summary>
        /// 按钮状态
        /// </summary>
        public MouseButtonState ButtonState { get; }

        /// <summary>
        /// 点击次数
        /// </summary>
        public int ClickCount { get; }

        /// <summary>
        /// 初始化鼠标按钮事件参数
        /// </summary>
        /// <param name="button">鼠标按钮</param>
        /// <param name="buttonState">按钮状态</param>
        /// <param name="position">鼠标位置</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="clickCount">点击次数</param>
        public MouseButtonEventArgs(MouseButton button, MouseButtonState buttonState, Point position, uint timestamp, int clickCount = 1)
            : base(position, timestamp)
        {
            Button = button;
            ButtonState = buttonState;
            ClickCount = clickCount;
        }
    }

    /// <summary>
    /// 鼠标滚轮事件参数
    /// </summary>
    public class MouseWheelEventArgs : MouseEventArgs
    {
        /// <summary>
        /// 滚轮增量
        /// </summary>
        public int Delta { get; }

        /// <summary>
        /// 初始化鼠标滚轮事件参数
        /// </summary>
        /// <param name="delta">滚轮增量</param>
        /// <param name="position">鼠标位置</param>
        /// <param name="timestamp">时间戳</param>
        public MouseWheelEventArgs(int delta, Point position, uint timestamp)
            : base(position, timestamp)
        {
            Delta = delta;
        }
    }

    /// <summary>
    /// 键盘事件参数基类
    /// </summary>
    public class KeyEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 按键代码
        /// </summary>
        public int Key { get; }

        /// <summary>
        /// 按键状态
        /// </summary>
        public KeyState KeyState { get; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// 是否为重复按键
        /// </summary>
        public bool IsRepeat { get; }

        /// <summary>
        /// 初始化键盘事件参数
        /// </summary>
        /// <param name="key">按键代码</param>
        /// <param name="keyState">按键状态</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="isRepeat">是否为重复按键</param>
        public KeyEventArgs(int key, KeyState keyState, uint timestamp, bool isRepeat = false)
        {
            Key = key;
            KeyState = keyState;
            Timestamp = timestamp;
            IsRepeat = isRepeat;
        }
    }

    /// <summary>
    /// 文本输入事件参数
    /// </summary>
    public class TextInputEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 输入的文本
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// 初始化文本输入事件参数
        /// </summary>
        /// <param name="text">输入的文本</param>
        /// <param name="timestamp">时间戳</param>
        public TextInputEventArgs(string text, uint timestamp)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Timestamp = timestamp;
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
    /// 输入事件的路由事件定义
    /// </summary>
    public static class InputEvents
    {
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public static readonly RoutedEvent MouseMoveEvent = RoutedEvent.Register(
            "MouseMove", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(InputEvents));

        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public static readonly RoutedEvent MouseDownEvent = RoutedEvent.Register(
            "MouseDown", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(InputEvents));

        /// <summary>
        /// 鼠标释放事件
        /// </summary>
        public static readonly RoutedEvent MouseUpEvent = RoutedEvent.Register(
            "MouseUp", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(InputEvents));

        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        public static readonly RoutedEvent MouseWheelEvent = RoutedEvent.Register(
            "MouseWheel", RoutingStrategy.Bubble, typeof(MouseWheelEventHandler), typeof(InputEvents));

        /// <summary>
        /// 按键按下事件
        /// </summary>
        public static readonly RoutedEvent KeyDownEvent = RoutedEvent.Register(
            "KeyDown", RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(InputEvents));

        /// <summary>
        /// 按键释放事件
        /// </summary>
        public static readonly RoutedEvent KeyUpEvent = RoutedEvent.Register(
            "KeyUp", RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(InputEvents));

        /// <summary>
        /// 文本输入事件
        /// </summary>
        public static readonly RoutedEvent TextInputEvent = RoutedEvent.Register(
            "TextInput", RoutingStrategy.Bubble, typeof(TextInputEventHandler), typeof(InputEvents));
    }

    /// <summary>
    /// 鼠标事件处理器委托
    /// </summary>
    public delegate void MouseEventHandler(object sender, MouseEventArgs e);

    /// <summary>
    /// 鼠标按钮事件处理器委托
    /// </summary>
    public delegate void MouseButtonEventHandler(object sender, MouseButtonEventArgs e);

    /// <summary>
    /// 鼠标滚轮事件处理器委托
    /// </summary>
    public delegate void MouseWheelEventHandler(object sender, MouseWheelEventArgs e);

    /// <summary>
    /// 键盘事件处理器委托
    /// </summary>
    public delegate void KeyEventHandler(object sender, KeyEventArgs e);

    /// <summary>
    /// 文本输入事件处理器委托
    /// </summary>
    public delegate void TextInputEventHandler(object sender, TextInputEventArgs e);
}
