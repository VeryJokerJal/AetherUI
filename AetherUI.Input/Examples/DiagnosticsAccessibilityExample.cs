using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AetherUI.Input.Accessibility;
using AetherUI.Input.Core;
using AetherUI.Input.Diagnostics;
using AetherUI.Input.Events;

namespace AetherUI.Input.Examples
{
    /// <summary>
    /// 诊断与无障碍示例
    /// </summary>
    public class DiagnosticsAccessibilityExample
    {
        private InputDiagnostics? _diagnostics;
        private AccessibilityManager? _accessibilityManager;
        private ExampleAccessibilityProvider? _accessibilityProvider;

        /// <summary>
        /// 运行示例
        /// </summary>
        public async Task RunAsync()
        {
            Debug.WriteLine("开始诊断与无障碍示例");

            try
            {
                // 初始化诊断系统
                InitializeDiagnostics();

                // 初始化无障碍系统
                InitializeAccessibility();

                // 测试诊断功能
                await TestDiagnosticsAsync();

                // 测试无障碍功能
                TestAccessibility();

                // 生成报告
                GenerateReports();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"示例运行失败: {ex.Message}");
            }

            Debug.WriteLine("诊断与无障碍示例完成");
        }

        /// <summary>
        /// 初始化诊断系统
        /// </summary>
        private void InitializeDiagnostics()
        {
            _diagnostics = new InputDiagnostics(5000);

            // 订阅诊断事件
            _diagnostics.DiagnosticEventOccurred += OnDiagnosticEvent;

            Debug.WriteLine("诊断系统初始化完成");
        }

        /// <summary>
        /// 初始化无障碍系统
        /// </summary>
        private void InitializeAccessibility()
        {
            _accessibilityManager = new AccessibilityManager();

            // 配置无障碍设置
            _accessibilityManager.Settings = new AccessibilitySettings
            {
                ScreenReaderEnabled = true,
                HighContrastEnabled = false,
                HapticFeedbackEnabled = true,
                AudioCuesEnabled = true,
                KeyboardNavigationEnabled = true,
                ReduceAnimations = false,
                FontScale = 1.2
            };

            // 注册无障碍提供者
            _accessibilityProvider = new ExampleAccessibilityProvider();
            _accessibilityManager.RegisterProvider(_accessibilityProvider);

            // 订阅无障碍事件
            _accessibilityManager.AccessibilityEvent += OnAccessibilityEvent;

            Debug.WriteLine("无障碍系统初始化完成");
        }

        /// <summary>
        /// 测试诊断功能
        /// </summary>
        private async Task TestDiagnosticsAsync()
        {
            Debug.WriteLine("\n=== 诊断功能测试 ===");

            if (_diagnostics == null)
                return;

            // 模拟各种输入事件
            var device = new InputDevice(InputDeviceType.Mouse, 0, "Mouse");

            Debug.WriteLine("\n--- 记录输入事件 ---");
            for (int i = 0; i < 10; i++)
            {
                var pointerEvent = new PointerEvent(
                    (uint)(1000 + i * 100),
                    device,
                    PointerId.Mouse,
                    new Point(100 + i * 10, 100 + i * 5),
                    PointerEventType.Moved,
                    PointerButton.None,
                    PointerButton.None);

                _diagnostics.LogInputEvent(pointerEvent, "Processing", $"Event {i + 1}");
                await Task.Delay(10);
            }

            Debug.WriteLine("\n--- 记录性能指标 ---");
            for (int i = 0; i < 5; i++)
            {
                using (_diagnostics.StartTiming("HitTest"))
                {
                    // 模拟命中测试操作
                    await Task.Delay(Random.Shared.Next(1, 10));
                }

                using (_diagnostics.StartTiming("EventRouting"))
                {
                    // 模拟事件路由操作
                    await Task.Delay(Random.Shared.Next(2, 15));
                }
            }

            Debug.WriteLine("\n--- 记录错误和警告 ---");
            _diagnostics.LogError("模拟错误", new InvalidOperationException("测试异常"), "测试上下文");
            _diagnostics.LogWarning("模拟警告", "测试场景");
            _diagnostics.LogInfo("模拟信息", "测试", "详细信息");

            Debug.WriteLine("\n--- 增加计数器 ---");
            for (int i = 0; i < 20; i++)
            {
                _diagnostics.IncrementCounter("TestCounter");
                _diagnostics.IncrementCounter("AnotherCounter", 2);
            }
        }

