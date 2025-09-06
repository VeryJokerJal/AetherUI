using System;
using System.Collections.Generic;
using AetherUI.Input.Core;

namespace AetherUI.Input.Gestures
{
    /// <summary>
    /// 多点触控手势数据接口
    /// </summary>
    public interface IMultiTouchGestureData
    {
        /// <summary>
        /// 触控点数量
        /// </summary>
        int TouchCount { get; }

        /// <summary>
        /// 中心位置
        /// </summary>
        Point Center { get; }

        /// <summary>
        /// 初始中心位置
        /// </summary>
        Point InitialCenter { get; }
    }

    /// <summary>
    /// 捏合手势数据
    /// </summary>
    public class PinchGestureData : IMultiTouchGestureData
    {
        /// <summary>
        /// 触控点数量
        /// </summary>
        public int TouchCount => 2;

        /// <summary>
        /// 中心位置
        /// </summary>
        public Point Center { get; }

        /// <summary>
        /// 初始中心位置
        /// </summary>
        public Point InitialCenter { get; }

        /// <summary>
        /// 初始距离
        /// </summary>
        public double InitialDistance { get; }

        /// <summary>
        /// 缩放比例
        /// </summary>
        public double Scale { get; }

        /// <summary>
        /// 缩放增量
        /// </summary>
        public double ScaleDelta => Scale - 1.0;

        /// <summary>
        /// 初始化捏合手势数据
        /// </summary>
        /// <param name="initialCenter">初始中心位置</param>
        /// <param name="center">当前中心位置</param>
        /// <param name="initialDistance">初始距离</param>
        /// <param name="scale">缩放比例</param>
        public PinchGestureData(Point initialCenter, Point center, double initialDistance, double scale)
        {
            InitialCenter = initialCenter;
            Center = center;
            InitialDistance = initialDistance;
            Scale = scale;
        }

        public override string ToString() =>
            $"Pinch: Scale={Scale:F2}, Center={Center}, InitialDistance={InitialDistance:F1}";
    }

    /// <summary>
    /// 旋转手势数据
    /// </summary>
    public class RotationGestureData : IMultiTouchGestureData
    {
        /// <summary>
        /// 触控点数量
        /// </summary>
        public int TouchCount => 2;

        /// <summary>
        /// 中心位置
        /// </summary>
        public Point Center { get; }

        /// <summary>
        /// 初始中心位置
        /// </summary>
        public Point InitialCenter { get; }

        /// <summary>
        /// 旋转角度（弧度）
        /// </summary>
        public double Rotation { get; }

        /// <summary>
        /// 旋转角度（度）
        /// </summary>
        public double RotationDegrees => Rotation * 180.0 / Math.PI;

        /// <summary>
        /// 初始化旋转手势数据
        /// </summary>
        /// <param name="initialCenter">初始中心位置</param>
        /// <param name="center">当前中心位置</param>
        /// <param name="rotation">旋转角度（弧度）</param>
        public RotationGestureData(Point initialCenter, Point center, double rotation)
        {
            InitialCenter = initialCenter;
            Center = center;
            Rotation = rotation;
        }

        public override string ToString() =>
            $"Rotation: {RotationDegrees:F1}°, Center={Center}";
    }

    /// <summary>
    /// 双指点击手势数据
    /// </summary>
    public class TwoFingerTapGestureData : IMultiTouchGestureData
    {
        /// <summary>
        /// 触控点数量
        /// </summary>
        public int TouchCount => 2;

        /// <summary>
        /// 中心位置
        /// </summary>
        public Point Center { get; }

        /// <summary>
        /// 初始中心位置
        /// </summary>
        public Point InitialCenter => Center;

        /// <summary>
        /// 初始化双指点击手势数据
        /// </summary>
        /// <param name="center">中心位置</param>
        public TwoFingerTapGestureData(Point center)
        {
            Center = center;
        }

        public override string ToString() =>
            $"TwoFingerTap: Center={Center}";
    }

    /// <summary>
    /// 滑动方向
    /// </summary>
    public enum SwipeDirection
    {
        /// <summary>
        /// 向上
        /// </summary>
        Up,

