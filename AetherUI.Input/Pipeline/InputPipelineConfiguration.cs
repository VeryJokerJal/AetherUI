using System;

namespace AetherUI.Input.Pipeline
{
    /// <summary>
    /// 输入管道配置
    /// </summary>
    public class InputPipelineConfiguration
    {
        /// <summary>
        /// 默认配置
        /// </summary>
        public static InputPipelineConfiguration Default => new();

        /// <summary>
        /// 最大队列大小
        /// </summary>
        public int MaxQueueSize { get; set; } = 1000;

        /// <summary>
        /// 处理间隔（毫秒）
        /// </summary>
        public int ProcessingIntervalMs { get; set; } = 1;

        /// <summary>
        /// 是否在手势处理后停止路由
        /// </summary>
        public bool StopRoutingAfterGesture { get; set; } = false;

        /// <summary>
        /// 是否启用异步处理
        /// </summary>
        public bool EnableAsyncProcessing { get; set; } = true;

        /// <summary>
        /// 处理超时时间（毫秒）
        /// </summary>
        public int ProcessingTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// 是否启用性能监控
        /// </summary>
        public bool EnablePerformanceMonitoring { get; set; } = true;

        /// <summary>
        /// 是否启用错误恢复
        /// </summary>
        public bool EnableErrorRecovery { get; set; } = true;

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// 批处理大小
        /// </summary>
        public int BatchSize { get; set; } = 10;

        /// <summary>
        /// 是否启用事件合并
        /// </summary>
        public bool EnableEventCoalescing { get; set; } = true;

        /// <summary>
        /// 事件合并窗口（毫秒）
        /// </summary>
        public int CoalescingWindowMs { get; set; } = 16; // ~60fps

        /// <summary>
        /// 创建高性能配置
        /// </summary>
        /// <returns>高性能配置</returns>
        public static InputPipelineConfiguration CreateHighPerformance()
        {
            return new InputPipelineConfiguration
            {
                MaxQueueSize = 2000,
                ProcessingIntervalMs = 0,
                EnableAsyncProcessing = true,
                EnablePerformanceMonitoring = true,
                BatchSize = 20,
                EnableEventCoalescing = true,
                CoalescingWindowMs = 8 // ~120fps
            };
        }

        /// <summary>
        /// 创建低延迟配置
        /// </summary>
        /// <returns>低延迟配置</returns>
        public static InputPipelineConfiguration CreateLowLatency()
        {
            return new InputPipelineConfiguration
            {
                MaxQueueSize = 500,
                ProcessingIntervalMs = 0,
                EnableAsyncProcessing = false,
                EnablePerformanceMonitoring = false,
                BatchSize = 1,
                EnableEventCoalescing = false
            };
        }

        /// <summary>
        /// 创建调试配置
        /// </summary>
        /// <returns>调试配置</returns>
        public static InputPipelineConfiguration CreateDebug()
        {
            return new InputPipelineConfiguration
            {
                MaxQueueSize = 100,
                ProcessingIntervalMs = 10,
                EnableAsyncProcessing = true,
                EnablePerformanceMonitoring = true,
                EnableErrorRecovery = true,
                MaxRetryCount = 1,
                BatchSize = 1,
                EnableEventCoalescing = false
            };
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        /// <returns>验证结果</returns>
        public ValidationResult Validate()
        {
            var result = new ValidationResult();

            if (MaxQueueSize <= 0)
            {
                result.AddError("MaxQueueSize 必须大于 0");
            }

            if (ProcessingIntervalMs < 0)
            {
                result.AddError("ProcessingIntervalMs 不能为负数");
            }

            if (ProcessingTimeoutMs <= 0)
            {
                result.AddError("ProcessingTimeoutMs 必须大于 0");
            }

            if (MaxRetryCount < 0)
            {
                result.AddError("MaxRetryCount 不能为负数");
            }

            if (BatchSize <= 0)
            {
                result.AddError("BatchSize 必须大于 0");
            }

            if (CoalescingWindowMs < 0)
            {
                result.AddError("CoalescingWindowMs 不能为负数");
            }

            return result;
        }

        /// <summary>
        /// 克隆配置
        /// </summary>
        /// <returns>配置副本</returns>
        public InputPipelineConfiguration Clone()
        {
            return new InputPipelineConfiguration
            {
                MaxQueueSize = MaxQueueSize,
                ProcessingIntervalMs = ProcessingIntervalMs,
                StopRoutingAfterGesture = StopRoutingAfterGesture,
                EnableAsyncProcessing = EnableAsyncProcessing,
                ProcessingTimeoutMs = ProcessingTimeoutMs,
                EnablePerformanceMonitoring = EnablePerformanceMonitoring,
                EnableErrorRecovery = EnableErrorRecovery,
                MaxRetryCount = MaxRetryCount,
                BatchSize = BatchSize,
                EnableEventCoalescing = EnableEventCoalescing,
                CoalescingWindowMs = CoalescingWindowMs
            };
        }

        public override string ToString() =>
            $"InputPipelineConfig: Queue={MaxQueueSize}, Interval={ProcessingIntervalMs}ms, " +
            $"Async={EnableAsyncProcessing}, Batch={BatchSize}, Coalescing={EnableEventCoalescing}";
    }

