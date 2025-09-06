using System;
using AetherUI.Input.Core;

namespace AetherUI.Input.Events
{
    /// <summary>
    /// 输入事件基类
    /// </summary>
    public abstract class InputEvent
    {
        /// <summary>
        /// 事件时间戳（毫秒）
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// 输入设备
        /// </summary>
        public InputDevice Device { get; }

        /// <summary>
        /// 修饰键状态
        /// </summary>
        public ModifierKeys Modifiers { get; }

        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool IsHandled { get; set; }

        /// <summary>
        /// 事件源元素
        /// </summary>
        public object? Source { get; set; }

        /// <summary>
        /// 原始事件源元素
        /// </summary>
        public object? OriginalSource { get; set; }

        /// <summary>
        /// 初始化输入事件
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="device">输入设备</param>
        /// <param name="modifiers">修饰键</param>
        protected InputEvent(uint timestamp, InputDevice device, ModifierKeys modifiers = ModifierKeys.None)
        {
            Timestamp = timestamp;
            Device = device;
            Modifiers = modifiers;
        }
    }

    /// <summary>
    /// 指针事件
    /// </summary>
    public class PointerEvent : InputEvent
    {
        /// <summary>
        /// 指针ID
        /// </summary>
        public PointerId PointerId { get; }

        /// <summary>
        /// 指针位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public PointerEventType EventType { get; }

        /// <summary>
        /// 按钮状态
        /// </summary>
        public PointerButton Button { get; }

        /// <summary>
        /// 按钮变化（按下或释放的按钮）
        /// </summary>
        public PointerButton ChangedButton { get; }

        /// <summary>
        /// 压力值（0.0-1.0）
        /// </summary>
        public float Pressure { get; }

        /// <summary>
        /// 倾斜角度X（手写笔）
        /// </summary>
        public float TiltX { get; }

        /// <summary>
        /// 倾斜角度Y（手写笔）
        /// </summary>
        public float TiltY { get; }

        /// <summary>
        /// 滚轮增量（仅用于滚轮事件）
        /// </summary>
        public Point WheelDelta { get; }

        /// <summary>
        /// 初始化指针事件
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="device">输入设备</param>
        /// <param name="pointerId">指针ID</param>
        /// <param name="position">指针位置</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="button">按钮状态</param>
        /// <param name="changedButton">变化的按钮</param>
        /// <param name="modifiers">修饰键</param>
        /// <param name="pressure">压力值</param>
        /// <param name="tiltX">倾斜角度X</param>
        /// <param name="tiltY">倾斜角度Y</param>
        /// <param name="wheelDelta">滚轮增量</param>
        public PointerEvent(
            uint timestamp,
            InputDevice device,
            PointerId pointerId,
            Point position,
            PointerEventType eventType,
            PointerButton button = PointerButton.None,
            PointerButton changedButton = PointerButton.None,
            ModifierKeys modifiers = ModifierKeys.None,
            float pressure = 1.0f,
            float tiltX = 0.0f,
            float tiltY = 0.0f,
            Point wheelDelta = default)
            : base(timestamp, device, modifiers)
        {
            PointerId = pointerId;
            Position = position;
            EventType = eventType;
            Button = button;
            ChangedButton = changedButton;
            Pressure = pressure;
            TiltX = tiltX;
            TiltY = tiltY;
            WheelDelta = wheelDelta;
        }

        /// <summary>
        /// 获取相对于指定元素的位置
        /// </summary>
        /// <param name="relativeTo">相对元素</param>
        /// <returns>相对位置</returns>
        public Point GetPosition(object? relativeTo)
        {
            // TODO: 实现坐标变换
            return Position;
        }

        public override string ToString() =>
            $"PointerEvent: {EventType} {PointerId} at {Position} Button={Button} Modifiers={Modifiers}";
    }

    /// <summary>
    /// 键盘事件
    /// </summary>
    public class KeyboardEvent : InputEvent
    {
        /// <summary>
        /// 虚拟键码
        /// </summary>
        public VirtualKey Key { get; }

