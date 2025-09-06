using System;
using System.Collections.Generic;
using System.Diagnostics;
using AetherUI.Input.Events;
using AetherUI.Input.Platform;

namespace AetherUI.Input.Core
{
    /// <summary>
    /// 输入事件标准化器实现
    /// </summary>
    public class InputEventNormalizer : IInputEventNormalizer
    {
        private readonly Dictionary<uint, PointerState> _pointerStates = new();
        private readonly Dictionary<VirtualKey, bool> _keyStates = new();

        /// <summary>
        /// 标准化原始鼠标事件
        /// </summary>
        /// <param name="rawEvent">原始鼠标事件</param>
        /// <returns>标准化的指针事件集合</returns>
        public IEnumerable<PointerEvent> NormalizeMouseEvent(RawMouseEventArgs rawEvent)
        {
            var device = new InputDevice(rawEvent.DeviceType, rawEvent.DeviceId, "Mouse");
            var pointerId = PointerId.Mouse;

            // 获取或创建指针状态
            if (!_pointerStates.TryGetValue(pointerId.Value, out PointerState? state))
            {
                state = new PointerState(pointerId);
                _pointerStates[pointerId.Value] = state;
            }

            var events = new List<PointerEvent>();

            // 处理位置变化
            if (rawEvent.Delta.X != 0 || rawEvent.Delta.Y != 0)
            {
                state.Position = rawEvent.Position;
                
                var moveEvent = new PointerEvent(
                    rawEvent.Timestamp,
                    device,
                    pointerId,
                    rawEvent.Position,
                    PointerEventType.Moved,
                    rawEvent.ButtonState,
                    PointerButton.None,
                    rawEvent.Modifiers);

                events.Add(moveEvent);
            }

            // 处理按钮变化
            var buttonChanges = GetButtonChanges(state.ButtonState, rawEvent.ButtonState);
            foreach (var (button, isPressed) in buttonChanges)
            {
                var eventType = isPressed ? PointerEventType.Pressed : PointerEventType.Released;
                
                var buttonEvent = new PointerEvent(
                    rawEvent.Timestamp,
                    device,
                    pointerId,
                    rawEvent.Position,
                    eventType,
                    rawEvent.ButtonState,
                    button,
                    rawEvent.Modifiers);

                events.Add(buttonEvent);
            }

            // 处理滚轮
            if (rawEvent.WheelDelta.X != 0 || rawEvent.WheelDelta.Y != 0)
            {
                var wheelEvent = new PointerEvent(
                    rawEvent.Timestamp,
                    device,
                    pointerId,
                    rawEvent.Position,
                    PointerEventType.Wheel,
                    rawEvent.ButtonState,
                    PointerButton.None,
                    rawEvent.Modifiers,
                    wheelDelta: rawEvent.WheelDelta);

                events.Add(wheelEvent);
            }

            // 更新状态
            state.ButtonState = rawEvent.ButtonState;
            state.Position = rawEvent.Position;
            state.LastUpdateTime = rawEvent.Timestamp;

            return events;
        }

        /// <summary>
        /// 标准化原始键盘事件
        /// </summary>
        /// <param name="rawEvent">原始键盘事件</param>
        /// <returns>标准化的键盘事件集合</returns>
        public IEnumerable<KeyboardEvent> NormalizeKeyboardEvent(RawKeyboardEventArgs rawEvent)
        {
            var device = new InputDevice(rawEvent.DeviceType, rawEvent.DeviceId, "Keyboard");
            var events = new List<KeyboardEvent>();

            // 检查是否为重复按键
            bool wasPressed = _keyStates.TryGetValue(rawEvent.Key, out bool previousState) && previousState;
            bool isRepeat = rawEvent.IsPressed && wasPressed;

            // 更新按键状态
            _keyStates[rawEvent.Key] = rawEvent.IsPressed;

            var eventType = rawEvent.IsPressed ? KeyEventType.Down : KeyEventType.Up;

            var keyboardEvent = new KeyboardEvent(
                rawEvent.Timestamp,
                device,
                rawEvent.Key,
                rawEvent.ScanCode,
                eventType,
                rawEvent.Modifiers,
                isRepeat,
                rawEvent.Character);

            events.Add(keyboardEvent);

            // 如果有字符，生成字符输入事件
            if (rawEvent.Character != '\0' && rawEvent.IsPressed && !isRepeat)
            {
                var charEvent = new KeyboardEvent(
                    rawEvent.Timestamp,
                    device,
                    rawEvent.Key,
                    rawEvent.ScanCode,
                    KeyEventType.Char,
                    rawEvent.Modifiers,
                    false,
                    rawEvent.Character);

                events.Add(charEvent);
            }

            return events;
        }

