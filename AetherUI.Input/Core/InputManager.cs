using System;
using System.Diagnostics;
using AetherUI.Input.Capture;
using AetherUI.Input.Events;
using AetherUI.Input.Focus;
using AetherUI.Input.Gestures;
using AetherUI.Input.HitTesting;
using AetherUI.Input.Platform;
using AetherUI.Input.Platform.Windows;
using AetherUI.Input.Routing;

namespace AetherUI.Input.Core
{
    /// <summary>
    /// 输入管理器实现
    /// </summary>
    public class InputManager : IInputManager
    {
        private readonly InputManagerConfiguration _configuration;
        private readonly IInputEventNormalizer _normalizer;
        private readonly IInputEventDispatcher _dispatcher;
        private readonly Stopwatch _performanceStopwatch = new();
        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// 输入事件处理完成事件
        /// </summary>
        public event EventHandler<InputEventProcessedEventArgs>? InputEventProcessed;

        /// <summary>
        /// 未处理的输入事件
        /// </summary>
        public event EventHandler<InputEvent>? UnhandledInputEvent;

        /// <summary>
        /// 平台输入提供者
        /// </summary>
        public IPlatformInputProvider? PlatformProvider { get; private set; }

        /// <summary>
        /// 命中测试引擎
        /// </summary>
        public IHitTestEngine HitTestEngine { get; }

        /// <summary>
        /// 事件路由器
        /// </summary>
        public IEventRouter EventRouter { get; }

        /// <summary>
        /// 输入捕获管理器
        /// </summary>
        public IInputCaptureManager CaptureManager { get; }

        /// <summary>
        /// 焦点管理器
        /// </summary>
        public IFocusManager FocusManager { get; }

        /// <summary>
        /// 手势管理器
        /// </summary>
        public IGestureManager GestureManager { get; }

