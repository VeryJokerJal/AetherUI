using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AetherUI.Input.Core;
using AetherUI.Input.Events;
using AetherUI.Input.Gestures;
using AetherUI.Input.HitTesting;
using AetherUI.Input.Pipeline;
using AetherUI.Input.Routing;

namespace AetherUI.Input.Examples
{
    /// <summary>
    /// 完整输入系统示例
    /// </summary>
    public class CompleteInputSystemExample : IDisposable
    {
        private InputPipeline? _inputPipeline;
        private CompleteVisualElement? _rootElement;
        private bool _isDisposed;

        /// <summary>
        /// 运行示例
        /// </summary>
        public async Task RunAsync()
        {
            Debug.WriteLine("开始完整输入系统示例");

            try
            {
                // 初始化输入系统
                await InitializeInputSystemAsync();

                // 创建UI元素树
                CreateUIElementTree();

                // 测试完整输入流程
                await TestCompleteInputFlowAsync();

                // 测试性能
                await TestPerformanceAsync();

                // 显示系统统计
                ShowSystemStatistics();

                // 等待一段时间让异步处理完成
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"示例运行失败: {ex.Message}");
            }

            Debug.WriteLine("完整输入系统示例完成");
        }

        /// <summary>
        /// 初始化输入系统
        /// </summary>
        private async Task InitializeInputSystemAsync()
        {
            Debug.WriteLine("初始化输入系统...");

            // 创建配置
            var config = InputPipelineConfiguration.CreateHighPerformance();
            var validation = config.Validate();
            if (!validation.IsValid)
            {
                Debug.WriteLine($"配置验证失败: {validation}");
                return;
            }

            // 创建命中测试引擎
            var hitTestOptions = new HitTestOptions
            {
                UseBoundingBoxOnly = false,
                MaxDepth = 20
            };
            var hitTestEngine = new AdvancedHitTestEngine(hitTestOptions);

            // 创建事件路由器
            var eventRouter = new AdvancedEventRouter();
            eventRouter.AddInterceptor(InterceptorFactory.CreateLogging(false, true));
            eventRouter.AddInterceptor(InterceptorFactory.CreatePerformanceMonitor());

            // 创建手势引擎
            var gestureConfig = new GestureConfiguration
            {
                TapTimeoutMs = 300,
                LongPressDelayMs = 800,
                DragThreshold = 10.0,
                PinchThreshold = 5.0
            };
            var gestureEngine = new AdvancedGestureEngine(gestureConfig);

            // 创建输入管道
            _inputPipeline = new InputPipeline(config, hitTestEngine, eventRouter, gestureEngine);

            // 订阅事件
            _inputPipeline.InputEventProcessed += OnInputEventProcessed;
            _inputPipeline.PipelineError += OnPipelineError;

            // 添加自定义处理器
            var customProcessor = new CustomInputProcessor();
            _inputPipeline.AddProcessor(customProcessor);

            Debug.WriteLine("输入系统初始化完成");
            await Task.CompletedTask;
        }

        /// <summary>
        /// 创建UI元素树
        /// </summary>
        private void CreateUIElementTree()
        {
            Debug.WriteLine("创建UI元素树...");

            _rootElement = new CompleteVisualElement("Root", new Rect(0, 0, 1200, 800));

            // 创建主面板
            var mainPanel = new CompleteVisualElement("MainPanel", new Rect(50, 50, 1100, 700))
            {
                ZIndex = 1
            };

            // 创建按钮区域
            var buttonPanel = new CompleteVisualElement("ButtonPanel", new Rect(20, 20, 300, 200))
            {
                ZIndex = 2
            };

            var button1 = new CompleteVisualElement("Button1", new Rect(10, 10, 80, 40)) { ZIndex = 10 };
            var button2 = new CompleteVisualElement("Button2", new Rect(100, 10, 80, 40)) { ZIndex = 10 };
            var button3 = new CompleteVisualElement("Button3", new Rect(190, 10, 80, 40)) { ZIndex = 10 };

            buttonPanel.AddChild(button1);
            buttonPanel.AddChild(button2);
            buttonPanel.AddChild(button3);

            // 创建内容区域
            var contentArea = new CompleteVisualElement("ContentArea", new Rect(350, 20, 700, 600))
            {
                ZIndex = 2
            };

            var textBox = new CompleteVisualElement("TextBox", new Rect(20, 20, 300, 30)) { ZIndex = 5 };
            var canvas = new CompleteVisualElement("Canvas", new Rect(20, 70, 650, 500)) { ZIndex = 5 };

            contentArea.AddChild(textBox);
            contentArea.AddChild(canvas);

            // 创建工具栏
            var toolbar = new CompleteVisualElement("Toolbar", new Rect(20, 650, 1060, 40))
            {
                ZIndex = 3
            };

            var tool1 = new CompleteVisualElement("Tool1", new Rect(10, 5, 30, 30)) { ZIndex = 15 };
            var tool2 = new CompleteVisualElement("Tool2", new Rect(50, 5, 30, 30)) { ZIndex = 15 };
            var tool3 = new CompleteVisualElement("Tool3", new Rect(90, 5, 30, 30)) { ZIndex = 15 };

            toolbar.AddChild(tool1);
            toolbar.AddChild(tool2);
            toolbar.AddChild(tool3);

            // 构建树结构
            mainPanel.AddChild(buttonPanel);
            mainPanel.AddChild(contentArea);
            mainPanel.AddChild(toolbar);

            _rootElement.AddChild(mainPanel);

            // 设置根元素
            _inputPipeline?.SetRootElement(_rootElement);

            Debug.WriteLine("UI元素树创建完成");
        }

