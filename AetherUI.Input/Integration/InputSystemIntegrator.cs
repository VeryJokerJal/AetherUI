using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AetherUI.Input.Accessibility;
using AetherUI.Input.Core;
using AetherUI.Input.Diagnostics;
using AetherUI.Input.Events;
using AetherUI.Input.Gestures;
using AetherUI.Input.HitTesting;
using AetherUI.Input.Pipeline;
using AetherUI.Input.Platform.CrossPlatform;
using AetherUI.Input.Routing;
using AetherUI.Input.TextInput;

namespace AetherUI.Input.Integration
{
    /// <summary>
    /// 输入系统集成器
    /// </summary>
    public class InputSystemIntegrator : IDisposable
    {
        private readonly InputSystemConfiguration _configuration;
        private IPlatformInputProvider? _platformProvider;
        private InputPipeline? _inputPipeline;
        private AdvancedHitTestEngine? _hitTestEngine;
        private AdvancedEventRouter? _eventRouter;
        private AdvancedGestureEngine? _gestureEngine;
        private TextInputManager? _textInputManager;
        private AccessibilityManager? _accessibilityManager;
        private InputDiagnostics? _diagnostics;
        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 平台提供者
        /// </summary>
        public IPlatformInputProvider? PlatformProvider => _platformProvider;

        /// <summary>
        /// 输入管道
        /// </summary>
        public InputPipeline? InputPipeline => _inputPipeline;

        /// <summary>
        /// 命中测试引擎
        /// </summary>
        public AdvancedHitTestEngine? HitTestEngine => _hitTestEngine;

        /// <summary>
        /// 事件路由器
        /// </summary>
        public AdvancedEventRouter? EventRouter => _eventRouter;

        /// <summary>
        /// 手势引擎
        /// </summary>
        public AdvancedGestureEngine? GestureEngine => _gestureEngine;

        /// <summary>
        /// 文本输入管理器
        /// </summary>
        public TextInputManager? TextInputManager => _textInputManager;

        /// <summary>
        /// 无障碍管理器
        /// </summary>
        public AccessibilityManager? AccessibilityManager => _accessibilityManager;

        /// <summary>
        /// 诊断系统
        /// </summary>
        public InputDiagnostics? Diagnostics => _diagnostics;

        /// <summary>
        /// 系统就绪事件
        /// </summary>
        public event EventHandler? SystemReady;

        /// <summary>
        /// 系统错误事件
        /// </summary>
        public event EventHandler<SystemErrorEventArgs>? SystemError;

        /// <summary>
        /// 初始化输入系统集成器
        /// </summary>
        /// <param name="configuration">系统配置</param>
        public InputSystemIntegrator(InputSystemConfiguration? configuration = null)
        {
            _configuration = configuration ?? InputSystemConfiguration.Default;
        }

        /// <summary>
        /// 异步初始化输入系统
        /// </summary>
        /// <returns>是否成功初始化</returns>
        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized || _isDisposed)
                return false;

