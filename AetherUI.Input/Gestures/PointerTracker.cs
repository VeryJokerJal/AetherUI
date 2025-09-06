using System;
using System.Collections.Generic;
using System.Linq;
using AetherUI.Input.Core;

namespace AetherUI.Input.Gestures
{
    /// <summary>
    /// 指针跟踪器
    /// </summary>
    public class PointerTracker
    {
        private readonly List<PointerPoint> _points = new();
        private readonly int _maxPoints;

        /// <summary>
        /// 指针ID
        /// </summary>
        public PointerId PointerId { get; }

        /// <summary>
        /// 是否活动
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 起始位置
        /// </summary>
        public Point StartPosition { get; }

        /// <summary>
        /// 当前位置
        /// </summary>
        public Point CurrentPosition => _points.Count > 0 ? _points[^1].Position : StartPosition;

        /// <summary>
        /// 起始时间
        /// </summary>
        public uint StartTime { get; }

        /// <summary>
        /// 当前时间
        /// </summary>
        public uint CurrentTime => _points.Count > 0 ? _points[^1].Timestamp : StartTime;

        /// <summary>
        /// 总移动距离
        /// </summary>
        public double TotalDistance { get; private set; }

        /// <summary>
        /// 直线距离
        /// </summary>
        public double StraightDistance => CalculateDistance(StartPosition, CurrentPosition);

        /// <summary>
        /// 持续时间（毫秒）
        /// </summary>
        public uint Duration => CurrentTime - StartTime;

        /// <summary>
        /// 平均速度（像素/秒）
        /// </summary>
        public double AverageVelocity => Duration > 0 ? TotalDistance / (Duration / 1000.0) : 0;

        /// <summary>
        /// 当前速度（像素/秒）
        /// </summary>
        public double CurrentVelocity => CalculateCurrentVelocity();

        /// <summary>
        /// 所有点
        /// </summary>
        public IReadOnlyList<PointerPoint> Points => _points;

        /// <summary>
        /// 初始化指针跟踪器
        /// </summary>
        /// <param name="pointerId">指针ID</param>
        /// <param name="startPosition">起始位置</param>
        /// <param name="startTime">起始时间</param>
        /// <param name="maxPoints">最大点数</param>
        public PointerTracker(PointerId pointerId, Point startPosition, uint startTime, int maxPoints = 100)
        {
            PointerId = pointerId;
            StartPosition = startPosition;
            StartTime = startTime;
            _maxPoints = maxPoints;

            // 添加起始点
            _points.Add(new PointerPoint(startPosition, startTime));
        }

        /// <summary>
        /// 添加点
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="timestamp">时间戳</param>
        public void AddPoint(Point position, uint timestamp)
        {
            if (_points.Count > 0)
            {
                var lastPoint = _points[^1];
                var distance = CalculateDistance(lastPoint.Position, position);
                TotalDistance += distance;
            }

            _points.Add(new PointerPoint(position, timestamp));

            // 限制点数，移除最旧的点
            if (_points.Count > _maxPoints)
            {
                _points.RemoveAt(0);
            }
        }

        /// <summary>
        /// 获取最近的点
        /// </summary>
        /// <param name="count">点数</param>
        /// <returns>最近的点</returns>
        public IEnumerable<PointerPoint> GetRecentPoints(int count)
        {
            var startIndex = Math.Max(0, _points.Count - count);
            return _points.Skip(startIndex);
        }

        /// <summary>
        /// 获取指定时间范围内的点
        /// </summary>
        /// <param name="timeRangeMs">时间范围（毫秒）</param>
        /// <returns>时间范围内的点</returns>
        public IEnumerable<PointerPoint> GetPointsInTimeRange(uint timeRangeMs)
        {
            var cutoffTime = CurrentTime - timeRangeMs;
            return _points.Where(p => p.Timestamp >= cutoffTime);
        }

        /// <summary>
        /// 计算平滑位置
        /// </summary>
        /// <param name="windowSize">窗口大小</param>
        /// <returns>平滑位置</returns>
        public Point GetSmoothedPosition(int windowSize = 5)
        {
            var recentPoints = GetRecentPoints(windowSize).ToList();
            if (recentPoints.Count == 0)
                return CurrentPosition;

            var avgX = recentPoints.Average(p => p.Position.X);
            var avgY = recentPoints.Average(p => p.Position.Y);
            return new Point(avgX, avgY);
        }

