using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AetherUI.Input.HitTesting;

namespace AetherUI.Input.Routing
{
    /// <summary>
    /// 事件路由器实现
    /// </summary>
    public class EventRouter : IEventRouter
    {
        private readonly IRoutedEventManager _eventManager;
        private readonly IEventHandlerStore _handlerStore;
        private int _nextGlobalIndex = 0;

        /// <summary>
        /// 初始化事件路由器
        /// </summary>
        /// <param name="eventManager">路由事件管理器</param>
        /// <param name="handlerStore">事件处理器存储</param>
        public EventRouter(IRoutedEventManager? eventManager = null, IEventHandlerStore? handlerStore = null)
        {
            _eventManager = eventManager ?? new RoutedEventManager();
            _handlerStore = handlerStore ?? new EventHandlerStore();
        }

        /// <summary>
        /// 路由事件
        /// </summary>
        /// <param name="target">目标元素</param>
        /// <param name="eventArgs">事件参数</param>
        /// <param name="hitPath">命中路径</param>
        public void RouteEvent(object target, RoutedEventArgs eventArgs, IReadOnlyList<IHitTestable>? hitPath = null)
        {
            if (eventArgs.RoutedEvent == null)
            {
                Debug.WriteLine("警告: 路由事件定义为空，跳过路由");
                return;
            }

            try
            {
                // 构建路由路径
                var routePath = BuildRoutePath(target, hitPath);
                var sourceIndex = routePath.Count - 1; // 目标元素在路径末尾

                var eventRoute = new EventRoute(routePath, eventArgs.RoutedEvent.RoutingStrategy, sourceIndex);

                // 设置事件源信息
                eventArgs.Source = target;
                eventArgs.OriginalSource = target;

                // 执行路由
                RouteEventInternal(eventRoute, eventArgs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"事件路由失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 注册路由事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="routingStrategy">路由策略</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="handlerType">处理器类型</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>路由事件定义</returns>
        public RoutedEventDefinition RegisterRoutedEvent(
            string name,
            RoutingStrategy routingStrategy,
            Type eventType,
            Type handlerType,
            Type ownerType)
        {
            return _eventManager.RegisterEvent(name, routingStrategy, eventType, handlerType, ownerType);
        }

        /// <summary>
        /// 获取路由事件定义
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>路由事件定义</returns>
        public RoutedEventDefinition? GetRoutedEvent(string name, Type ownerType)
        {
            return _eventManager.GetEvent(name, ownerType);
        }

        /// <summary>
        /// 添加事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        /// <param name="handledEventsToo">是否处理已处理的事件</param>
        public void AddHandler(object element, RoutedEventDefinition routedEvent, Delegate handler, bool handledEventsToo = false)
        {
            _handlerStore.AddHandler(element, routedEvent, handler, handledEventsToo);
        }

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        public void RemoveHandler(object element, RoutedEventDefinition routedEvent, Delegate handler)
        {
            _handlerStore.RemoveHandler(element, routedEvent, handler);
        }

        /// <summary>
        /// 构建路由路径
        /// </summary>
        private List<object> BuildRoutePath(object target, IReadOnlyList<IHitTestable>? hitPath)
        {
            var path = new List<object>();

            if (hitPath != null && hitPath.Count > 0)
            {
                // 使用命中路径
                path.AddRange(hitPath.Cast<object>());
            }
            else
            {
                // 构建从目标到根的路径
                var current = target;
                while (current != null)
                {
                    path.Insert(0, current); // 插入到开头，保持从根到目标的顺序

                    // 尝试获取父元素
                    if (current is IHitTestable hitTestable)
                    {
                        current = hitTestable.Parent;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return path;
        }

        /// <summary>
        /// 内部路由事件
        /// </summary>
        private void RouteEventInternal(EventRoute eventRoute, RoutedEventArgs eventArgs)
        {
            var routingOrder = eventRoute.GetRoutingOrder().ToList();

            foreach (var element in routingOrder)
            {
                // 检查事件是否已被处理且不需要继续路由
                if (eventArgs.Handled && !eventArgs.HandledToo)
                {
                    break;
                }

                // 更新当前事件源
                eventArgs.Source = element;

                // 获取并调用事件处理器
                InvokeHandlers(element, eventArgs);
            }
        }

        /// <summary>
        /// 调用事件处理器
        /// </summary>
        private void InvokeHandlers(object element, RoutedEventArgs eventArgs)
        {
            if (eventArgs.RoutedEvent == null) return;

            var handlers = _handlerStore.GetHandlers(element, eventArgs.RoutedEvent);

            foreach (var handlerInfo in handlers)
            {
                // 检查是否应该调用此处理器
                if (eventArgs.Handled && !handlerInfo.HandledEventsToo)
                {
                    continue;
                }

                try
                {
                    // 调用处理器
                    handlerInfo.Handler.DynamicInvoke(element, eventArgs);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"事件处理器调用失败: {ex.Message}");
                    // 继续处理其他处理器，不让一个处理器的异常影响整个路由
                }
            }
        }
    }

    /// <summary>
    /// 路由事件管理器实现
    /// </summary>
    public class RoutedEventManager : IRoutedEventManager
    {
        private readonly ConcurrentDictionary<string, RoutedEventDefinition> _events = new();
        private int _nextGlobalIndex = 0;

        /// <summary>
        /// 注册路由事件
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="routingStrategy">路由策略</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="handlerType">处理器类型</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>路由事件定义</returns>
        public RoutedEventDefinition RegisterEvent(
            string name,
            RoutingStrategy routingStrategy,
            Type eventType,
            Type handlerType,
            Type ownerType)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("事件名称不能为空", nameof(name));

            string key = $"{ownerType.FullName}.{name}";

            if (_events.ContainsKey(key))
                throw new ArgumentException($"事件 '{name}' 已在类型 '{ownerType.FullName}' 中注册");

            var globalIndex = System.Threading.Interlocked.Increment(ref _nextGlobalIndex);
            var eventDefinition = new RoutedEventDefinition(name, routingStrategy, eventType, handlerType, ownerType, globalIndex);

            _events[key] = eventDefinition;
            return eventDefinition;
        }

        /// <summary>
        /// 获取路由事件定义
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>路由事件定义</returns>
        public RoutedEventDefinition? GetEvent(string name, Type ownerType)
        {
            string key = $"{ownerType.FullName}.{name}";
            _events.TryGetValue(key, out RoutedEventDefinition? eventDefinition);
            return eventDefinition;
        }

        /// <summary>
        /// 获取所有注册的事件
        /// </summary>
        /// <returns>所有路由事件定义</returns>
        public IEnumerable<RoutedEventDefinition> GetAllEvents()
        {
            return _events.Values;
        }
    }

    /// <summary>
    /// 事件处理器存储实现
    /// </summary>
    public class EventHandlerStore : IEventHandlerStore
    {
        private readonly ConcurrentDictionary<object, Dictionary<RoutedEventDefinition, List<EventHandlerInfo>>> _handlers = new();

        /// <summary>
        /// 添加事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        /// <param name="handledEventsToo">是否处理已处理的事件</param>
        public void AddHandler(object element, RoutedEventDefinition routedEvent, Delegate handler, bool handledEventsToo)
        {
            var elementHandlers = _handlers.GetOrAdd(element, _ => new Dictionary<RoutedEventDefinition, List<EventHandlerInfo>>());

            lock (elementHandlers)
            {
                if (!elementHandlers.TryGetValue(routedEvent, out List<EventHandlerInfo>? handlerList))
                {
                    handlerList = new List<EventHandlerInfo>();
                    elementHandlers[routedEvent] = handlerList;
                }

                var handlerInfo = new EventHandlerInfo(handler, handledEventsToo);
                handlerList.Add(handlerInfo);
            }
        }

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <param name="handler">处理器</param>
        public void RemoveHandler(object element, RoutedEventDefinition routedEvent, Delegate handler)
        {
            if (!_handlers.TryGetValue(element, out Dictionary<RoutedEventDefinition, List<EventHandlerInfo>>? elementHandlers))
                return;

            lock (elementHandlers)
            {
                if (elementHandlers.TryGetValue(routedEvent, out List<EventHandlerInfo>? handlerList))
                {
                    handlerList.RemoveAll(h => h.Handler == handler);

                    if (handlerList.Count == 0)
                    {
                        elementHandlers.Remove(routedEvent);
                    }
                }

                if (elementHandlers.Count == 0)
                {
                    _handlers.TryRemove(element, out _);
                }
            }
        }

        /// <summary>
        /// 获取事件处理器
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="routedEvent">路由事件</param>
        /// <returns>处理器集合</returns>
        public IEnumerable<EventHandlerInfo> GetHandlers(object element, RoutedEventDefinition routedEvent)
        {
            if (!_handlers.TryGetValue(element, out Dictionary<RoutedEventDefinition, List<EventHandlerInfo>>? elementHandlers))
                return Enumerable.Empty<EventHandlerInfo>();

            lock (elementHandlers)
            {
                if (elementHandlers.TryGetValue(routedEvent, out List<EventHandlerInfo>? handlerList))
                {
                    return handlerList.ToArray(); // 返回副本以避免并发修改
                }
            }

            return Enumerable.Empty<EventHandlerInfo>();
        }

        /// <summary>
        /// 清除元素的所有处理器
        /// </summary>
        /// <param name="element">元素</param>
        public void ClearHandlers(object element)
        {
            _handlers.TryRemove(element, out _);
        }
    }
}
