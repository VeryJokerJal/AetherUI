using System;
using System.Collections.Generic;
using AetherUI.Input.Core;
using AetherUI.Input.Events;

namespace AetherUI.Input.Gestures
{
    /// <summary>
    /// 手势识别器接口
    /// </summary>
    public interface IGestureRecognizer
    {
        /// <summary>
        /// 手势类型名称
        /// </summary>
        string GestureType { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 手势识别事件
        /// </summary>
        event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 处理指针事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>是否处理了事件</returns>
        bool ProcessPointerEvent(PointerEvent pointerEvent);

        /// <summary>
        /// 重置识别器状态
        /// </summary>
        void Reset();

        /// <summary>
        /// 获取当前手势状态
        /// </summary>
        /// <returns>手势状态</returns>
        GestureState GetCurrentState();
    }

    /// <summary>
    /// 单点手势识别器接口
    /// </summary>
    public interface ISinglePointerGestureRecognizer : IGestureRecognizer
    {
        // 继承基础接口，专门用于单点手势
    }

    /// <summary>
    /// 多点手势识别器接口
    /// </summary>
    public interface IMultiPointerGestureRecognizer : IGestureRecognizer
    {
        /// <summary>
        /// 处理多点指针事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <param name="activePointers">活动指针</param>
        /// <returns>是否处理了事件</returns>
        bool ProcessMultiPointerEvent(PointerEvent pointerEvent, System.Collections.Generic.IReadOnlyList<PointerTracker> activePointers);
    }

    /// <summary>
    /// 手势识别事件参数
    /// </summary>
    public class GestureRecognizedEventArgs : EventArgs
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
        /// 触发的指针事件
        /// </summary>
        public PointerEvent TriggerEvent { get; }

        /// <summary>
        /// 初始化手势识别事件参数
        /// </summary>
        /// <param name="gestureType">手势类型</param>
        /// <param name="state">手势状态</param>
        /// <param name="position">手势位置</param>
        /// <param name="triggerEvent">触发的指针事件</param>
        /// <param name="data">手势数据</param>
        public GestureRecognizedEventArgs(
            string gestureType,
            GestureState state,
            Point position,
            PointerEvent triggerEvent,
            object? data = null)
        {
            GestureType = gestureType ?? throw new ArgumentNullException(nameof(gestureType));
            State = state;
            Position = position;
            TriggerEvent = triggerEvent ?? throw new ArgumentNullException(nameof(triggerEvent));
            Data = data;
        }

        public override string ToString() =>
            $"GestureRecognized: {GestureType} {State} at {Position}";
    }

    /// <summary>
    /// 点击手势数据
    /// </summary>
    public class TapGestureData
    {
        /// <summary>
        /// 点击次数
        /// </summary>
        public int TapCount { get; }

        /// <summary>
        /// 点击位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 按钮
        /// </summary>
        public PointerButton Button { get; }

        /// <summary>
        /// 初始化点击手势数据
        /// </summary>
        /// <param name="tapCount">点击次数</param>
        /// <param name="position">点击位置</param>
        /// <param name="button">按钮</param>
        public TapGestureData(int tapCount, Point position, PointerButton button)
        {
            TapCount = tapCount;
            Position = position;
            Button = button;
        }

        public override string ToString() => $"Tap: {TapCount}x at {Position} ({Button})";
    }

    /// <summary>
    /// 拖拽手势数据
    /// </summary>
    public class DragGestureData
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        public Point StartPosition { get; }

        /// <summary>
        /// 当前位置
        /// </summary>
        public Point CurrentPosition { get; }

        /// <summary>
        /// 累计偏移
        /// </summary>
        public Point TotalOffset { get; }

        /// <summary>
        /// 增量偏移
        /// </summary>
        public Point DeltaOffset { get; }

        /// <summary>
        /// 拖拽速度（像素/秒）
        /// </summary>
        public Point Velocity { get; }

        /// <summary>
        /// 按钮
        /// </summary>
        public PointerButton Button { get; }

        /// <summary>
        /// 初始化拖拽手势数据
        /// </summary>
        /// <param name="startPosition">起始位置</param>
        /// <param name="currentPosition">当前位置</param>
        /// <param name="deltaOffset">增量偏移</param>
        /// <param name="velocity">拖拽速度</param>
        /// <param name="button">按钮</param>
        public DragGestureData(
            Point startPosition,
            Point currentPosition,
            Point deltaOffset,
            Point velocity,
            PointerButton button)
        {
            StartPosition = startPosition;
            CurrentPosition = currentPosition;
            TotalOffset = currentPosition - startPosition;
            DeltaOffset = deltaOffset;
            Velocity = velocity;
            Button = button;
        }

