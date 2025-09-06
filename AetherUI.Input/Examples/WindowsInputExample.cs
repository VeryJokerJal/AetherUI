using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AetherUI.Input.Core;
using AetherUI.Input.Events;
using AetherUI.Input.HitTesting;
using AetherUI.Input.Platform.Windows;

namespace AetherUI.Input.Examples
{
    /// <summary>
    /// Windows输入事件管线示例
    /// </summary>
    public class WindowsInputExample
    {
        private IInputManager? _inputManager;
        private ExampleWindow? _window;

        /// <summary>
        /// 运行示例
        /// </summary>
        public void Run()
        {
            Debug.WriteLine("开始Windows输入事件管线示例");

            try
            {
                // 创建示例窗口
                _window = new ExampleWindow();
                _window.Create();

                // 初始化输入管理器
                InitializeInputManager(_window.Handle);

                // 运行消息循环
                _window.RunMessageLoop();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"示例运行失败: {ex.Message}");
            }
            finally
            {
                Cleanup();
            }

            Debug.WriteLine("Windows输入事件管线示例完成");
        }

        /// <summary>
        /// 初始化输入管理器
        /// </summary>
        private void InitializeInputManager(IntPtr windowHandle)
        {
            // 创建配置
            var config = new InputManagerConfiguration
            {
                EnableGestures = true,
                EnableFocusManagement = true,
                EnableInputCapture = true,
                EnableEventLogging = true,
                LogLevel = InputEventLogLevel.Information
            };

            // 创建输入管理器
            _inputManager = new InputManager(config);

            // 创建根元素
            var rootElement = new ExampleElement("Root", new Rect(0, 0, 800, 600));
            
            // 添加子元素
            var button1 = new ExampleElement("Button1", new Rect(100, 100, 200, 50));
            var button2 = new ExampleElement("Button2", new Rect(100, 200, 200, 50));
            var textBox = new ExampleElement("TextBox", new Rect(100, 300, 300, 30));

            rootElement.AddChild(button1);
            rootElement.AddChild(button2);
            rootElement.AddChild(textBox);

            _inputManager.RootElement = rootElement;

            // 订阅事件
            _inputManager.InputEventProcessed += OnInputEventProcessed;
            _inputManager.UnhandledInputEvent += OnUnhandledInputEvent;

            // 初始化
            _inputManager.Initialize(windowHandle);

            Debug.WriteLine("输入管理器初始化完成");
        }

        /// <summary>
        /// 处理输入事件完成
        /// </summary>
        private void OnInputEventProcessed(object? sender, InputEventProcessedEventArgs e)
        {
            Debug.WriteLine($"输入事件已处理: {e.InputEvent.GetType().Name} -> {(e.IsHandled ? "已处理" : "未处理")} 耗时: {e.ProcessingTimeMs:F2}ms");

            // 记录性能警告
            if (e.ProcessingTimeMs > 10)
            {
                Debug.WriteLine($"警告: 输入事件处理时间过长: {e.ProcessingTimeMs:F2}ms");
            }
        }

        /// <summary>
        /// 处理未处理的输入事件
        /// </summary>
        private void OnUnhandledInputEvent(object? sender, InputEvent e)
        {
            Debug.WriteLine($"未处理的输入事件: {e.GetType().Name}");
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void Cleanup()
        {
            _inputManager?.Shutdown();
            _inputManager?.Dispose();
            _window?.Destroy();

            Debug.WriteLine("资源已清理");
        }
    }

    /// <summary>
    /// 示例窗口类
    /// </summary>
    public class ExampleWindow
    {
        private IntPtr _handle;
        private Win32.WndProc? _wndProcDelegate;
        private bool _isRunning;

        /// <summary>
        /// 窗口句柄
        /// </summary>
        public IntPtr Handle => _handle;

        /// <summary>
        /// 创建窗口
        /// </summary>
        public void Create()
        {
            // 注册窗口类
            var className = "AetherUIInputExample";
            _wndProcDelegate = WndProc;

            var wc = new Win32.WNDCLASS
            {
                lpfnWndProc = _wndProcDelegate,
                hInstance = Win32.GetModuleHandle(null),
                lpszClassName = className,
                hCursor = Win32.LoadCursor(IntPtr.Zero, Win32.IDC_ARROW),
                hbrBackground = (IntPtr)(Win32.COLOR_WINDOW + 1)
            };

            var atom = Win32.RegisterClass(ref wc);
            if (atom == 0)
            {
                throw new InvalidOperationException("注册窗口类失败");
            }

            // 创建窗口
            _handle = Win32.CreateWindowEx(
                0,
                className,
                "AetherUI Input Example",
                Win32.WS_OVERLAPPEDWINDOW,
                Win32.CW_USEDEFAULT, Win32.CW_USEDEFAULT,
                800, 600,
                IntPtr.Zero,
                IntPtr.Zero,
                Win32.GetModuleHandle(null),
                IntPtr.Zero);

            if (_handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("创建窗口失败");
            }

            // 显示窗口
            Win32.ShowWindow(_handle, Win32.SW_SHOW);
            Win32.UpdateWindow(_handle);

            Debug.WriteLine("示例窗口已创建");
        }

        /// <summary>
        /// 运行消息循环
        /// </summary>
        public void RunMessageLoop()
        {
            _isRunning = true;

            while (_isRunning)
            {
                if (Win32.GetMessage(out Win32.MSG msg, IntPtr.Zero, 0, 0))
                {
                    Win32.TranslateMessage(ref msg);
                    Win32.DispatchMessage(ref msg);
                }
                else
                {
                    break; // WM_QUIT
                }
            }
        }

        /// <summary>
        /// 销毁窗口
        /// </summary>
        public void Destroy()
        {
            if (_handle != IntPtr.Zero)
            {
                Win32.DestroyWindow(_handle);
                _handle = IntPtr.Zero;
            }

            _isRunning = false;
            Debug.WriteLine("示例窗口已销毁");
        }

        /// <summary>
        /// 窗口过程
        /// </summary>
        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case Win32.WM_DESTROY:
                    Win32.PostQuitMessage(0);
                    return IntPtr.Zero;

                case Win32.WM_PAINT:
                    var ps = new Win32.PAINTSTRUCT();
                    var hdc = Win32.BeginPaint(hWnd, out ps);
                    
                    // 简单绘制
                    Win32.TextOut(hdc, 10, 10, "AetherUI Input Example", 23);
                    Win32.TextOut(hdc, 10, 30, "Move mouse and click to test input events", 41);
                    
                    Win32.EndPaint(hWnd, ref ps);
                    return IntPtr.Zero;

                default:
                    return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
            }
        }
    }

    /// <summary>
    /// 示例程序入口
    /// </summary>
    public static class WindowsInputExampleProgram
    {
        /// <summary>
        /// 程序入口点
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (!OperatingSystem.IsWindows())
            {
                Console.WriteLine("此示例仅支持Windows平台");
                return;
            }

            var example = new WindowsInputExample();
            example.Run();
        }
    }
}