    /// <summary>
    /// 验证结果
    /// </summary>
    public class ValidationResult
    {
        private readonly System.Collections.Generic.List<string> _errors = new();
        private readonly System.Collections.Generic.List<string> _warnings = new();

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid => _errors.Count == 0;

        /// <summary>
        /// 错误列表
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<string> Errors => _errors;

        /// <summary>
        /// 警告列表
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<string> Warnings => _warnings;

        /// <summary>
        /// 添加错误
        /// </summary>
        /// <param name="error">错误信息</param>
        public void AddError(string error)
        {
            _errors.Add(error);
        }

        /// <summary>
        /// 添加警告
        /// </summary>
        /// <param name="warning">警告信息</param>
        public void AddWarning(string warning)
        {
            _warnings.Add(warning);
        }

        public override string ToString()
        {
            if (IsValid)
                return "配置有效";

            return $"配置无效: {_errors.Count} 个错误, {_warnings.Count} 个警告";
        }
    }

    /// <summary>
    /// 输入统计信息
    /// </summary>
    public class InputStatistics
    {
        /// <summary>
        /// 总提交事件数
        /// </summary>
        public long TotalEventsSubmitted { get; set; }

        /// <summary>
        /// 总处理事件数
        /// </summary>
        public long TotalEventsProcessed { get; set; }

        /// <summary>
        /// 总错误数
        /// </summary>
        public long TotalErrors { get; set; }

        /// <summary>
        /// 总处理时间
        /// </summary>
        public TimeSpan TotalProcessingTime { get; set; }

        /// <summary>
        /// 当前队列大小
        /// </summary>
        public int QueueSize { get; set; }

        /// <summary>
        /// 处理器数量
        /// </summary>
        public int ProcessorCount { get; set; }

        /// <summary>
        /// 平均处理时间
        /// </summary>
        public TimeSpan AverageProcessingTime =>
            TotalEventsProcessed > 0 ? 
            TimeSpan.FromTicks(TotalProcessingTime.Ticks / TotalEventsProcessed) : 
            TimeSpan.Zero;

        /// <summary>
        /// 处理成功率
        /// </summary>
        public double SuccessRate =>
            TotalEventsSubmitted > 0 ? 
            (double)(TotalEventsProcessed - TotalErrors) / TotalEventsSubmitted : 
            0.0;

        /// <summary>
        /// 错误率
        /// </summary>
        public double ErrorRate =>
            TotalEventsSubmitted > 0 ? 
            (double)TotalErrors / TotalEventsSubmitted : 
            0.0;

        /// <summary>
        /// 处理吞吐量（事件/秒）
        /// </summary>
        public double Throughput =>
            TotalProcessingTime.TotalSeconds > 0 ? 
            TotalEventsProcessed / TotalProcessingTime.TotalSeconds : 
            0.0;

        /// <summary>
        /// 重置统计
        /// </summary>
        public void Reset()
        {
            TotalEventsSubmitted = 0;
            TotalEventsProcessed = 0;
            TotalErrors = 0;
            TotalProcessingTime = TimeSpan.Zero;
            QueueSize = 0;
        }

        /// <summary>
        /// 获取详细报告
        /// </summary>
        /// <returns>统计报告</returns>
        public string GetDetailedReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== 输入管道统计报告 ===");
            report.AppendLine($"提交事件数: {TotalEventsSubmitted:N0}");
            report.AppendLine($"处理事件数: {TotalEventsProcessed:N0}");
            report.AppendLine($"错误数: {TotalErrors:N0}");
            report.AppendLine($"成功率: {SuccessRate:P2}");
            report.AppendLine($"错误率: {ErrorRate:P2}");
            report.AppendLine($"总处理时间: {TotalProcessingTime.TotalMilliseconds:F2}ms");
            report.AppendLine($"平均处理时间: {AverageProcessingTime.TotalMilliseconds:F2}ms");
            report.AppendLine($"吞吐量: {Throughput:F2} 事件/秒");
            report.AppendLine($"当前队列大小: {QueueSize}");
            report.AppendLine($"处理器数量: {ProcessorCount}");
            return report.ToString();
        }

        public override string ToString() =>
            $"Events: {TotalEventsProcessed}/{TotalEventsSubmitted}, " +
            $"Errors: {TotalErrors}, " +
            $"AvgTime: {AverageProcessingTime.TotalMilliseconds:F2}ms, " +
            $"Queue: {QueueSize}";
    }
}
