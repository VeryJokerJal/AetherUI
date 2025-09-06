using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AetherUI.Input.HitTesting;

namespace AetherUI.Input.Routing
{
    /// <summary>
    /// 高级事件路由器，支持复杂的路由场景
    /// </summary>
    public class AdvancedEventRouter : IEventRouter
    {
        private readonly IRoutedEventManager _eventManager;
        private readonly IEventHandlerStore _handlerStore;
        private readonly EventRoutingCache _routingCache;
        private readonly List<IEventRoutingInterceptor> _interceptors = new();

        /// <summary>
        /// 初始化高级事件路由器
        /// </summary>
        /// <param name="eventManager">路由事件管理器</param>
        /// <param name="handlerStore">事件处理器存储</param>
        public AdvancedEventRouter(IRoutedEventManager? eventManager = null, IEventHandlerStore? handlerStore = null)
        {
            _eventManager = eventManager ?? new RoutedEventManager();
            _handlerStore = handlerStore ?? new EventHandlerStore();
            _routingCache = new EventRoutingCache();
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
                var context = new EventRoutingContext(target, eventArgs, hitPath);

                // 预处理拦截器
                foreach (var interceptor in _interceptors)
                {
                    if (!interceptor.PreProcess(context))
                    {
                        Debug.WriteLine($"事件被拦截器 {interceptor.GetType().Name} 阻止");
                        return;
                    }
                }

                // 构建或获取缓存的路由路径
                var routePath = GetOrBuildRoutePath(target, hitPath, eventArgs.RoutedEvent);

                // 执行路由
                RouteEventInternal(routePath, context);

                // 后处理拦截器
                foreach (var interceptor in _interceptors)
                {
                    interceptor.PostProcess(context);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"事件路由失败: {ex.Message}");
                
                // 触发错误处理
                HandleRoutingError(target, eventArgs, ex);
            }
        }

        /// <summary>
        /// 注册路由事件
        /// </summary>
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
        public RoutedEventDefinition? GetRoutedEvent(string name, Type ownerType)
        {
            return _eventManager.GetEvent(name, ownerType);
        }

        /// <summary>
        /// 添加事件处理器
        /// </summary>
        public void AddHandler(object element, RoutedEventDefinition routedEvent, Delegate handler, bool handledEventsToo = false)
        {
            _handlerStore.AddHandler(element, routedEvent, handler, handledEventsToo);
            
            // 清除相关的路由缓存
            _routingCache.InvalidateElement(element);
        }

        /// <summary>
        /// 移除事件处理器
        /// </summary>
        public void RemoveHandler(object element, RoutedEventDefinition routedEvent, Delegate handler)
        {
            _handlerStore.RemoveHandler(element, routedEvent, handler);
            
            // 清除相关的路由缓存
            _routingCache.InvalidateElement(element);
        }

        /// <summary>
        /// 添加路由拦截器
        /// </summary>
        /// <param name="interceptor">拦截器</param>
        public void AddInterceptor(IEventRoutingInterceptor interceptor)
        {
            if (interceptor != null && !_interceptors.Contains(interceptor))
            {
                _interceptors.Add(interceptor);
            }
        }

        /// <summary>
        /// 移除路由拦截器
        /// </summary>
        /// <param name="interceptor">拦截器</param>
        public void RemoveInterceptor(IEventRoutingInterceptor interceptor)
        {
            _interceptors.Remove(interceptor);
        }

        /// <summary>
        /// 获取或构建路由路径
        /// </summary>
        private EventRoute GetOrBuildRoutePath(object target, IReadOnlyList<IHitTestable>? hitPath, RoutedEventDefinition routedEvent)
        {
            var cacheKey = new RouteCacheKey(target, hitPath, routedEvent);
            
            if (_routingCache.TryGetRoute(cacheKey, out EventRoute? cachedRoute))
            {
                return cachedRoute;
            }

            var route = BuildRoutePath(target, hitPath, routedEvent);
            _routingCache.CacheRoute(cacheKey, route);
            
            return route;
        }