        /// <summary>
        /// 扫描码
        /// </summary>
        public uint ScanCode { get; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public KeyEventType EventType { get; }

        /// <summary>
        /// 是否为重复按键
        /// </summary>
        public bool IsRepeat { get; }

        /// <summary>
        /// 字符（仅用于字符输入事件）
        /// </summary>
        public char Character { get; }

        /// <summary>
        /// 初始化键盘事件
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="device">输入设备</param>
        /// <param name="key">虚拟键码</param>
        /// <param name="scanCode">扫描码</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="modifiers">修饰键</param>
        /// <param name="isRepeat">是否为重复按键</param>
        /// <param name="character">字符</param>
        public KeyboardEvent(
            uint timestamp,
            InputDevice device,
            VirtualKey key,
            uint scanCode,
            KeyEventType eventType,
            ModifierKeys modifiers = ModifierKeys.None,
            bool isRepeat = false,
            char character = '\0')
            : base(timestamp, device, modifiers)
        {
            Key = key;
            ScanCode = scanCode;
            EventType = eventType;
            IsRepeat = isRepeat;
            Character = character;
        }

        public override string ToString() =>
            $"KeyboardEvent: {EventType} {Key} ScanCode={ScanCode} Modifiers={Modifiers} Repeat={IsRepeat}";
    }

    /// <summary>
    /// 文本输入事件
    /// </summary>
    public class TextInputEvent : InputEvent
    {
        /// <summary>
        /// 输入的文本
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 是否为组合输入（IME）
        /// </summary>
        public bool IsComposition { get; }

        /// <summary>
        /// 组合文本的选择范围开始位置
        /// </summary>
        public int CompositionStart { get; }

        /// <summary>
        /// 组合文本的选择范围长度
        /// </summary>
        public int CompositionLength { get; }

        /// <summary>
        /// 初始化文本输入事件
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="device">输入设备</param>
        /// <param name="text">输入的文本</param>
        /// <param name="isComposition">是否为组合输入</param>
        /// <param name="compositionStart">组合文本选择开始位置</param>
        /// <param name="compositionLength">组合文本选择长度</param>
        public TextInputEvent(
            uint timestamp,
            InputDevice device,
            string text,
            bool isComposition = false,
            int compositionStart = 0,
            int compositionLength = 0)
            : base(timestamp, device)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IsComposition = isComposition;
            CompositionStart = compositionStart;
            CompositionLength = compositionLength;
        }

        public override string ToString() =>
            $"TextInputEvent: '{Text}' Composition={IsComposition}";
    }

    /// <summary>
    /// 手势事件
    /// </summary>
    public class GestureEvent : InputEvent
    {
        /// <summary>
        /// 手势类型
        /// </summary>
        public string GestureType { get; }

        /// <summary>
        /// 手势状态
        /// </summary>
        public GestureState State { get; }

        /// <summary>
        /// 手势位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 手势数据
        /// </summary>
        public object? Data { get; }

        /// <summary>
        /// 初始化手势事件
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="device">输入设备</param>
        /// <param name="gestureType">手势类型</param>
        /// <param name="state">手势状态</param>
        /// <param name="position">手势位置</param>
        /// <param name="data">手势数据</param>
        public GestureEvent(
            uint timestamp,
            InputDevice device,
            string gestureType,
            GestureState state,
            Point position,
            object? data = null)
            : base(timestamp, device)
        {
            GestureType = gestureType ?? throw new ArgumentNullException(nameof(gestureType));
            State = state;
            Position = position;
            Data = data;
        }

        public override string ToString() =>
            $"GestureEvent: {GestureType} {State} at {Position}";
    }

    /// <summary>
    /// 手势状态
    /// </summary>
    public enum GestureState
    {
        /// <summary>
        /// 开始
        /// </summary>
        Started,

        /// <summary>
        /// 更新
        /// </summary>
        Updated,

        /// <summary>
        /// 结束
        /// </summary>
        Ended,

        /// <summary>
        /// 取消
        /// </summary>
        Cancelled
    }
}