        /// <summary>
        /// 向下
        /// </summary>
        Down,

        /// <summary>
        /// 向左
        /// </summary>
        Left,

        /// <summary>
        /// 向右
        /// </summary>
        Right
    }

    /// <summary>
    /// 滑动手势数据
    /// </summary>
    public class SwipeGestureData
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        public Point StartPosition { get; }

        /// <summary>
        /// 结束位置
        /// </summary>
        public Point EndPosition { get; }

        /// <summary>
        /// 滑动方向
        /// </summary>
        public SwipeDirection Direction { get; }

        /// <summary>
        /// 滑动距离
        /// </summary>
        public double Distance { get; }

        /// <summary>
        /// 滑动速度（像素/秒）
        /// </summary>
        public double Velocity { get; }

        /// <summary>
        /// 滑动向量
        /// </summary>
        public Point Vector => new Point(EndPosition.X - StartPosition.X, EndPosition.Y - StartPosition.Y);

        /// <summary>
        /// 初始化滑动手势数据
        /// </summary>
        /// <param name="startPosition">起始位置</param>
        /// <param name="endPosition">结束位置</param>
        /// <param name="direction">滑动方向</param>
        /// <param name="distance">滑动距离</param>
        /// <param name="velocity">滑动速度</param>
        public SwipeGestureData(Point startPosition, Point endPosition, SwipeDirection direction, double distance, double velocity)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Direction = direction;
            Distance = distance;
            Velocity = velocity;
        }

        public override string ToString() =>
            $"Swipe: {Direction}, Distance={Distance:F1}, Velocity={Velocity:F1}px/s";
    }

    /// <summary>
    /// 手势类型常量
    /// </summary>
    public static class GestureTypes
    {
        /// <summary>
        /// 点击手势
        /// </summary>
        public const string Tap = "Tap";

        /// <summary>
        /// 双击手势
        /// </summary>
        public const string DoubleTap = "DoubleTap";

        /// <summary>
        /// 长按手势
        /// </summary>
        public const string LongPress = "LongPress";

        /// <summary>
        /// 拖拽手势
        /// </summary>
        public const string Drag = "Drag";

        /// <summary>
        /// 捏合手势
        /// </summary>
        public const string Pinch = "Pinch";

        /// <summary>
        /// 旋转手势
        /// </summary>
        public const string Rotation = "Rotation";

        /// <summary>
        /// 滑动手势
        /// </summary>
        public const string Swipe = "Swipe";

        /// <summary>
        /// 平移手势
        /// </summary>
        public const string Pan = "Pan";

        /// <summary>
        /// 双指点击手势
        /// </summary>
        public const string TwoFingerTap = "TwoFingerTap";

        /// <summary>
        /// 三指点击手势
        /// </summary>
        public const string ThreeFingerTap = "ThreeFingerTap";

        /// <summary>
        /// 双指滑动手势
        /// </summary>
        public const string TwoFingerSwipe = "TwoFingerSwipe";
    }

    /// <summary>
    /// 手势配置扩展
    /// </summary>
    public partial class GestureConfiguration
    {
        /// <summary>
        /// 捏合阈值（百分比）
        /// </summary>
        public double PinchThreshold { get; set; } = 5.0;

        /// <summary>
        /// 旋转阈值（弧度）
        /// </summary>
        public double RotationThreshold { get; set; } = Math.PI / 36; // 5度

        /// <summary>
        /// 滑动最小距离
        /// </summary>
        public double SwipeMinDistance { get; set; } = 50.0;

        /// <summary>
        /// 滑动最小速度（像素/秒）
        /// </summary>
        public double SwipeMinVelocity { get; set; } = 100.0;

        /// <summary>
        /// 多点触控超时时间（毫秒）
        /// </summary>
        public uint MultiTouchTimeoutMs { get; set; } = 500;

        /// <summary>
        /// 最大同时触控点数
        /// </summary>
        public int MaxSimultaneousPointers { get; set; } = 10;
    }
}
