using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AetherUI.Input.Core;
using AetherUI.Input.HitTesting;
using AetherUI.Input.Integration;

namespace AetherUI.Core.Input
{
    /// <summary>
    /// 布局输入适配器 - 连接AetherUI.Input和AetherUI.Core布局系统
    /// </summary>
    public class LayoutInputAdapter : IDisposable
    {
        private readonly InputSystemIntegrator _inputSystem;
        private readonly LayoutInputConfiguration _configuration;
        private UIElement? _rootElement;
        private readonly Dictionary<UIElement, LayoutElementAdapter> _elementAdapters = new();
        private bool _isDisposed;

        /// <summary>
        /// 输入系统集成器
        /// </summary>
        public InputSystemIntegrator InputSystem => _inputSystem;

        /// <summary>
        /// 根元素
        /// </summary>
        public UIElement? RootElement => _rootElement;

        /// <summary>
        /// 输入事件
        /// </summary>
        public event EventHandler<LayoutInputEventArgs>? InputEvent;

        /// <summary>
        /// 初始化布局输入适配器
        /// </summary>
        /// <param name="configuration">配置</param>
        public LayoutInputAdapter(LayoutInputConfiguration? configuration = null)
        {
            _configuration = configuration ?? LayoutInputConfiguration.Default;
            
            // 创建输入系统配置
            var inputConfig = new InputSystemConfiguration
            {
                EnableDiagnostics = _configuration.EnableDiagnostics,
                EnableTextInput = _configuration.EnableTextInput,
                EnableAccessibility = _configuration.EnableAccessibility,
                EnableAsyncProcessing = _configuration.EnableAsyncProcessing,
                MaxInputQueueSize = _configuration.MaxInputQueueSize,
                ProcessingIntervalMs = _configuration.ProcessingIntervalMs,
                HitTestUseBoundingBoxOnly = _configuration.HitTestUseBoundingBoxOnly,
                HitTestMaxDepth = _configuration.HitTestMaxDepth
            };

            _inputSystem = new InputSystemIntegrator(inputConfig);
            
            // 订阅输入系统事件
            _inputSystem.SystemReady += OnInputSystemReady;
            _inputSystem.SystemError += OnInputSystemError;
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        /// <returns>是否成功初始化</returns>
        public async System.Threading.Tasks.Task<bool> InitializeAsync()
        {
            if (_isDisposed)
                return false;

            var success = await _inputSystem.InitializeAsync();
            if (success)
            {
                Debug.WriteLine("布局输入适配器初始化成功");
            }
            else
            {
                Debug.WriteLine("布局输入适配器初始化失败");
            }

            return success;
        }

        /// <summary>
        /// 设置根元素
        /// </summary>
        /// <param name="rootElement">根元素</param>
        public void SetRootElement(UIElement rootElement)
        {
            if (_rootElement == rootElement)
                return;

            // 清理旧的根元素
            if (_rootElement != null)
            {
                UnregisterElementRecursive(_rootElement);
            }

            _rootElement = rootElement;

            if (_rootElement != null)
            {
                // 注册新的根元素
                RegisterElementRecursive(_rootElement);

                // 创建根元素适配器并设置到输入系统
                var rootAdapter = GetOrCreateElementAdapter(_rootElement);
                _inputSystem.SetRootElement(rootAdapter);

                Debug.WriteLine($"根元素已设置: {_rootElement.GetType().Name}");
            }
        }

        /// <summary>
        /// 注册UI元素
        /// </summary>
        /// <param name="element">UI元素</param>
        public void RegisterElement(UIElement element)
        {
            if (element == null || _elementAdapters.ContainsKey(element))
                return;

            var adapter = new LayoutElementAdapter(element, this);
            _elementAdapters[element] = adapter;

            // 订阅元素事件
            element.LayoutUpdated += OnElementLayoutUpdated;

            Debug.WriteLine($"UI元素已注册: {element.GetType().Name}");
        }

        /// <summary>
        /// 注销UI元素
        /// </summary>
        /// <param name="element">UI元素</param>
        public void UnregisterElement(UIElement element)
        {
            if (element == null || !_elementAdapters.ContainsKey(element))
                return;

            // 取消订阅元素事件
            element.LayoutUpdated -= OnElementLayoutUpdated;

            // 移除适配器
            var adapter = _elementAdapters[element];
            _elementAdapters.Remove(element);
            adapter.Dispose();

            Debug.WriteLine($"UI元素已注销: {element.GetType().Name}");
        }

        /// <summary>
        /// 获取或创建元素适配器
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <returns>元素适配器</returns>
        public LayoutElementAdapter GetOrCreateElementAdapter(UIElement element)
        {
            if (!_elementAdapters.TryGetValue(element, out LayoutElementAdapter? adapter))
            {
                RegisterElement(element);
                adapter = _elementAdapters[element];
            }
            return adapter;
        }

        /// <summary>
        /// 递归注册元素及其子元素
        /// </summary>
        /// <param name="element">元素</param>
        private void RegisterElementRecursive(UIElement element)
        {
            RegisterElement(element);

            // 注册子元素
            foreach (var child in element.GetVisualChildren())
            {
                RegisterElementRecursive(child);
            }
        }

        /// <summary>
        /// 递归注销元素及其子元素
        /// </summary>
        /// <param name="element">元素</param>
        private void UnregisterElementRecursive(UIElement element)
        {
            // 注销子元素
            foreach (var child in element.GetVisualChildren())
            {
                UnregisterElementRecursive(child);
            }

            UnregisterElement(element);
        }

        /// <summary>
        /// 处理元素布局更新
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void OnElementLayoutUpdated(object? sender, EventArgs e)
        {
            if (sender is UIElement element && _elementAdapters.TryGetValue(element, out LayoutElementAdapter? adapter))
            {
                // 通知适配器布局已更新
                adapter.OnLayoutUpdated();

                // 如果启用了命中测试缓存失效
                if (_configuration.InvalidateHitTestOnLayoutChange)
                {
                    _inputSystem.HitTestEngine?.InvalidateCache();
                }
            }
        }

        /// <summary>
        /// 处理输入系统就绪事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void OnInputSystemReady(object? sender, EventArgs e)
        {
            Debug.WriteLine("输入系统已就绪");
        }

        /// <summary>
        /// 处理输入系统错误事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void OnInputSystemError(object? sender, SystemErrorEventArgs e)
        {
            Debug.WriteLine($"输入系统错误: {e}");
        }

        /// <summary>
        /// 触发输入事件
        /// </summary>
        /// <param name="element">目标元素</param>
        /// <param name="inputEvent">输入事件</param>
        internal void RaiseInputEvent(UIElement element, AetherUI.Input.Events.InputEvent inputEvent)
        {
            var eventArgs = new LayoutInputEventArgs(element, inputEvent);
            InputEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// 获取系统状态
        /// </summary>
        /// <returns>系统状态</returns>
        public LayoutInputStatus GetStatus()
        {
            return new LayoutInputStatus
            {
                IsInitialized = _inputSystem.IsInitialized,
                RootElementSet = _rootElement != null,
                RegisteredElementCount = _elementAdapters.Count,
                InputSystemStatus = _inputSystem.GetSystemStatus()
            };
        }

        /// <summary>
        /// 生成诊断报告
        /// </summary>
        /// <returns>诊断报告</returns>
        public string GenerateDiagnosticReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== 布局输入适配器诊断报告 ===");
            report.AppendLine($"生成时间: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine();

            var status = GetStatus();
            report.AppendLine("=== 适配器状态 ===");
            report.AppendLine($"已初始化: {status.IsInitialized}");
            report.AppendLine($"根元素已设置: {status.RootElementSet}");
            report.AppendLine($"注册元素数量: {status.RegisteredElementCount}");
            report.AppendLine();

            if (_rootElement != null)
            {
                report.AppendLine($"根元素类型: {_rootElement.GetType().Name}");
                report.AppendLine($"根元素边界: {_rootElement.RenderBounds}");
                report.AppendLine();
            }

            report.AppendLine("=== 注册元素列表 ===");
            foreach (var kvp in _elementAdapters)
            {
                var element = kvp.Key;
                var adapter = kvp.Value;
                report.AppendLine($"- {element.GetType().Name}: {element.RenderBounds}");
            }
            report.AppendLine();

            // 添加输入系统报告
            report.AppendLine(_inputSystem.GenerateSystemReport());

            return report.ToString();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            // 清理所有元素适配器
            var adapters = _elementAdapters.Values.ToList();
            _elementAdapters.Clear();

            foreach (var adapter in adapters)
            {
                adapter.Dispose();
            }

            // 释放输入系统
            _inputSystem?.Dispose();

            Debug.WriteLine("布局输入适配器已释放");
        }
    }

    /// <summary>
    /// 布局输入配置
    /// </summary>
    public class LayoutInputConfiguration
    {
        /// <summary>
        /// 默认配置
        /// </summary>
        public static LayoutInputConfiguration Default => new();

        // 输入系统配置
        public bool EnableDiagnostics { get; set; } = true;
        public bool EnableTextInput { get; set; } = true;
        public bool EnableAccessibility { get; set; } = true;
        public bool EnableAsyncProcessing { get; set; } = true;

        // 管道配置
        public int MaxInputQueueSize { get; set; } = 1000;
        public int ProcessingIntervalMs { get; set; } = 1;

        // 命中测试配置
        public bool HitTestUseBoundingBoxOnly { get; set; } = false;
        public int HitTestMaxDepth { get; set; } = 50;

        // 布局集成配置
        public bool InvalidateHitTestOnLayoutChange { get; set; } = true;
        public bool AutoRegisterChildren { get; set; } = true;
        public bool EnableLayoutEventPropagation { get; set; } = true;

        /// <summary>
        /// 创建高性能配置
        /// </summary>
        /// <returns>高性能配置</returns>
        public static LayoutInputConfiguration CreateHighPerformance()
        {
            return new LayoutInputConfiguration
            {
                EnableDiagnostics = false,
                EnableAsyncProcessing = true,
                MaxInputQueueSize = 2000,
                ProcessingIntervalMs = 0,
                HitTestUseBoundingBoxOnly = true,
                InvalidateHitTestOnLayoutChange = false
            };
        }

        /// <summary>
        /// 创建调试配置
        /// </summary>
        /// <returns>调试配置</returns>
        public static LayoutInputConfiguration CreateDebug()
        {
            return new LayoutInputConfiguration
            {
                EnableDiagnostics = true,
                EnableAsyncProcessing = false,
                MaxInputQueueSize = 100,
                ProcessingIntervalMs = 10,
                HitTestUseBoundingBoxOnly = false,
                InvalidateHitTestOnLayoutChange = true
            };
        }
    }

    /// <summary>
    /// 布局输入状态
    /// </summary>
    public class LayoutInputStatus
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// 根元素是否已设置
        /// </summary>
        public bool RootElementSet { get; set; }

        /// <summary>
        /// 注册元素数量
        /// </summary>
        public int RegisteredElementCount { get; set; }

        /// <summary>
        /// 输入系统状态
        /// </summary>
        public InputSystemStatus? InputSystemStatus { get; set; }

        public override string ToString() =>
            $"Initialized: {IsInitialized}, RootSet: {RootElementSet}, Elements: {RegisteredElementCount}";
    }

    /// <summary>
    /// 布局输入事件参数
    /// </summary>
    public class LayoutInputEventArgs : EventArgs
    {
        /// <summary>
        /// 目标元素
        /// </summary>
        public UIElement TargetElement { get; }

        /// <summary>
        /// 输入事件
        /// </summary>
        public AetherUI.Input.Events.InputEvent InputEvent { get; }

        /// <summary>
        /// 初始化布局输入事件参数
        /// </summary>
        /// <param name="targetElement">目标元素</param>
        /// <param name="inputEvent">输入事件</param>
        public LayoutInputEventArgs(UIElement targetElement, AetherUI.Input.Events.InputEvent inputEvent)
        {
            TargetElement = targetElement ?? throw new ArgumentNullException(nameof(targetElement));
            InputEvent = inputEvent ?? throw new ArgumentNullException(nameof(inputEvent));
        }

        public override string ToString() =>
            $"{InputEvent.GetType().Name} -> {TargetElement.GetType().Name}";
    }
}
