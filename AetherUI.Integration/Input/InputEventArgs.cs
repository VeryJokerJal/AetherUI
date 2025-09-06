using System;
using AetherUI.Core;

namespace AetherUI.Integration.Input
{
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
        Alt = 18,
        
        // 数字键盘
        NumPad0 = 96, NumPad1, NumPad2, NumPad3, NumPad4, NumPad5, NumPad6, NumPad7, NumPad8, NumPad9,
        NumPadMultiply = 106,
        NumPadAdd = 107,
        NumPadSubtract = 109,
        NumPadDecimal = 110,
        NumPadDivide = 111
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

    /// <summary>
    /// 输入事件转换器 - 将AetherUI.Input事件转换为布局系统事件
    /// </summary>
    public static class InputEventConverter
    {
        /// <summary>
        /// 转换指针事件为鼠标事件参数
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>鼠标事件参数</returns>
        public static MouseEventArgs ConvertToMouseEventArgs(AetherUI.Input.Events.PointerEvent pointerEvent)
        {
            var position = new Point(pointerEvent.Position.X, pointerEvent.Position.Y);
            var modifiers = ConvertModifierKeys(ModifierKeys.None); // 需要从指针事件中获取修饰键
            return new MouseEventArgs(position, modifiers, pointerEvent.Timestamp);
        }

        /// <summary>
        /// 转换指针事件为鼠标按钮事件参数
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>鼠标按钮事件参数</returns>
        public static MouseButtonEventArgs ConvertToMouseButtonEventArgs(AetherUI.Input.Events.PointerEvent pointerEvent)
        {
            var button = ConvertPointerButton(pointerEvent.ChangedButton);
            var buttonState = pointerEvent.EventType == AetherUI.Input.Events.PointerEventType.Pressed 
                ? MouseButtonState.Pressed 
                : MouseButtonState.Released;
            var position = new Point(pointerEvent.Position.X, pointerEvent.Position.Y);
            var modifiers = ConvertModifierKeys(ModifierKeys.None);
            
            return new MouseButtonEventArgs(button, buttonState, position, 1, modifiers, pointerEvent.Timestamp);
        }

        /// <summary>
        /// 转换键盘事件为键盘事件参数
        /// </summary>
        /// <param name="keyboardEvent">键盘事件</param>
        /// <returns>键盘事件参数</returns>
        public static KeyEventArgs ConvertToKeyEventArgs(AetherUI.Input.Events.KeyboardEvent keyboardEvent)
        {
            var key = ConvertKey(keyboardEvent.Key);
            var modifiers = ConvertModifierKeys(keyboardEvent.Modifiers);
            return new KeyEventArgs(key, modifiers, keyboardEvent.Timestamp);
        }

        /// <summary>
        /// 转换指针按钮
        /// </summary>
        /// <param name="pointerButton">指针按钮</param>
        /// <returns>鼠标按钮</returns>
        private static MouseButton ConvertPointerButton(AetherUI.Input.Core.PointerButton pointerButton)
        {
            return pointerButton switch
            {
                AetherUI.Input.Core.PointerButton.Primary => MouseButton.Left,
                AetherUI.Input.Core.PointerButton.Secondary => MouseButton.Right,
                AetherUI.Input.Core.PointerButton.Middle => MouseButton.Middle,
                AetherUI.Input.Core.PointerButton.X1 => MouseButton.XButton1,
                AetherUI.Input.Core.PointerButton.X2 => MouseButton.XButton2,
                _ => MouseButton.None
            };
        }

