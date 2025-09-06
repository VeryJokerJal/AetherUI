using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AetherUI.Input.Core;
using AetherUI.Input.Events;

namespace AetherUI.Input.Gestures
{
    /// <summary>
    /// 捏合手势识别器
    /// </summary>
    public class PinchGestureRecognizer : IGestureRecognizer, IMultiPointerGestureRecognizer
    {
        private readonly GestureConfiguration _config;
        private Point _initialCenter;
        private double _initialDistance;
        private double _currentScale = 1.0;
        private bool _isPinching;
        private uint _startTime;

        /// <summary>
        /// 手势类型名称
        /// </summary>
        public string GestureType => GestureTypes.Pinch;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 手势识别事件
        /// </summary>
        public event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 初始化捏合手势识别器
        /// </summary>
        /// <param name="config">手势配置</param>
        public PinchGestureRecognizer(GestureConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        public bool ProcessPointerEvent(PointerEvent pointerEvent)
        {
            // 捏合手势需要多点触控，单点事件不处理
            return false;
        }

        /// <summary>
        /// 处理多点指针事件
        /// </summary>
        public bool ProcessMultiPointerEvent(PointerEvent pointerEvent, IReadOnlyList<PointerTracker> activePointers)
        {
            if (!IsEnabled || activePointers.Count < 2)
                return false;

            var pointer1 = activePointers[0];
            var pointer2 = activePointers[1];

            var currentCenter = CalculateCenter(pointer1.CurrentPosition, pointer2.CurrentPosition);
            var currentDistance = CalculateDistance(pointer1.CurrentPosition, pointer2.CurrentPosition);

            switch (pointerEvent.EventType)
            {
                case PointerEventType.Pressed:
                    if (activePointers.Count == 2)
                    {
                        _initialCenter = currentCenter;
                        _initialDistance = currentDistance;
                        _currentScale = 1.0;
                        _isPinching = false;
                        _startTime = pointerEvent.Timestamp;
                    }
                    break;

                case PointerEventType.Moved:
                    if (activePointers.Count == 2 && _initialDistance > 0)
                    {
                        var newScale = currentDistance / _initialDistance;
                        var scaleChange = Math.Abs(newScale - _currentScale);

                        if (!_isPinching && scaleChange >= _config.PinchThreshold / 100.0)
                        {
                            _isPinching = true;
                            TriggerPinchEvent(GestureState.Started, currentCenter, newScale, pointerEvent);
                        }
                        else if (_isPinching)
                        {
                            TriggerPinchEvent(GestureState.Updated, currentCenter, newScale, pointerEvent);
                        }

                        _currentScale = newScale;
                        return _isPinching;
                    }
                    break;

                case PointerEventType.Released:
                    if (_isPinching)
                    {
                        TriggerPinchEvent(GestureState.Ended, currentCenter, _currentScale, pointerEvent);
                        Reset();
                        return true;
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
            _isPinching = false;
            _currentScale = 1.0;
            _initialDistance = 0;
        }

        /// <summary>
        /// 获取当前手势状态
        /// </summary>
        public GestureState GetCurrentState()
        {
            return _isPinching ? GestureState.Updated : GestureState.Ended;
        }

        /// <summary>
        /// 触发捏合事件
        /// </summary>
        private void TriggerPinchEvent(GestureState state, Point center, double scale, PointerEvent triggerEvent)
        {
            var gestureData = new PinchGestureData(_initialCenter, center, _initialDistance, scale);
            var eventArgs = new GestureRecognizedEventArgs(
                GestureTypes.Pinch,
                state,
                center,
                triggerEvent,
                gestureData);

            GestureRecognized?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// 计算中心点
        /// </summary>
        private Point CalculateCenter(Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }

        /// <summary>
        /// 计算距离
        /// </summary>
        private double CalculateDistance(Point p1, Point p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    /// <summary>
    /// 旋转手势识别器
    /// </summary>
    public class RotationGestureRecognizer : IGestureRecognizer, IMultiPointerGestureRecognizer
    {
        private readonly GestureConfiguration _config;
        private Point _initialCenter;
        private double _initialAngle;
        private double _currentRotation;
        private bool _isRotating;
        private uint _startTime;

        /// <summary>
        /// 手势类型名称
        /// </summary>
        public string GestureType => GestureTypes.Rotation;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 手势识别事件
        /// </summary>
        public event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 初始化旋转手势识别器
        /// </summary>
        /// <param name="config">手势配置</param>
        public RotationGestureRecognizer(GestureConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        public bool ProcessPointerEvent(PointerEvent pointerEvent)
        {
            return false; // 旋转手势需要多点触控
        }

        /// <summary>
        /// 处理多点指针事件
        /// </summary>
        public bool ProcessMultiPointerEvent(PointerEvent pointerEvent, IReadOnlyList<PointerTracker> activePointers)
        {
            if (!IsEnabled || activePointers.Count < 2)
                return false;

            var pointer1 = activePointers[0];
            var pointer2 = activePointers[1];

            var currentCenter = CalculateCenter(pointer1.CurrentPosition, pointer2.CurrentPosition);
            var currentAngle = CalculateAngle(pointer1.CurrentPosition, pointer2.CurrentPosition);

            switch (pointerEvent.EventType)
            {
                case PointerEventType.Pressed:
                    if (activePointers.Count == 2)
                    {
                        _initialCenter = currentCenter;
                        _initialAngle = currentAngle;
                        _currentRotation = 0;
                        _isRotating = false;
                        _startTime = pointerEvent.Timestamp;
                    }
                    break;

                case PointerEventType.Moved:
                    if (activePointers.Count == 2)
                    {
                        var angleDiff = NormalizeAngle(currentAngle - _initialAngle);
                        var rotationChange = Math.Abs(angleDiff - _currentRotation);

                        if (!_isRotating && rotationChange >= _config.RotationThreshold)
                        {
                            _isRotating = true;
                            TriggerRotationEvent(GestureState.Started, currentCenter, angleDiff, pointerEvent);
                        }
                        else if (_isRotating)
                        {
                            TriggerRotationEvent(GestureState.Updated, currentCenter, angleDiff, pointerEvent);
                        }

                        _currentRotation = angleDiff;
                        return _isRotating;
                    }
                    break;

                case PointerEventType.Released:
                    if (_isRotating)
                    {
                        TriggerRotationEvent(GestureState.Ended, currentCenter, _currentRotation, pointerEvent);
                        Reset();
                        return true;
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
            _isRotating = false;
            _currentRotation = 0;
        }

        /// <summary>
        /// 获取当前手势状态
        /// </summary>
        public GestureState GetCurrentState()
        {
            return _isRotating ? GestureState.Updated : GestureState.Ended;
        }

        /// <summary>
        /// 触发旋转事件
        /// </summary>
        private void TriggerRotationEvent(GestureState state, Point center, double rotation, PointerEvent triggerEvent)
        {
            var gestureData = new RotationGestureData(_initialCenter, center, rotation);
            var eventArgs = new GestureRecognizedEventArgs(
                GestureTypes.Rotation,
                state,
                center,
                triggerEvent,
                gestureData);

            GestureRecognized?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// 计算中心点
        /// </summary>
        private Point CalculateCenter(Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }

        /// <summary>
        /// 计算角度
        /// </summary>
        private double CalculateAngle(Point p1, Point p2)
        {
            return Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
        }

        /// <summary>
        /// 标准化角度到 [-π, π] 范围
        /// </summary>
        private double NormalizeAngle(double angle)
        {
            while (angle > Math.PI) angle -= 2 * Math.PI;
            while (angle < -Math.PI) angle += 2 * Math.PI;
            return angle;
        }
    }

    /// <summary>
    /// 双指点击手势识别器
    /// </summary>
    public class TwoFingerTapGestureRecognizer : IGestureRecognizer, IMultiPointerGestureRecognizer
    {
        private readonly GestureConfiguration _config;
        private Point _initialCenter;
        private uint _startTime;
        private bool _isTwoFingerDown;

        /// <summary>
        /// 手势类型名称
        /// </summary>
        public string GestureType => GestureTypes.TwoFingerTap;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 手势识别事件
        /// </summary>
        public event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 初始化双指点击手势识别器
        /// </summary>
        /// <param name="config">手势配置</param>
        public TwoFingerTapGestureRecognizer(GestureConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        public bool ProcessPointerEvent(PointerEvent pointerEvent)
        {
            return false; // 双指点击需要多点触控
        }

        /// <summary>
        /// 处理多点指针事件
        /// </summary>
        public bool ProcessMultiPointerEvent(PointerEvent pointerEvent, IReadOnlyList<PointerTracker> activePointers)
        {
            if (!IsEnabled)
                return false;

            switch (pointerEvent.EventType)
            {
                case PointerEventType.Pressed:
                    if (activePointers.Count == 2)
                    {
                        var pointer1 = activePointers[0];
                        var pointer2 = activePointers[1];
                        _initialCenter = CalculateCenter(pointer1.CurrentPosition, pointer2.CurrentPosition);
                        _startTime = pointerEvent.Timestamp;
                        _isTwoFingerDown = true;
                    }
                    break;

                case PointerEventType.Moved:
                    if (_isTwoFingerDown && activePointers.Count == 2)
                    {
                        var pointer1 = activePointers[0];
                        var pointer2 = activePointers[1];
                        var currentCenter = CalculateCenter(pointer1.CurrentPosition, pointer2.CurrentPosition);
                        var movement = CalculateDistance(_initialCenter, currentCenter);

                        // 如果移动太多，取消双指点击
                        if (movement > _config.MaxTapMovement)
                        {
                            _isTwoFingerDown = false;
                        }
                    }
                    break;

                case PointerEventType.Released:
                    if (_isTwoFingerDown && activePointers.Count <= 2)
                    {
                        var duration = pointerEvent.Timestamp - _startTime;
                        
                        // 检查是否在时间限制内
                        if (duration <= _config.TapTimeoutMs)
                        {
                            TriggerTwoFingerTapEvent(pointerEvent);
                            Reset();
                            return true;
                        }
                    }
                    
                    if (activePointers.Count < 2)
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
            _isTwoFingerDown = false;
        }

        /// <summary>
        /// 获取当前手势状态
        /// </summary>
        public GestureState GetCurrentState()
        {
            return _isTwoFingerDown ? GestureState.Started : GestureState.Ended;
        }

        /// <summary>
        /// 触发双指点击事件
        /// </summary>
        private void TriggerTwoFingerTapEvent(PointerEvent triggerEvent)
        {
            var gestureData = new TwoFingerTapGestureData(_initialCenter);
            var eventArgs = new GestureRecognizedEventArgs(
                GestureTypes.TwoFingerTap,
                GestureState.Ended,
                _initialCenter,
                triggerEvent,
                gestureData);

            GestureRecognized?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// 计算中心点
        /// </summary>
        private Point CalculateCenter(Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        }

        /// <summary>
        /// 计算距离
        /// </summary>
        private double CalculateDistance(Point p1, Point p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    /// <summary>
    /// 滑动手势识别器
    /// </summary>
    public class SwipeGestureRecognizer : IGestureRecognizer, ISinglePointerGestureRecognizer
    {
        private readonly GestureConfiguration _config;
        private Point _startPosition;
        private uint _startTime;
        private bool _isTracking;

        /// <summary>
        /// 手势类型名称
        /// </summary>
        public string GestureType => GestureTypes.Swipe;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 手势识别事件
        /// </summary>
        public event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 初始化滑动手势识别器
        /// </summary>
        /// <param name="config">手势配置</param>
        public SwipeGestureRecognizer(GestureConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        public bool ProcessPointerEvent(PointerEvent pointerEvent)
        {
            if (!IsEnabled)
                return false;

            switch (pointerEvent.EventType)
            {
                case PointerEventType.Pressed:
                    _startPosition = pointerEvent.Position;
                    _startTime = pointerEvent.Timestamp;
                    _isTracking = true;
                    break;

                case PointerEventType.Released:
                    if (_isTracking)
                    {
                        var duration = pointerEvent.Timestamp - _startTime;
                        var distance = CalculateDistance(_startPosition, pointerEvent.Position);
                        var velocity = distance / (duration / 1000.0); // 像素/秒

                        if (distance >= _config.SwipeMinDistance && velocity >= _config.SwipeMinVelocity)
                        {
                            var direction = CalculateDirection(_startPosition, pointerEvent.Position);
                            TriggerSwipeEvent(direction, distance, velocity, pointerEvent);
                            Reset();
                            return true;
                        }
                    }
                    Reset();
                    break;

                case PointerEventType.Cancelled:
                    Reset();
                    break;
            }

            return false;
        }

        /// <summary>
        /// 重置识别器状态
        /// </summary>
        public void Reset()
        {
            _isTracking = false;
        }

        /// <summary>
        /// 获取当前手势状态
        /// </summary>
        public GestureState GetCurrentState()
        {
            return _isTracking ? GestureState.Started : GestureState.Ended;
        }

        /// <summary>
        /// 触发滑动事件
        /// </summary>
        private void TriggerSwipeEvent(SwipeDirection direction, double distance, double velocity, PointerEvent triggerEvent)
        {
            var gestureData = new SwipeGestureData(_startPosition, triggerEvent.Position, direction, distance, velocity);
            var eventArgs = new GestureRecognizedEventArgs(
                GestureTypes.Swipe,
                GestureState.Ended,
                triggerEvent.Position,
                triggerEvent,
                gestureData);

            GestureRecognized?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// 计算距离
        /// </summary>
        private double CalculateDistance(Point p1, Point p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 计算方向
        /// </summary>
        private SwipeDirection CalculateDirection(Point start, Point end)
        {
            var dx = end.X - start.X;
            var dy = end.Y - start.Y;
            var angle = Math.Atan2(dy, dx) * 180 / Math.PI;

            // 标准化角度到 [0, 360)
            if (angle < 0) angle += 360;

            // 确定方向
            if (angle >= 315 || angle < 45) return SwipeDirection.Right;
            if (angle >= 45 && angle < 135) return SwipeDirection.Down;
            if (angle >= 135 && angle < 225) return SwipeDirection.Left;
            return SwipeDirection.Up;
        }
    }
}