        public override string ToString() =>
            $"Drag: from {StartPosition} to {CurrentPosition}, delta {DeltaOffset}, velocity {Velocity}";
    }

    /// <summary>
    /// 捏合手势数据
    /// </summary>
    public class PinchGestureData
    {
        /// <summary>
        /// 中心点
        /// </summary>
        public Point Center { get; }

        /// <summary>
        /// 缩放比例
        /// </summary>
        public float Scale { get; }

        /// <summary>
        /// 增量缩放
        /// </summary>
        public float DeltaScale { get; }

        /// <summary>
        /// 旋转角度（弧度）
        /// </summary>
        public float Rotation { get; }

        /// <summary>
        /// 增量旋转
        /// </summary>
        public float DeltaRotation { get; }

        /// <summary>
        /// 两指距离
        /// </summary>
        public float Distance { get; }

        /// <summary>
        /// 初始化捏合手势数据
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="deltaScale">增量缩放</param>
        /// <param name="rotation">旋转角度</param>
        /// <param name="deltaRotation">增量旋转</param>
        /// <param name="distance">两指距离</param>
        public PinchGestureData(
            Point center,
            float scale,
            float deltaScale,
            float rotation,
            float deltaRotation,
            float distance)
        {
            Center = center;
            Scale = scale;
            DeltaScale = deltaScale;
            Rotation = rotation;
            DeltaRotation = deltaRotation;
            Distance = distance;
        }

        public override string ToString() =>
            $"Pinch: center {Center}, scale {Scale:F2}, rotation {Rotation:F2}°";
    }

    /// <summary>
    /// 长按手势数据
    /// </summary>
    public class LongPressGestureData
    {
        /// <summary>
        /// 长按位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 长按持续时间（毫秒）
        /// </summary>
        public int Duration { get; }

        /// <summary>
        /// 按钮
        /// </summary>
        public PointerButton Button { get; }

        /// <summary>
        /// 初始化长按手势数据
        /// </summary>
        /// <param name="position">长按位置</param>
        /// <param name="duration">长按持续时间</param>
        /// <param name="button">按钮</param>
        public LongPressGestureData(Point position, int duration, PointerButton button)
        {
            Position = position;
            Duration = duration;
            Button = button;
        }

        public override string ToString() => $"LongPress: at {Position} for {Duration}ms ({Button})";
    }

    /// <summary>
    /// 手势管理器接口
    /// </summary>
    public interface IGestureManager
    {
        /// <summary>
        /// 手势识别事件
        /// </summary>
        event EventHandler<GestureRecognizedEventArgs>? GestureRecognized;

        /// <summary>
        /// 注册手势识别器
        /// </summary>
        /// <param name="recognizer">手势识别器</param>
        void RegisterRecognizer(IGestureRecognizer recognizer);

        /// <summary>
        /// 注销手势识别器
        /// </summary>
        /// <param name="recognizer">手势识别器</param>
        void UnregisterRecognizer(IGestureRecognizer recognizer);

        /// <summary>
        /// 获取手势识别器
        /// </summary>
        /// <param name="gestureType">手势类型</param>
        /// <returns>手势识别器</returns>
        IGestureRecognizer? GetRecognizer(string gestureType);

        /// <summary>
        /// 获取所有手势识别器
        /// </summary>
        /// <returns>手势识别器集合</returns>
        IEnumerable<IGestureRecognizer> GetAllRecognizers();

        /// <summary>
        /// 处理指针事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>是否有手势识别器处理了事件</returns>
        bool ProcessPointerEvent(PointerEvent pointerEvent);

        /// <summary>
        /// 重置所有手势识别器
        /// </summary>
        void ResetAll();

        /// <summary>
        /// 启用/禁用手势类型
        /// </summary>
        /// <param name="gestureType">手势类型</param>
        /// <param name="enabled">是否启用</param>
        void SetGestureEnabled(string gestureType, bool enabled);

        /// <summary>
        /// 检查手势类型是否启用
        /// </summary>
        /// <param name="gestureType">手势类型</param>
        /// <returns>是否启用</returns>
        bool IsGestureEnabled(string gestureType);
    }

    /// <summary>
    /// 手势配置
    /// </summary>
    public class GestureConfiguration
    {
        /// <summary>
        /// 点击超时时间（毫秒）
        /// </summary>
        public int TapTimeoutMs { get; set; } = 300;

        /// <summary>
        /// 双击间隔时间（毫秒）
        /// </summary>
        public int DoubleTapIntervalMs { get; set; } = 500;

        /// <summary>
        /// 长按延迟时间（毫秒）
        /// </summary>
        public int LongPressDelayMs { get; set; } = 1000;

        /// <summary>
        /// 拖拽阈值（像素）
        /// </summary>
        public double DragThreshold { get; set; } = 5.0;

        /// <summary>
        /// 捏合阈值（像素）
        /// </summary>
        public double PinchThreshold { get; set; } = 10.0;

        /// <summary>
        /// 最大点击移动距离（像素）
        /// </summary>
        public double MaxTapMovement { get; set; } = 10.0;

        /// <summary>
        /// 速度计算时间窗口（毫秒）
        /// </summary>
        public int VelocityWindowMs { get; set; } = 100;

        /// <summary>
        /// 默认配置
        /// </summary>
        public static GestureConfiguration Default { get; } = new();
    }

    /// <summary>
    /// 手势常量
    /// </summary>
    public static class GestureTypes
    {
        /// <summary>
        /// 点击
        /// </summary>
        public const string Tap = "Tap";

        /// <summary>
        /// 双击
        /// </summary>
        public const string DoubleTap = "DoubleTap";

        /// <summary>
        /// 长按
        /// </summary>
        public const string LongPress = "LongPress";

        /// <summary>
        /// 拖拽
        /// </summary>
        public const string Drag = "Drag";

        /// <summary>
        /// 捏合
        /// </summary>
        public const string Pinch = "Pinch";

        /// <summary>
        /// 平移
        /// </summary>
        public const string Pan = "Pan";

        /// <summary>
        /// 旋转
        /// </summary>
        public const string Rotation = "Rotation";

        /// <summary>
        /// 滑动
        /// </summary>
        public const string Swipe = "Swipe";
    }
}
