using System;
using System.Collections.Generic;
using AetherUI.Core.Input;

namespace AetherUI.Core
{
    /// <summary>
    /// UI元素的基类，提供布局、渲染和输入处理的基础功能
    /// </summary>
    public abstract class UIElement : DependencyObject, IInputElement
    {
        private readonly List<EventHandlerInfo> _eventHandlers = [];
        private bool _isArrangeValid = false;
        private bool _isMeasureValid = false;

        /// <summary>
        /// 父元素引用，用于事件路由与命中测试
        /// </summary>
        public UIElement? Parent { get; set; }

        #region 依赖属性

        /// <summary>
        /// 可见性依赖属性
        /// </summary>
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register(
            nameof(Visibility), typeof(Visibility), typeof(UIElement),
            new PropertyMetadata(Visibility.Visible, OnVisibilityChanged));

        /// <summary>
        /// 不透明度依赖属性
        /// </summary>
        public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register(
            nameof(Opacity), typeof(double), typeof(UIElement),
            new PropertyMetadata(1.0, OnOpacityChanged));

        /// <summary>
        /// 是否启用依赖属性
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            nameof(IsEnabled), typeof(bool), typeof(UIElement),
            new PropertyMetadata(true, OnIsEnabledChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 可见性
        /// </summary>
        public Visibility Visibility
        {
            get => (Visibility)GetValue(VisibilityProperty);
            set => SetValue(VisibilityProperty, value);
        }

        /// <summary>
        /// 不透明度
        /// </summary>
        public double Opacity
        {
            get => (double)GetValue(OpacityProperty);
            set => SetValue(OpacityProperty, value);
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        /// <summary>
        /// 期望尺寸
        /// </summary>
        public Size DesiredSize { get; private set; } = Size.Empty;

        /// <summary>
        /// 渲染尺寸
        /// </summary>
        public Size RenderSize { get; private set; } = Size.Empty;

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible => Visibility == Visibility.Visible;

        /// <summary>
        /// 布局矩形（元素在父容器中的位置和尺寸）
        /// </summary>
        public Rect LayoutRect { get; private set; } = Rect.Empty;

        /// <summary>
        /// 渲染边界（用于命中测试和输入处理）
        /// </summary>
        public Rect RenderBounds => new Rect(0, 0, RenderSize.Width, RenderSize.Height);

        /// <summary>
        /// 获取视觉子元素集合（用于渲染与命中测试）
        /// </summary>
        /// <returns>子元素枚举</returns>
        public virtual System.Collections.Generic.IEnumerable<UIElement> GetVisualChildren()
        {
            yield break;
        }

        #endregion

        #region 布局方法

        /// <summary>
        /// 测量元素的期望尺寸
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        public void Measure(Size availableSize)
        {
            if (Visibility == Visibility.Collapsed)
            {
                DesiredSize = Size.Empty;
                _isMeasureValid = true;
                return;
            }
            Size measuredSize = MeasureCore(availableSize);
            DesiredSize = measuredSize;
            _isMeasureValid = true;
        }

        /// <summary>
        /// 排列元素到指定矩形
        /// </summary>
        /// <param name="finalRect">最终矩形</param>
        public void Arrange(Rect finalRect)
        {
            if (Visibility == Visibility.Collapsed)
            {
                LayoutRect = Rect.Empty;
                RenderSize = Size.Empty;
                _isArrangeValid = true;
                return;
            }
            LayoutRect = finalRect;
            Size arrangedSize = ArrangeCore(finalRect);
            RenderSize = arrangedSize;
            _isArrangeValid = true;

            // 触发布局更新事件
            OnLayoutUpdated();
        }

        /// <summary>
        /// 使布局无效
        /// </summary>
        public void InvalidateLayout()
        {
            InvalidateMeasure();
            InvalidateArrange();
        }

        /// <summary>
        /// 使测量无效
        /// </summary>
        public void InvalidateMeasure()
        {
            _isMeasureValid = false;
        }

        /// <summary>
        /// 使排列无效
        /// </summary>
        public void InvalidateArrange()
        {
            _isArrangeValid = false;
        }

        /// <summary>
        /// 核心测量方法，子类重写以提供自定义测量逻辑
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected virtual Size MeasureCore(Size availableSize)
        {
            return Size.Empty;
        }

        /// <summary>
        /// 核心排列方法，子类重写以提供自定义排列逻辑
        /// </summary>
        /// <param name="finalRect">最终矩形</param>
        /// <returns>实际尺寸</returns>
        protected virtual Size ArrangeCore(Rect finalRect)
        {
            return finalRect.Size;
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 添加事件处理器
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">事件处理器</param>
        public void AddHandler(object routedEvent, Delegate handler)
        {
            if (routedEvent == null)
            {
                throw new ArgumentNullException(nameof(routedEvent));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _eventHandlers.Add(new EventHandlerInfo(routedEvent, handler));
        }

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">事件处理器</param>
        public void RemoveHandler(object routedEvent, Delegate handler)
        {
            if (routedEvent == null)
            {
                throw new ArgumentNullException(nameof(routedEvent));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            for (int i = _eventHandlers.Count - 1; i >= 0; i--)
            {
                EventHandlerInfo info = _eventHandlers[i];
                if (info.RoutedEvent == routedEvent && info.Handler == handler)
                {
                    _eventHandlers.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="args">事件参数</param>
        public void RaiseEvent(object args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            if (args is RoutedEventArgs routedArgs)
            {
                if (routedArgs.RoutedEvent == null)
                {
                    return;
                }
                foreach (EventHandlerInfo info in _eventHandlers)
                {
                    if (info.RoutedEvent == routedArgs.RoutedEvent)
                    {
                        if (routedArgs.Handled && !info.HandledEventsToo)
                        {
                            continue;
                        }

                        try
                        {
                            _ = info.Handler.DynamicInvoke(this, routedArgs);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        #endregion

        #region 属性更改回调

        private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.InvalidateLayout();
            }
        }

        private static void OnOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement)
            {
            }
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement)
            {
            }
        }

        #endregion

        #region 输入相关属性

        /// <summary>
        /// 是否可命中测试依赖属性
        /// </summary>
        public static readonly DependencyProperty IsHitTestVisibleProperty = DependencyProperty.Register(
            nameof(IsHitTestVisible), typeof(bool), typeof(UIElement),
            new PropertyMetadata(true));

        /// <summary>
        /// 是否可获得焦点依赖属性
        /// </summary>
        public static readonly DependencyProperty FocusableProperty = DependencyProperty.Register(
            nameof(Focusable), typeof(bool), typeof(UIElement),
            new PropertyMetadata(false));

        /// <summary>
        /// 是否可命中测试
        /// </summary>
        public bool IsHitTestVisible
        {
            get => (bool)GetValue(IsHitTestVisibleProperty);
            set => SetValue(IsHitTestVisibleProperty, value);
        }

        /// <summary>
        /// 是否可获得焦点
        /// </summary>
        public bool Focusable
        {
            get => (bool)GetValue(FocusableProperty);
            set => SetValue(FocusableProperty, value);
        }

        /// <summary>
        /// 是否有焦点
        /// </summary>
        public bool IsFocused { get; private set; }

        #endregion

        #region 输入事件

        /// <summary>
        /// 布局更新事件
        /// </summary>
        public event EventHandler? LayoutUpdated;

        /// <summary>
        /// 鼠标进入事件
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseEnter;

        /// <summary>
        /// 鼠标离开事件
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseLeave;

        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public event EventHandler<MouseButtonEventArgs>? MouseDown;

        /// <summary>
        /// 鼠标抬起事件
        /// </summary>
        public event EventHandler<MouseButtonEventArgs>? MouseUp;

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public event EventHandler<MouseEventArgs>? MouseMove;

        /// <summary>
        /// 键盘按下事件
        /// </summary>
        public event EventHandler<KeyEventArgs>? KeyDown;

        /// <summary>
        /// 键盘抬起事件
        /// </summary>
        public event EventHandler<KeyEventArgs>? KeyUp;

        /// <summary>
        /// 获得焦点事件
        /// </summary>
        public event EventHandler<FocusEventArgs>? GotFocus;

        /// <summary>
        /// 失去焦点事件
        /// </summary>
        public event EventHandler<FocusEventArgs>? LostFocus;

        /// <summary>
        /// 触发布局更新事件
        /// </summary>
        protected virtual void OnLayoutUpdated()
        {
            LayoutUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 设置焦点状态
        /// </summary>
        /// <param name="focused">是否有焦点</param>
        internal void SetFocused(bool focused)
        {
            if (IsFocused != focused)
            {
                IsFocused = focused;
                if (focused)
                {
                    GotFocus?.Invoke(this, new FocusEventArgs());
                }
                else
                {
                    LostFocus?.Invoke(this, new FocusEventArgs());
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 可见性枚举
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// 可见
        /// </summary>
        Visible,

        /// <summary>
        /// 隐藏但占用空间
        /// </summary>
        Hidden,

        /// <summary>
        /// 折叠，不占用空间
        /// </summary>
        Collapsed
    }
}
