using System;
using System.Collections.Generic;
using AetherUI.Input.Core;
using AetherUI.Input.Events;
using AetherUI.Input.HitTesting;

namespace AetherUI.Input.Diagnostics
{
    /// <summary>
    /// 输入诊断接口
    /// </summary>
    public interface IInputDiagnostics
    {
        /// <summary>
        /// 是否启用诊断
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        InputEventLogLevel LogLevel { get; set; }

        /// <summary>
        /// 记录输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <param name="stage">处理阶段</param>
        /// <param name="target">目标元素</param>
        /// <param name="additionalInfo">附加信息</param>
        void LogInputEvent(InputEvent inputEvent, InputProcessingStage stage, object? target = null, string? additionalInfo = null);

        /// <summary>
        /// 记录命中测试
        /// </summary>
        /// <param name="point">测试点</param>
        /// <param name="result">测试结果</param>
        /// <param name="processingTimeMs">处理时间</param>
        void LogHitTest(Point point, HitTestResult result, double processingTimeMs);

        /// <summary>
        /// 记录事件路由
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <param name="routePath">路由路径</param>
        /// <param name="handledBy">处理者</param>
        void LogEventRouting(InputEvent inputEvent, IReadOnlyList<object> routePath, object? handledBy = null);

        /// <summary>
        /// 记录性能指标
        /// </summary>
        /// <param name="metricName">指标名称</param>
        /// <param name="value">指标值</param>
        /// <param name="unit">单位</param>
        void LogPerformanceMetric(string metricName, double value, string unit = "ms");

        /// <summary>
        /// 获取事件统计
        /// </summary>
        /// <returns>事件统计信息</returns>
        InputEventStatistics GetEventStatistics();

        /// <summary>
        /// 获取性能统计
        /// </summary>
        /// <returns>性能统计信息</returns>
        PerformanceStatistics GetPerformanceStatistics();

        /// <summary>
        /// 清除统计数据
        /// </summary>
        void ClearStatistics();

        /// <summary>
        /// 导出诊断数据
        /// </summary>
        /// <param name="format">导出格式</param>
        /// <returns>诊断数据</returns>
        string ExportDiagnostics(DiagnosticsExportFormat format = DiagnosticsExportFormat.Json);
    }

    /// <summary>
    /// 输入处理阶段
    /// </summary>
    public enum InputProcessingStage
    {
        /// <summary>
        /// 原始输入接收
        /// </summary>
        RawInputReceived,

        /// <summary>
        /// 事件标准化
        /// </summary>
        EventNormalized,

        /// <summary>
        /// 命中测试
        /// </summary>
        HitTesting,

        /// <summary>
        /// 捕获处理
        /// </summary>
        CaptureProcessing,

        /// <summary>
        /// 手势识别
        /// </summary>
        GestureRecognition,

        /// <summary>
        /// 事件路由开始
        /// </summary>
        RoutingStarted,

        /// <summary>
        /// 隧道阶段
        /// </summary>
        TunnelPhase,

        /// <summary>
        /// 直接阶段
        /// </summary>
        DirectPhase,

        /// <summary>
        /// 冒泡阶段
        /// </summary>
        BubblePhase,

        /// <summary>
        /// 事件处理完成
        /// </summary>
        EventHandled,

        /// <summary>
        /// 焦点处理
        /// </summary>
        FocusProcessing,

        /// <summary>
        /// 处理完成
        /// </summary>
        ProcessingCompleted
    }

    /// <summary>
    /// 输入事件统计
    /// </summary>
    public class InputEventStatistics
    {
        /// <summary>
        /// 总事件数
        /// </summary>
        public long TotalEvents { get; set; }

        /// <summary>
        /// 按类型分组的事件数
        /// </summary>
        public Dictionary<string, long> EventsByType { get; set; } = new();

        /// <summary>
        /// 已处理事件数
        /// </summary>
        public long HandledEvents { get; set; }

        /// <summary>
        /// 未处理事件数
        /// </summary>
        public long UnhandledEvents { get; set; }

        /// <summary>
        /// 错误事件数
        /// </summary>
        public long ErrorEvents { get; set; }

        /// <summary>
        /// 平均处理时间（毫秒）
        /// </summary>
        public double AverageProcessingTimeMs { get; set; }

        /// <summary>
        /// 最大处理时间（毫秒）
        /// </summary>
        public double MaxProcessingTimeMs { get; set; }

        /// <summary>
        /// 最小处理时间（毫秒）
        /// </summary>
        public double MinProcessingTimeMs { get; set; }

        /// <summary>
        /// 统计开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 统计结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 处理率（事件/秒）
        /// </summary>
        public double EventsPerSecond =>
            (EndTime - StartTime).TotalSeconds > 0 ? TotalEvents / (EndTime - StartTime).TotalSeconds : 0;

        /// <summary>
        /// 处理成功率
        /// </summary>
        public double SuccessRate =>
            TotalEvents > 0 ? (double)HandledEvents / TotalEvents : 0;
    }

    /// <summary>
    /// 性能统计
    /// </summary>
    public class PerformanceStatistics
    {
        /// <summary>
        /// 命中测试统计
        /// </summary>
        public PerformanceMetric HitTestPerformance { get; set; } = new();

        /// <summary>
        /// 事件路由统计
        /// </summary>
        public PerformanceMetric EventRoutingPerformance { get; set; } = new();

