using System;
using System.Collections.Generic;
using AetherUI.Input.Capture;
using AetherUI.Input.Events;
using AetherUI.Input.Focus;
using AetherUI.Input.Gestures;
using AetherUI.Input.HitTesting;
using AetherUI.Input.Platform;
using AetherUI.Input.Routing;

namespace AetherUI.Input.Core
{
    /// <summary>
    /// 输入管理器接口 - 输入事件管线的核心协调器
    /// </summary>
    public interface IInputManager : IDisposable
    {
        /// <summary>
        /// 输入事件处理完成事件
        /// </summary>
        event EventHandler<InputEventProcessedEventArgs>? InputEventProcessed;

        /// <summary>
        /// 未处理的输入事件
        /// </summary>
        event EventHandler<InputEvent>? UnhandledInputEvent;

        /// <summary>
        /// 平台输入提供者
        /// </summary>
        IPlatformInputProvider? PlatformProvider { get; }

        /// <summary>
        /// 命中测试引擎
        /// </summary>
        IHitTestEngine HitTestEngine { get; }

        /// <summary>
        /// 事件路由器
        /// </summary>
        IEventRouter EventRouter { get; }

        /// <summary>
        /// 输入捕获管理器
        /// </summary>
        IInputCaptureManager CaptureManager { get; }

        /// <summary>
        /// 焦点管理器
        /// </summary>
        IFocusManager FocusManager { get; }

        /// <summary>
        /// 手势管理器
        /// </summary>
        IGestureManager GestureManager { get; }

        /// <summary>
        /// 根元素
        /// </summary>
        IHitTestable? RootElement { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 初始化输入管理器
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <param name="platformProvider">平台输入提供者</param>
        void Initialize(IntPtr windowHandle, IPlatformInputProvider? platformProvider = null);

        /// <summary>
        /// 关闭输入管理器
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 处理原始输入事件
        /// </summary>
        /// <param name="rawEvent">原始输入事件</param>
        void ProcessRawInput(RawInputEventArgs rawEvent);

        /// <summary>
        /// 处理标准化输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        void ProcessInputEvent(InputEvent inputEvent);

        /// <summary>
        /// 注入输入事件（用于测试）
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        void InjectInputEvent(InputEvent inputEvent);

        /// <summary>
        /// 获取当前鼠标位置
        /// </summary>
        /// <returns>鼠标位置</returns>
        Point GetMousePosition();

        /// <summary>
        /// 获取当前按键状态
        /// </summary>
        /// <param name="key">虚拟键码</param>
        /// <returns>是否按下</returns>
        bool IsKeyPressed(VirtualKey key);

        /// <summary>
        /// 获取当前修饰键状态
        /// </summary>
        /// <returns>修饰键状态</returns>
        ModifierKeys GetModifierKeys();

        /// <summary>
        /// 设置鼠标捕获
        /// </summary>
        /// <param name="capture">是否捕获</param>
        void SetMouseCapture(bool capture);

        /// <summary>
        /// 设置鼠标光标样式
        /// </summary>
        /// <param name="cursor">光标样式</param>
        void SetCursor(SystemCursor cursor);
    }

    /// <summary>
    /// 输入事件处理完成事件参数
    /// </summary>
    public class InputEventProcessedEventArgs : EventArgs
    {
        /// <summary>
        /// 输入事件
        /// </summary>
        public InputEvent InputEvent { get; }

        /// <summary>
        /// 命中测试结果
        /// </summary>
        public HitTestResult? HitTestResult { get; }

        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool IsHandled { get; }

        /// <summary>
        /// 处理时间（毫秒）
        /// </summary>
        public double ProcessingTimeMs { get; }

        /// <summary>
        /// 初始化输入事件处理完成事件参数
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <param name="hitTestResult">命中测试结果</param>
        /// <param name="isHandled">是否已处理</param>
        /// <param name="processingTimeMs">处理时间</param>
        public InputEventProcessedEventArgs(
            InputEvent inputEvent,
            HitTestResult? hitTestResult,
            bool isHandled,
            double processingTimeMs)
        {
            InputEvent = inputEvent ?? throw new ArgumentNullException(nameof(inputEvent));
            HitTestResult = hitTestResult;
            IsHandled = isHandled;
            ProcessingTimeMs = processingTimeMs;
        }

        public override string ToString() =>
            $"InputEventProcessed: {InputEvent} -> {(IsHandled ? "Handled" : "Unhandled")} in {ProcessingTimeMs:F2}ms";
    }

    /// <summary>
    /// 输入事件分发器接口
    /// </summary>
    public interface IInputEventDispatcher
    {
        /// <summary>
        /// 分发输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <param name="target">目标元素</param>
        /// <param name="hitPath">命中路径</param>
        /// <returns>是否已处理</returns>
        bool DispatchEvent(InputEvent inputEvent, object? target, IReadOnlyList<IHitTestable>? hitPath = null);

        /// <summary>
        /// 分发指针事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <param name="target">目标元素</param>
        /// <param name="hitPath">命中路径</param>
        /// <returns>是否已处理</returns>
        bool DispatchPointerEvent(PointerEvent pointerEvent, object? target, IReadOnlyList<IHitTestable>? hitPath = null);

        /// <summary>
        /// 分发键盘事件
        /// </summary>
        /// <param name="keyboardEvent">键盘事件</param>
        /// <param name="target">目标元素</param>
        /// <returns>是否已处理</returns>
        bool DispatchKeyboardEvent(KeyboardEvent keyboardEvent, object? target);

