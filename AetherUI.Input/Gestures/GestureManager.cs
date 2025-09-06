using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AetherUI.Input.Events;

namespace AetherUI.Input.Gestures
{
    /// <summary>
    /// 手势管理器实现
    /// </summary>
    public class GestureManager : IGestureManager
    {
        private readonly GestureConfiguration _configuration;
        private readonly ConcurrentDictionary<string, IGestureRecognizer> _recognizers = new();

        /// <summary>
        /// 手势识别事件
        /// </summary>
        public event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 初始化手势管理器
        /// </summary>
        /// <param name="configuration">手势配置</param>
        public GestureManager(GestureConfiguration? configuration = null)
        {
            _configuration = configuration ?? GestureConfiguration.Default;

            // 注册默认手势识别器
            RegisterDefaultRecognizers();
        }

        /// <summary>
        /// 注册手势识别器
        /// </summary>
        /// <param name="recognizer">手势识别器</param>
        public void RegisterRecognizer(IGestureRecognizer recognizer)
        {
            if (recognizer == null)
                throw new ArgumentNullException(nameof(recognizer));

            _recognizers[recognizer.GestureType] = recognizer;
            recognizer.GestureRecognized += OnGestureRecognized;

            Debug.WriteLine($"手势识别器已注册: {recognizer.GestureType}");
        }

        /// <summary>
        /// 注销手势识别器
        /// </summary>
        /// <param name="recognizer">手势识别器</param>
        public void UnregisterRecognizer(IGestureRecognizer recognizer)
        {
            if (recognizer == null)
                throw new ArgumentNullException(nameof(recognizer));

            if (_recognizers.TryRemove(recognizer.GestureType, out IGestureRecognizer? removed))
            {
                removed.GestureRecognized -= OnGestureRecognized;
                Debug.WriteLine($"手势识别器已注销: {recognizer.GestureType}");
            }
        }

        /// <summary>
        /// 获取手势识别器
        /// </summary>
        /// <param name="gestureType">手势类型</param>
        /// <returns>手势识别器</returns>
        public IGestureRecognizer? GetRecognizer(string gestureType)
        {
            _recognizers.TryGetValue(gestureType, out IGestureRecognizer? recognizer);
            return recognizer;
        }

        /// <summary>
        /// 获取所有手势识别器
        /// </summary>
        /// <returns>手势识别器集合</returns>
        public IEnumerable<IGestureRecognizer> GetAllRecognizers()
        {
            return _recognizers.Values.ToArray();
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>是否有手势识别器处理了事件</returns>
        public bool ProcessPointerEvent(PointerEvent pointerEvent)
        {
            bool handled = false;

            foreach (var recognizer in _recognizers.Values)
            {
                if (recognizer.IsEnabled)
                {
                    try
                    {
                        if (recognizer.ProcessPointerEvent(pointerEvent))
                        {
                            handled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"手势识别器 {recognizer.GestureType} 处理事件失败: {ex.Message}");
                    }
                }
            }

            return handled;
        }

        /// <summary>
        /// 重置所有手势识别器
        /// </summary>
        public void ResetAll()
        {
            foreach (var recognizer in _recognizers.Values)
            {
                try
                {
                    recognizer.Reset();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"重置手势识别器 {recognizer.GestureType} 失败: {ex.Message}");
                }
            }

            Debug.WriteLine("所有手势识别器已重置");
        }

        /// <summary>
        /// 启用/禁用手势类型
        /// </summary>
        /// <param name="gestureType">手势类型</param>
        /// <param name="enabled">是否启用</param>
        public void SetGestureEnabled(string gestureType, bool enabled)
        {
            if (_recognizers.TryGetValue(gestureType, out IGestureRecognizer? recognizer))
            {
                recognizer.IsEnabled = enabled;
                Debug.WriteLine($"手势 {gestureType} 已{(enabled ? "启用" : "禁用")}");
            }
        }

