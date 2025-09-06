using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AetherUI.Input.Routing
{
    /// <summary>
    /// 日志记录拦截器
    /// </summary>
    public class LoggingInterceptor : IEventRoutingInterceptor
    {
        private readonly bool _logPreProcess;
        private readonly bool _logPostProcess;

        /// <summary>
        /// 初始化日志记录拦截器
        /// </summary>
        /// <param name="logPreProcess">是否记录预处理</param>
        /// <param name="logPostProcess">是否记录后处理</param>
        public LoggingInterceptor(bool logPreProcess = true, bool logPostProcess = true)
        {
            _logPreProcess = logPreProcess;
            _logPostProcess = logPostProcess;
        }

        /// <summary>
        /// 预处理
        /// </summary>
        public bool PreProcess(EventRoutingContext context)
        {
            if (_logPreProcess)
            {
                Debug.WriteLine($"[EventRouting] PreProcess: {context.EventArgs.RoutedEvent?.Name} -> {context.Target}");
            }
            return true; // 继续路由
        }

        /// <summary>
        /// 后处理
        /// </summary>
        public void PostProcess(EventRoutingContext context)
        {
            if (_logPostProcess)
            {
                var handlerCount = context.InvokedHandlers.Count;
                var errorCount = context.Errors.Count;
                var handled = context.EventArgs.Handled;

                Debug.WriteLine($"[EventRouting] PostProcess: {context.EventArgs.RoutedEvent?.Name} -> " +
                              $"Handlers: {handlerCount}, Errors: {errorCount}, Handled: {handled}");
            }
        }
    }

    /// <summary>
    /// 性能监控拦截器
    /// </summary>
    public class PerformanceInterceptor : IEventRoutingInterceptor
    {
        private readonly Dictionary<string, PerformanceMetrics> _metrics = new();
        private readonly object _lock = new();

        /// <summary>
        /// 预处理
        /// </summary>
        public bool PreProcess(EventRoutingContext context)
        {
            var eventName = context.EventArgs.RoutedEvent?.Name ?? "Unknown";
            context.Data["StartTime"] = DateTime.UtcNow;
            context.Data["EventName"] = eventName;
            return true;
        }

        /// <summary>
        /// 后处理
        /// </summary>
        public void PostProcess(EventRoutingContext context)
        {
            if (context.Data.TryGetValue("StartTime", out object? startTimeObj) &&
                context.Data.TryGetValue("EventName", out object? eventNameObj) &&
                startTimeObj is DateTime startTime &&
                eventNameObj is string eventName)
            {
                var duration = DateTime.UtcNow - startTime;
                RecordMetrics(eventName, duration, context.InvokedHandlers.Count, context.Errors.Count);
            }
        }

        /// <summary>
        /// 记录性能指标
        /// </summary>
        private void RecordMetrics(string eventName, TimeSpan duration, int handlerCount, int errorCount)
        {
            lock (_lock)
            {
                if (!_metrics.TryGetValue(eventName, out PerformanceMetrics? metrics))
                {
                    metrics = new PerformanceMetrics();
                    _metrics[eventName] = metrics;
                }

                metrics.TotalCalls++;
                metrics.TotalDuration += duration;
                metrics.TotalHandlers += handlerCount;
                metrics.TotalErrors += errorCount;

                if (duration > metrics.MaxDuration)
                    metrics.MaxDuration = duration;

                if (metrics.MinDuration == TimeSpan.Zero || duration < metrics.MinDuration)
                    metrics.MinDuration = duration;
            }
        }

        /// <summary>
        /// 获取性能报告
        /// </summary>
        /// <returns>性能报告</returns>
        public string GetPerformanceReport()
        {
            lock (_lock)
            {
                var report = new System.Text.StringBuilder();
                report.AppendLine("=== Event Routing Performance Report ===");

                foreach (var kvp in _metrics.OrderByDescending(m => m.Value.TotalCalls))
                {
                    var eventName = kvp.Key;
                    var metrics = kvp.Value;
                    var avgDuration = metrics.TotalCalls > 0 ? metrics.TotalDuration.TotalMilliseconds / metrics.TotalCalls : 0;
                    var avgHandlers = metrics.TotalCalls > 0 ? (double)metrics.TotalHandlers / metrics.TotalCalls : 0;

                    report.AppendLine($"{eventName}:");
                    report.AppendLine($"  Calls: {metrics.TotalCalls}");
                    report.AppendLine($"  Avg Duration: {avgDuration:F2}ms");
                    report.AppendLine($"  Max Duration: {metrics.MaxDuration.TotalMilliseconds:F2}ms");
                    report.AppendLine($"  Min Duration: {metrics.MinDuration.TotalMilliseconds:F2}ms");
                    report.AppendLine($"  Avg Handlers: {avgHandlers:F1}");
                    report.AppendLine($"  Total Errors: {metrics.TotalErrors}");
                    report.AppendLine();
                }

                return report.ToString();
            }
        }

        /// <summary>
        /// 清除统计数据
        /// </summary>
        public void ClearMetrics()
        {
            lock (_lock)
            {
                _metrics.Clear();
            }
        }
    }

    /// <summary>
    /// 安全检查拦截器
    /// </summary>
    public class SecurityInterceptor : IEventRoutingInterceptor
    {
        private readonly HashSet<string> _blockedEvents = new();
        private readonly HashSet<Type> _blockedElements = new();

        /// <summary>
        /// 阻止事件类型
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void BlockEvent(string eventName)
        {
            _blockedEvents.Add(eventName);
        }

        /// <summary>
        /// 允许事件类型
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void AllowEvent(string eventName)
        {
            _blockedEvents.Remove(eventName);
        }

        /// <summary>
        /// 阻止元素类型
        /// </summary>
        /// <param name="elementType">元素类型</param>
        public void BlockElementType(Type elementType)
        {
            _blockedElements.Add(elementType);
        }

        /// <summary>
        /// 允许元素类型
        /// </summary>
        /// <param name="elementType">元素类型</param>
        public void AllowElementType(Type elementType)
        {
            _blockedElements.Remove(elementType);
        }

        /// <summary>
        /// 预处理
        /// </summary>
        public bool PreProcess(EventRoutingContext context)
        {
            var eventName = context.EventArgs.RoutedEvent?.Name;
            if (eventName != null && _blockedEvents.Contains(eventName))
            {
                Debug.WriteLine($"[Security] 阻止事件: {eventName}");
                return false;
            }

            var elementType = context.Target.GetType();
            if (_blockedElements.Any(t => t.IsAssignableFrom(elementType)))
            {
                Debug.WriteLine($"[Security] 阻止元素类型: {elementType.Name}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 后处理
        /// </summary>
        public void PostProcess(EventRoutingContext context)
        {
            // 安全检查通常在预处理阶段完成
        }
    }

    /// <summary>
    /// 条件路由拦截器
    /// </summary>
    public class ConditionalRoutingInterceptor : IEventRoutingInterceptor
    {
        private readonly Func<EventRoutingContext, bool> _condition;

        /// <summary>
        /// 初始化条件路由拦截器
        /// </summary>
        /// <param name="condition">路由条件</param>
        public ConditionalRoutingInterceptor(Func<EventRoutingContext, bool> condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <summary>
        /// 预处理
        /// </summary>
        public bool PreProcess(EventRoutingContext context)
        {
            try
            {
                return _condition(context);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConditionalRouting] 条件检查失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 后处理
        /// </summary>
        public void PostProcess(EventRoutingContext context)
        {
            // 条件检查通常在预处理阶段完成
        }
    }

    /// <summary>
    /// 处理器过滤拦截器
    /// </summary>
    public class HandlerFilterInterceptor : IEventHandlerInterceptor
    {
        private readonly Func<object, RoutedEventDefinition, EventHandlerInfo, bool> _filter;

        /// <summary>
        /// 初始化处理器过滤拦截器
        /// </summary>
        /// <param name="filter">过滤条件</param>
        public HandlerFilterInterceptor(Func<object, RoutedEventDefinition, EventHandlerInfo, bool> filter)
        {
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        /// <summary>
        /// 过滤处理器
        /// </summary>
        public IEnumerable<EventHandlerInfo> FilterHandlers(object element, RoutedEventDefinition routedEvent, IEnumerable<EventHandlerInfo> handlers)
        {
            return handlers.Where(handler =>
            {
                try
                {
                    return _filter(element, routedEvent, handler);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[HandlerFilter] 过滤器异常: {ex.Message}");
                    return true; // 默认允许
                }
            });
        }
    }

    /// <summary>
    /// 性能指标
    /// </summary>
    internal class PerformanceMetrics
    {
        public long TotalCalls { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan MaxDuration { get; set; }
        public TimeSpan MinDuration { get; set; }
        public long TotalHandlers { get; set; }
        public long TotalErrors { get; set; }
    }

    /// <summary>
    /// 拦截器工厂
    /// </summary>
    public static class InterceptorFactory
    {
        /// <summary>
        /// 创建日志拦截器
        /// </summary>
        public static LoggingInterceptor CreateLogging(bool preProcess = true, bool postProcess = true)
        {
            return new LoggingInterceptor(preProcess, postProcess);
        }

        /// <summary>
        /// 创建性能监控拦截器
        /// </summary>
        public static PerformanceInterceptor CreatePerformanceMonitor()
        {
            return new PerformanceInterceptor();
        }

        /// <summary>
        /// 创建安全检查拦截器
        /// </summary>
        public static SecurityInterceptor CreateSecurity()
        {
            return new SecurityInterceptor();
        }

        /// <summary>
        /// 创建条件路由拦截器
        /// </summary>
        public static ConditionalRoutingInterceptor CreateConditional(Func<EventRoutingContext, bool> condition)
        {
            return new ConditionalRoutingInterceptor(condition);
        }

        /// <summary>
        /// 创建处理器过滤拦截器
        /// </summary>
        public static HandlerFilterInterceptor CreateHandlerFilter(Func<object, RoutedEventDefinition, EventHandlerInfo, bool> filter)
        {
            return new HandlerFilterInterceptor(filter);
        }

        /// <summary>
        /// 创建调试拦截器组合
        /// </summary>
        public static IEnumerable<IEventRoutingInterceptor> CreateDebugInterceptors()
        {
            yield return CreateLogging();
            yield return CreatePerformanceMonitor();
        }

        /// <summary>
        /// 创建生产环境拦截器组合
        /// </summary>
        public static IEnumerable<IEventRoutingInterceptor> CreateProductionInterceptors()
        {
            yield return CreateSecurity();
            yield return CreatePerformanceMonitor();
        }
    }
}