        /// <summary>
        /// 手势识别统计
        /// </summary>
        public PerformanceMetric GestureRecognitionPerformance { get; set; } = new();

        /// <summary>
        /// 焦点管理统计
        /// </summary>
        public PerformanceMetric FocusManagementPerformance { get; set; } = new();

        /// <summary>
        /// 内存使用统计
        /// </summary>
        public MemoryUsageStatistics MemoryUsage { get; set; } = new();
    }

    /// <summary>
    /// 性能指标
    /// </summary>
    public class PerformanceMetric
    {
        /// <summary>
        /// 总调用次数
        /// </summary>
        public long TotalCalls { get; set; }

        /// <summary>
        /// 平均时间（毫秒）
        /// </summary>
        public double AverageTimeMs { get; set; }

        /// <summary>
        /// 最大时间（毫秒）
        /// </summary>
        public double MaxTimeMs { get; set; }

        /// <summary>
        /// 最小时间（毫秒）
        /// </summary>
        public double MinTimeMs { get; set; }

        /// <summary>
        /// 总时间（毫秒）
        /// </summary>
        public double TotalTimeMs { get; set; }

        /// <summary>
        /// 95百分位时间（毫秒）
        /// </summary>
        public double P95TimeMs { get; set; }

        /// <summary>
        /// 99百分位时间（毫秒）
        /// </summary>
        public double P99TimeMs { get; set; }
    }

    /// <summary>
    /// 内存使用统计
    /// </summary>
    public class MemoryUsageStatistics
    {
        /// <summary>
        /// 当前内存使用（字节）
        /// </summary>
        public long CurrentMemoryBytes { get; set; }

        /// <summary>
        /// 峰值内存使用（字节）
        /// </summary>
        public long PeakMemoryBytes { get; set; }

        /// <summary>
        /// GC回收次数
        /// </summary>
        public long GCCollections { get; set; }

        /// <summary>
        /// 对象池命中率
        /// </summary>
        public double ObjectPoolHitRate { get; set; }

        /// <summary>
        /// 缓存命中率
        /// </summary>
        public double CacheHitRate { get; set; }
    }

    /// <summary>
    /// 诊断导出格式
    /// </summary>
    public enum DiagnosticsExportFormat
    {
        /// <summary>
        /// JSON格式
        /// </summary>
        Json,

        /// <summary>
        /// XML格式
        /// </summary>
        Xml,

        /// <summary>
        /// CSV格式
        /// </summary>
        Csv,

        /// <summary>
        /// 纯文本格式
        /// </summary>
        Text
    }

    /// <summary>
    /// 输入事件跟踪器接口
    /// </summary>
    public interface IInputEventTracker
    {
        /// <summary>
        /// 开始跟踪事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <returns>跟踪ID</returns>
        Guid StartTracking(InputEvent inputEvent);

        /// <summary>
        /// 记录处理阶段
        /// </summary>
        /// <param name="trackingId">跟踪ID</param>
        /// <param name="stage">处理阶段</param>
        /// <param name="target">目标元素</param>
        /// <param name="additionalInfo">附加信息</param>
        void RecordStage(Guid trackingId, InputProcessingStage stage, object? target = null, string? additionalInfo = null);

        /// <summary>
        /// 结束跟踪
        /// </summary>
        /// <param name="trackingId">跟踪ID</param>
        /// <param name="isHandled">是否已处理</param>
        void EndTracking(Guid trackingId, bool isHandled);

        /// <summary>
        /// 获取跟踪信息
        /// </summary>
        /// <param name="trackingId">跟踪ID</param>
        /// <returns>跟踪信息</returns>
        InputEventTrace? GetTrace(Guid trackingId);

        /// <summary>
        /// 获取所有活动跟踪
        /// </summary>
        /// <returns>活动跟踪集合</returns>
        IEnumerable<InputEventTrace> GetActiveTraces();

        /// <summary>
        /// 清除跟踪历史
        /// </summary>
        void ClearHistory();
    }

    /// <summary>
    /// 输入事件跟踪信息
    /// </summary>
    public class InputEventTrace
    {
        /// <summary>
        /// 跟踪ID
        /// </summary>
        public Guid TrackingId { get; set; }

        /// <summary>
        /// 输入事件
        /// </summary>
        public InputEvent InputEvent { get; set; } = null!;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool? IsHandled { get; set; }

        /// <summary>
        /// 处理阶段记录
        /// </summary>
        public List<StageRecord> Stages { get; set; } = new();

        /// <summary>
        /// 总处理时间（毫秒）
        /// </summary>
        public double TotalProcessingTimeMs =>
            EndTime.HasValue ? (EndTime.Value - StartTime).TotalMilliseconds : 0;

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsCompleted => EndTime.HasValue;
    }

    /// <summary>
    /// 处理阶段记录
    /// </summary>
    public class StageRecord
    {
        /// <summary>
        /// 处理阶段
        /// </summary>
        public InputProcessingStage Stage { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 目标元素
        /// </summary>
        public object? Target { get; set; }

        /// <summary>
        /// 附加信息
        /// </summary>
        public string? AdditionalInfo { get; set; }

        /// <summary>
        /// 处理时间（毫秒）
        /// </summary>
        public double ProcessingTimeMs { get; set; }
    }
}
