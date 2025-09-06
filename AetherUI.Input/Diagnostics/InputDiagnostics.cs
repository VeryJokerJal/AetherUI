using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AetherUI.Input.Core;
using AetherUI.Input.Events;

namespace AetherUI.Input.Diagnostics
{
    /// <summary>
    /// 输入诊断系统
    /// </summary>
    public class InputDiagnostics
    {
        private readonly ConcurrentQueue<DiagnosticEvent> _events = new();
        private readonly Dictionary<string, DiagnosticCounter> _counters = new();
        private readonly Dictionary<string, DiagnosticTimer> _timers = new();
        private readonly object _lock = new();
        private readonly int _maxEventCount;
        private bool _isEnabled = true;

        /// <summary>
        /// 诊断事件
        /// </summary>
        public event EventHandler<DiagnosticEventArgs>? DiagnosticEventOccurred;

        /// <summary>
        /// 是否启用诊断
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// 事件数量
        /// </summary>
        public int EventCount => _events.Count;

        /// <summary>
        /// 初始化输入诊断系统
        /// </summary>
        /// <param name="maxEventCount">最大事件数量</param>
        public InputDiagnostics(int maxEventCount = 10000)
        {
            _maxEventCount = maxEventCount;
        }

        /// <summary>
        /// 记录输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <param name="stage">处理阶段</param>
        /// <param name="details">详细信息</param>
        public void LogInputEvent(InputEvent inputEvent, string stage, string? details = null)
        {
            if (!_isEnabled)
                return;

            var diagnosticEvent = new DiagnosticEvent(
                DateTime.UtcNow,
                DiagnosticEventType.InputEvent,
                $"{inputEvent.GetType().Name}",
                stage,
                details ?? inputEvent.ToString());

            AddEvent(diagnosticEvent);
            IncrementCounter($"InputEvent.{inputEvent.GetType().Name}");
            IncrementCounter($"Stage.{stage}");
        }

