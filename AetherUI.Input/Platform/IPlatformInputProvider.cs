using System;
using AetherUI.Input.Core;
using AetherUI.Input.Events;

namespace AetherUI.Input.Platform
{
    /// <summary>
    /// 平台输入提供者接口
    /// </summary>
    public interface IPlatformInputProvider : IDisposable
    {
        /// <summary>
        /// 原始输入事件
        /// </summary>
        event EventHandler<RawInputEventArgs>? RawInputReceived;

        /// <summary>
        /// 初始化平台输入提供者
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        void Initialize(IntPtr windowHandle);

        /// <summary>
        /// 关闭平台输入提供者
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 获取当前鼠标位置
        /// </summary>
        /// <returns>鼠标位置</returns>
        Point GetMousePosition();

        /// <summary>
        /// 获取当前按键状态
        /// </summary>
        /// <param name="key">虚拟键码</param>
        /// <returns>是否按下</returns>
        bool IsKeyPressed(VirtualKey key);

        /// <summary>
        /// 获取当前修饰键状态
        /// </summary>
        /// <returns>修饰键状态</returns>
        ModifierKeys GetModifierKeys();

        /// <summary>
        /// 设置鼠标捕获
        /// </summary>
        /// <param name="capture">是否捕获</param>
        void SetMouseCapture(bool capture);

        /// <summary>
        /// 显示/隐藏鼠标光标
        /// </summary>
        /// <param name="visible">是否可见</param>
        void SetCursorVisible(bool visible);

        /// <summary>
        /// 设置鼠标光标样式
        /// </summary>
        /// <param name="cursor">光标样式</param>
        void SetCursor(SystemCursor cursor);
    }

    /// <summary>
    /// 原始输入事件参数基类
    /// </summary>
    public abstract class RawInputEventArgs : EventArgs
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public InputDeviceType DeviceType { get; }

        /// <summary>
        /// 设备ID
        /// </summary>
        public uint DeviceId { get; }