        /// <summary>
        /// 构建路由路径
        /// </summary>
        private EventRoute BuildRoutePath(object target, IReadOnlyList<IHitTestable>? hitPath, RoutedEventDefinition routedEvent)
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
                BuildElementPath(target, path);
            }

            var sourceIndex = path.Count > 0 ? path.Count - 1 : 0;
            return new EventRoute(path, routedEvent.RoutingStrategy, sourceIndex);
        }

        /// <summary>
        /// 构建元素路径
        /// </summary>
        private void BuildElementPath(object target, List<object> path)
        {
            var current = target;
            var visited = new HashSet<object>(); // 防止循环引用

            while (current != null && !visited.Contains(current))
            {
                visited.Add(current);
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

        /// <summary>
        /// 内部路由事件
        /// </summary>
        private void RouteEventInternal(EventRoute eventRoute, EventRoutingContext context)
        {
            var routingOrder = eventRoute.GetRoutingOrder().ToList();
            context.RoutePath = routingOrder;

            foreach (var element in routingOrder)
            {
                // 检查事件是否已被处理且不需要继续路由
                if (context.EventArgs.Handled && !context.EventArgs.HandledToo)
                {
                    break;
                }

                // 更新当前事件源
                context.EventArgs.Source = element;
                context.CurrentElement = element;

                // 获取并调用事件处理器
                InvokeHandlers(element, context);

                // 检查是否被拦截器停止
                if (context.IsStopped)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 调用事件处理器
        /// </summary>
        private void InvokeHandlers(object element, EventRoutingContext context)
        {
            if (context.EventArgs.RoutedEvent == null) return;

            var handlers = _handlerStore.GetHandlers(element, context.EventArgs.RoutedEvent);

            foreach (var handlerInfo in handlers)
            {
                // 检查是否应该调用此处理器
                if (context.EventArgs.Handled && !handlerInfo.HandledEventsToo)
                {
                    continue;
                }

                try
                {
                    // 记录处理器调用
                    context.InvokedHandlers.Add(new HandlerInvocation(element, handlerInfo.Handler));

                    // 调用处理器
                    handlerInfo.Handler.DynamicInvoke(element, context.EventArgs);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"事件处理器调用失败: {ex.Message}");
                    
                    // 记录错误但继续处理其他处理器
                    context.Errors.Add(new RoutingError(element, handlerInfo.Handler, ex));
                }
            }
        }

        /// <summary>
        /// 处理路由错误
        /// </summary>
        private void HandleRoutingError(object target, RoutedEventArgs eventArgs, Exception exception)
        {
            // 可以在这里实现错误恢复逻辑
            Debug.WriteLine($"路由错误处理: 目标={target}, 事件={eventArgs.RoutedEvent?.Name}, 错误={exception.Message}");
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            _routingCache.Clear();
        }

        /// <summary>
        /// 获取缓存统计
        /// </summary>
        public RoutingCacheStats GetCacheStats()
        {
            return _routingCache.GetStats();
        }
    }

    /// <summary>
    /// 事件路由上下文
    /// </summary>
    public class EventRoutingContext
    {
        /// <summary>
        /// 目标元素
        /// </summary>
        public object Target { get; }

        /// <summary>
        /// 事件参数
        /// </summary>
        public RoutedEventArgs EventArgs { get; }

        /// <summary>
        /// 命中路径
        /// </summary>
        public IReadOnlyList<IHitTestable>? HitPath { get; }

        /// <summary>
        /// 路由路径
        /// </summary>
        public IReadOnlyList<object>? RoutePath { get; set; }

        /// <summary>
        /// 当前元素
        /// </summary>
        public object? CurrentElement { get; set; }

        /// <summary>
        /// 是否停止路由
        /// </summary>
        public bool IsStopped { get; set; }

        /// <summary>
        /// 已调用的处理器
        /// </summary>
        public List<HandlerInvocation> InvokedHandlers { get; } = new();

        /// <summary>
        /// 路由错误
        /// </summary>
        public List<RoutingError> Errors { get; } = new();

        /// <summary>
        /// 自定义数据
        /// </summary>
        public Dictionary<string, object> Data { get; } = new();

        /// <summary>
        /// 初始化事件路由上下文
        /// </summary>
        /// <param name="target">目标元素</param>
        /// <param name="eventArgs">事件参数</param>
        /// <param name="hitPath">命中路径</param>
        public EventRoutingContext(object target, RoutedEventArgs eventArgs, IReadOnlyList<IHitTestable>? hitPath)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            EventArgs = eventArgs ?? throw new ArgumentNullException(nameof(eventArgs));
            HitPath = hitPath;
        }

        /// <summary>
        /// 停止路由
        /// </summary>
        public void StopRouting()
        {
            IsStopped = true;
        }
    }

    /// <summary>
    /// 处理器调用记录
    /// </summary>
    public class HandlerInvocation
    {
        /// <summary>
        /// 元素
        /// </summary>
        public object Element { get; }

        /// <summary>
        /// 处理器
        /// </summary>
        public Delegate Handler { get; }

        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime InvokeTime { get; }

        /// <summary>
        /// 初始化处理器调用记录
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="handler">处理器</param>
        public HandlerInvocation(object element, Delegate handler)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            InvokeTime = DateTime.UtcNow;
        }

        public override string ToString() => $"{Element}.{Handler.Method.Name} at {InvokeTime:HH:mm:ss.fff}";
    }

    /// <summary>
    /// 路由错误
    /// </summary>
    public class RoutingError
    {
        /// <summary>
        /// 元素
        /// </summary>
        public object Element { get; }

        /// <summary>
        /// 处理器
        /// </summary>
        public Delegate Handler { get; }

        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// 错误时间
        /// </summary>
        public DateTime ErrorTime { get; }

        /// <summary>
        /// 初始化路由错误
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="handler">处理器</param>
        /// <param name="exception">异常</param>
        public RoutingError(object element, Delegate handler, Exception exception)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            ErrorTime = DateTime.UtcNow;
        }

        public override string ToString() => $"Error in {Element}.{Handler.Method.Name}: {Exception.Message}";
    }

    /// <summary>
    /// 事件路由拦截器接口
    /// </summary>
    public interface IEventRoutingInterceptor
    {
        /// <summary>
        /// 预处理
        /// </summary>
        /// <param name="context">路由上下文</param>
        /// <returns>是否继续路由</returns>
        bool PreProcess(EventRoutingContext context);

        /// <summary>
        /// 后处理
        /// </summary>
        /// <param name="context">路由上下文</param>
        void PostProcess(EventRoutingContext context);
    }
}