        /// <summary>
        /// 记录性能指标
        /// </summary>
        /// <param name="operation">操作名称</param>
        /// <param name="duration">持续时间</param>
        /// <param name="details">详细信息</param>
        public void LogPerformance(string operation, TimeSpan duration, string? details = null)
        {
            if (!_isEnabled)
                return;

            var diagnosticEvent = new DiagnosticEvent(
                DateTime.UtcNow,
                DiagnosticEventType.Performance,
                operation,
                "Completed",
                $"Duration: {duration.TotalMilliseconds:F2}ms" + (details != null ? $", {details}" : ""));

            AddEvent(diagnosticEvent);
            UpdateTimer(operation, duration);
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        /// <param name="error">错误信息</param>
        /// <param name="exception">异常</param>
        /// <param name="context">上下文</param>
        public void LogError(string error, Exception? exception = null, string? context = null)
        {
            if (!_isEnabled)
                return;

            var details = error;
            if (exception != null)
            {
                details += $" Exception: {exception.Message}";
            }
            if (context != null)
            {
                details += $" Context: {context}";
            }

            var diagnosticEvent = new DiagnosticEvent(
                DateTime.UtcNow,
                DiagnosticEventType.Error,
                "Error",
                "Occurred",
                details);

            AddEvent(diagnosticEvent);
            IncrementCounter("Errors");
        }

        /// <summary>
        /// 记录警告
        /// </summary>
        /// <param name="warning">警告信息</param>
        /// <param name="context">上下文</param>
        public void LogWarning(string warning, string? context = null)
        {
            if (!_isEnabled)
                return;

            var details = warning;
            if (context != null)
            {
                details += $" Context: {context}";
            }

            var diagnosticEvent = new DiagnosticEvent(
                DateTime.UtcNow,
                DiagnosticEventType.Warning,
                "Warning",
                "Occurred",
                details);

            AddEvent(diagnosticEvent);
            IncrementCounter("Warnings");
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="category">类别</param>
        /// <param name="details">详细信息</param>
        public void LogInfo(string message, string category = "Info", string? details = null)
        {
            if (!_isEnabled)
                return;

            var diagnosticEvent = new DiagnosticEvent(
                DateTime.UtcNow,
                DiagnosticEventType.Info,
                category,
                "Logged",
                details ?? message);

            AddEvent(diagnosticEvent);
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="operation">操作名称</param>
        /// <returns>计时器</returns>
        public IDisposable StartTiming(string operation)
        {
            return new DiagnosticStopwatch(this, operation);
        }

        /// <summary>
        /// 增加计数器
        /// </summary>
        /// <param name="name">计数器名称</param>
        /// <param name="increment">增量</param>
        public void IncrementCounter(string name, long increment = 1)
        {
            if (!_isEnabled)
                return;

            lock (_lock)
            {
                if (!_counters.TryGetValue(name, out DiagnosticCounter? counter))
                {
                    counter = new DiagnosticCounter(name);
                    _counters[name] = counter;
                }
                counter.Increment(increment);
            }
        }

        /// <summary>
        /// 更新计时器
        /// </summary>
        /// <param name="name">计时器名称</param>
        /// <param name="duration">持续时间</param>
        public void UpdateTimer(string name, TimeSpan duration)
        {
            if (!_isEnabled)
                return;

            lock (_lock)
            {
                if (!_timers.TryGetValue(name, out DiagnosticTimer? timer))
                {
                    timer = new DiagnosticTimer(name);
                    _timers[name] = timer;
                }
                timer.Update(duration);
            }
        }

        /// <summary>
        /// 获取最近的事件
        /// </summary>
        /// <param name="count">事件数量</param>
        /// <returns>最近的事件</returns>
        public IEnumerable<DiagnosticEvent> GetRecentEvents(int count = 100)
        {
            return _events.TakeLast(count);
        }

        /// <summary>
        /// 获取指定类型的事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="count">事件数量</param>
        /// <returns>指定类型的事件</returns>
        public IEnumerable<DiagnosticEvent> GetEventsByType(DiagnosticEventType eventType, int count = 100)
        {
            return _events.Where(e => e.EventType == eventType).TakeLast(count);
        }

        /// <summary>
        /// 获取计数器
        /// </summary>
        /// <returns>所有计数器</returns>
        public IReadOnlyDictionary<string, DiagnosticCounter> GetCounters()
        {
            lock (_lock)
            {
                return new Dictionary<string, DiagnosticCounter>(_counters);
            }
        }

        /// <summary>
        /// 获取计时器
        /// </summary>
        /// <returns>所有计时器</returns>
        public IReadOnlyDictionary<string, DiagnosticTimer> GetTimers()
        {
            lock (_lock)
            {
                return new Dictionary<string, DiagnosticTimer>(_timers);
            }
        }

        /// <summary>
        /// 生成诊断报告
        /// </summary>
        /// <returns>诊断报告</returns>
        public string GenerateReport()
        {
            var report = new StringBuilder();
            report.AppendLine("=== 输入系统诊断报告 ===");
            report.AppendLine($"生成时间: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine($"诊断状态: {(IsEnabled ? "启用" : "禁用")}");
            report.AppendLine($"事件数量: {EventCount}");
            report.AppendLine();

            // 计数器统计
            report.AppendLine("=== 计数器统计 ===");
            lock (_lock)
            {
                foreach (var counter in _counters.Values.OrderByDescending(c => c.Value))
                {
                    report.AppendLine($"{counter.Name}: {counter.Value:N0}");
                }
            }
            report.AppendLine();

            // 计时器统计
            report.AppendLine("=== 性能统计 ===");
            lock (_lock)
            {
                foreach (var timer in _timers.Values.OrderByDescending(t => t.TotalTime))
                {
                    report.AppendLine($"{timer.Name}:");
                    report.AppendLine($"  调用次数: {timer.Count:N0}");
                    report.AppendLine($"  总时间: {timer.TotalTime.TotalMilliseconds:F2}ms");
                    report.AppendLine($"  平均时间: {timer.AverageTime.TotalMilliseconds:F2}ms");
                    report.AppendLine($"  最小时间: {timer.MinTime.TotalMilliseconds:F2}ms");
                    report.AppendLine($"  最大时间: {timer.MaxTime.TotalMilliseconds:F2}ms");
                }
            }
            report.AppendLine();

            // 最近错误
            var recentErrors = GetEventsByType(DiagnosticEventType.Error, 10);
            if (recentErrors.Any())
            {
                report.AppendLine("=== 最近错误 ===");
                foreach (var error in recentErrors)
                {
                    report.AppendLine($"[{error.Timestamp:HH:mm:ss}] {error.Details}");
                }
                report.AppendLine();
            }

            // 最近警告
            var recentWarnings = GetEventsByType(DiagnosticEventType.Warning, 10);
            if (recentWarnings.Any())
            {
                report.AppendLine("=== 最近警告 ===");
                foreach (var warning in recentWarnings)
                {
                    report.AppendLine($"[{warning.Timestamp:HH:mm:ss}] {warning.Details}");
                }
                report.AppendLine();
            }

            return report.ToString();
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void Clear()
        {
            while (_events.TryDequeue(out _)) { }

            lock (_lock)
            {
                _counters.Clear();
                _timers.Clear();
            }
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        private void AddEvent(DiagnosticEvent diagnosticEvent)
        {
            _events.Enqueue(diagnosticEvent);

            // 限制事件数量
            while (_events.Count > _maxEventCount)
            {
                _events.TryDequeue(out _);
            }

            // 触发事件
            DiagnosticEventOccurred?.Invoke(this, new DiagnosticEventArgs(diagnosticEvent));
        }
    }

    /// <summary>
    /// 诊断事件类型
    /// </summary>
    public enum DiagnosticEventType
    {
        /// <summary>
        /// 输入事件
        /// </summary>
        InputEvent,

        /// <summary>
        /// 性能指标
        /// </summary>
        Performance,

        /// <summary>
        /// 错误
        /// </summary>
        Error,

        /// <summary>
        /// 警告
        /// </summary>
        Warning,

        /// <summary>
        /// 信息
        /// </summary>
        Info
    }

    /// <summary>
    /// 诊断事件
    /// </summary>
    public readonly struct DiagnosticEvent
    {
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public DiagnosticEventType EventType { get; }

        /// <summary>
        /// 类别
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// 阶段
        /// </summary>
        public string Stage { get; }

        /// <summary>
        /// 详细信息
        /// </summary>
        public string Details { get; }

        /// <summary>
        /// 初始化诊断事件
        /// </summary>
        public DiagnosticEvent(DateTime timestamp, DiagnosticEventType eventType, string category, string stage, string details)
        {
            Timestamp = timestamp;
            EventType = eventType;
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            Details = details ?? throw new ArgumentNullException(nameof(details));
        }

        public override string ToString() => $"[{Timestamp:HH:mm:ss.fff}] {EventType} {Category}.{Stage}: {Details}";
    }

    /// <summary>
    /// 诊断计数器
    /// </summary>
    public class DiagnosticCounter
    {
        private long _value;

        /// <summary>
        /// 计数器名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 计数器值
        /// </summary>
        public long Value => _value;

        /// <summary>
        /// 初始化诊断计数器
        /// </summary>
        /// <param name="name">计数器名称</param>
        public DiagnosticCounter(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// 增加计数
        /// </summary>
        /// <param name="increment">增量</param>
        public void Increment(long increment = 1)
        {
            System.Threading.Interlocked.Add(ref _value, increment);
        }

        /// <summary>
        /// 重置计数
        /// </summary>
        public void Reset()
        {
            System.Threading.Interlocked.Exchange(ref _value, 0);
        }

        public override string ToString() => $"{Name}: {Value}";
    }

    /// <summary>
    /// 诊断计时器
    /// </summary>
    public class DiagnosticTimer
    {
        private readonly object _lock = new();
        private long _count;
        private TimeSpan _totalTime;
        private TimeSpan _minTime = TimeSpan.MaxValue;
        private TimeSpan _maxTime = TimeSpan.MinValue;

        /// <summary>
        /// 计时器名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 调用次数
        /// </summary>
        public long Count => _count;

        /// <summary>
        /// 总时间
        /// </summary>
        public TimeSpan TotalTime => _totalTime;

        /// <summary>
        /// 平均时间
        /// </summary>
        public TimeSpan AverageTime => _count > 0 ? TimeSpan.FromTicks(_totalTime.Ticks / _count) : TimeSpan.Zero;

        /// <summary>
        /// 最小时间
        /// </summary>
        public TimeSpan MinTime => _minTime == TimeSpan.MaxValue ? TimeSpan.Zero : _minTime;

        /// <summary>
        /// 最大时间
        /// </summary>
        public TimeSpan MaxTime => _maxTime == TimeSpan.MinValue ? TimeSpan.Zero : _maxTime;

        /// <summary>
        /// 初始化诊断计时器
        /// </summary>
        /// <param name="name">计时器名称</param>
        public DiagnosticTimer(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// 更新计时器
        /// </summary>
        /// <param name="duration">持续时间</param>
        public void Update(TimeSpan duration)
        {
            lock (_lock)
            {
                _count++;
                _totalTime = _totalTime.Add(duration);
                
                if (duration < _minTime)
                    _minTime = duration;
                
                if (duration > _maxTime)
                    _maxTime = duration;
            }
        }

        /// <summary>
        /// 重置计时器
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _count = 0;
                _totalTime = TimeSpan.Zero;
                _minTime = TimeSpan.MaxValue;
                _maxTime = TimeSpan.MinValue;
            }
        }

        public override string ToString() => $"{Name}: {Count} calls, avg {AverageTime.TotalMilliseconds:F2}ms";
    }

    /// <summary>
    /// 诊断秒表
    /// </summary>
    internal class DiagnosticStopwatch : IDisposable
    {
        private readonly InputDiagnostics _diagnostics;
        private readonly string _operation;
        private readonly Stopwatch _stopwatch;

        public DiagnosticStopwatch(InputDiagnostics diagnostics, string operation)
        {
            _diagnostics = diagnostics;
            _operation = operation;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _diagnostics.LogPerformance(_operation, _stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// 诊断事件参数
    /// </summary>
    public class DiagnosticEventArgs : EventArgs
    {
        /// <summary>
        /// 诊断事件
        /// </summary>
        public DiagnosticEvent Event { get; }

        /// <summary>
        /// 初始化诊断事件参数
        /// </summary>
        /// <param name="diagnosticEvent">诊断事件</param>
        public DiagnosticEventArgs(DiagnosticEvent diagnosticEvent)
        {
            Event = diagnosticEvent;
        }
    }
}
