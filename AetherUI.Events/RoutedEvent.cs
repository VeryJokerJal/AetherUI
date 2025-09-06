using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Events
{
    /// <summary>
    /// 路由策略枚举
    /// </summary>
    public enum RoutingStrategy
    {
        /// <summary>
        /// 冒泡：从事件源向上传播到根元素
        /// </summary>
        Bubble,

        /// <summary>
        /// 隧道：从根元素向下传播到事件源
        /// </summary>
        Tunnel,

        /// <summary>
        /// 直接：只在事件源上触发
        /// </summary>
        Direct
    }

    /// <summary>
    /// 路由事件类，类似于WPF的RoutedEvent
    /// </summary>
    public sealed class RoutedEvent
    {
        private static readonly ConcurrentDictionary<string, RoutedEvent> _registeredEvents = new();
        private static int _nextGlobalIndex = 0;

        /// <summary>
        /// 事件名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 路由策略
        /// </summary>
        public RoutingStrategy RoutingStrategy { get; }

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

        private RoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
        {
            Name = name;
            RoutingStrategy = routingStrategy;
            HandlerType = handlerType;
            OwnerType = ownerType;
            GlobalIndex = System.Threading.Interlocked.Increment(ref _nextGlobalIndex);
        }

        /// <summary>
        /// 注册路由事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="routingStrategy">路由策略</param>
        /// <param name="handlerType">处理器类型</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>注册的路由事件</returns>
        public static RoutedEvent Register(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Event name cannot be null or empty", nameof(name));
            if (handlerType == null)
                throw new ArgumentNullException(nameof(handlerType));
            if (ownerType == null)
                throw new ArgumentNullException(nameof(ownerType));

            string key = $"{ownerType.FullName}.{name}";

            if (_registeredEvents.ContainsKey(key))
                throw new ArgumentException($"Event '{name}' is already registered for type '{ownerType.FullName}'");

            RoutedEvent routedEvent = new RoutedEvent(name, routingStrategy, handlerType, ownerType);
            _registeredEvents[key] = routedEvent;
return routedEvent;
        }

        /// <summary>
        /// 获取已注册的路由事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>路由事件，如果未找到则返回null</returns>
        public static RoutedEvent? GetEvent(string name, Type ownerType)
        {
            string key = $"{ownerType.FullName}.{name}";
            _registeredEvents.TryGetValue(key, out RoutedEvent? routedEvent);
            return routedEvent;
        }

        /// <summary>
        /// 添加所有者类型
        /// </summary>
        /// <param name="ownerType">新的所有者类型</param>
        /// <returns>新的路由事件实例</returns>
        public RoutedEvent AddOwner(Type ownerType)
        {
            if (ownerType == null)
                throw new ArgumentNullException(nameof(ownerType));

            string key = $"{ownerType.FullName}.{Name}";

            if (_registeredEvents.ContainsKey(key))
                throw new ArgumentException($"Event '{Name}' is already registered for type '{ownerType.FullName}'");

            RoutedEvent newEvent = new RoutedEvent(Name, RoutingStrategy, HandlerType, ownerType);
            _registeredEvents[key] = newEvent;
return newEvent;
        }

        public override string ToString()
        {
            return $"{OwnerType.Name}.{Name}";
        }
    }

    /// <summary>
    /// 路由事件参数基类
    /// </summary>
    public class RoutedEventArgs : EventArgs
    {
        /// <summary>
        /// 路由事件
        /// </summary>
        public RoutedEvent? RoutedEvent { get; set; }

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
        public RoutedEventArgs(RoutedEvent routedEvent)
        {
            RoutedEvent = routedEvent;
        }

        /// <summary>
        /// 初始化路由事件参数
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="source">事件源</param>
        public RoutedEventArgs(RoutedEvent routedEvent, object source)
        {
            RoutedEvent = routedEvent;
            Source = source;
            OriginalSource = source;
        }
    }

    /// <summary>
    /// 路由事件处理器委托
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">路由事件参数</param>
    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);
}