            try
            {
                Debug.WriteLine("开始初始化输入系统...");

                // 1. 初始化诊断系统
                if (_configuration.EnableDiagnostics)
                {
                    _diagnostics = new InputDiagnostics(_configuration.MaxDiagnosticEvents);
                    Debug.WriteLine("诊断系统已初始化");
                }

                // 2. 初始化平台提供者
                if (!await InitializePlatformProviderAsync())
                {
                    OnSystemError(new SystemErrorEventArgs("平台提供者初始化失败", null));
                    return false;
                }

                // 3. 初始化核心组件
                InitializeCoreComponents();

                // 4. 初始化扩展组件
                InitializeExtendedComponents();

                // 5. 连接组件
                ConnectComponents();

                // 6. 启动系统
                if (!StartSystem())
                {
                    OnSystemError(new SystemErrorEventArgs("系统启动失败", null));
                    return false;
                }

                _isInitialized = true;
                Debug.WriteLine("输入系统初始化完成");

                OnSystemReady();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"输入系统初始化失败: {ex.Message}");
                OnSystemError(new SystemErrorEventArgs("初始化异常", ex));
                return false;
            }
        }

        /// <summary>
        /// 设置根元素
        /// </summary>
        /// <param name="rootElement">根元素</param>
        public void SetRootElement(IHitTestable rootElement)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("系统尚未初始化");

            _inputPipeline?.SetRootElement(rootElement);
            Debug.WriteLine($"根元素已设置: {rootElement}");
        }

        /// <summary>
        /// 获取系统状态
        /// </summary>
        /// <returns>系统状态</returns>
        public InputSystemStatus GetSystemStatus()
        {
            return new InputSystemStatus
            {
                IsInitialized = _isInitialized,
                Platform = PlatformAbstraction.CurrentPlatform,
                Capabilities = PlatformAbstraction.GetCapabilities(),
                PlatformProviderRunning = _platformProvider?.IsRunning ?? false,
                InputPipelineStatistics = _inputPipeline?.GetStatistics(),
                HitTestStatistics = _hitTestEngine?.GetStatistics(),
                GestureStatistics = _gestureEngine?.GetStatistics(),
                DiagnosticsEnabled = _diagnostics?.IsEnabled ?? false
            };
        }

        /// <summary>
        /// 生成系统报告
        /// </summary>
        /// <returns>系统报告</returns>
        public string GenerateSystemReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== AetherUI 输入系统报告 ===");
            report.AppendLine($"生成时间: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine();

            var status = GetSystemStatus();
            report.AppendLine("=== 系统状态 ===");
            report.AppendLine($"已初始化: {status.IsInitialized}");
            report.AppendLine($"平台: {status.Platform}");
            report.AppendLine($"平台能力: {status.Capabilities}");
            report.AppendLine($"平台提供者运行: {status.PlatformProviderRunning}");
            report.AppendLine();

            if (status.InputPipelineStatistics != null)
            {
                report.AppendLine("=== 输入管道统计 ===");
                report.AppendLine(status.InputPipelineStatistics.GetDetailedReport());
            }

            if (status.HitTestStatistics != null)
            {
                report.AppendLine("=== 命中测试统计 ===");
                report.AppendLine(status.HitTestStatistics.ToString());
                report.AppendLine();
            }

            if (status.GestureStatistics != null)
            {
                report.AppendLine("=== 手势识别统计 ===");
                report.AppendLine(status.GestureStatistics.ToString());
                report.AppendLine();
            }

            if (_diagnostics != null)
            {
                report.AppendLine(_diagnostics.GenerateReport());
            }

            return report.ToString();
        }

        /// <summary>
        /// 初始化平台提供者
        /// </summary>
        private async Task<bool> InitializePlatformProviderAsync()
        {
            if (!PlatformAbstraction.Initialize())
            {
                return false;
            }

            _platformProvider = PlatformAbstraction.CurrentProvider;
            if (_platformProvider == null)
            {
                return false;
            }

            // 订阅平台输入事件
            _platformProvider.InputReceived += OnPlatformInputReceived;

            Debug.WriteLine($"平台提供者已初始化: {_platformProvider.Platform}");
            await Task.CompletedTask;
            return true;
        }

        /// <summary>
        /// 初始化核心组件
        /// </summary>
        private void InitializeCoreComponents()
        {
            // 命中测试引擎
            var hitTestOptions = new HitTestOptions
            {
                UseBoundingBoxOnly = _configuration.HitTestUseBoundingBoxOnly,
                MaxDepth = _configuration.HitTestMaxDepth
            };
            _hitTestEngine = new AdvancedHitTestEngine(hitTestOptions);

            // 事件路由器
            _eventRouter = new AdvancedEventRouter();

            // 手势引擎
            var gestureConfig = new GestureConfiguration
            {
                TapTimeoutMs = _configuration.TapTimeoutMs,
                LongPressDelayMs = _configuration.LongPressDelayMs,
                DragThreshold = _configuration.DragThreshold
            };
            _gestureEngine = new AdvancedGestureEngine(gestureConfig);

            Debug.WriteLine("核心组件已初始化");
        }

        /// <summary>
        /// 初始化扩展组件
        /// </summary>
        private void InitializeExtendedComponents()
        {
            // 文本输入管理器
            if (_configuration.EnableTextInput)
            {
                _textInputManager = new TextInputManager();
                Debug.WriteLine("文本输入管理器已初始化");
            }

            // 无障碍管理器
            if (_configuration.EnableAccessibility)
            {
                _accessibilityManager = new AccessibilityManager();
                Debug.WriteLine("无障碍管理器已初始化");
            }
        }

        /// <summary>
        /// 连接组件
        /// </summary>
        private void ConnectComponents()
        {
            // 创建输入管道
            var pipelineConfig = new InputPipelineConfiguration
            {
                MaxQueueSize = _configuration.MaxInputQueueSize,
                ProcessingIntervalMs = _configuration.ProcessingIntervalMs,
                EnableAsyncProcessing = _configuration.EnableAsyncProcessing
            };

            _inputPipeline = new InputPipeline(pipelineConfig, _hitTestEngine, _eventRouter, _gestureEngine);

            // 添加诊断处理器
            if (_diagnostics != null)
            {
                _inputPipeline.AddProcessor(new DiagnosticInputProcessor(_diagnostics));
            }

            // 添加无障碍处理器
            if (_accessibilityManager != null)
            {
                _inputPipeline.AddProcessor(new AccessibilityInputProcessor(_accessibilityManager));
            }

            Debug.WriteLine("组件连接完成");
        }

        /// <summary>
        /// 启动系统
        /// </summary>
        private bool StartSystem()
        {
            if (_platformProvider == null)
                return false;

            return _platformProvider.Start();
        }

        /// <summary>
        /// 处理平台输入事件
        /// </summary>
        private void OnPlatformInputReceived(object? sender, InputReceivedEventArgs e)
        {
            try
            {
                _inputPipeline?.SubmitEvent(e.InputEvent);
                _diagnostics?.LogInputEvent(e.InputEvent, "PlatformReceived");
            }
            catch (Exception ex)
            {
                _diagnostics?.LogError("处理平台输入事件失败", ex);
            }
        }

        /// <summary>
        /// 触发系统就绪事件
        /// </summary>
        private void OnSystemReady()
        {
            SystemReady?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 触发系统错误事件
        /// </summary>
        private void OnSystemError(SystemErrorEventArgs e)
        {
            SystemError?.Invoke(this, e);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            try
            {
                _inputPipeline?.Dispose();
                _platformProvider?.Dispose();
                PlatformAbstraction.Shutdown();

                Debug.WriteLine("输入系统已释放");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"释放输入系统失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 输入系统配置
    /// </summary>
    public class InputSystemConfiguration
    {
        /// <summary>
        /// 默认配置
        /// </summary>
        public static InputSystemConfiguration Default => new();

        // 核心配置
        public bool EnableDiagnostics { get; set; } = true;
        public bool EnableTextInput { get; set; } = true;
        public bool EnableAccessibility { get; set; } = true;
        public bool EnableAsyncProcessing { get; set; } = true;

        // 管道配置
        public int MaxInputQueueSize { get; set; } = 1000;
        public int ProcessingIntervalMs { get; set; } = 1;
        public int MaxDiagnosticEvents { get; set; } = 10000;

        // 命中测试配置
        public bool HitTestUseBoundingBoxOnly { get; set; } = false;
        public int HitTestMaxDepth { get; set; } = 50;

        // 手势配置
        public uint TapTimeoutMs { get; set; } = 300;
        public uint LongPressDelayMs { get; set; } = 800;
        public double DragThreshold { get; set; } = 10.0;
    }

    /// <summary>
    /// 输入系统状态
    /// </summary>
    public class InputSystemStatus
    {
        public bool IsInitialized { get; set; }
        public PlatformType Platform { get; set; }
        public PlatformCapabilities? Capabilities { get; set; }
        public bool PlatformProviderRunning { get; set; }
        public InputStatistics? InputPipelineStatistics { get; set; }
        public HitTestStatistics? HitTestStatistics { get; set; }
        public GestureStatistics? GestureStatistics { get; set; }
        public bool DiagnosticsEnabled { get; set; }
    }

    /// <summary>
    /// 系统错误事件参数
    /// </summary>
    public class SystemErrorEventArgs : EventArgs
    {
        public string Message { get; }
        public Exception? Exception { get; }

        public SystemErrorEventArgs(string message, Exception? exception)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Exception = exception;
        }

        public override string ToString() => Exception != null ? $"{Message}: {Exception.Message}" : Message;
    }

    /// <summary>
    /// 诊断输入处理器
    /// </summary>
    public class DiagnosticInputProcessor : IInputProcessor
    {
        private readonly InputDiagnostics _diagnostics;

        public int Priority => 1;
        public string Name => "Diagnostic";

        public DiagnosticInputProcessor(InputDiagnostics diagnostics)
        {
            _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        }

        public bool CanProcess(InputEvent inputEvent) => true;

        public async Task<InputProcessingResult> ProcessAsync(InputEvent inputEvent, InputProcessingContext context)
        {
            _diagnostics.LogInputEvent(inputEvent, "Processing", $"Context: {context.GetType().Name}");
            await Task.CompletedTask;
            return InputProcessingResult.CreateSuccess("诊断记录完成");
        }
    }

    /// <summary>
    /// 无障碍输入处理器
    /// </summary>
    public class AccessibilityInputProcessor : IInputProcessor
    {
        private readonly AccessibilityManager _accessibilityManager;

        public int Priority => 2;
        public string Name => "Accessibility";

        public AccessibilityInputProcessor(AccessibilityManager accessibilityManager)
        {
            _accessibilityManager = accessibilityManager ?? throw new ArgumentNullException(nameof(accessibilityManager));
        }

        public bool CanProcess(InputEvent inputEvent) => true;

        public async Task<InputProcessingResult> ProcessAsync(InputEvent inputEvent, InputProcessingContext context)
        {
            var targetElement = context.HitTestResult?.HitElement;
            if (targetElement != null)
            {
                _accessibilityManager.ProcessInputAccessibility(inputEvent, targetElement);
            }

            await Task.CompletedTask;
            return InputProcessingResult.CreateSuccess("无障碍处理完成");
        }
    }
}