        /// <summary>
        /// 根元素
        /// </summary>
        public IHitTestable? RootElement { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 初始化输入管理器
        /// </summary>
        /// <param name="configuration">配置</param>
        /// <param name="hitTestEngine">命中测试引擎</param>
        /// <param name="eventRouter">事件路由器</param>
        /// <param name="captureManager">捕获管理器</param>
        /// <param name="focusManager">焦点管理器</param>
        /// <param name="gestureManager">手势管理器</param>
        public InputManager(
            InputManagerConfiguration? configuration = null,
            IHitTestEngine? hitTestEngine = null,
            IEventRouter? eventRouter = null,
            IInputCaptureManager? captureManager = null,
            IFocusManager? focusManager = null,
            IGestureManager? gestureManager = null)
        {
            _configuration = configuration ?? InputManagerConfiguration.Default;
            _normalizer = new InputEventNormalizer();
            _dispatcher = new InputEventDispatcher(this);

            HitTestEngine = hitTestEngine ?? new HitTestEngine(_configuration.HitTestOptions);
            EventRouter = eventRouter ?? new EventRouter();
            CaptureManager = captureManager ?? new InputCaptureManager(_configuration.CaptureOptions);
            FocusManager = focusManager ?? new FocusManager();
            GestureManager = gestureManager ?? new GestureManager(_configuration.GestureConfiguration);

            // 订阅手势事件
            if (_configuration.EnableGestures)
            {
                GestureManager.GestureRecognized += OnGestureRecognized;
            }

            Debug.WriteLine("InputManager已创建");
        }

        /// <summary>
        /// 初始化输入管理器
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <param name="platformProvider">平台输入提供者</param>
        public void Initialize(IntPtr windowHandle, IPlatformInputProvider? platformProvider = null)
        {
            if (_isInitialized)
                throw new InvalidOperationException("InputManager已经初始化");

            try
            {
                // 创建或使用提供的平台输入提供者
                PlatformProvider = platformProvider ?? CreateDefaultPlatformProvider();

                // 订阅原始输入事件
                PlatformProvider.RawInputReceived += OnRawInputReceived;

                // 初始化平台提供者
                PlatformProvider.Initialize(windowHandle);

                _isInitialized = true;
                Debug.WriteLine("InputManager初始化成功");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InputManager初始化失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 关闭输入管理器
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized || _isDisposed)
                return;

            try
            {
                // 取消订阅事件
                if (PlatformProvider != null)
                {
                    PlatformProvider.RawInputReceived -= OnRawInputReceived;
                    PlatformProvider.Shutdown();
                }

                if (_configuration.EnableGestures)
                {
                    GestureManager.GestureRecognized -= OnGestureRecognized;
                }

                _isInitialized = false;
                Debug.WriteLine("InputManager已关闭");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InputManager关闭失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理原始输入事件
        /// </summary>
        /// <param name="rawEvent">原始输入事件</param>
        public void ProcessRawInput(RawInputEventArgs rawEvent)
        {
            if (!IsEnabled || _isDisposed)
                return;

            try
            {
                // 标准化原始事件
                var inputEvents = NormalizeRawEvent(rawEvent);

                // 处理每个标准化事件
                foreach (var inputEvent in inputEvents)
                {
                    ProcessInputEvent(inputEvent);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理原始输入事件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理标准化输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        public void ProcessInputEvent(InputEvent inputEvent)
        {
            if (!IsEnabled || _isDisposed)
                return;

            _performanceStopwatch.Restart();
            HitTestResult? hitTestResult = null;
            bool isHandled = false;

            try
            {
                // 分发事件
                isHandled = _dispatcher.DispatchEvent(inputEvent, null);

                // 触发事件处理完成事件
                _performanceStopwatch.Stop();
                var processedArgs = new InputEventProcessedEventArgs(
                    inputEvent,
                    hitTestResult,
                    isHandled,
                    _performanceStopwatch.Elapsed.TotalMilliseconds);

                InputEventProcessed?.Invoke(this, processedArgs);

                // 如果事件未处理，触发未处理事件
                if (!isHandled)
                {
                    UnhandledInputEvent?.Invoke(this, inputEvent);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理输入事件失败: {ex.Message}");
                _performanceStopwatch.Stop();
            }
        }

        /// <summary>
        /// 注入输入事件（用于测试）
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        public void InjectInputEvent(InputEvent inputEvent)
        {
            ProcessInputEvent(inputEvent);
        }

        /// <summary>
        /// 获取当前鼠标位置
        /// </summary>
        /// <returns>鼠标位置</returns>
        public Point GetMousePosition()
        {
            return PlatformProvider?.GetMousePosition() ?? Point.Zero;
        }

        /// <summary>
        /// 获取当前按键状态
        /// </summary>
        /// <param name="key">虚拟键码</param>
        /// <returns>是否按下</returns>
        public bool IsKeyPressed(VirtualKey key)
        {
            return PlatformProvider?.IsKeyPressed(key) ?? false;
        }

        /// <summary>
        /// 获取当前修饰键状态
        /// </summary>
        /// <returns>修饰键状态</returns>
        public ModifierKeys GetModifierKeys()
        {
            return PlatformProvider?.GetModifierKeys() ?? ModifierKeys.None;
        }

        /// <summary>
        /// 设置鼠标捕获
        /// </summary>
        /// <param name="capture">是否捕获</param>
        public void SetMouseCapture(bool capture)
        {
            PlatformProvider?.SetMouseCapture(capture);
        }

        /// <summary>
        /// 设置鼠标光标样式
        /// </summary>
        /// <param name="cursor">光标样式</param>
        public void SetCursor(SystemCursor cursor)
        {
            PlatformProvider?.SetCursor(cursor);
        }

        /// <summary>
        /// 处理原始输入事件
        /// </summary>
        private void OnRawInputReceived(object? sender, RawInputEventArgs e)
        {
            ProcessRawInput(e);
        }

        /// <summary>
        /// 处理手势识别事件
        /// </summary>
        private void OnGestureRecognized(object? sender, GestureRecognizedEventArgs e)
        {
            var gestureEvent = new GestureEvent(
                e.TriggerEvent.Timestamp,
                e.TriggerEvent.Device,
                e.GestureType,
                e.State,
                e.Position,
                e.Data);

            ProcessInputEvent(gestureEvent);
        }

        /// <summary>
        /// 标准化原始事件
        /// </summary>
        private System.Collections.Generic.IEnumerable<InputEvent> NormalizeRawEvent(RawInputEventArgs rawEvent)
        {
            return rawEvent switch
            {
                RawMouseEventArgs mouseEvent => _normalizer.NormalizeMouseEvent(mouseEvent).Cast<InputEvent>(),
                RawKeyboardEventArgs keyboardEvent => _normalizer.NormalizeKeyboardEvent(keyboardEvent).Cast<InputEvent>(),
                RawTouchEventArgs touchEvent => _normalizer.NormalizeTouchEvent(touchEvent).Cast<InputEvent>(),
                _ => System.Linq.Enumerable.Empty<InputEvent>()
            };
        }

        /// <summary>
        /// 创建默认平台输入提供者
        /// </summary>
        private IPlatformInputProvider CreateDefaultPlatformProvider()
        {
            // 根据当前平台创建相应的输入提供者
            if (OperatingSystem.IsWindows())
            {
                return new WindowsInputProvider();
            }
            else if (OperatingSystem.IsMacOS())
            {
                // TODO: 实现macOS输入提供者
                throw new PlatformNotSupportedException("macOS输入提供者尚未实现");
            }
            else if (OperatingSystem.IsLinux())
            {
                // TODO: 实现Linux输入提供者
                throw new PlatformNotSupportedException("Linux输入提供者尚未实现");
            }
            else
            {
                throw new PlatformNotSupportedException($"不支持的平台: {Environment.OSVersion.Platform}");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            Shutdown();
            PlatformProvider?.Dispose();
            _isDisposed = true;

            Debug.WriteLine("InputManager已释放");
        }
    }

    /// <summary>
    /// 输入事件分发器实现
    /// </summary>
    public class InputEventDispatcher : IInputEventDispatcher
    {
        private readonly InputManager _inputManager;

        /// <summary>
        /// 初始化输入事件分发器
        /// </summary>
        /// <param name="inputManager">输入管理器</param>
        public InputEventDispatcher(InputManager inputManager)
        {
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
        }

        /// <summary>
        /// 分发输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        /// <param name="target">目标元素</param>
        /// <param name="hitPath">命中路径</param>
        /// <returns>是否已处理</returns>
        public bool DispatchEvent(InputEvent inputEvent, object? target, System.Collections.Generic.IReadOnlyList<IHitTestable>? hitPath = null)
        {
            return inputEvent switch
            {
                PointerEvent pointerEvent => DispatchPointerEvent(pointerEvent, target, hitPath),
                KeyboardEvent keyboardEvent => DispatchKeyboardEvent(keyboardEvent, target),
                TextInputEvent textInputEvent => DispatchTextInputEvent(textInputEvent, target),
                GestureEvent gestureEvent => DispatchGestureEvent(gestureEvent, target, hitPath),
                _ => false
            };
        }

        /// <summary>
        /// 分发指针事件
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <param name="target">目标元素</param>
        /// <param name="hitPath">命中路径</param>
        /// <returns>是否已处理</returns>
        public bool DispatchPointerEvent(PointerEvent pointerEvent, object? target, System.Collections.Generic.IReadOnlyList<IHitTestable>? hitPath = null)
        {
            // 处理手势识别
            if (_inputManager._configuration.EnableGestures)
            {
                _inputManager.GestureManager.ProcessPointerEvent(pointerEvent);
            }

            // 处理捕获
            object? actualTarget = target;
            if (_inputManager._configuration.EnableInputCapture)
            {
                var capturedTarget = _inputManager.CaptureManager.ProcessPointerCapture(pointerEvent);
                if (capturedTarget != null)
                {
                    actualTarget = capturedTarget;
                    hitPath = null; // 捕获时不使用命中路径
                }
            }

            // 如果没有指定目标，进行命中测试
            if (actualTarget == null)
            {
                var hitResult = _inputManager.HitTestEngine.HitTest(_inputManager.RootElement, pointerEvent.Position);
                if (hitResult.IsHit)
                {
                    actualTarget = hitResult.HitElement;
                    hitPath = hitResult.HitPath;
                }
            }

            // 路由事件
            if (actualTarget != null)
            {
                // 这里需要创建适当的路由事件参数
                // 暂时返回false，表示未处理
                return false;
            }

            return false;
        }

        /// <summary>
        /// 分发键盘事件
        /// </summary>
        /// <param name="keyboardEvent">键盘事件</param>
        /// <param name="target">目标元素</param>
        /// <returns>是否已处理</returns>
        public bool DispatchKeyboardEvent(KeyboardEvent keyboardEvent, object? target)
        {
            // 获取焦点目标
            object? actualTarget = target;
            if (_inputManager._configuration.EnableFocusManagement && actualTarget == null)
            {
                actualTarget = _inputManager.FocusManager.KeyboardFocus;
            }

            // 路由事件
            if (actualTarget != null)
            {
                // 这里需要创建适当的路由事件参数
                // 暂时返回false，表示未处理
                return false;
            }

            return false;
        }

        /// <summary>
        /// 分发文本输入事件
        /// </summary>
        /// <param name="textInputEvent">文本输入事件</param>
        /// <param name="target">目标元素</param>
        /// <returns>是否已处理</returns>
        public bool DispatchTextInputEvent(TextInputEvent textInputEvent, object? target)
        {
            // 获取焦点目标
            object? actualTarget = target;
            if (_inputManager._configuration.EnableFocusManagement && actualTarget == null)
            {
                actualTarget = _inputManager.FocusManager.KeyboardFocus;
            }

            // 路由事件
            if (actualTarget != null)
            {
                // 这里需要创建适当的路由事件参数
                // 暂时返回false，表示未处理
                return false;
            }

            return false;
        }

        /// <summary>
        /// 分发手势事件
        /// </summary>
        /// <param name="gestureEvent">手势事件</param>
        /// <param name="target">目标元素</param>
        /// <param name="hitPath">命中路径</param>
        /// <returns>是否已处理</returns>
        public bool DispatchGestureEvent(GestureEvent gestureEvent, object? target, System.Collections.Generic.IReadOnlyList<IHitTestable>? hitPath = null)
        {
            // 如果没有指定目标，进行命中测试
            if (target == null)
            {
                var hitResult = _inputManager.HitTestEngine.HitTest(_inputManager.RootElement, gestureEvent.Position);
                if (hitResult.IsHit)
                {
                    target = hitResult.HitElement;
                    hitPath = hitResult.HitPath;
                }
            }

            // 路由事件
            if (target != null)
            {
                // 这里需要创建适当的路由事件参数
                // 暂时返回false，表示未处理
                return false;
            }

            return false;
        }
    }
}