        /// <summary>
        /// 检查手势类型是否启用
        /// </summary>
        /// <param name="gestureType">手势类型</param>
        /// <returns>是否启用</returns>
        public bool IsGestureEnabled(string gestureType)
        {
            return _recognizers.TryGetValue(gestureType, out IGestureRecognizer? recognizer) && 
                   recognizer.IsEnabled;
        }

        /// <summary>
        /// 注册默认手势识别器
        /// </summary>
        private void RegisterDefaultRecognizers()
        {
            // 注册点击手势识别器
            RegisterRecognizer(new TapGestureRecognizer(_configuration));

            // 注册拖拽手势识别器
            RegisterRecognizer(new DragGestureRecognizer(_configuration));

            // 注册长按手势识别器
            RegisterRecognizer(new LongPressGestureRecognizer(_configuration));

            Debug.WriteLine("默认手势识别器已注册");
        }

        /// <summary>
        /// 处理手势识别事件
        /// </summary>
        private void OnGestureRecognized(object? sender, GestureRecognizedEventArgs e)
        {
            GestureRecognized?.Invoke(this, e);
        }
    }

    /// <summary>
    /// 点击手势识别器
    /// </summary>
    public class TapGestureRecognizer : IGestureRecognizer
    {
        private readonly GestureConfiguration _config;
        private Core.Point _startPosition;
        private uint _startTime;
        private bool _isPressed;
        private int _tapCount;
        private uint _lastTapTime;

