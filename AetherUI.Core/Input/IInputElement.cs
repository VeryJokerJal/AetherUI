using System;

namespace AetherUI.Core.Input
{
    /// <summary>
    /// 输入元素接口 - 定义基础输入属性和事件
    /// </summary>
    public interface IInputElement
    {
        /// <summary>
        /// 是否可命中测试
        /// </summary>
        bool IsHitTestVisible { get; set; }

        /// <summary>
        /// 是否可获得焦点
        /// </summary>
        bool Focusable { get; set; }

        /// <summary>
        /// 是否有焦点
        /// </summary>
        bool IsFocused { get; }

        /// <summary>
        /// 鼠标进入事件
        /// </summary>
        event EventHandler<MouseEventArgs>? MouseEnter;

        /// <summary>
        /// 鼠标离开事件
        /// </summary>
        event EventHandler<MouseEventArgs>? MouseLeave;

        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        event EventHandler<MouseButtonEventArgs>? MouseDown;

        /// <summary>
        /// 鼠标抬起事件
        /// </summary>
        event EventHandler<MouseButtonEventArgs>? MouseUp;

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        event EventHandler<MouseEventArgs>? MouseMove;

        /// <summary>
        /// 键盘按下事件
        /// </summary>
        event EventHandler<KeyEventArgs>? KeyDown;

        /// <summary>
        /// 键盘抬起事件
        /// </summary>
        event EventHandler<KeyEventArgs>? KeyUp;

        /// <summary>
        /// 获得焦点事件
        /// </summary>
        event EventHandler<FocusEventArgs>? GotFocus;

        /// <summary>
        /// 失去焦点事件
        /// </summary>
        event EventHandler<FocusEventArgs>? LostFocus;
    }

    /// <summary>
    /// 鼠标事件参数基类
    /// </summary>
    public class MouseEventArgs : EventArgs
    {
        /// <summary>
        /// 鼠标位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 修饰键状态
        /// </summary>
        public ModifierKeys Modifiers { get; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// 初始化鼠标事件参数
        /// </summary>
        /// <param name="position">鼠标位置</param>
        /// <param name="modifiers">修饰键状态</param>
        /// <param name="timestamp">时间戳</param>
        public MouseEventArgs(Point position, ModifierKeys modifiers = ModifierKeys.None, uint timestamp = 0)
        {
            Position = position;
            Modifiers = modifiers;
            Timestamp = timestamp;
        }

        public override string ToString() => $"Mouse at {Position}, Modifiers: {Modifiers}";
    }

    /// <summary>
    /// 鼠标按钮事件参数
    /// </summary>
    public class MouseButtonEventArgs : MouseEventArgs
    {
        /// <summary>
        /// 按钮
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
        /// <param name="button">按钮</param>
        /// <param name="buttonState">按钮状态</param>
        /// <param name="position">鼠标位置</param>
        /// <param name="clickCount">点击次数</param>
        /// <param name="modifiers">修饰键状态</param>
        /// <param name="timestamp">时间戳</param>
        public MouseButtonEventArgs(
            MouseButton button,
            MouseButtonState buttonState,
            Point position,
            int clickCount = 1,
            ModifierKeys modifiers = ModifierKeys.None,
            uint timestamp = 0)
            : base(position, modifiers, timestamp)
        {
            Button = button;
            ButtonState = buttonState;
            ClickCount = clickCount;
        }

        public override string ToString() => 
            $"Mouse {Button} {ButtonState} at {Position}, Clicks: {ClickCount}, Modifiers: {Modifiers}";
    }

    /// <summary>
    /// 键盘事件参数
    /// </summary>
    public class KeyEventArgs : EventArgs
    {
        /// <summary>
        /// 按键
        /// </summary>
        public Key Key { get; }

        /// <summary>
        /// 修饰键状态
        /// </summary>
        public ModifierKeys Modifiers { get; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// 初始化键盘事件参数
        /// </summary>
        /// <param name="key">按键</param>
        /// <param name="modifiers">修饰键状态</param>
        /// <param name="timestamp">时间戳</param>
        public KeyEventArgs(Key key, ModifierKeys modifiers = ModifierKeys.None, uint timestamp = 0)
        {
            Key = key;
            Modifiers = modifiers;
            Timestamp = timestamp;
        }

        public override string ToString() => $"Key {Key}, Modifiers: {Modifiers}";
    }

    /// <summary>
    /// 焦点事件参数
    /// </summary>
    public class FocusEventArgs : EventArgs
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// 初始化焦点事件参数
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        public FocusEventArgs(uint timestamp = 0)
        {
            Timestamp = timestamp;
        }

        public override string ToString() => "Focus";
    }

    /// <summary>
    /// 鼠标按钮枚举
    /// </summary>
    public enum MouseButton
    {
        /// <summary>
        /// 无按钮
        /// </summary>
        None,

        /// <summary>
        /// 左键
        /// </summary>
        Left,

        /// <summary>
        /// 右键
        /// </summary>
        Right,

        /// <summary>
        /// 中键
        /// </summary>
        Middle,

        /// <summary>
        /// X1键
        /// </summary>
        XButton1,

        /// <summary>
        /// X2键
        /// </summary>
        XButton2
    }

    /// <summary>
    /// 鼠标按钮状态枚举
    /// </summary>
    public enum MouseButtonState
    {
        /// <summary>
        /// 释放
        /// </summary>
        Released,

        /// <summary>
        /// 按下
        /// </summary>
        Pressed
    }

    /// <summary>
    /// 按键枚举
    /// </summary>
    public enum Key
    {
        None = 0,
        
        // 字母键
        A = 65, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        
        // 数字键
        D0 = 48, D1, D2, D3, D4, D5, D6, D7, D8, D9,
        
        // 功能键
        F1 = 112, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
        
        // 特殊键
        Space = 32,
        Enter = 13,
        Escape = 27,
        Tab = 9,
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
        
        // 修饰键
        Shift = 16,
        Control = 17,
        Alt = 18
    }

    /// <summary>
    /// 修饰键枚举
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        /// <summary>
        /// 无修饰键
        /// </summary>
        None = 0,

        /// <summary>
        /// Alt键
        /// </summary>
        Alt = 1,

        /// <summary>
        /// Control键
        /// </summary>
        Control = 2,

        /// <summary>
        /// Shift键
        /// </summary>
        Shift = 4,

        /// <summary>
        /// Windows键
        /// </summary>
        Windows = 8
    }
}