        /// <summary>
        /// 转换按键
        /// </summary>
        /// <param name="inputKey">输入系统按键</param>
        /// <returns>布局系统按键</returns>
        private static Key ConvertKey(AetherUI.Input.Core.Key inputKey)
        {
            return inputKey switch
            {
                AetherUI.Input.Core.Key.A => Key.A,
                AetherUI.Input.Core.Key.B => Key.B,
                AetherUI.Input.Core.Key.C => Key.C,
                AetherUI.Input.Core.Key.D => Key.D,
                AetherUI.Input.Core.Key.E => Key.E,
                AetherUI.Input.Core.Key.F => Key.F,
                AetherUI.Input.Core.Key.G => Key.G,
                AetherUI.Input.Core.Key.H => Key.H,
                AetherUI.Input.Core.Key.I => Key.I,
                AetherUI.Input.Core.Key.J => Key.J,
                AetherUI.Input.Core.Key.K => Key.K,
                AetherUI.Input.Core.Key.L => Key.L,
                AetherUI.Input.Core.Key.M => Key.M,
                AetherUI.Input.Core.Key.N => Key.N,
                AetherUI.Input.Core.Key.O => Key.O,
                AetherUI.Input.Core.Key.P => Key.P,
                AetherUI.Input.Core.Key.Q => Key.Q,
                AetherUI.Input.Core.Key.R => Key.R,
                AetherUI.Input.Core.Key.S => Key.S,
                AetherUI.Input.Core.Key.T => Key.T,
                AetherUI.Input.Core.Key.U => Key.U,
                AetherUI.Input.Core.Key.V => Key.V,
                AetherUI.Input.Core.Key.W => Key.W,
                AetherUI.Input.Core.Key.X => Key.X,
                AetherUI.Input.Core.Key.Y => Key.Y,
                AetherUI.Input.Core.Key.Z => Key.Z,
                AetherUI.Input.Core.Key.D0 => Key.D0,
                AetherUI.Input.Core.Key.D1 => Key.D1,
                AetherUI.Input.Core.Key.D2 => Key.D2,
                AetherUI.Input.Core.Key.D3 => Key.D3,
                AetherUI.Input.Core.Key.D4 => Key.D4,
                AetherUI.Input.Core.Key.D5 => Key.D5,
                AetherUI.Input.Core.Key.D6 => Key.D6,
                AetherUI.Input.Core.Key.D7 => Key.D7,
                AetherUI.Input.Core.Key.D8 => Key.D8,
                AetherUI.Input.Core.Key.D9 => Key.D9,
                AetherUI.Input.Core.Key.Space => Key.Space,
                AetherUI.Input.Core.Key.Enter => Key.Enter,
                AetherUI.Input.Core.Key.Escape => Key.Escape,
                AetherUI.Input.Core.Key.Tab => Key.Tab,
                AetherUI.Input.Core.Key.Backspace => Key.Backspace,
                AetherUI.Input.Core.Key.Delete => Key.Delete,
                AetherUI.Input.Core.Key.Insert => Key.Insert,
                AetherUI.Input.Core.Key.Home => Key.Home,
                AetherUI.Input.Core.Key.End => Key.End,
                AetherUI.Input.Core.Key.PageUp => Key.PageUp,
                AetherUI.Input.Core.Key.PageDown => Key.PageDown,
                AetherUI.Input.Core.Key.Left => Key.Left,
                AetherUI.Input.Core.Key.Up => Key.Up,
                AetherUI.Input.Core.Key.Right => Key.Right,
                AetherUI.Input.Core.Key.Down => Key.Down,
                AetherUI.Input.Core.Key.Shift => Key.Shift,
                AetherUI.Input.Core.Key.Control => Key.Control,
                AetherUI.Input.Core.Key.Alt => Key.Alt,
                _ => Key.None
            };
        }

        /// <summary>
        /// 转换修饰键
        /// </summary>
        /// <param name="inputModifiers">输入系统修饰键</param>
        /// <returns>布局系统修饰键</returns>
        private static ModifierKeys ConvertModifierKeys(AetherUI.Input.Core.ModifierKeys inputModifiers)
        {
            var modifiers = ModifierKeys.None;

            if (inputModifiers.HasFlag(AetherUI.Input.Core.ModifierKeys.Alt))
                modifiers |= ModifierKeys.Alt;
            if (inputModifiers.HasFlag(AetherUI.Input.Core.ModifierKeys.Control))
                modifiers |= ModifierKeys.Control;
            if (inputModifiers.HasFlag(AetherUI.Input.Core.ModifierKeys.Shift))
                modifiers |= ModifierKeys.Shift;

            return modifiers;
        }
    }
}
