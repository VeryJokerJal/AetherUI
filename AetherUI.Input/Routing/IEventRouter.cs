using System;
using System.Collections.Generic;
using AetherUI.Input.Events;
using AetherUI.Input.HitTesting;

namespace AetherUI.Input.Routing
{
    /// <summary>
    /// 路由策略
    /// </summary>
    public enum RoutingStrategy
    {
        /// <summary>
        /// 隧道：从根元素向下传播到事件源（Preview事件）
        /// </summary>
        Tunnel,

        /// <summary>
        /// 直接：只在事件源上触发
        /// </summary>
        Direct,

        /// <summary>
        /// 冒泡：从事件源向上传播到根元素
        /// </summary>
        Bubble
    }

    /// <summary>
    /// 路由事件定义
    /// </summary>
    public class RoutedEventDefinition
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 路由策略
        /// </summary>
        public RoutingStrategy RoutingStrategy { get; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public Type EventType { get; }

        /// <summary>
        /// 处理器类型
        /// </summary>
        public Type HandlerType { get; }

        /// <summary>
        /// 所有者类型
        /// </summary>
        public Type OwnerType { get; }

        /// <summary>
        /// 全局索引
        /// </summary>
        public int GlobalIndex { get; }

        /// <summary>
        /// 初始化路由事件定义
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="routingStrategy">路由策略</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="handlerType">处理器类型</param>
        /// <param name="ownerType">所有者类型</param>
        /// <param name="globalIndex">全局索引</param>
        public RoutedEventDefinition(
            string name,
            RoutingStrategy routingStrategy,
            Type eventType,
            Type handlerType,
            Type ownerType,
            int globalIndex)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            RoutingStrategy = routingStrategy;
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
            HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
            OwnerType = ownerType ?? throw new ArgumentNullException(nameof(ownerType));
            GlobalIndex = globalIndex;
        }