        /// <summary>
        /// 分发文本输入事件
        /// </summary>
        /// <param name="textInputEvent">文本输入事件</param>
        /// <param name="target">目标元素</param>
        /// <returns>是否已处理</returns>
        bool DispatchTextInputEvent(TextInputEvent textInputEvent, object? target);

        /// <summary>
        /// 分发手势事件
        /// </summary>
        /// <param name="gestureEvent">手势事件</param>
        /// <param name="target">目标元素</param>
        /// <param name="hitPath">命中路径</param>
        /// <returns>是否已处理</returns>
        bool DispatchGestureEvent(GestureEvent gestureEvent, object? target, IReadOnlyList<IHitTestable>? hitPath = null);
    }

    /// <summary>
    /// 输入事件标准化器接口
    /// </summary>
    public interface IInputEventNormalizer
    {
        /// <summary>
        /// 标准化原始鼠标事件
        /// </summary>
        /// <param name="rawEvent">原始鼠标事件</param>
        /// <returns>标准化的指针事件集合</returns>
        IEnumerable<PointerEvent> NormalizeMouseEvent(RawMouseEventArgs rawEvent);

        /// <summary>
        /// 标准化原始键盘事件
        /// </summary>
        /// <param name="rawEvent">原始键盘事件</param>
        /// <returns>标准化的键盘事件集合</returns>
        IEnumerable<KeyboardEvent> NormalizeKeyboardEvent(RawKeyboardEventArgs rawEvent);

        /// <summary>
        /// 标准化原始触摸事件
        /// </summary>
        /// <param name="rawEvent">原始触摸事件</param>
        /// <returns>标准化的指针事件集合</returns>
        IEnumerable<PointerEvent> NormalizeTouchEvent(RawTouchEventArgs rawEvent);
    }

    /// <summary>
    /// 输入管理器配置
    /// </summary>
    public class InputManagerConfiguration
    {
        /// <summary>
        /// 是否启用手势识别
        /// </summary>
        public bool EnableGestures { get; set; } = true;

        /// <summary>
        /// 是否启用焦点管理
        /// </summary>
        public bool EnableFocusManagement { get; set; } = true;

        /// <summary>
        /// 是否启用输入捕获
        /// </summary>
        public bool EnableInputCapture { get; set; } = true;

        /// <summary>
        /// 是否启用命中测试缓存
        /// </summary>
        public bool EnableHitTestCache { get; set; } = true;

        /// <summary>
        /// 命中测试缓存大小
        /// </summary>
        public int HitTestCacheSize { get; set; } = 1000;

        /// <summary>
        /// 事件处理超时时间（毫秒）
        /// </summary>
        public int EventProcessingTimeoutMs { get; set; } = 5000;

        /// <summary>
        /// 是否启用事件日志
        /// </summary>
        public bool EnableEventLogging { get; set; } = false;

        /// <summary>
        /// 事件日志级别
        /// </summary>
        public InputEventLogLevel LogLevel { get; set; } = InputEventLogLevel.Warning;

        /// <summary>
        /// 手势配置
        /// </summary>
        public GestureConfiguration GestureConfiguration { get; set; } = GestureConfiguration.Default;

        /// <summary>
        /// 捕获选项
        /// </summary>
        public CaptureOptions CaptureOptions { get; set; } = CaptureOptions.Default;

        /// <summary>
        /// 命中测试选项
        /// </summary>
        public HitTestOptions HitTestOptions { get; set; } = HitTestOptions.Default;

        /// <summary>
        /// 默认配置
        /// </summary>
        public static InputManagerConfiguration Default { get; } = new();
    }

    /// <summary>
    /// 输入事件日志级别
    /// </summary>
    public enum InputEventLogLevel
    {
        /// <summary>
        /// 无日志
        /// </summary>
        None,

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
        Information,

        /// <summary>
        /// 调试
        /// </summary>
        Debug,

        /// <summary>
        /// 详细
        /// </summary>
        Verbose
    }

    /// <summary>
    /// 输入管理器工厂接口
    /// </summary>
    public interface IInputManagerFactory
    {
        /// <summary>
        /// 创建输入管理器
        /// </summary>
        /// <param name="configuration">配置</param>
        /// <returns>输入管理器</returns>
        IInputManager CreateInputManager(InputManagerConfiguration? configuration = null);

        /// <summary>
        /// 创建平台输入提供者
        /// </summary>
        /// <returns>平台输入提供者</returns>
        IPlatformInputProvider CreatePlatformProvider();

        /// <summary>
        /// 创建命中测试引擎
        /// </summary>
        /// <param name="options">命中测试选项</param>
        /// <returns>命中测试引擎</returns>
        IHitTestEngine CreateHitTestEngine(HitTestOptions? options = null);

        /// <summary>
        /// 创建事件路由器
        /// </summary>
        /// <returns>事件路由器</returns>
        IEventRouter CreateEventRouter();

        /// <summary>
        /// 创建输入捕获管理器
        /// </summary>
        /// <param name="options">捕获选项</param>
        /// <returns>输入捕获管理器</returns>
        IInputCaptureManager CreateCaptureManager(CaptureOptions? options = null);

        /// <summary>
        /// 创建焦点管理器
        /// </summary>
        /// <returns>焦点管理器</returns>
        IFocusManager CreateFocusManager();

        /// <summary>
        /// 创建手势管理器
        /// </summary>
        /// <param name="configuration">手势配置</param>
        /// <returns>手势管理器</returns>
        IGestureManager CreateGestureManager(GestureConfiguration? configuration = null);
    }
}