        /// <summary>
        /// 测试完整输入流程
        /// </summary>
        private async Task TestCompleteInputFlowAsync()
        {
            Debug.WriteLine("\n=== 测试完整输入流程 ===");

            if (_inputPipeline == null)
                return;

            var device = new InputDevice(InputDeviceType.Touch, 0, "TouchScreen");

            // 测试点击按钮
            Debug.WriteLine("\n--- 测试按钮点击 ---");
            await SimulateClickAsync(new Point(100, 80), device, 1000);

            // 测试拖拽操作
            Debug.WriteLine("\n--- 测试拖拽操作 ---");
            await SimulateDragAsync(new Point(200, 200), new Point(300, 250), device, 2000);

            // 测试多点触控
            Debug.WriteLine("\n--- 测试多点触控 ---");
            await SimulateMultiTouchAsync(device, 3000);

            // 测试键盘输入
            Debug.WriteLine("\n--- 测试键盘输入 ---");
            await SimulateKeyboardInputAsync(4000);

            // 等待处理完成
            await Task.Delay(500);
        }

        /// <summary>
        /// 模拟点击
        /// </summary>
        private async Task SimulateClickAsync(Point position, InputDevice device, uint baseTime)
        {
            var pressEvent = new PointerEvent(
                baseTime,
                device,
                PointerId.Touch(1),
                position,
                PointerEventType.Pressed,
                PointerButton.Primary,
                PointerButton.Primary);

            var releaseEvent = new PointerEvent(
                baseTime + 100,
                device,
                PointerId.Touch(1),
                position,
                PointerEventType.Released,
                PointerButton.None,
                PointerButton.Primary);

            _inputPipeline?.SubmitEvent(pressEvent);
            await Task.Delay(50);
            _inputPipeline?.SubmitEvent(releaseEvent);
        }

        /// <summary>
        /// 模拟拖拽
        /// </summary>
        private async Task SimulateDragAsync(Point start, Point end, InputDevice device, uint baseTime)
        {
            var pressEvent = new PointerEvent(
                baseTime,
                device,
                PointerId.Touch(2),
                start,
                PointerEventType.Pressed,
                PointerButton.Primary,
                PointerButton.Primary);

            _inputPipeline?.SubmitEvent(pressEvent);
            await Task.Delay(20);

            // 模拟移动
            var steps = 10;
            for (int i = 1; i <= steps; i++)
            {
                var progress = (double)i / steps;
                var currentPos = new Point(
                    start.X + (end.X - start.X) * progress,
                    start.Y + (end.Y - start.Y) * progress);

                var moveEvent = new PointerEvent(
                    baseTime + (uint)(i * 20),
                    device,
                    PointerId.Touch(2),
                    currentPos,
                    PointerEventType.Moved,
                    PointerButton.Primary,
                    PointerButton.None);

                _inputPipeline?.SubmitEvent(moveEvent);
                await Task.Delay(10);
            }

            var releaseEvent = new PointerEvent(
                baseTime + 300,
                device,
                PointerId.Touch(2),
                end,
                PointerEventType.Released,
                PointerButton.None,
                PointerButton.Primary);

            _inputPipeline?.SubmitEvent(releaseEvent);
        }

        /// <summary>
        /// 模拟多点触控
        /// </summary>
        private async Task SimulateMultiTouchAsync(InputDevice device, uint baseTime)
        {
            // 双指捏合手势
            var finger1Start = new Point(400, 300);
            var finger2Start = new Point(500, 300);
            var finger1End = new Point(420, 300);
            var finger2End = new Point(480, 300);

            // 两指同时按下
            _inputPipeline?.SubmitEvent(new PointerEvent(baseTime, device, PointerId.Touch(1), finger1Start, PointerEventType.Pressed, PointerButton.Primary, PointerButton.Primary));
            _inputPipeline?.SubmitEvent(new PointerEvent(baseTime + 10, device, PointerId.Touch(2), finger2Start, PointerEventType.Pressed, PointerButton.Primary, PointerButton.Primary));

            await Task.Delay(50);

            // 模拟捏合移动
            for (int i = 1; i <= 5; i++)
            {
                var progress = (double)i / 5;
                var pos1 = new Point(
                    finger1Start.X + (finger1End.X - finger1Start.X) * progress,
                    finger1Start.Y);
                var pos2 = new Point(
                    finger2Start.X + (finger2End.X - finger2Start.X) * progress,
                    finger2Start.Y);

                _inputPipeline?.SubmitEvent(new PointerEvent(baseTime + (uint)(100 + i * 50), device, PointerId.Touch(1), pos1, PointerEventType.Moved, PointerButton.Primary, PointerButton.None));
                _inputPipeline?.SubmitEvent(new PointerEvent(baseTime + (uint)(110 + i * 50), device, PointerId.Touch(2), pos2, PointerEventType.Moved, PointerButton.Primary, PointerButton.None));

                await Task.Delay(30);
            }

            // 两指抬起
            _inputPipeline?.SubmitEvent(new PointerEvent(baseTime + 400, device, PointerId.Touch(1), finger1End, PointerEventType.Released, PointerButton.None, PointerButton.Primary));
            _inputPipeline?.SubmitEvent(new PointerEvent(baseTime + 410, device, PointerId.Touch(2), finger2End, PointerEventType.Released, PointerButton.None, PointerButton.Primary));
        }