        public override string ToString() => $"{OwnerType.Name}.{Name} ({RoutingStrategy})";
    }

    /// <summary>
    /// 路由事件参数
    /// </summary>
    public class RoutedEventArgs : EventArgs
    {
        /// <summary>
        /// 路由事件定义
        /// </summary>
        public RoutedEventDefinition? RoutedEvent { get; set; }

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
        /// 即使已处理也继续路由
        /// </summary>
        public bool HandledToo { get; set; }

        /// <summary>
        /// 输入事件
        /// </summary>
        public InputEvent? InputEvent { get; set; }

        /// <summary>
        /// 初始化路由事件参数
        /// </summary>
        public RoutedEventArgs()
        {
        }

        /// <summary>
        /// 初始化路由事件参数
        /// </summary>
        /// <param name="routedEvent">路由事件定义</param>
        /// <param name="source">事件源</param>
        public RoutedEventArgs(RoutedEventDefinition routedEvent, object source)
        {
            RoutedEvent = routedEvent;
            Source = source;
            OriginalSource = source;
        }
    }

    /// <summary>
    /// 事件处理器委托
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

    /// <summary>
    /// 事件路由接口
    /// </summary>
    public interface IEventRouter
    {
        /// <summary>
        /// 路由事件
        /// </summary>
        /// <param name="target">目标元素</param>
        /// <param name="eventArgs">事件参数</param>
        /// <param name="hitPath">命中路径</param>
        void RouteEvent(object target, RoutedEventArgs eventArgs, IReadOnlyList<IHitTestable>? hitPath = null);

        /// <summary>
        /// 注册路由事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="routingStrategy">路由策略</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="handlerType">处理器类型</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>路由事件定义</returns>
        RoutedEventDefinition RegisterRoutedEvent(
            string name,
            RoutingStrategy routingStrategy,
            Type eventType,
            Type handlerType,
            Type ownerType);

        /// <summary>
        /// 获取路由事件定义
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>路由事件定义</returns>
        RoutedEventDefinition? GetRoutedEvent(string name, Type ownerType);

        /// <summary>
        /// 添加事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        /// <param name="handledEventsToo">是否处理已处理的事件</param>
        void AddHandler(object element, RoutedEventDefinition routedEvent, Delegate handler, bool handledEventsToo = false);

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        void RemoveHandler(object element, RoutedEventDefinition routedEvent, Delegate handler);
    }

    /// <summary>
    /// 事件路由路径
    /// </summary>
    public class EventRoute
    {
        /// <summary>
        /// 路由路径
        /// </summary>
        public IReadOnlyList<object> Path { get; }

        /// <summary>
        /// 路由策略
        /// </summary>
        public RoutingStrategy Strategy { get; }

        /// <summary>
        /// 事件源索引
        /// </summary>
        public int SourceIndex { get; }

        /// <summary>
        /// 初始化事件路由路径
        /// </summary>
        /// <param name="path">路由路径</param>
        /// <param name="strategy">路由策略</param>
        /// <param name="sourceIndex">事件源索引</param>
        public EventRoute(IReadOnlyList<object> path, RoutingStrategy strategy, int sourceIndex)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Strategy = strategy;
            SourceIndex = sourceIndex;
        }

        /// <summary>
        /// 获取路由顺序
        /// </summary>
        /// <returns>路由顺序的元素集合</returns>
        public IEnumerable<object> GetRoutingOrder()
        {
            return Strategy switch
            {
                RoutingStrategy.Tunnel => GetTunnelOrder(),
                RoutingStrategy.Direct => GetDirectOrder(),
                RoutingStrategy.Bubble => GetBubbleOrder(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private IEnumerable<object> GetTunnelOrder()
        {
            // 从根到源（不包括源）
            for (int i = 0; i < SourceIndex; i++)
            {
                yield return Path[i];
            }
        }

        private IEnumerable<object> GetDirectOrder()
        {
            // 只有源元素
            if (SourceIndex >= 0 && SourceIndex < Path.Count)
            {
                yield return Path[SourceIndex];
            }
        }

        private IEnumerable<object> GetBubbleOrder()
        {
            // 从源到根
            for (int i = SourceIndex; i >= 0; i--)
            {
                yield return Path[i];
            }
        }
    }

    /// <summary>
    /// 事件处理器信息
    /// </summary>
    public class EventHandlerInfo
    {
        /// <summary>
        /// 处理器委托
        /// </summary>
        public Delegate Handler { get; }

        /// <summary>
        /// 是否处理已处理的事件
        /// </summary>
        public bool HandledEventsToo { get; }

        /// <summary>
        /// 初始化事件处理器信息
        /// </summary>
        /// <param name="handler">处理器委托</param>
        /// <param name="handledEventsToo">是否处理已处理的事件</param>
        public EventHandlerInfo(Delegate handler, bool handledEventsToo)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            HandledEventsToo = handledEventsToo;
        }
    }

    /// <summary>
    /// 事件处理器存储接口
    /// </summary>
    public interface IEventHandlerStore
    {
        /// <summary>
        /// 添加事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        /// <param name="handledEventsToo">是否处理已处理的事件</param>
        void AddHandler(object element, RoutedEventDefinition routedEvent, Delegate handler, bool handledEventsToo);

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        void RemoveHandler(object element, RoutedEventDefinition routedEvent, Delegate handler);

        /// <summary>
        /// 获取事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <returns>处理器集合</returns>
        IEnumerable<EventHandlerInfo> GetHandlers(object element, RoutedEventDefinition routedEvent);

        /// <summary>
        /// 清除元素的所有处理器
        /// </summary>
        /// <param name="element">元素</param>
        void ClearHandlers(object element);
    }

    /// <summary>
    /// 路由事件管理器接口
    /// </summary>
    public interface IRoutedEventManager
    {
        /// <summary>
        /// 注册路由事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="routingStrategy">路由策略</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="handlerType">处理器类型</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>路由事件定义</returns>
        RoutedEventDefinition RegisterEvent(
            string name,
            RoutingStrategy routingStrategy,
            Type eventType,
            Type handlerType,
            Type ownerType);

        /// <summary>
        /// 获取路由事件定义
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>路由事件定义</returns>
        RoutedEventDefinition? GetEvent(string name, Type ownerType);

        /// <summary>
        /// 获取所有注册的事件
        /// </summary>
        /// <returns>所有路由事件定义</returns>
        IEnumerable<RoutedEventDefinition> GetAllEvents();
    }
}