        /// <summary>
        /// 测试无障碍功能
        /// </summary>
        private void TestAccessibility()
        {
            Debug.WriteLine("\n=== 无障碍功能测试 ===");

            if (_accessibilityManager == null)
                return;

            // 创建测试元素
            var button = new ExampleUIElement("TestButton");
            var textBox = new ExampleUIElement("TestTextBox");
            var label = new ExampleUIElement("TestLabel");

            // 设置无障碍信息
            _accessibilityManager.SetAccessibilityInfo(button, new AccessibilityInfo
            {
                Name = "确定按钮",
                Description = "点击确定操作",
                Role = AccessibilityRole.Button,
                States = AccessibilityStates.None,
                KeyboardShortcut = "Enter",
                HelpText = "按回车键或点击此按钮确定操作"
            });

            _accessibilityManager.SetAccessibilityInfo(textBox, new AccessibilityInfo
            {
                Name = "用户名输入框",
                Description = "请输入您的用户名",
                Role = AccessibilityRole.TextBox,
                States = AccessibilityStates.Required,
                HelpText = "用户名长度应在3-20个字符之间"
            });

            _accessibilityManager.SetAccessibilityInfo(label, new AccessibilityInfo
            {
                Name = "用户名标签",
                Description = "用户名字段的标签",
                Role = AccessibilityRole.Label,
                States = AccessibilityStates.None
            });

            Debug.WriteLine("\n--- 测试键盘无障碍 ---");
            var keyboard = new InputDevice(InputDeviceType.Keyboard, 1, "Keyboard");

            // 模拟Tab键导航
            var tabEvent = new KeyboardEvent(
                2000,
                keyboard,
                Key.Tab,
                KeyboardEventType.KeyDown,
                ModifierKeys.None);

            _accessibilityManager.ProcessInputAccessibility(tabEvent, button);

            // 模拟Enter键激活
            var enterEvent = new KeyboardEvent(
                2100,
                keyboard,
                Key.Enter,
                KeyboardEventType.KeyDown,
                ModifierKeys.None);

            _accessibilityManager.ProcessInputAccessibility(enterEvent, button);

            Debug.WriteLine("\n--- 测试指针无障碍 ---");
            var mouse = new InputDevice(InputDeviceType.Mouse, 0, "Mouse");

            // 模拟鼠标点击
            var clickEvent = new PointerEvent(
                2200,
                mouse,
                PointerId.Mouse,
                new Point(100, 100),
                PointerEventType.Pressed,
                PointerButton.Primary,
                PointerButton.Primary);

            _accessibilityManager.ProcessInputAccessibility(clickEvent, textBox);

            Debug.WriteLine("\n--- 检查元素可访问性 ---");
            Debug.WriteLine($"按钮可访问: {_accessibilityManager.IsElementAccessible(button)}");
            Debug.WriteLine($"文本框可访问: {_accessibilityManager.IsElementAccessible(textBox)}");
            Debug.WriteLine($"标签可访问: {_accessibilityManager.IsElementAccessible(label)}");

            Debug.WriteLine("\n--- 构建无障碍树 ---");
            var accessibilityTree = _accessibilityManager.BuildAccessibilityTree(button);
            Debug.WriteLine($"无障碍树根节点子节点数: {accessibilityTree.Root.Children.Count}");
        }

        /// <summary>
        /// 生成报告
        /// </summary>
        private void GenerateReports()
        {
            Debug.WriteLine("\n=== 生成报告 ===");

            if (_diagnostics != null)
            {
                Debug.WriteLine("\n--- 诊断报告 ---");
                var diagnosticReport = _diagnostics.GenerateReport();
                Debug.WriteLine(diagnosticReport);

                Debug.WriteLine("\n--- 最近事件 ---");
                var recentEvents = _diagnostics.GetRecentEvents(5);
                foreach (var evt in recentEvents)
                {
                    Debug.WriteLine($"  {evt}");
                }

                Debug.WriteLine("\n--- 计数器统计 ---");
                var counters = _diagnostics.GetCounters();
                foreach (var counter in counters.Values)
                {
                    Debug.WriteLine($"  {counter}");
                }

                Debug.WriteLine("\n--- 计时器统计 ---");
                var timers = _diagnostics.GetTimers();
                foreach (var timer in timers.Values)
                {
                    Debug.WriteLine($"  {timer}");
                }
            }

            if (_accessibilityProvider != null)
            {
                Debug.WriteLine("\n--- 无障碍统计 ---");
                Debug.WriteLine($"处理的事件数: {_accessibilityProvider.ProcessedEventCount}");
                Debug.WriteLine($"触发的通知数: {_accessibilityProvider.NotificationCount}");
            }
        }

        /// <summary>
        /// 处理诊断事件
        /// </summary>
        private void OnDiagnosticEvent(object? sender, DiagnosticEventArgs e)
        {
            if (e.Event.EventType == DiagnosticEventType.Error)
            {
                Debug.WriteLine($"[Diagnostic Error] {e.Event}");
            }
        }

        /// <summary>
        /// 处理无障碍事件
        /// </summary>
        private void OnAccessibilityEvent(object? sender, AccessibilityEventArgs e)
        {
            Debug.WriteLine($"[Accessibility] {e}");
        }
    }

    /// <summary>
    /// 示例无障碍提供者
    /// </summary>
    public class ExampleAccessibilityProvider : IAccessibilityProvider
    {
        /// <summary>
        /// 处理的事件数
        /// </summary>
        public int ProcessedEventCount { get; private set; }

        /// <summary>
        /// 通知数
        /// </summary>
        public int NotificationCount { get; private set; }

        /// <summary>
        /// 处理输入事件
        /// </summary>
        public void OnInputEvent(InputEvent inputEvent, object targetElement, AccessibilityInfo accessibilityInfo)
        {
            ProcessedEventCount++;

            // 模拟屏幕阅读器通知
            if (inputEvent is KeyboardEvent keyboardEvent && keyboardEvent.Key == Key.Tab)
            {
                NotificationCount++;
                Debug.WriteLine($"[ScreenReader] 焦点移动到: {accessibilityInfo.Name}");
            }

            // 模拟触觉反馈
            if (inputEvent is PointerEvent pointerEvent && pointerEvent.EventType == PointerEventType.Pressed)
            {
                NotificationCount++;
                Debug.WriteLine($"[HapticFeedback] 触觉反馈: {accessibilityInfo.Name}");
            }
        }
    }

    /// <summary>
    /// 示例UI元素
    /// </summary>
    public class ExampleUIElement
    {
        /// <summary>
        /// 元素名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 初始化示例UI元素
        /// </summary>
        /// <param name="name">元素名称</param>
        public ExampleUIElement(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => Name;
    }

    /// <summary>
    /// 诊断与无障碍示例程序
    /// </summary>
    public static class DiagnosticsAccessibilityExampleProgram
    {
        /// <summary>
        /// 运行示例
        /// </summary>
        public static async Task RunExampleAsync()
        {
            var example = new DiagnosticsAccessibilityExample();
            await example.RunAsync();
        }
    }
}
