using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace AetherUI.Input.Routing
{
    /// <summary>
    /// 高级事件处理器存储
    /// </summary>
    public class AdvancedEventHandlerStore : IEventHandlerStore
    {
        private readonly ConcurrentDictionary<object, ElementHandlers> _elementHandlers = new();
        private readonly ConcurrentDictionary<RoutedEventDefinition, GlobalHandlers> _globalHandlers = new();
        private readonly List<IEventHandlerInterceptor> _interceptors = new();

        /// <summary>
        /// 添加事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        /// <param name="handledEventsToo">是否处理已处理的事件</param>
        public void AddHandler(object element, RoutedEventDefinition routedEvent, Delegate handler, bool handledEventsToo)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (routedEvent == null)
                throw new ArgumentNullException(nameof(routedEvent));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // 验证处理器签名
            ValidateHandlerSignature(handler, routedEvent);

            var elementHandlers = _elementHandlers.GetOrAdd(element, _ => new ElementHandlers());
            elementHandlers.AddHandler(routedEvent, handler, handledEventsToo);

            Debug.WriteLine($"添加事件处理器: {element}.{routedEvent.Name} -> {handler.Method.Name}");
        }

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        public void RemoveHandler(object element, RoutedEventDefinition routedEvent, Delegate handler)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (routedEvent == null)
                throw new ArgumentNullException(nameof(routedEvent));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (_elementHandlers.TryGetValue(element, out ElementHandlers? elementHandlers))
            {
                elementHandlers.RemoveHandler(routedEvent, handler);

                // 如果元素没有任何处理器，移除元素条目
                if (elementHandlers.IsEmpty)
                {
                    _elementHandlers.TryRemove(element, out _);
                }
            }

            Debug.WriteLine($"移除事件处理器: {element}.{routedEvent.Name} -> {handler.Method.Name}");
        }

        /// <summary>
        /// 获取事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <returns>处理器集合</returns>
        public IEnumerable<EventHandlerInfo> GetHandlers(object element, RoutedEventDefinition routedEvent)
        {
            var handlers = new List<EventHandlerInfo>();

            // 获取元素特定的处理器
            if (_elementHandlers.TryGetValue(element, out ElementHandlers? elementHandlers))
            {
                handlers.AddRange(elementHandlers.GetHandlers(routedEvent));
            }

            // 获取全局处理器
            if (_globalHandlers.TryGetValue(routedEvent, out GlobalHandlers? globalHandlers))
            {
                handlers.AddRange(globalHandlers.GetHandlers(element));
            }

            // 应用拦截器
            foreach (var interceptor in _interceptors)
            {
                handlers = interceptor.FilterHandlers(element, routedEvent, handlers).ToList();
            }

            return handlers;
        }

        /// <summary>
        /// 清除元素的所有处理器
        /// </summary>
        /// <param name="element">元素</param>
        public void ClearHandlers(object element)
        {
            _elementHandlers.TryRemove(element, out _);
        }

        /// <summary>
        /// 添加全局事件处理器
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        /// <param name="filter">元素过滤器</param>
        public void AddGlobalHandler(RoutedEventDefinition routedEvent, Delegate handler, Func<object, bool>? filter = null)
        {
            if (routedEvent == null)
                throw new ArgumentNullException(nameof(routedEvent));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            ValidateHandlerSignature(handler, routedEvent);

            var globalHandlers = _globalHandlers.GetOrAdd(routedEvent, _ => new GlobalHandlers());
            globalHandlers.AddHandler(handler, filter);

            Debug.WriteLine($"添加全局事件处理器: {routedEvent.Name} -> {handler.Method.Name}");
        }

        /// <summary>
        /// 移除全局事件处理器
        /// </summary>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        public void RemoveGlobalHandler(RoutedEventDefinition routedEvent, Delegate handler)
        {
            if (routedEvent == null)
                throw new ArgumentNullException(nameof(routedEvent));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (_globalHandlers.TryGetValue(routedEvent, out GlobalHandlers? globalHandlers))
            {
                globalHandlers.RemoveHandler(handler);

                if (globalHandlers.IsEmpty)
                {
                    _globalHandlers.TryRemove(routedEvent, out _);
                }
            }

            Debug.WriteLine($"移除全局事件处理器: {routedEvent.Name} -> {handler.Method.Name}");
        }

        /// <summary>
        /// 添加处理器拦截器
        /// </summary>
        /// <param name="interceptor">拦截器</param>
        public void AddInterceptor(IEventHandlerInterceptor interceptor)
        {
            if (interceptor != null && !_interceptors.Contains(interceptor))
            {
                _interceptors.Add(interceptor);
            }
        }

        /// <summary>
        /// 移除处理器拦截器
        /// </summary>
        /// <param name="interceptor">拦截器</param>
        public void RemoveInterceptor(IEventHandlerInterceptor interceptor)
        {
            _interceptors.Remove(interceptor);
        }

        /// <summary>
        /// 验证处理器签名
        /// </summary>
        private void ValidateHandlerSignature(Delegate handler, RoutedEventDefinition routedEvent)
        {
            var method = handler.Method;
            var parameters = method.GetParameters();

            // 检查参数数量
            if (parameters.Length != 2)
            {
                throw new ArgumentException($"事件处理器必须有两个参数，实际有 {parameters.Length} 个");
            }

            // 检查第一个参数（sender）
            if (!parameters[0].ParameterType.IsAssignableFrom(typeof(object)))
            {
                throw new ArgumentException($"第一个参数必须是 object 类型或其基类");
            }

            // 检查第二个参数（事件参数）
            if (!routedEvent.EventType.IsAssignableFrom(parameters[1].ParameterType))
            {
                throw new ArgumentException($"第二个参数类型 {parameters[1].ParameterType.Name} 与事件类型 {routedEvent.EventType.Name} 不兼容");
            }
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        public HandlerStoreStats GetStats()
        {
            var totalElementHandlers = _elementHandlers.Values.Sum(eh => eh.HandlerCount);
            var totalGlobalHandlers = _globalHandlers.Values.Sum(gh => gh.HandlerCount);

            return new HandlerStoreStats
            {
                ElementCount = _elementHandlers.Count,
                TotalElementHandlers = totalElementHandlers,
                TotalGlobalHandlers = totalGlobalHandlers,
                InterceptorCount = _interceptors.Count
            };
        }
    }

    /// <summary>
    /// 元素处理器集合
    /// </summary>
    internal class ElementHandlers
    {
        private readonly ConcurrentDictionary<RoutedEventDefinition, List<EventHandlerInfo>> _handlers = new();
        private readonly object _lock = new();

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => _handlers.IsEmpty;

        /// <summary>
        /// 处理器数量
        /// </summary>
        public int HandlerCount => _handlers.Values.Sum(list => list.Count);

        /// <summary>
        /// 添加处理器
        /// </summary>
        public void AddHandler(RoutedEventDefinition routedEvent, Delegate handler, bool handledEventsToo)
        {
            lock (_lock)
            {
                var handlerList = _handlers.GetOrAdd(routedEvent, _ => new List<EventHandlerInfo>());
                var handlerInfo = new EventHandlerInfo(handler, handledEventsToo);
                handlerList.Add(handlerInfo);
            }
        }

        /// <summary>
        /// 移除处理器
        /// </summary>
        public void RemoveHandler(RoutedEventDefinition routedEvent, Delegate handler)
        {
            lock (_lock)
            {
                if (_handlers.TryGetValue(routedEvent, out List<EventHandlerInfo>? handlerList))
                {
                    handlerList.RemoveAll(h => h.Handler == handler);

                    if (handlerList.Count == 0)
                    {
                        _handlers.TryRemove(routedEvent, out _);
                    }
                }
            }
        }

        /// <summary>
        /// 获取处理器
        /// </summary>
        public IEnumerable<EventHandlerInfo> GetHandlers(RoutedEventDefinition routedEvent)
        {
            lock (_lock)
            {
                if (_handlers.TryGetValue(routedEvent, out List<EventHandlerInfo>? handlerList))
                {
                    return handlerList.ToArray(); // 返回副本
                }
            }

            return Enumerable.Empty<EventHandlerInfo>();
        }
    }

    /// <summary>
    /// 全局处理器集合
    /// </summary>
    internal class GlobalHandlers
    {
        private readonly List<GlobalHandlerInfo> _handlers = new();
        private readonly object _lock = new();

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                lock (_lock)
                {
                    return _handlers.Count == 0;
                }
            }
        }

        /// <summary>
        /// 处理器数量
        /// </summary>
        public int HandlerCount
        {
            get
            {
                lock (_lock)
                {
                    return _handlers.Count;
                }
            }
        }

        /// <summary>
        /// 添加处理器
        /// </summary>
        public void AddHandler(Delegate handler, Func<object, bool>? filter)
        {
            lock (_lock)
            {
                var handlerInfo = new GlobalHandlerInfo(handler, filter);
                _handlers.Add(handlerInfo);
            }
        }

        /// <summary>
        /// 移除处理器
        /// </summary>
        public void RemoveHandler(Delegate handler)
        {
            lock (_lock)
            {
                _handlers.RemoveAll(h => h.Handler == handler);
            }
        }

        /// <summary>
        /// 获取处理器
        /// </summary>
        public IEnumerable<EventHandlerInfo> GetHandlers(object element)
        {
            lock (_lock)
            {
                return _handlers
                    .Where(h => h.Filter == null || h.Filter(element))
                    .Select(h => new EventHandlerInfo(h.Handler, false))
                    .ToArray();
            }
        }
    }

    /// <summary>
    /// 全局处理器信息
    /// </summary>
    internal class GlobalHandlerInfo
    {
        public Delegate Handler { get; }
        public Func<object, bool>? Filter { get; }

        public GlobalHandlerInfo(Delegate handler, Func<object, bool>? filter)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Filter = filter;
        }
    }

    /// <summary>
    /// 事件处理器拦截器接口
    /// </summary>
    public interface IEventHandlerInterceptor
    {
        /// <summary>
        /// 过滤处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handlers">原始处理器列表</param>
        /// <returns>过滤后的处理器列表</returns>
        IEnumerable<EventHandlerInfo> FilterHandlers(object element, RoutedEventDefinition routedEvent, IEnumerable<EventHandlerInfo> handlers);
    }

    /// <summary>
    /// 处理器存储统计
    /// </summary>
    public class HandlerStoreStats
    {
        /// <summary>
        /// 元素数量
        /// </summary>
        public int ElementCount { get; set; }

        /// <summary>
        /// 总元素处理器数量
        /// </summary>
        public int TotalElementHandlers { get; set; }

        /// <summary>
        /// 总全局处理器数量
        /// </summary>
        public int TotalGlobalHandlers { get; set; }

        /// <summary>
        /// 拦截器数量
        /// </summary>
        public int InterceptorCount { get; set; }

        public override string ToString() =>
            $"Elements: {ElementCount}, ElementHandlers: {TotalElementHandlers}, GlobalHandlers: {TotalGlobalHandlers}, Interceptors: {InterceptorCount}";
    }
}
