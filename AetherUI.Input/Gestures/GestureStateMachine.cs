using System;
using System.Collections.Generic;
using System.Diagnostics;
using AetherUI.Input.Events;

namespace AetherUI.Input.Gestures
{
    /// <summary>
    /// 手势状态机
    /// </summary>
    public class GestureStateMachine
    {
        private readonly Dictionary<string, GestureStateInfo> _gestureStates = new();
        private readonly List<IGestureStateTransition> _transitions = new();

        /// <summary>
        /// 状态变化事件
        /// </summary>
        public event EventHandler<GestureStateChangedEventArgs>? StateChanged;

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        public void ProcessEvent(PointerEvent pointerEvent)
        {
            try
            {
                // 检查所有转换
                foreach (var transition in _transitions)
                {
                    if (transition.CanTransition(pointerEvent, _gestureStates))
                    {
                        var oldState = GetGestureState(transition.GestureType);
                        var newState = transition.GetNextState(pointerEvent, oldState);

                        if (newState != oldState.State)
                        {
                            UpdateGestureState(transition.GestureType, newState);
                            
                            var eventArgs = new GestureStateChangedEventArgs(
                                transition.GestureType,
                                oldState.State,
                                newState,
                                pointerEvent);

                            StateChanged?.Invoke(this, eventArgs);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"手势状态机处理事件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 注册转换
        /// </summary>
        /// <param name="transition">转换</param>
        public void RegisterTransition(IGestureStateTransition transition)
        {
            if (transition != null && !_transitions.Contains(transition))
            {
                _transitions.Add(transition);
            }
        }

        /// <summary>
        /// 注销转换
        /// </summary>
        /// <param name="transition">转换</param>
        public void UnregisterTransition(IGestureStateTransition transition)
        {
            _transitions.Remove(transition);
        }

        /// <summary>
        /// 获取手势状态
        /// </summary>
        /// <param name="gestureType">手势类型</param>
        /// <returns>手势状态信息</returns>
        public GestureStateInfo GetGestureState(string gestureType)
        {
            if (_gestureStates.TryGetValue(gestureType, out GestureStateInfo stateInfo))
            {
                return stateInfo;
            }

            return new GestureStateInfo(GestureState.Ended, DateTime.UtcNow);
        }

        /// <summary>
        /// 设置手势状态
        /// </summary>
        /// <param name="gestureType">手势类型</param>
        /// <param name="state">状态</param>
        public void SetGestureState(string gestureType, GestureState state)
        {
            UpdateGestureState(gestureType, state);
        }

        /// <summary>
        /// 重置所有状态
        /// </summary>
        public void Reset()
        {
            _gestureStates.Clear();
        }

        /// <summary>
        /// 更新手势状态
        /// </summary>
        private void UpdateGestureState(string gestureType, GestureState state)
        {
            _gestureStates[gestureType] = new GestureStateInfo(state, DateTime.UtcNow);
        }

        /// <summary>
        /// 获取所有手势状态
        /// </summary>
        /// <returns>所有手势状态</returns>
        public IReadOnlyDictionary<string, GestureStateInfo> GetAllStates()
        {
            return _gestureStates;
        }
    }

    /// <summary>
    /// 手势状态信息
    /// </summary>
    public readonly struct GestureStateInfo
    {
        /// <summary>
        /// 状态
        /// </summary>
        public GestureState State { get; }

        /// <summary>
        /// 状态时间
        /// </summary>
        public DateTime StateTime { get; }

        /// <summary>
        /// 状态持续时间
        /// </summary>
        public TimeSpan Duration => DateTime.UtcNow - StateTime;

        /// <summary>
        /// 初始化手势状态信息
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="stateTime">状态时间</param>
        public GestureStateInfo(GestureState state, DateTime stateTime)
        {
            State = state;
            StateTime = stateTime;
        }

        public override string ToString() => $"{State} ({Duration.TotalMilliseconds:F0}ms)";
    }

    /// <summary>
    /// 手势状态变化事件参数
    /// </summary>
    public class GestureStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 手势类型
        /// </summary>
        public string GestureType { get; }

        /// <summary>
        /// 旧状态
        /// </summary>
        public GestureState OldState { get; }

        /// <summary>
        /// 新状态
        /// </summary>
        public GestureState NewState { get; }

        /// <summary>
        /// 触发事件
        /// </summary>
        public PointerEvent TriggerEvent { get; }

        /// <summary>
        /// 初始化手势状态变化事件参数
        /// </summary>
        /// <param name="gestureType">手势类型</param>
        /// <param name="oldState">旧状态</param>
        /// <param name="newState">新状态</param>
        /// <param name="triggerEvent">触发事件</param>
        public GestureStateChangedEventArgs(string gestureType, GestureState oldState, GestureState newState, PointerEvent triggerEvent)
        {
            GestureType = gestureType ?? throw new ArgumentNullException(nameof(gestureType));
            OldState = oldState;
            NewState = newState;
            TriggerEvent = triggerEvent ?? throw new ArgumentNullException(nameof(triggerEvent));
        }

        public override string ToString() => $"{GestureType}: {OldState} -> {NewState}";
    }

    /// <summary>
    /// 手势状态转换接口
    /// </summary>
    public interface IGestureStateTransition
    {
        /// <summary>
        /// 手势类型
        /// </summary>
        string GestureType { get; }

        /// <summary>
        /// 检查是否可以转换
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <param name="currentStates">当前状态</param>
        /// <returns>是否可以转换</returns>
        bool CanTransition(PointerEvent pointerEvent, IReadOnlyDictionary<string, GestureStateInfo> currentStates);

        /// <summary>
        /// 获取下一个状态
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <param name="currentState">当前状态</param>
        /// <returns>下一个状态</returns>
        GestureState GetNextState(PointerEvent pointerEvent, GestureStateInfo currentState);
    }

    /// <summary>
    /// 点击手势状态转换
    /// </summary>
    public class TapGestureStateTransition : IGestureStateTransition
    {
        /// <summary>
        /// 手势类型
        /// </summary>
        public string GestureType => GestureTypes.Tap;

        /// <summary>
        /// 检查是否可以转换
        /// </summary>
        public bool CanTransition(PointerEvent pointerEvent, IReadOnlyDictionary<string, GestureStateInfo> currentStates)
        {
            return pointerEvent.PointerId.DeviceType == Core.InputDeviceType.Mouse ||
                   pointerEvent.PointerId.DeviceType == Core.InputDeviceType.Touch;
        }

        /// <summary>
        /// 获取下一个状态
        /// </summary>
        public GestureState GetNextState(PointerEvent pointerEvent, GestureStateInfo currentState)
        {
            switch (pointerEvent.EventType)
            {
                case PointerEventType.Pressed:
                    return GestureState.Started;

                case PointerEventType.Released:
                    return currentState.State == GestureState.Started ? GestureState.Ended : currentState.State;

                case PointerEventType.Cancelled:
                    return GestureState.Cancelled;

                default:
                    return currentState.State;
            }
        }
    }

    /// <summary>
    /// 拖拽手势状态转换
    /// </summary>
    public class DragGestureStateTransition : IGestureStateTransition
    {
        private readonly double _dragThreshold;

        /// <summary>
        /// 手势类型
        /// </summary>
        public string GestureType => GestureTypes.Drag;

        /// <summary>
        /// 初始化拖拽手势状态转换
        /// </summary>
        /// <param name="dragThreshold">拖拽阈值</param>
        public DragGestureStateTransition(double dragThreshold = 10.0)
        {
            _dragThreshold = dragThreshold;
        }

        /// <summary>
        /// 检查是否可以转换
        /// </summary>
        public bool CanTransition(PointerEvent pointerEvent, IReadOnlyDictionary<string, GestureStateInfo> currentStates)
        {
            return true; // 拖拽可以应用于所有指针类型
        }

        /// <summary>
        /// 获取下一个状态
        /// </summary>
        public GestureState GetNextState(PointerEvent pointerEvent, GestureStateInfo currentState)
        {
            switch (pointerEvent.EventType)
            {
                case PointerEventType.Pressed:
                    return GestureState.Started;

                case PointerEventType.Moved:
                    // 这里需要更复杂的逻辑来检查移动距离
                    // 简化实现，假设移动就是拖拽
                    return currentState.State == GestureState.Started ? GestureState.Updated : currentState.State;

                case PointerEventType.Released:
                    return GestureState.Ended;

                case PointerEventType.Cancelled:
                    return GestureState.Cancelled;

                default:
                    return currentState.State;
            }
        }
    }

    /// <summary>
    /// 状态转换工厂
    /// </summary>
    public static class GestureStateTransitionFactory
    {
        /// <summary>
        /// 创建默认转换
        /// </summary>
        /// <returns>默认转换集合</returns>
        public static IEnumerable<IGestureStateTransition> CreateDefaultTransitions()
        {
            yield return new TapGestureStateTransition();
            yield return new DragGestureStateTransition();
        }

        /// <summary>
        /// 创建点击转换
        /// </summary>
        /// <returns>点击转换</returns>
        public static TapGestureStateTransition CreateTapTransition()
        {
            return new TapGestureStateTransition();
        }

        /// <summary>
        /// 创建拖拽转换
        /// </summary>
        /// <param name="dragThreshold">拖拽阈值</param>
        /// <returns>拖拽转换</returns>
        public static DragGestureStateTransition CreateDragTransition(double dragThreshold = 10.0)
        {
            return new DragGestureStateTransition(dragThreshold);
        }
    }
}
