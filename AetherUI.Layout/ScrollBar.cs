using System;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 滚动条可见性
    /// </summary>
    public enum ScrollBarVisibility
    {
        /// <summary>
        /// 自动显示/隐藏
        /// </summary>
        Auto,

        /// <summary>
        /// 始终可见
        /// </summary>
        Visible,

        /// <summary>
        /// 始终隐藏
        /// </summary>
        Hidden,

        /// <summary>
        /// 禁用
        /// </summary>
        Disabled
    }

    /// <summary>
    /// 滚动条控件
    /// </summary>
    public class ScrollBar : FrameworkElement
    {
        #region 依赖属性

        /// <summary>
        /// 方向依赖属性
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation), typeof(Orientation), typeof(ScrollBar),
            new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

        /// <summary>
        /// 当前值依赖属性
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(double), typeof(ScrollBar),
            new PropertyMetadata(0.0, OnValueChanged));

        /// <summary>
        /// 最小值依赖属性
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double), typeof(ScrollBar),
            new PropertyMetadata(0.0, OnRangeChanged));

        /// <summary>
        /// 最大值依赖属性
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double), typeof(ScrollBar),
            new PropertyMetadata(100.0, OnRangeChanged));

        /// <summary>
        /// 视口大小依赖属性
        /// </summary>
        public static readonly DependencyProperty ViewportSizeProperty = DependencyProperty.Register(
            nameof(ViewportSize), typeof(double), typeof(ScrollBar),
            new PropertyMetadata(10.0, OnViewportSizeChanged));

        /// <summary>
        /// 小步长依赖属性
        /// </summary>
        public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register(
            nameof(SmallChange), typeof(double), typeof(ScrollBar),
            new PropertyMetadata(1.0));

        /// <summary>
        /// 大步长依赖属性
        /// </summary>
        public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register(
            nameof(LargeChange), typeof(double), typeof(ScrollBar),
            new PropertyMetadata(10.0));

        #endregion

        #region 属性

        /// <summary>
        /// 滚动条方向
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// 当前值
        /// </summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, CoerceValue(value));
        }

        /// <summary>
        /// 最小值
        /// </summary>
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        /// <summary>
        /// 最大值
        /// </summary>
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        /// <summary>
        /// 视口大小
        /// </summary>
        public double ViewportSize
        {
            get => (double)GetValue(ViewportSizeProperty);
            set => SetValue(ViewportSizeProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 小步长
        /// </summary>
        public double SmallChange
        {
            get => (double)GetValue(SmallChangeProperty);
            set => SetValue(SmallChangeProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 大步长
        /// </summary>
        public double LargeChange
        {
            get => (double)GetValue(LargeChangeProperty);
            set => SetValue(LargeChangeProperty, Math.Max(0, value));
        }

        /// <summary>
        /// 滑块矩形区域
        /// </summary>
        public Rect ThumbRect { get; private set; }

        /// <summary>
        /// 轨道矩形区域
        /// </summary>
        public Rect TrackRect { get; private set; }

        /// <summary>
        /// 上/左箭头按钮矩形区域
        /// </summary>
        public Rect UpButtonRect { get; private set; }

        /// <summary>
        /// 下/右箭头按钮矩形区域
        /// </summary>
        public Rect DownButtonRect { get; private set; }

        /// <summary>
        /// 是否正在拖拽滑块
        /// </summary>
        public bool IsDragging { get; set; }

        /// <summary>
        /// 拖拽起始位置
        /// </summary>
        public Point DragStartPoint { get; set; }

        /// <summary>
        /// 拖拽起始值
        /// </summary>
        public double DragStartValue { get; set; }

        #endregion

        #region 事件

        /// <summary>
        /// 值变化事件
        /// </summary>
        public event EventHandler<double>? ValueChanged;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化滚动条
        /// </summary>
        public ScrollBar()
        {
            Width = 16; // 默认宽度
            Height = 100; // 默认高度
        }

        #endregion

        #region 布局

        /// <summary>
        /// 测量控件尺寸
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected override Size MeasureCore(Size availableSize)
        {
            return Orientation == Orientation.Vertical
                ? new Size(16, Math.Min(availableSize.Height, 100))
                : new Size(Math.Min(availableSize.Width, 100), 16);
        }

        /// <summary>
        /// 排列控件
        /// </summary>
        /// <param name="finalRect">最终矩形</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeCore(Rect finalRect)
        {
            UpdateLayout();
            return finalRect.Size;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 更新布局
        /// </summary>
        private void UpdateLayout()
        {
            double buttonSize = Orientation == Orientation.Vertical ? RenderSize.Width : RenderSize.Height;

            if (Orientation == Orientation.Vertical)
            {
                // 垂直滚动条
                UpButtonRect = new Rect(0, 0, RenderSize.Width, buttonSize);
                DownButtonRect = new Rect(0, RenderSize.Height - buttonSize, RenderSize.Width, buttonSize);

                double trackHeight = RenderSize.Height - (2 * buttonSize);
                TrackRect = new Rect(0, buttonSize, RenderSize.Width, trackHeight);

                // 计算滑块位置和大小
                UpdateThumbRect();
            }
            else
            {
                // 水平滚动条
                UpButtonRect = new Rect(0, 0, buttonSize, RenderSize.Height);
                DownButtonRect = new Rect(RenderSize.Width - buttonSize, 0, buttonSize, RenderSize.Height);

                double trackWidth = RenderSize.Width - (2 * buttonSize);
                TrackRect = new Rect(buttonSize, 0, trackWidth, RenderSize.Height);

                // 计算滑块位置和大小
                UpdateThumbRect();
            }
        }

        /// <summary>
        /// 更新滑块矩形
        /// </summary>
        private void UpdateThumbRect()
        {
            double range = Maximum - Minimum;
            if (range <= 0 || ViewportSize <= 0)
            {
                ThumbRect = Rect.Empty;
                return;
            }

            if (Orientation == Orientation.Vertical)
            {
                double thumbHeight = Math.Max(20, TrackRect.Height * ViewportSize / (range + ViewportSize));
                double availableHeight = TrackRect.Height - thumbHeight;
                double thumbY = TrackRect.Y + (availableHeight * (Value - Minimum) / range);

                ThumbRect = new Rect(TrackRect.X, thumbY, TrackRect.Width, thumbHeight);
            }
            else
            {
                double thumbWidth = Math.Max(20, TrackRect.Width * ViewportSize / (range + ViewportSize));
                double availableWidth = TrackRect.Width - thumbWidth;
                double thumbX = TrackRect.X + (availableWidth * (Value - Minimum) / range);

                ThumbRect = new Rect(thumbX, TrackRect.Y, thumbWidth, TrackRect.Height);
            }
        }

        /// <summary>
        /// 约束值在有效范围内
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>约束后的值</returns>
        private double CoerceValue(double value)
        {
            return Math.Max(Minimum, Math.Min(Maximum, value));
        }

        #endregion

        #region 依赖属性回调

        /// <summary>
        /// 方向变化回调
        /// </summary>
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollBar scrollBar)
            {
                scrollBar.InvalidateLayout();
            }
        }

        /// <summary>
        /// 值变化回调
        /// </summary>
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollBar scrollBar)
            {
                scrollBar.UpdateThumbRect();
                scrollBar.ValueChanged?.Invoke(scrollBar, (double)e.NewValue);
            }
        }

        /// <summary>
        /// 范围变化回调
        /// </summary>
        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollBar scrollBar)
            {
                // 使用内部方法设置值，避免触发属性变化回调导致无限递归
                double coercedValue = scrollBar.CoerceValue(scrollBar.Value);
                if (coercedValue != scrollBar.Value)
                {
                    scrollBar.SetValue(ValueProperty, coercedValue);
                }
                scrollBar.UpdateThumbRect();
            }
        }

        /// <summary>
        /// 视口大小变化回调
        /// </summary>
        private static void OnViewportSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollBar scrollBar)
            {
                scrollBar.UpdateThumbRect();
            }
        }

        #endregion
    }
}