        /// <summary>
        /// 计算加速度
        /// </summary>
        /// <returns>加速度（像素/秒²）</returns>
        public double CalculateAcceleration()
        {
            var recentPoints = GetRecentPoints(3).ToList();
            if (recentPoints.Count < 3)
                return 0;

            var v1 = CalculateVelocity(recentPoints[0], recentPoints[1]);
            var v2 = CalculateVelocity(recentPoints[1], recentPoints[2]);
            var dt = (recentPoints[2].Timestamp - recentPoints[1].Timestamp) / 1000.0;

            return dt > 0 ? (v2 - v1) / dt : 0;
        }

        /// <summary>
        /// 预测下一个位置
        /// </summary>
        /// <param name="deltaTimeMs">预测时间（毫秒）</param>
        /// <returns>预测位置</returns>
        public Point PredictPosition(uint deltaTimeMs)
        {
            var velocity = GetVelocityVector();
            var deltaTime = deltaTimeMs / 1000.0;

            return new Point(
                CurrentPosition.X + velocity.X * deltaTime,
                CurrentPosition.Y + velocity.Y * deltaTime);
        }

        /// <summary>
        /// 获取速度向量
        /// </summary>
        /// <returns>速度向量</returns>
        public Point GetVelocityVector()
        {
            var recentPoints = GetRecentPoints(2).ToList();
            if (recentPoints.Count < 2)
                return Point.Zero;

            var p1 = recentPoints[0];
            var p2 = recentPoints[1];
            var dt = (p2.Timestamp - p1.Timestamp) / 1000.0;

            if (dt <= 0)
                return Point.Zero;

            return new Point(
                (p2.Position.X - p1.Position.X) / dt,
                (p2.Position.Y - p1.Position.Y) / dt);
        }

        /// <summary>
        /// 计算当前速度
        /// </summary>
        private double CalculateCurrentVelocity()
        {
            var recentPoints = GetRecentPoints(2).ToList();
            if (recentPoints.Count < 2)
                return 0;

            return CalculateVelocity(recentPoints[0], recentPoints[1]);
        }

        /// <summary>
        /// 计算两点间速度
        /// </summary>
        private double CalculateVelocity(PointerPoint p1, PointerPoint p2)
        {
            var distance = CalculateDistance(p1.Position, p2.Position);
            var time = (p2.Timestamp - p1.Timestamp) / 1000.0;
            return time > 0 ? distance / time : 0;
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

        public override string ToString() =>
            $"Pointer {PointerId}: {StartPosition} -> {CurrentPosition}, Distance={TotalDistance:F1}, Velocity={CurrentVelocity:F1}";
    }

    /// <summary>
    /// 指针点
    /// </summary>
    public readonly struct PointerPoint
    {
        /// <summary>
        /// 位置
        /// </summary>
        public Point Position { get; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public uint Timestamp { get; }

        /// <summary>
        /// 初始化指针点
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="timestamp">时间戳</param>
        public PointerPoint(Point position, uint timestamp)
        {
            Position = position;
            Timestamp = timestamp;
        }

        public override string ToString() => $"{Position} @ {Timestamp}";
    }

    /// <summary>
    /// 手势处理器接口
    /// </summary>
    public interface IGestureProcessor
    {
        /// <summary>
        /// 处理指针事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <param name="activePointers">活动指针</param>
        void ProcessPointerEvent(PointerEvent pointerEvent, IEnumerable<PointerTracker> activePointers);
    }

    /// <summary>
    /// 速度处理器
    /// </summary>
    public class VelocityProcessor : IGestureProcessor
    {
        /// <summary>
        /// 处理指针事件
        /// </summary>
        public void ProcessPointerEvent(PointerEvent pointerEvent, IEnumerable<PointerTracker> activePointers)
        {
            // 可以在这里实现速度相关的处理逻辑
            // 例如：速度阈值检查、速度平滑等
        }
    }

    /// <summary>
    /// 加速度处理器
    /// </summary>
    public class AccelerationProcessor : IGestureProcessor
    {
        /// <summary>
        /// 处理指针事件
        /// </summary>
        public void ProcessPointerEvent(PointerEvent pointerEvent, IEnumerable<PointerTracker> activePointers)
        {
            // 可以在这里实现加速度相关的处理逻辑
            // 例如：加速度计算、惯性预测等
        }
    }

    /// <summary>
    /// 平滑处理器
    /// </summary>
    public class SmoothingProcessor : IGestureProcessor
    {
        private readonly int _windowSize;

        /// <summary>
        /// 初始化平滑处理器
        /// </summary>
        /// <param name="windowSize">平滑窗口大小</param>
        public SmoothingProcessor(int windowSize = 5)
        {
            _windowSize = windowSize;
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        public void ProcessPointerEvent(PointerEvent pointerEvent, IEnumerable<PointerTracker> activePointers)
        {
            // 可以在这里实现位置平滑逻辑
            // 例如：移动平均、卡尔曼滤波等
        }
    }
}