        /// <summary>
        /// 标准化原始触摸事件
        /// </summary>
        /// <param name="rawEvent">原始触摸事件</param>
        /// <returns>标准化的指针事件集合</returns>
        public IEnumerable<PointerEvent> NormalizeTouchEvent(RawTouchEventArgs rawEvent)
        {
            var device = new InputDevice(rawEvent.DeviceType, rawEvent.DeviceId, "Touch");
            var pointerId = PointerId.Touch(rawEvent.TouchId);

            var eventType = rawEvent.State switch
            {
                TouchState.Down => PointerEventType.Pressed,
                TouchState.Move => PointerEventType.Moved,
                TouchState.Up => PointerEventType.Released,
                TouchState.Cancel => PointerEventType.Cancelled,
                _ => PointerEventType.Moved
            };

            var button = rawEvent.State == TouchState.Down || rawEvent.State == TouchState.Move 
                ? PointerButton.Primary 
                : PointerButton.None;

            var changedButton = rawEvent.State == TouchState.Down || rawEvent.State == TouchState.Up
                ? PointerButton.Primary
                : PointerButton.None;

            var touchEvent = new PointerEvent(
                rawEvent.Timestamp,
                device,
                pointerId,
                rawEvent.Position,
                eventType,
                button,
                changedButton,
                ModifierKeys.None,
                rawEvent.Pressure);

            // 更新或移除指针状态
            if (rawEvent.State == TouchState.Up || rawEvent.State == TouchState.Cancel)
            {
                _pointerStates.Remove(pointerId.Value);
            }
            else
            {
                if (!_pointerStates.TryGetValue(pointerId.Value, out PointerState? state))
                {
                    state = new PointerState(pointerId);
                    _pointerStates[pointerId.Value] = state;
                }

                state.Position = rawEvent.Position;
                state.ButtonState = button;
                state.LastUpdateTime = rawEvent.Timestamp;
            }

            return new[] { touchEvent };
        }

        /// <summary>
        /// 获取按钮变化
        /// </summary>
        private IEnumerable<(PointerButton button, bool isPressed)> GetButtonChanges(PointerButton oldState, PointerButton newState)
        {
            var changes = new List<(PointerButton, bool)>();

            // 检查每个按钮的状态变化
            var buttons = new[]
            {
                PointerButton.Primary,
                PointerButton.Secondary,
                PointerButton.Middle,
                PointerButton.X1,
                PointerButton.X2
            };

            foreach (var button in buttons)
            {
                bool wasPressed = (oldState & button) != 0;
                bool isPressed = (newState & button) != 0;

                if (wasPressed != isPressed)
                {
                    changes.Add((button, isPressed));
                }
            }

            return changes;
        }

        /// <summary>
        /// 清除所有状态
        /// </summary>
        public void ClearStates()
        {
            _pointerStates.Clear();
            _keyStates.Clear();
        }

        /// <summary>
        /// 获取指针状态
        /// </summary>
        /// <param name="pointerId">指针ID</param>
        /// <returns>指针状态</returns>
        public PointerState? GetPointerState(PointerId pointerId)
        {
            _pointerStates.TryGetValue(pointerId.Value, out PointerState? state);
            return state;
        }

        /// <summary>
        /// 获取按键状态
        /// </summary>
        /// <param name="key">虚拟键码</param>
        /// <returns>是否按下</returns>
        public bool GetKeyState(VirtualKey key)
        {
            return _keyStates.TryGetValue(key, out bool state) && state;
        }
    }

    /// <summary>
    /// 指针状态
    /// </summary>
    public class PointerState
    {
        /// <summary>
        /// 指针ID
        /// </summary>
        public PointerId PointerId { get; }

        /// <summary>
        /// 当前位置
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// 按钮状态
        /// </summary>
        public PointerButton ButtonState { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public uint LastUpdateTime { get; set; }

        /// <summary>
        /// 压力值
        /// </summary>
        public float Pressure { get; set; } = 1.0f;

        /// <summary>
        /// 倾斜角度X
        /// </summary>
        public float TiltX { get; set; }

        /// <summary>
        /// 倾斜角度Y
        /// </summary>
        public float TiltY { get; set; }

        /// <summary>
        /// 初始化指针状态
        /// </summary>
        /// <param name="pointerId">指针ID</param>
        public PointerState(PointerId pointerId)
        {
            PointerId = pointerId;
        }

        public override string ToString() => $"Pointer {PointerId} at {Position} buttons={ButtonState}";
    }
}
