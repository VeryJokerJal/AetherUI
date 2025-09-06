using System;
using System.Diagnostics;
using AetherUI.Input.Core;
using AetherUI.Input.Events;
using AetherUI.Input.HitTesting;
using AetherUI.Input.Platform;

namespace AetherUI.Input.Examples
{
    /// <summary>
    /// 基本输入事件管线使用示例
    /// </summary>
    public class BasicInputExample
    {
        private IInputManager? _inputManager;
        private ExampleElement? _rootElement;

        /// <summary>
        /// 初始化输入管线示例
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        public void Initialize(IntPtr windowHandle)
        {
            // 创建输入管理器配置
            var config = new InputManagerConfiguration
            {
                EnableGestures = true,
                EnableFocusManagement = true,
                EnableInputCapture = true,
                EnableEventLogging = true,
                LogLevel = InputEventLogLevel.Information
            };

            // 创建输入管理器（这里需要实际的工厂实现）
            // _inputManager = InputManagerFactory.CreateInputManager(config);

            // 创建根元素
            _rootElement = new ExampleElement("Root", new Rect(0, 0, 800, 600));

            // 添加子元素
            var button1 = new ExampleElement("Button1", new Rect(100, 100, 200, 50));
            var button2 = new ExampleElement("Button2", new Rect(100, 200, 200, 50));
            var textBox = new ExampleElement("TextBox", new Rect(100, 300, 300, 30));

            _rootElement.AddChild(button1);
            _rootElement.AddChild(button2);
            _rootElement.AddChild(textBox);

            // 设置根元素
            if (_inputManager != null)
            {
                _inputManager.RootElement = _rootElement;

                // 订阅事件
                _inputManager.InputEventProcessed += OnInputEventProcessed;
                _inputManager.UnhandledInputEvent += OnUnhandledInputEvent;

                // 初始化输入管理器
                _inputManager.Initialize(windowHandle);
            }

            Debug.WriteLine("输入事件管线初始化完成");
        }

        /// <summary>
        /// 处理输入事件完成
        /// </summary>
        private void OnInputEventProcessed(object? sender, InputEventProcessedEventArgs e)
        {
            Debug.WriteLine($"输入事件已处理: {e.InputEvent} -> {(e.IsHandled ? "已处理" : "未处理")} 耗时: {e.ProcessingTimeMs:F2}ms");

            // 记录性能指标
            if (e.ProcessingTimeMs > 10) // 超过10ms的事件
            {
                Debug.WriteLine($"警告: 输入事件处理时间过长: {e.ProcessingTimeMs:F2}ms");
            }
        }

        /// <summary>
        /// 处理未处理的输入事件
        /// </summary>
        private void OnUnhandledInputEvent(object? sender, InputEvent e)
        {
            Debug.WriteLine($"未处理的输入事件: {e}");
        }

        /// <summary>
        /// 模拟鼠标点击
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public void SimulateMouseClick(double x, double y)
        {
            if (_inputManager == null) return;

            var timestamp = (uint)Environment.TickCount;
            var device = new InputDevice(InputDeviceType.Mouse, 0, "Mouse");
            var position = new Point(x, y);

            // 创建鼠标按下事件
            var mouseDownEvent = new PointerEvent(
                timestamp,
                device,
                PointerId.Mouse,
                position,
                PointerEventType.Pressed,
                PointerButton.Primary,
                PointerButton.Primary);

            // 创建鼠标释放事件
            var mouseUpEvent = new PointerEvent(
                timestamp + 50,
                device,
                PointerId.Mouse,
                position,
                PointerEventType.Released,
                PointerButton.None,
                PointerButton.Primary);

            // 注入事件
            _inputManager.InjectInputEvent(mouseDownEvent);
            _inputManager.InjectInputEvent(mouseUpEvent);

            Debug.WriteLine($"模拟鼠标点击: ({x}, {y})");
        }

        /// <summary>
        /// 模拟键盘输入
        /// </summary>
        /// <param name="key">虚拟键码</param>
        public void SimulateKeyPress(VirtualKey key)
        {
            if (_inputManager == null) return;

            var timestamp = (uint)Environment.TickCount;
            var device = new InputDevice(InputDeviceType.Keyboard, 0, "Keyboard");

            // 创建按键按下事件
            var keyDownEvent = new KeyboardEvent(
                timestamp,
                device,
                key,
                0, // 扫描码
                KeyEventType.Down);

            // 创建按键释放事件
            var keyUpEvent = new KeyboardEvent(
                timestamp + 50,
                device,
                key,
                0, // 扫描码
                KeyEventType.Up);

            // 注入事件
            _inputManager.InjectInputEvent(keyDownEvent);
            _inputManager.InjectInputEvent(keyUpEvent);

            Debug.WriteLine($"模拟按键: {key}");
        }