        /// <summary>
        /// 手势类型名称
        /// </summary>
        public string GestureType => GestureTypes.Tap;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 手势识别事件
        /// </summary>
        public event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 初始化点击手势识别器
        /// </summary>
        /// <param name="config">手势配置</param>
        public TapGestureRecognizer(GestureConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>是否处理了事件</returns>
        public bool ProcessPointerEvent(PointerEvent pointerEvent)
        {
            if (!IsEnabled || pointerEvent.PointerId.DeviceType != Core.InputDeviceType.Mouse)
                return false;

            switch (pointerEvent.EventType)
            {
                case PointerEventType.Pressed:
                    if (pointerEvent.ChangedButton == Core.PointerButton.Primary)
                    {
                        _startPosition = pointerEvent.Position;
                        _startTime = pointerEvent.Timestamp;
                        _isPressed = true;
                        return true;
                    }
                    break;

                case PointerEventType.Released:
                    if (_isPressed && pointerEvent.ChangedButton == Core.PointerButton.Primary)
                    {
                        _isPressed = false;
                        
                        // 检查是否在时间和距离限制内
                        var duration = pointerEvent.Timestamp - _startTime;
                        var distance = CalculateDistance(_startPosition, pointerEvent.Position);

                        if (duration <= _config.TapTimeoutMs && distance <= _config.MaxTapMovement)
                        {
                            // 检查是否为双击
                            var timeSinceLastTap = pointerEvent.Timestamp - _lastTapTime;
                            if (timeSinceLastTap <= _config.DoubleTapIntervalMs)
                            {
                                _tapCount++;
                            }
                            else
                            {
                                _tapCount = 1;
                            }

                            _lastTapTime = pointerEvent.Timestamp;

                            // 触发点击事件
                            var gestureData = new TapGestureData(_tapCount, pointerEvent.Position, Core.PointerButton.Primary);
                            var eventArgs = new GestureRecognizedEventArgs(
                                _tapCount > 1 ? GestureTypes.DoubleTap : GestureTypes.Tap,
                                GestureState.Ended,
                                pointerEvent.Position,
                                pointerEvent,
                                gestureData);

                            GestureRecognized?.Invoke(this, eventArgs);
                            return true;
                        }
                    }
                    break;

                case PointerEventType.Moved:
                    if (_isPressed)
                    {
                        var distance = CalculateDistance(_startPosition, pointerEvent.Position);
                        if (distance > _config.MaxTapMovement)
                        {
                            _isPressed = false; // 移动太远，取消点击
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// 重置识别器状态
        /// </summary>
        public void Reset()
        {
            _isPressed = false;
            _tapCount = 0;
            _lastTapTime = 0;
        }

        /// <summary>
        /// 获取当前手势状态
        /// </summary>
        /// <returns>手势状态</returns>
        public GestureState GetCurrentState()
        {
            return _isPressed ? GestureState.Started : GestureState.Ended;
        }

        /// <summary>
        /// 计算两点间距离
        /// </summary>
        private double CalculateDistance(Core.Point p1, Core.Point p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    /// <summary>
    /// 拖拽手势识别器
    /// </summary>
    public class DragGestureRecognizer : IGestureRecognizer
    {
        private readonly GestureConfiguration _config;
        private Core.Point _startPosition;
        private Core.Point _lastPosition;
        private uint _startTime;
        private uint _lastTime;
        private bool _isDragging;
        private bool _isPressed;

        /// <summary>
        /// 手势类型名称
        /// </summary>
        public string GestureType => GestureTypes.Drag;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 手势识别事件
        /// </summary>
        public event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 初始化拖拽手势识别器
        /// </summary>
        /// <param name="config">手势配置</param>
        public DragGestureRecognizer(GestureConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>是否处理了事件</returns>
        public bool ProcessPointerEvent(PointerEvent pointerEvent)
        {
            if (!IsEnabled)
                return false;

            switch (pointerEvent.EventType)
            {
                case PointerEventType.Pressed:
                    if (pointerEvent.ChangedButton == Core.PointerButton.Primary)
                    {
                        _startPosition = pointerEvent.Position;
                        _lastPosition = pointerEvent.Position;
                        _startTime = pointerEvent.Timestamp;
                        _lastTime = pointerEvent.Timestamp;
                        _isPressed = true;
                        _isDragging = false;
                        return true;
                    }
                    break;

                case PointerEventType.Moved:
                    if (_isPressed)
                    {
                        var distance = CalculateDistance(_startPosition, pointerEvent.Position);
                        
                        if (!_isDragging && distance >= _config.DragThreshold)
                        {
                            // 开始拖拽
                            _isDragging = true;
                            TriggerDragEvent(GestureState.Started, pointerEvent);
                        }
                        else if (_isDragging)
                        {
                            // 更新拖拽
                            TriggerDragEvent(GestureState.Updated, pointerEvent);
                        }

                        _lastPosition = pointerEvent.Position;
                        _lastTime = pointerEvent.Timestamp;
                        return _isDragging;
                    }
                    break;

                case PointerEventType.Released:
                    if (_isDragging && pointerEvent.ChangedButton == Core.PointerButton.Primary)
                    {
                        TriggerDragEvent(GestureState.Ended, pointerEvent);
                        Reset();
                        return true;
                    }
                    else if (_isPressed)
                    {
                        Reset();
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// 重置识别器状态
        /// </summary>
        public void Reset()
        {
            _isPressed = false;
            _isDragging = false;
        }

        /// <summary>
        /// 获取当前手势状态
        /// </summary>
        /// <returns>手势状态</returns>
        public GestureState GetCurrentState()
        {
            if (_isDragging) return GestureState.Updated;
            if (_isPressed) return GestureState.Started;
            return GestureState.Ended;
        }

        /// <summary>
        /// 触发拖拽事件
        /// </summary>
        private void TriggerDragEvent(GestureState state, PointerEvent pointerEvent)
        {
            var deltaOffset = new Core.Point(
                pointerEvent.Position.X - _lastPosition.X,
                pointerEvent.Position.Y - _lastPosition.Y);

            var velocity = CalculateVelocity(deltaOffset, pointerEvent.Timestamp - _lastTime);

            var gestureData = new DragGestureData(
                _startPosition,
                pointerEvent.Position,
                deltaOffset,
                velocity,
                Core.PointerButton.Primary);

            var eventArgs = new GestureRecognizedEventArgs(
                GestureTypes.Drag,
                state,
                pointerEvent.Position,
                pointerEvent,
                gestureData);

            GestureRecognized?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// 计算两点间距离
        /// </summary>
        private double CalculateDistance(Core.Point p1, Core.Point p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 计算速度
        /// </summary>
        private Core.Point CalculateVelocity(Core.Point delta, uint timeDelta)
        {
            if (timeDelta == 0) return Core.Point.Zero;

            var timeInSeconds = timeDelta / 1000.0;
            return new Core.Point(delta.X / timeInSeconds, delta.Y / timeInSeconds);
        }
    }

    /// <summary>
    /// 长按手势识别器
    /// </summary>
    public class LongPressGestureRecognizer : IGestureRecognizer
    {
        private readonly GestureConfiguration _config;
        private Core.Point _startPosition;
        private uint _startTime;
        private bool _isPressed;
        private bool _hasTriggered;

        /// <summary>
        /// 手势类型名称
        /// </summary>
        public string GestureType => GestureTypes.LongPress;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 手势识别事件
        /// </summary>
        public event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 初始化长按手势识别器
        /// </summary>
        /// <param name="config">手势配置</param>
        public LongPressGestureRecognizer(GestureConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>是否处理了事件</returns>
        public bool ProcessPointerEvent(PointerEvent pointerEvent)
        {
            if (!IsEnabled)
                return false;

            switch (pointerEvent.EventType)
            {
                case PointerEventType.Pressed:
                    if (pointerEvent.ChangedButton == Core.PointerButton.Primary)
                    {
                        _startPosition = pointerEvent.Position;
                        _startTime = pointerEvent.Timestamp;
                        _isPressed = true;
                        _hasTriggered = false;
                        return true;
                    }
                    break;

                case PointerEventType.Moved:
                    if (_isPressed)
                    {
                        var distance = CalculateDistance(_startPosition, pointerEvent.Position);
                        if (distance > _config.MaxTapMovement)
                        {
                            Reset(); // 移动太远，取消长按
                        }
                        else if (!_hasTriggered)
                        {
                            var duration = pointerEvent.Timestamp - _startTime;
                            if (duration >= _config.LongPressDelayMs)
                            {
                                TriggerLongPress(pointerEvent);
                            }
                        }
                    }
                    break;

                case PointerEventType.Released:
                    if (_isPressed)
                    {
                        if (!_hasTriggered)
                        {
                            var duration = pointerEvent.Timestamp - _startTime;
                            if (duration >= _config.LongPressDelayMs)
                            {
                                TriggerLongPress(pointerEvent);
                            }
                        }
                        Reset();
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// 重置识别器状态
        /// </summary>
        public void Reset()
        {
            _isPressed = false;
            _hasTriggered = false;
        }

        /// <summary>
        /// 获取当前手势状态
        /// </summary>
        /// <returns>手势状态</returns>
        public GestureState GetCurrentState()
        {
            if (_hasTriggered) return GestureState.Ended;
            if (_isPressed) return GestureState.Started;
            return GestureState.Ended;
        }

        /// <summary>
        /// 触发长按事件
        /// </summary>
        private void TriggerLongPress(PointerEvent pointerEvent)
        {
            _hasTriggered = true;

            var duration = (int)(pointerEvent.Timestamp - _startTime);
            var gestureData = new LongPressGestureData(_startPosition, duration, Core.PointerButton.Primary);

            var eventArgs = new GestureRecognizedEventArgs(
                GestureTypes.LongPress,
                GestureState.Ended,
                _startPosition,
                pointerEvent,
                gestureData);

            GestureRecognized?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// 计算两点间距离
        /// </summary>
        private double CalculateDistance(Core.Point p1, Core.Point p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
