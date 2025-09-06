using System;
using System.Collections.Generic;
using System.Diagnostics;
using AetherUI.Events;

namespace AetherUI.Core
{
    /// <summary>
    /// UI元素的基类，提供布局、渲染和输入处理的基础功能
    /// </summary>
    public abstract class UIElement : DependencyObject, IInputElement
    {
        private readonly List<EventHandlerInfo> _eventHandlers = new List<EventHandlerInfo>();
        private Size _desiredSize = Size.Empty;
        private Size _renderSize = Size.Empty;
        private Rect _arrangeRect = Rect.Empty;
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
        public Size DesiredSize => _desiredSize;

        /// <summary>
        /// 渲染尺寸
        /// </summary>
        public Size RenderSize => _renderSize;

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible => Visibility == Visibility.Visible;

        /// <summary>
        /// 布局矩形（元素在父容器中的位置和尺寸）
        /// </summary>
        public Rect LayoutRect => _arrangeRect;

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
                _desiredSize = Size.Empty;
                _isMeasureValid = true;
                return;
            }

            Debug.WriteLine($"Measuring {GetType().Name} with available size: {availableSize}");

            Size measuredSize = MeasureCore(availableSize);
            _desiredSize = measuredSize;
            _isMeasureValid = true;

            Debug.WriteLine($"Measured {GetType().Name} desired size: {_desiredSize}");
        }

        /// <summary>
        /// 排列元素到指定矩形
        /// </summary>
        /// <param name="finalRect">最终矩形</param>
        public void Arrange(Rect finalRect)
        {
            if (Visibility == Visibility.Collapsed)
            {
                _arrangeRect = Rect.Empty;
                _renderSize = Size.Empty;
                _isArrangeValid = true;
                return;
            }

            Debug.WriteLine($"Arranging {GetType().Name} to rect: {finalRect}");

            _arrangeRect = finalRect;
            Size arrangedSize = ArrangeCore(finalRect);
            _renderSize = arrangedSize;
            _isArrangeValid = true;

            Debug.WriteLine($"Arranged {GetType().Name} render size: {_renderSize}");
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
            Debug.WriteLine($"Invalidated measure for {GetType().Name}");
        }

        /// <summary>
        /// 使排列无效
        /// </summary>
        public void InvalidateArrange()
        {
            _isArrangeValid = false;
            Debug.WriteLine($"Invalidated arrange for {GetType().Name}");
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
                throw new ArgumentNullException(nameof(routedEvent));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _eventHandlers.Add(new EventHandlerInfo(routedEvent, handler));
            Debug.WriteLine($"Added handler for {routedEvent} to {GetType().Name}");
        }

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">事件处理器</param>
        public void RemoveHandler(object routedEvent, Delegate handler)
        {
            if (routedEvent == null)
                throw new ArgumentNullException(nameof(routedEvent));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            for (int i = _eventHandlers.Count - 1; i >= 0; i--)
            {
                EventHandlerInfo info = _eventHandlers[i];
                if (info.RoutedEvent == routedEvent && info.Handler == handler)
                {
                    _eventHandlers.RemoveAt(i);
                    Debug.WriteLine($"Removed handler for {routedEvent} from {GetType().Name}");
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
                throw new ArgumentNullException(nameof(args));

            if (args is RoutedEventArgs routedArgs)
            {
                if (routedArgs.RoutedEvent == null)
                    return;

                Debug.WriteLine($"Raising event {routedArgs.RoutedEvent} on {GetType().Name}");

                foreach (EventHandlerInfo info in _eventHandlers)
                {
                    if (info.RoutedEvent == routedArgs.RoutedEvent)
                    {
                        if (routedArgs.Handled && !info.HandledEventsToo)
                            continue;

                        try
                        {
                            info.Handler.DynamicInvoke(this, routedArgs);
                            Debug.WriteLine($"Invoked handler for {routedArgs.RoutedEvent}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error invoking handler: {ex.Message}");
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
                Debug.WriteLine($"Visibility changed for {element.GetType().Name}: {e.NewValue}");
            }
        }

        private static void OnOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                Debug.WriteLine($"Opacity changed for {element.GetType().Name}: {e.NewValue}");
            }
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                Debug.WriteLine($"IsEnabled changed for {element.GetType().Name}: {e.NewValue}");
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