        /// <summary>
        /// 初始化原始输入事件参数
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="deviceType">设备类型</param>
        /// <param name="deviceId">设备ID</param>
        protected RawInputEventArgs(uint timestamp, InputDeviceType deviceType, uint deviceId)
        {
            Timestamp = timestamp;
            DeviceType = deviceType;
            DeviceId = deviceId;
        }
    }

    /// <summary>
    /// 原始鼠标事件参数
    /// </summary>
    public class RawMouseEventArgs : RawInputEventArgs
    {
        /// <summary>
        /// 鼠标位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 位置增量
        /// </summary>
        public Point Delta { get; }

        /// <summary>
        /// 按钮状态
        /// </summary>
        public PointerButton ButtonState { get; }

        /// <summary>
        /// 按钮变化
        /// </summary>
        public PointerButton ButtonChanges { get; }

        /// <summary>
        /// 滚轮增量
        /// </summary>
        public Point WheelDelta { get; }

        /// <summary>
        /// 修饰键状态
        /// </summary>
        public ModifierKeys Modifiers { get; }

        /// <summary>
        /// 初始化原始鼠标事件参数
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="position">鼠标位置</param>
        /// <param name="delta">位置增量</param>
        /// <param name="buttonState">按钮状态</param>
        /// <param name="buttonChanges">按钮变化</param>
        /// <param name="wheelDelta">滚轮增量</param>
        /// <param name="modifiers">修饰键状态</param>
        public RawMouseEventArgs(
            uint timestamp,
            uint deviceId,
            Point position,
            Point delta,
            PointerButton buttonState,
            PointerButton buttonChanges,
            Point wheelDelta,
            ModifierKeys modifiers)
            : base(timestamp, InputDeviceType.Mouse, deviceId)
        {
            Position = position;
            Delta = delta;
            ButtonState = buttonState;
            ButtonChanges = buttonChanges;
            WheelDelta = wheelDelta;
            Modifiers = modifiers;
        }
    }

    /// <summary>
    /// 原始键盘事件参数
    /// </summary>
    public class RawKeyboardEventArgs : RawInputEventArgs
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
        /// 是否按下
        /// </summary>
        public bool IsPressed { get; }

        /// <summary>
        /// 是否为重复按键
        /// </summary>
        public bool IsRepeat { get; }

        /// <summary>
        /// 修饰键状态
        /// </summary>
        public ModifierKeys Modifiers { get; }

        /// <summary>
        /// 字符（如果有）
        /// </summary>
        public char Character { get; }

        /// <summary>
        /// 初始化原始键盘事件参数
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="key">虚拟键码</param>
        /// <param name="scanCode">扫描码</param>
        /// <param name="isPressed">是否按下</param>
        /// <param name="isRepeat">是否为重复按键</param>
        /// <param name="modifiers">修饰键状态</param>
        /// <param name="character">字符</param>
        public RawKeyboardEventArgs(
            uint timestamp,
            uint deviceId,
            VirtualKey key,
            uint scanCode,
            bool isPressed,
            bool isRepeat,
            ModifierKeys modifiers,
            char character = '\0')
            : base(timestamp, InputDeviceType.Keyboard, deviceId)
        {
            Key = key;
            ScanCode = scanCode;
            IsPressed = isPressed;
            IsRepeat = isRepeat;
            Modifiers = modifiers;
            Character = character;
        }
    }

    /// <summary>
    /// 原始触摸事件参数
    /// </summary>
    public class RawTouchEventArgs : RawInputEventArgs
    {
        /// <summary>
        /// 触摸ID
        /// </summary>
        public uint TouchId { get; }

        /// <summary>
        /// 触摸位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 触摸状态
        /// </summary>
        public TouchState State { get; }

        /// <summary>
        /// 压力值
        /// </summary>
        public float Pressure { get; }

        /// <summary>
        /// 触摸区域大小
        /// </summary>
        public Point Size { get; }

        /// <summary>
        /// 初始化原始触摸事件参数
        /// </summary>
        /// <param name="timestamp">时间戳</param>
        /// <param name="deviceId">设备ID</param>
        /// <param name="touchId">触摸ID</param>
        /// <param name="position">触摸位置</param>
        /// <param name="state">触摸状态</param>
        /// <param name="pressure">压力值</param>
        /// <param name="size">触摸区域大小</param>
        public RawTouchEventArgs(
            uint timestamp,
            uint deviceId,
            uint touchId,
            Point position,
            TouchState state,
            float pressure,
            Point size)
            : base(timestamp, InputDeviceType.Touch, deviceId)
        {
            TouchId = touchId;
            Position = position;
            State = state;
            Pressure = pressure;
            Size = size;
        }
    }

    /// <summary>
    /// 触摸状态
    /// </summary>
    public enum TouchState
    {
        /// <summary>
        /// 开始触摸
        /// </summary>
        Down,

        /// <summary>
        /// 移动
        /// </summary>
        Move,

        /// <summary>
        /// 结束触摸
        /// </summary>
        Up,

        /// <summary>
        /// 取消触摸
        /// </summary>
        Cancel
    }

    /// <summary>
    /// 系统光标样式
    /// </summary>
    public enum SystemCursor
    {
        /// <summary>
        /// 默认箭头
        /// </summary>
        Arrow,

        /// <summary>
        /// 文本输入
        /// </summary>
        IBeam,

        /// <summary>
        /// 等待
        /// </summary>
        Wait,

        /// <summary>
        /// 十字
        /// </summary>
        Cross,

        /// <summary>
        /// 手型
        /// </summary>
        Hand,

        /// <summary>
        /// 禁止
        /// </summary>
        No,

        /// <summary>
        /// 调整大小（上下）
        /// </summary>
        SizeNS,

        /// <summary>
        /// 调整大小（左右）
        /// </summary>
        SizeWE,

        /// <summary>
        /// 调整大小（对角线）
        /// </summary>
        SizeNWSE,

        /// <summary>
        /// 调整大小（对角线）
        /// </summary>
        SizeNESW,

        /// <summary>
        /// 调整大小（全方向）
        /// </summary>
        SizeAll,

        /// <summary>
        /// 帮助
        /// </summary>
        Help
    }
}
