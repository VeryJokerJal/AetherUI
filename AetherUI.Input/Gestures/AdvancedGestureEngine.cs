using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AetherUI.Input.Core;
using AetherUI.Input.Events;

namespace AetherUI.Input.Gestures
{
    /// <summary>
    /// 高级手势识别引擎
    /// </summary>
    public class AdvancedGestureEngine : IGestureManager
    {
        private readonly GestureConfiguration _configuration;
        private readonly ConcurrentDictionary<string, IGestureRecognizer> _recognizers = new();
        private readonly ConcurrentDictionary<uint, PointerTracker> _pointerTrackers = new();
        private readonly List<IGestureProcessor> _processors = new();
        private readonly GestureStateMachine _stateMachine;

        /// <summary>
        /// 手势识别事件
        /// </summary>
        public event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 多点触控手势事件
        /// </summary>
        public event EventHandler<MultiTouchGestureEventArgs>? MultiTouchGestureRecognized;

        /// <summary>
        /// 初始化高级手势识别引擎
        /// </summary>
        /// <param name="configuration">手势配置</param>
        public AdvancedGestureEngine(GestureConfiguration? configuration = null)
        {
            _configuration = configuration ?? GestureConfiguration.Default;
            _stateMachine = new GestureStateMachine();

            // 注册默认手势识别器
            RegisterDefaultRecognizers();

            // 注册默认手势处理器
            RegisterDefaultProcessors();
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

            try
            {
                // 更新指针跟踪
                UpdatePointerTracking(pointerEvent);

                // 处理单点手势
                handled |= ProcessSinglePointerGestures(pointerEvent);

                // 处理多点手势
                handled |= ProcessMultiPointerGestures(pointerEvent);

                // 更新状态机
                _stateMachine.ProcessEvent(pointerEvent);

                // 应用手势处理器
                foreach (var processor in _processors)
                {
                    processor.ProcessPointerEvent(pointerEvent, _pointerTrackers.Values);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理指针事件失败: {ex.Message}");
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

            _pointerTrackers.Clear();
            _stateMachine.Reset();

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
        /// 添加手势处理器
        /// </summary>
        /// <param name="processor">手势处理器</param>
        public void AddProcessor(IGestureProcessor processor)
        {
            if (processor != null && !_processors.Contains(processor))
            {
                _processors.Add(processor);
            }
        }

        /// <summary>
        /// 移除手势处理器
        /// </summary>
        /// <param name="processor">手势处理器</param>
        public void RemoveProcessor(IGestureProcessor processor)
        {
            _processors.Remove(processor);
        }

        /// <summary>
        /// 更新指针跟踪
        /// </summary>
        private void UpdatePointerTracking(PointerEvent pointerEvent)
        {
            var pointerId = pointerEvent.PointerId.Value;

            switch (pointerEvent.EventType)
            {
                case PointerEventType.Pressed:
                    var tracker = new PointerTracker(pointerEvent.PointerId, pointerEvent.Position, pointerEvent.Timestamp);
                    _pointerTrackers[pointerId] = tracker;
                    break;

                case PointerEventType.Moved:
                    if (_pointerTrackers.TryGetValue(pointerId, out PointerTracker? existingTracker))
                    {
                        existingTracker.AddPoint(pointerEvent.Position, pointerEvent.Timestamp);
                    }
                    break;

                case PointerEventType.Released:
                case PointerEventType.Cancelled:
                    if (_pointerTrackers.TryGetValue(pointerId, out PointerTracker? releasedTracker))
                    {
                        releasedTracker.AddPoint(pointerEvent.Position, pointerEvent.Timestamp);
                        releasedTracker.IsActive = false;
                        
                        // 延迟移除，给手势识别器时间处理
                        _ = System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
                        {
                            _pointerTrackers.TryRemove(pointerId, out _);
                        });
                    }
                    break;
            }
        }

        /// <summary>
        /// 处理单点手势
        /// </summary>
        private bool ProcessSinglePointerGestures(PointerEvent pointerEvent)
        {
            bool handled = false;

            foreach (var recognizer in _recognizers.Values)
            {
                if (recognizer.IsEnabled && recognizer is ISinglePointerGestureRecognizer singlePointer)
                {
                    try
                    {
                        if (singlePointer.ProcessPointerEvent(pointerEvent))
                        {
                            handled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"单点手势识别器 {recognizer.GestureType} 处理失败: {ex.Message}");
                    }
                }
            }

            return handled;
        }

        /// <summary>
        /// 处理多点手势
        /// </summary>
        private bool ProcessMultiPointerGestures(PointerEvent pointerEvent)
        {
            bool handled = false;
            var activePointers = _pointerTrackers.Values.Where(t => t.IsActive).ToList();

            if (activePointers.Count < 2)
                return false;

            foreach (var recognizer in _recognizers.Values)
            {
                if (recognizer.IsEnabled && recognizer is IMultiPointerGestureRecognizer multiPointer)
                {
                    try
                    {
                        if (multiPointer.ProcessMultiPointerEvent(pointerEvent, activePointers))
                        {
                            handled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"多点手势识别器 {recognizer.GestureType} 处理失败: {ex.Message}");
                    }
                }
            }

            return handled;
        }

        /// <summary>
        /// 注册默认手势识别器
        /// </summary>
        private void RegisterDefaultRecognizers()
        {
            // 单点手势
            RegisterRecognizer(new TapGestureRecognizer(_configuration));
            RegisterRecognizer(new DragGestureRecognizer(_configuration));
            RegisterRecognizer(new LongPressGestureRecognizer(_configuration));
            RegisterRecognizer(new SwipeGestureRecognizer(_configuration));

            // 多点手势
            RegisterRecognizer(new PinchGestureRecognizer(_configuration));
            RegisterRecognizer(new RotationGestureRecognizer(_configuration));
            RegisterRecognizer(new TwoFingerTapGestureRecognizer(_configuration));

            Debug.WriteLine("默认手势识别器已注册");
        }

        /// <summary>
        /// 注册默认手势处理器
        /// </summary>
        private void RegisterDefaultProcessors()
        {
            AddProcessor(new VelocityProcessor());
            AddProcessor(new AccelerationProcessor());
            AddProcessor(new SmoothingProcessor());

            Debug.WriteLine("默认手势处理器已注册");
        }

        /// <summary>
        /// 处理手势识别事件
        /// </summary>
        private void OnGestureRecognized(object? sender, GestureRecognizedEventArgs e)
        {
            GestureRecognized?.Invoke(this, e);

            // 如果是多点手势，触发多点手势事件
            if (e.Data is IMultiTouchGestureData multiTouchData)
            {
                var multiTouchArgs = new MultiTouchGestureEventArgs(
                    e.GestureType,
                    e.State,
                    e.Position,
                    e.TriggerEvent,
                    multiTouchData);

                MultiTouchGestureRecognized?.Invoke(this, multiTouchArgs);
            }
        }

        /// <summary>
        /// 获取手势统计
        /// </summary>
        /// <returns>手势统计</returns>
        public GestureStatistics GetStatistics()
        {
            var stats = new GestureStatistics
            {
                ActivePointers = _pointerTrackers.Values.Count(t => t.IsActive),
                TotalPointers = _pointerTrackers.Count,
                EnabledRecognizers = _recognizers.Values.Count(r => r.IsEnabled),
                TotalRecognizers = _recognizers.Count,
                ProcessorCount = _processors.Count
            };

            return stats;
        }
    }

    /// <summary>
    /// 多点触控手势事件参数
    /// </summary>
    public class MultiTouchGestureEventArgs : EventArgs
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
        /// 中心位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 触发事件
        /// </summary>
        public PointerEvent TriggerEvent { get; }

        /// <summary>
        /// 多点触控数据
        /// </summary>
        public IMultiTouchGestureData Data { get; }

        /// <summary>
        /// 初始化多点触控手势事件参数
        /// </summary>
        public MultiTouchGestureEventArgs(
            string gestureType,
            GestureState state,
            Point position,
            PointerEvent triggerEvent,
            IMultiTouchGestureData data)
        {
            GestureType = gestureType ?? throw new ArgumentNullException(nameof(gestureType));
            State = state;
            Position = position;
            TriggerEvent = triggerEvent ?? throw new ArgumentNullException(nameof(triggerEvent));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public override string ToString() =>
            $"MultiTouchGesture: {GestureType} {State} at {Position}";
    }

    /// <summary>
    /// 手势统计
    /// </summary>
    public class GestureStatistics
    {
        /// <summary>
        /// 活动指针数
        /// </summary>
        public int ActivePointers { get; set; }

        /// <summary>
        /// 总指针数
        /// </summary>
        public int TotalPointers { get; set; }

        /// <summary>
        /// 启用的识别器数
        /// </summary>
        public int EnabledRecognizers { get; set; }

        /// <summary>
        /// 总识别器数
        /// </summary>
        public int TotalRecognizers { get; set; }

        /// <summary>
        /// 处理器数
        /// </summary>
        public int ProcessorCount { get; set; }

        public override string ToString() =>
            $"Pointers: {ActivePointers}/{TotalPointers}, Recognizers: {EnabledRecognizers}/{TotalRecognizers}, Processors: {ProcessorCount}";
    }
}