        /// <summary>
        /// 获取性能统计
        /// </summary>
        public void PrintPerformanceStatistics()
        {
            if (_inputManager?.HitTestEngine is IInputDiagnostics diagnostics)
            {
                var eventStats = diagnostics.GetEventStatistics();
                var perfStats = diagnostics.GetPerformanceStatistics();

                Debug.WriteLine("=== 输入事件统计 ===");
                Debug.WriteLine($"总事件数: {eventStats.TotalEvents}");
                Debug.WriteLine($"已处理事件数: {eventStats.HandledEvents}");
                Debug.WriteLine($"未处理事件数: {eventStats.UnhandledEvents}");
                Debug.WriteLine($"平均处理时间: {eventStats.AverageProcessingTimeMs:F2}ms");
                Debug.WriteLine($"最大处理时间: {eventStats.MaxProcessingTimeMs:F2}ms");
                Debug.WriteLine($"处理成功率: {eventStats.SuccessRate:P2}");

                Debug.WriteLine("\n=== 性能统计 ===");
                Debug.WriteLine($"命中测试平均时间: {perfStats.HitTestPerformance.AverageTimeMs:F2}ms");
                Debug.WriteLine($"事件路由平均时间: {perfStats.EventRoutingPerformance.AverageTimeMs:F2}ms");
                Debug.WriteLine($"手势识别平均时间: {perfStats.GestureRecognitionPerformance.AverageTimeMs:F2}ms");
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Cleanup()
        {
            _inputManager?.Shutdown();
            _inputManager?.Dispose();
            _inputManager = null;

            Debug.WriteLine("输入事件管线已清理");
        }
    }

    /// <summary>
    /// 示例元素类，实现IHitTestable接口
    /// </summary>
    public class ExampleElement : IHitTestable
    {
        private readonly List<IHitTestable> _children = new();

        public string Name { get; }
        public bool IsVisible { get; set; } = true;
        public bool IsHitTestVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;
        public Rect Bounds { get; set; }
        public Rect RenderBounds => Bounds;
        public System.Numerics.Matrix4x4 Transform { get; set; } = System.Numerics.Matrix4x4.Identity;
        public float Opacity { get; set; } = 1.0f;
        public int ZIndex { get; set; } = 0;
        public Rect? ClipBounds { get; set; }
        public IHitTestable? Parent { get; private set; }
        public IEnumerable<IHitTestable> Children => _children;

        public ExampleElement(string name, Rect bounds)
        {
            Name = name;
            Bounds = bounds;
        }

        public void AddChild(IHitTestable child)
        {
            _children.Add(child);
            if (child is ExampleElement element)
            {
                element.Parent = this;
            }
        }

        public bool HitTest(Point point)
        {
            return Bounds.Contains(point);
        }

        public System.Numerics.Matrix4x4 GetTransformToRoot()
        {
            var transform = Transform;
            var current = Parent;
            while (current != null)
            {
                transform = System.Numerics.Matrix4x4.Multiply(transform, current.Transform);
                current = current.Parent;
            }
            return transform;
        }

        public System.Numerics.Matrix4x4 GetTransformFromRoot()
        {
            System.Numerics.Matrix4x4.Invert(GetTransformToRoot(), out var inverse);
            return inverse;
        }

        public override string ToString() => Name;
    }

    /// <summary>
    /// 示例程序入口
    /// </summary>
    public static class InputExampleProgram
    {
        public static void RunExample()
        {
            Debug.WriteLine("开始输入事件管线示例");

            var example = new BasicInputExample();

            try
            {
                // 初始化（这里使用虚拟窗口句柄）
                example.Initialize(IntPtr.Zero);

                // 模拟一些输入事件
                example.SimulateMouseClick(150, 125); // 点击Button1
                example.SimulateMouseClick(150, 225); // 点击Button2
                example.SimulateMouseClick(200, 315); // 点击TextBox

                // 模拟键盘输入
                example.SimulateKeyPress(VirtualKey.Tab);
                example.SimulateKeyPress(VirtualKey.Enter);

                // 等待事件处理完成
                System.Threading.Thread.Sleep(100);

                // 打印性能统计
                example.PrintPerformanceStatistics();
            }
            finally
            {
                // 清理资源
                example.Cleanup();
            }

            Debug.WriteLine("输入事件管线示例完成");
        }
    }
}
