using System;

namespace AetherUI.Core
{
    /// <summary>
    /// 输入元素接口，支持事件处理
    /// </summary>
    public interface IInputElement
    {
        /// <summary>
        /// 添加事件处理器
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">事件处理器</param>
        void AddHandler(object routedEvent, Delegate handler);

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">事件处理器</param>
        void RemoveHandler(object routedEvent, Delegate handler);

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="args">事件参数</param>
        void RaiseEvent(object args);
    }

    /// <summary>
    /// 事件处理器信息
    /// </summary>
    public class EventHandlerInfo
    {
        /// <summary>
        /// 路由事件
        /// </summary>
        public object RoutedEvent { get; }

        /// <summary>
        /// 事件处理器
        /// </summary>
        public Delegate Handler { get; }

        /// <summary>
        /// 是否处理已处理的事件
        /// </summary>
        public bool HandledEventsToo { get; }

        /// <summary>
        /// 初始化事件处理器信息
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">事件处理器</param>
        /// <param name="handledEventsToo">是否处理已处理的事件</param>
        public EventHandlerInfo(object routedEvent, Delegate handler, bool handledEventsToo = false)
        {
            RoutedEvent = routedEvent ?? throw new ArgumentNullException(nameof(routedEvent));
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            HandledEventsToo = handledEventsToo;
        }
    }

    /// <summary>
    /// 基础路由事件参数
    /// </summary>
    public class RoutedEventArgs : EventArgs
    {
        /// <summary>
        /// 路由事件
        /// </summary>
        public object? RoutedEvent { get; set; }

        /// <summary>
        /// 事件源
        /// </summary>
        public object? Source { get; set; }

        /// <summary>
        /// 原始事件源
        /// </summary>
        public object? OriginalSource { get; set; }

        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// 初始化路由事件参数
        /// </summary>
        public RoutedEventArgs()
        {
        }

        /// <summary>
        /// 初始化路由事件参数
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        public RoutedEventArgs(object routedEvent)
        {
            RoutedEvent = routedEvent;
        }

        /// <summary>
        /// 初始化路由事件参数
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="source">事件源</param>
        public RoutedEventArgs(object routedEvent, object source)
        {
            RoutedEvent = routedEvent;
            Source = source;
            OriginalSource = source;
        }
    }
}
