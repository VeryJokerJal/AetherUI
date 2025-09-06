using System;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 滚动视图控件
    /// </summary>
    public class ScrollViewer : FrameworkElement
    {
        #region 私有字段

        /// <summary>
        /// 防止递归更新的标志
        /// </summary>
        private bool _isUpdatingScrollBars = false;

        #endregion

        #region 依赖属性

        /// <summary>
        /// 内容依赖属性
        /// </summary>
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), typeof(UIElement), typeof(ScrollViewer),
            new PropertyMetadata(null, OnContentChanged));

        /// <summary>
        /// 垂直滚动条可见性依赖属性
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(
            nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ScrollViewer),
            new PropertyMetadata(ScrollBarVisibility.Auto, OnScrollBarVisibilityChanged));

        /// <summary>
        /// 水平滚动条可见性依赖属性
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(
            nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ScrollViewer),
            new PropertyMetadata(ScrollBarVisibility.Auto, OnScrollBarVisibilityChanged));

        /// <summary>
        /// 垂直偏移依赖属性
        /// </summary>
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            nameof(VerticalOffset), typeof(double), typeof(ScrollViewer),
            new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        /// <summary>
        /// 水平偏移依赖属性
        /// </summary>
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(
            nameof(HorizontalOffset), typeof(double), typeof(ScrollViewer),
            new PropertyMetadata(0.0, OnHorizontalOffsetChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 内容
        /// </summary>
        public UIElement? Content
        {
            get => (UIElement?)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>
        /// 垂直滚动条可见性
        /// </summary>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }

        /// <summary>
        /// 水平滚动条可见性
        /// </summary>
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
            set => SetValue(HorizontalScrollBarVisibilityProperty, value);
        }

        /// <summary>
        /// 垂直偏移
        /// </summary>
        public double VerticalOffset
        {
            get => (double)GetValue(VerticalOffsetProperty);
            set => SetValue(VerticalOffsetProperty, value);
        }

        /// <summary>
        /// 水平偏移
        /// </summary>
        public double HorizontalOffset
        {
            get => (double)GetValue(HorizontalOffsetProperty);
            set => SetValue(HorizontalOffsetProperty, value);
        }

        /// <summary>
        /// 垂直滚动条
        /// </summary>
        public ScrollBar VerticalScrollBar { get; private set; }

        /// <summary>
        /// 水平滚动条
        /// </summary>
        public ScrollBar HorizontalScrollBar { get; private set; }

        /// <summary>
        /// 内容区域
        /// </summary>
        public Rect ContentRect { get; private set; }

        /// <summary>
        /// 视口区域
        /// </summary>
        public Rect ViewportRect { get; private set; }

        /// <summary>
        /// 内容尺寸
        /// </summary>
        public Size ContentSize { get; private set; }

        /// <summary>
        /// 可滚动内容尺寸
        /// </summary>
        public Size ScrollableSize { get; private set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化滚动视图
        /// </summary>
        public ScrollViewer()
        {
            // 创建滚动条
            VerticalScrollBar = new ScrollBar
            {
                Orientation = Orientation.Vertical,
                Visibility = Visibility.Collapsed
            };
            VerticalScrollBar.ValueChanged += OnVerticalScrollBarValueChanged;

            HorizontalScrollBar = new ScrollBar
            {
                Orientation = Orientation.Horizontal,
                Visibility = Visibility.Collapsed
            };
            HorizontalScrollBar.ValueChanged += OnHorizontalScrollBarValueChanged;

            // 设置默认尺寸
            Width = 200;
            Height = 200;
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
            Size desiredSize = availableSize;
            double scrollBarSize = 16;

            // 测量内容
            if (Content != null)
            {
                // 给内容无限空间进行测量，以获取其自然尺寸
                Content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                ContentSize = Content.DesiredSize;

                Debug.WriteLine($"ScrollViewer: ContentSize = {ContentSize}, AvailableSize = {availableSize}");
            }
            else
            {
                ContentSize = Size.Empty;
            }

            // 测量滚动条
            VerticalScrollBar.Measure(new Size(scrollBarSize, availableSize.Height));
            HorizontalScrollBar.Measure(new Size(availableSize.Width, scrollBarSize));

            return desiredSize;
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

        #region 滚动方法

        /// <summary>
        /// 滚动到指定位置
        /// </summary>
        /// <param name="horizontalOffset">水平偏移</param>
        /// <param name="verticalOffset">垂直偏移</param>
        public void ScrollToOffset(double horizontalOffset, double verticalOffset)
        {
            HorizontalOffset = Math.Max(0, Math.Min(ScrollableSize.Width, horizontalOffset));
            VerticalOffset = Math.Max(0, Math.Min(ScrollableSize.Height, verticalOffset));
        }

        /// <summary>
        /// 向上滚动
        /// </summary>
        public void ScrollUp()
        {
            VerticalOffset = Math.Max(0, VerticalOffset - VerticalScrollBar.SmallChange);
        }

        /// <summary>
        /// 向下滚动
        /// </summary>
        public void ScrollDown()
        {
            VerticalOffset = Math.Min(ScrollableSize.Height, VerticalOffset + VerticalScrollBar.SmallChange);
        }

        /// <summary>
        /// 向左滚动
        /// </summary>
        public void ScrollLeft()
        {
            HorizontalOffset = Math.Max(0, HorizontalOffset - HorizontalScrollBar.SmallChange);
        }

        /// <summary>
        /// 向右滚动
        /// </summary>
        public void ScrollRight()
        {
            HorizontalOffset = Math.Min(ScrollableSize.Width, HorizontalOffset + HorizontalScrollBar.SmallChange);
        }

        /// <summary>
        /// 页面向上滚动
        /// </summary>
        public void PageUp()
        {
            VerticalOffset = Math.Max(0, VerticalOffset - VerticalScrollBar.LargeChange);
        }

        /// <summary>
        /// 页面向下滚动
        /// </summary>
        public void PageDown()
        {
            VerticalOffset = Math.Min(ScrollableSize.Height, VerticalOffset + VerticalScrollBar.LargeChange);
        }

        /// <summary>
        /// 鼠标滚轮滚动
        /// </summary>
        /// <param name="delta">滚轮增量</param>
        public void ScrollByWheel(double delta)
        {
            double scrollAmount = delta * VerticalScrollBar.SmallChange * 3; // 滚轮滚动倍数
            VerticalOffset = Math.Max(0, Math.Min(ScrollableSize.Height, VerticalOffset - scrollAmount));
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 更新布局
        /// </summary>
        private void UpdateLayout()
        {
            // 防止递归更新
            if (_isUpdatingScrollBars)
                return;

            _isUpdatingScrollBars = true;
            try
            {
                double scrollBarSize = 16;

                // 先计算不考虑滚动条的视口尺寸
                double baseViewportWidth = RenderSize.Width;
                double baseViewportHeight = RenderSize.Height;

                // 判断是否需要滚动条（考虑滚动条占用空间的循环依赖）
                bool needVerticalScrollBar = ShouldShowVerticalScrollBar(baseViewportWidth, baseViewportHeight);
                bool needHorizontalScrollBar = ShouldShowHorizontalScrollBar(baseViewportWidth, baseViewportHeight);

                // 如果需要垂直滚动条，重新检查是否需要水平滚动条
                if (needVerticalScrollBar && !needHorizontalScrollBar)
                {
                    needHorizontalScrollBar = ShouldShowHorizontalScrollBar(baseViewportWidth - scrollBarSize, baseViewportHeight);
                }

                // 如果需要水平滚动条，重新检查是否需要垂直滚动条
                if (needHorizontalScrollBar && !needVerticalScrollBar)
                {
                    needVerticalScrollBar = ShouldShowVerticalScrollBar(baseViewportWidth, baseViewportHeight - scrollBarSize);
                }

                // 计算最终视口区域
                double viewportWidth = baseViewportWidth - (needVerticalScrollBar ? scrollBarSize : 0);
                double viewportHeight = baseViewportHeight - (needHorizontalScrollBar ? scrollBarSize : 0);
                ViewportRect = new Rect(0, 0, viewportWidth, viewportHeight);

                // 计算可滚动尺寸
                ScrollableSize = new Size(
                    Math.Max(0, ContentSize.Width - viewportWidth),
                    Math.Max(0, ContentSize.Height - viewportHeight));

                Debug.WriteLine($"ScrollViewer: ContentSize={ContentSize}, ViewportSize=({viewportWidth},{viewportHeight}), ScrollableSize={ScrollableSize}");

                // 约束偏移量在有效范围内
                double maxHorizontalOffset = Math.Max(0, ScrollableSize.Width);
                double maxVerticalOffset = Math.Max(0, ScrollableSize.Height);

                if (HorizontalOffset > maxHorizontalOffset)
                {
                    HorizontalOffset = maxHorizontalOffset;
                }
                if (VerticalOffset > maxVerticalOffset)
                {
                    VerticalOffset = maxVerticalOffset;
                }

                // 排列内容
                if (Content != null)
                {
                    ContentRect = new Rect(-HorizontalOffset, -VerticalOffset, ContentSize.Width, ContentSize.Height);
                    Content.Arrange(ContentRect);
                }

                // 排列滚动条
                if (needVerticalScrollBar)
                {
                    VerticalScrollBar.Visibility = Visibility.Visible;
                    VerticalScrollBar.Maximum = Math.Max(0, ScrollableSize.Height);
                    VerticalScrollBar.ViewportSize = viewportHeight;
                    VerticalScrollBar.Value = VerticalOffset;

                    Rect vScrollRect = new Rect(RenderSize.Width - scrollBarSize, 0, scrollBarSize,
                        RenderSize.Height - (needHorizontalScrollBar ? scrollBarSize : 0));
                    VerticalScrollBar.Arrange(vScrollRect);

                    Debug.WriteLine($"VerticalScrollBar: Max={VerticalScrollBar.Maximum}, ViewportSize={VerticalScrollBar.ViewportSize}, Value={VerticalScrollBar.Value}");
                }
                else
                {
                    VerticalScrollBar.Visibility = Visibility.Collapsed;
                }

                if (needHorizontalScrollBar)
                {
                    HorizontalScrollBar.Visibility = Visibility.Visible;
                    HorizontalScrollBar.Maximum = Math.Max(0, ScrollableSize.Width);
                    HorizontalScrollBar.ViewportSize = viewportWidth;
                    HorizontalScrollBar.Value = HorizontalOffset;

                    Rect hScrollRect = new Rect(0, RenderSize.Height - scrollBarSize,
                        RenderSize.Width - (needVerticalScrollBar ? scrollBarSize : 0), scrollBarSize);
                    HorizontalScrollBar.Arrange(hScrollRect);

                    Debug.WriteLine($"HorizontalScrollBar: Max={HorizontalScrollBar.Maximum}, ViewportSize={HorizontalScrollBar.ViewportSize}, Value={HorizontalScrollBar.Value}");
                }
                else
                {
                    HorizontalScrollBar.Visibility = Visibility.Collapsed;
                }
            }
            finally
            {
                _isUpdatingScrollBars = false;
            }
        }

        /// <summary>
        /// 判断是否需要显示垂直滚动条
        /// </summary>
        private bool ShouldShowVerticalScrollBar()
        {
            return ShouldShowVerticalScrollBar(RenderSize.Width, RenderSize.Height);
        }

        /// <summary>
        /// 判断是否需要显示垂直滚动条
        /// </summary>
        /// <param name="availableWidth">可用宽度</param>
        /// <param name="availableHeight">可用高度</param>
        private bool ShouldShowVerticalScrollBar(double availableWidth, double availableHeight)
        {
            return VerticalScrollBarVisibility switch
            {
                ScrollBarVisibility.Visible => true,
                ScrollBarVisibility.Hidden => false,
                ScrollBarVisibility.Disabled => false,
                ScrollBarVisibility.Auto => ContentSize.Height > availableHeight,
                _ => false
            };
        }

        /// <summary>
        /// 判断是否需要显示水平滚动条
        /// </summary>
        private bool ShouldShowHorizontalScrollBar()
        {
            return ShouldShowHorizontalScrollBar(RenderSize.Width, RenderSize.Height);
        }

        /// <summary>
        /// 判断是否需要显示水平滚动条
        /// </summary>
        /// <param name="availableWidth">可用宽度</param>
        /// <param name="availableHeight">可用高度</param>
        private bool ShouldShowHorizontalScrollBar(double availableWidth, double availableHeight)
        {
            return HorizontalScrollBarVisibility switch
            {
                ScrollBarVisibility.Visible => true,
                ScrollBarVisibility.Hidden => false,
                ScrollBarVisibility.Disabled => false,
                ScrollBarVisibility.Auto => ContentSize.Width > availableWidth,
                _ => false
            };
        }

        #endregion



        #region 事件处理

        /// <summary>
        /// 垂直滚动条值变化处理
        /// </summary>
        private void OnVerticalScrollBarValueChanged(object? sender, double value)
        {
            // 防止递归更新
            if (!_isUpdatingScrollBars)
            {
                VerticalOffset = value;
            }
        }

        /// <summary>
        /// 水平滚动条值变化处理
        /// </summary>
        private void OnHorizontalScrollBarValueChanged(object? sender, double value)
        {
            // 防止递归更新
            if (!_isUpdatingScrollBars)
            {
                HorizontalOffset = value;
            }
        }

        #endregion

        #region 依赖属性回调

        /// <summary>
        /// 内容变化回调
        /// </summary>
        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                if (e.OldValue is UIElement oldContent)
                {
                    oldContent.Parent = null;
                }

                if (e.NewValue is UIElement newContent)
                {
                    newContent.Parent = scrollViewer;
                }

                scrollViewer.InvalidateLayout();
            }
        }

        /// <summary>
        /// 滚动条可见性变化回调
        /// </summary>
        private static void OnScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.InvalidateLayout();
            }
        }

        /// <summary>
        /// 垂直偏移变化回调
        /// </summary>
        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.UpdateLayout();
            }
        }

        /// <summary>
        /// 水平偏移变化回调
        /// </summary>
        private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.UpdateLayout();
            }
        }

        #endregion
    }
}