        /// <summary>
        /// 模拟键盘输入
        /// </summary>
        private async Task SimulateKeyboardInputAsync(uint baseTime)
        {
            var keyboard = new InputDevice(InputDeviceType.Keyboard, 1, "Keyboard");

            var keys = new[] { Key.H, Key.E, Key.L, Key.L, Key.O };

            for (int i = 0; i < keys.Length; i++)
            {
                var keyDownEvent = new KeyboardEvent(
                    baseTime + (uint)(i * 100),
                    keyboard,
                    keys[i],
                    KeyboardEventType.KeyDown,
                    ModifierKeys.None);

                var keyUpEvent = new KeyboardEvent(
                    baseTime + (uint)(i * 100 + 50),
                    keyboard,
                    keys[i],
                    KeyboardEventType.KeyUp,
                    ModifierKeys.None);

                _inputPipeline?.SubmitEvent(keyDownEvent);
                await Task.Delay(30);
                _inputPipeline?.SubmitEvent(keyUpEvent);
                await Task.Delay(70);
            }
        }

        /// <summary>
        /// 测试性能
        /// </summary>
        private async Task TestPerformanceAsync()
        {
            Debug.WriteLine("\n=== 性能测试 ===");

            if (_inputPipeline == null)
                return;

            var device = new InputDevice(InputDeviceType.Mouse, 2, "Mouse");
            var eventCount = 1000;
            var stopwatch = Stopwatch.StartNew();

            // 批量提交事件
            for (int i = 0; i < eventCount; i++)
            {
                var position = new Point(100 + i % 800, 100 + (i / 10) % 600);
                var pointerEvent = new PointerEvent(
                    (uint)(5000 + i),
                    device,
                    PointerId.Mouse,
                    position,
                    PointerEventType.Moved,
                    PointerButton.None,
                    PointerButton.None);

                _inputPipeline.SubmitEvent(pointerEvent);
            }

            stopwatch.Stop();
            Debug.WriteLine($"提交 {eventCount} 个事件耗时: {stopwatch.ElapsedMilliseconds}ms");

            // 等待处理完成
            await Task.Delay(2000);
        }

        /// <summary>
        /// 显示系统统计
        /// </summary>
        private void ShowSystemStatistics()
        {
            Debug.WriteLine("\n=== 系统统计 ===");

            if (_inputPipeline == null)
                return;

            var stats = _inputPipeline.GetStatistics();
            Debug.WriteLine(stats.GetDetailedReport());
        }

        /// <summary>
        /// 处理输入事件处理完成
        /// </summary>
        private void OnInputEventProcessed(object? sender, InputEventProcessedEventArgs e)
        {
            if (e.ProcessingTime.TotalMilliseconds > 10) // 只记录较慢的事件
            {
                Debug.WriteLine($"[Pipeline] {e}");
            }
        }

        /// <summary>
        /// 处理管道错误
        /// </summary>
        private void OnPipelineError(object? sender, PipelineErrorEventArgs e)
        {
            Debug.WriteLine($"[Pipeline Error] {e}");
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _inputPipeline?.Dispose();
        }
    }

    /// <summary>
    /// 自定义输入处理器
    /// </summary>
    public class CustomInputProcessor : IInputProcessor
    {
        public int Priority => 50;
        public string Name => "Custom";

        public bool CanProcess(InputEvent inputEvent)
        {
            return inputEvent is PointerEvent;
        }

        public async Task<InputProcessingResult> ProcessAsync(InputEvent inputEvent, InputProcessingContext context)
        {
            // 自定义处理逻辑
            await Task.CompletedTask;
            return InputProcessingResult.CreateSuccess("自定义处理完成");
        }
    }

    /// <summary>
    /// 完整输入系统示例程序
    /// </summary>
    public static class CompleteInputSystemExampleProgram
    {
        /// <summary>
        /// 运行示例
        /// </summary>
        public static async Task RunExampleAsync()
        {
            using var example = new CompleteInputSystemExample();
            await example.RunAsync();
        }
    }
}
