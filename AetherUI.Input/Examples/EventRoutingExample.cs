using System;
using System.Diagnostics;
using AetherUI.Input.Core;
using AetherUI.Input.Events;
using AetherUI.Input.HitTesting;
using AetherUI.Input.Routing;

namespace AetherUI.Input.Examples
{
    /// <summary>
    /// 事件路由示例
    /// </summary>
    public class EventRoutingExample
    {
        private AdvancedEventRouter? _eventRouter;
        private RoutableElement? _rootElement;
        private RoutedEventDefinition? _clickEvent;
        private RoutedEventDefinition? _mouseOverEvent;

        /// <summary>
        /// 运行示例
        /// </summary>
        public void Run()
        {
            Debug.WriteLine("开始事件路由示例");

            try
            {
                // 初始化事件路由器
                InitializeEventRouter();

                // 创建可路由元素树
                CreateRoutableElementTree();

                // 注册事件处理器
                RegisterEventHandlers();

                // 测试事件路由
                TestEventRouting();

                // 测试拦截器
                TestInterceptors();

                // 显示性能统计
                ShowPerformanceStats();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"示例运行失败: {ex.Message}");
            }

            Debug.WriteLine("事件路由示例完成");
        }

        /// <summary>
        /// 初始化事件路由器
        /// </summary>
        private void InitializeEventRouter()
        {
            var eventManager = new RoutedEventManager();
            var handlerStore = new AdvancedEventHandlerStore();
            _eventRouter = new AdvancedEventRouter(eventManager, handlerStore);

            // 注册路由事件
            _clickEvent = _eventRouter.RegisterRoutedEvent(
                "Click",
                RoutingStrategy.Bubble,
                typeof(PointerEvent),
                typeof(EventHandler<PointerEvent>),
                typeof(RoutableElement));

            _mouseOverEvent = _eventRouter.RegisterRoutedEvent(
                "MouseOver",
                RoutingStrategy.Direct,
                typeof(PointerEvent),
                typeof(EventHandler<PointerEvent>),
                typeof(RoutableElement));

            // 添加拦截器
            _eventRouter.AddInterceptor(InterceptorFactory.CreateLogging());
            var perfInterceptor = InterceptorFactory.CreatePerformanceMonitor();
            _eventRouter.AddInterceptor(perfInterceptor);

            Debug.WriteLine("事件路由器初始化完成");
        }

        /// <summary>
        /// 创建可路由元素树
        /// </summary>
        private void CreateRoutableElementTree()
        {
            _rootElement = new RoutableElement("Root", new Rect(0, 0, 800, 600));

            var panel1 = new RoutableElement("Panel1", new Rect(50, 50, 300, 200));
            var panel2 = new RoutableElement("Panel2", new Rect(400, 50, 300, 200));

            var button1 = new RoutableElement("Button1", new Rect(20, 20, 100, 40));
            var button2 = new RoutableElement("Button2", new Rect(150, 20, 100, 40));
            var button3 = new RoutableElement("Button3", new Rect(20, 80, 100, 40));

            var textBox = new RoutableElement("TextBox", new Rect(20, 20, 200, 30));

            // 构建树结构
            panel1.AddChild(button1);
            panel1.AddChild(button2);
            panel1.AddChild(button3);

            panel2.AddChild(textBox);

            _rootElement.AddChild(panel1);
            _rootElement.AddChild(panel2);

            Debug.WriteLine("可路由元素树已创建");
        }

        /// <summary>
        /// 注册事件处理器
        /// </summary>
        private void RegisterEventHandlers()
        {
            if (_eventRouter == null || _clickEvent == null || _mouseOverEvent == null)
                return;

            // 根元素处理器
            _eventRouter.AddHandler(_rootElement!, _clickEvent, new EventHandler<PointerEvent>((sender, e) =>
            {
                Debug.WriteLine($"[Root] Click handled: {sender}");
            }));

            // Panel处理器
            var panel1 = FindElement("Panel1");
            var panel2 = FindElement("Panel2");

            if (panel1 != null)
            {
                _eventRouter.AddHandler(panel1, _clickEvent, new EventHandler<PointerEvent>((sender, e) =>
                {
                    Debug.WriteLine($"[Panel1] Click handled: {sender}");
                }));
            }

            if (panel2 != null)
            {
                _eventRouter.AddHandler(panel2, _clickEvent, new EventHandler<PointerEvent>((sender, e) =>
                {
                    Debug.WriteLine($"[Panel2] Click handled: {sender}");
                    e.Handled = true; // 阻止冒泡
                }));
            }

            // Button处理器
            var button1 = FindElement("Button1");
            var button2 = FindElement("Button2");
            var button3 = FindElement("Button3");

            if (button1 != null)
            {
                _eventRouter.AddHandler(button1, _clickEvent, new EventHandler<PointerEvent>((sender, e) =>
                {
                    Debug.WriteLine($"[Button1] Click handled: {sender}");
                }));

                _eventRouter.AddHandler(button1, _mouseOverEvent, new EventHandler<PointerEvent>((sender, e) =>
                {
                    Debug.WriteLine($"[Button1] MouseOver handled: {sender}");
                }));
            }

            if (button2 != null)
            {
                _eventRouter.AddHandler(button2, _clickEvent, new EventHandler<PointerEvent>((sender, e) =>
                {
                    Debug.WriteLine($"[Button2] Click handled: {sender}");
                }), true); // 即使已处理也要调用
            }

            if (button3 != null)
            {
                _eventRouter.AddHandler(button3, _clickEvent, new EventHandler<PointerEvent>((sender, e) =>
                {
                    Debug.WriteLine($"[Button3] Click handled: {sender}");
                }));
            }

            // TextBox处理器
            var textBox = FindElement("TextBox");
            if (textBox != null)
            {
                _eventRouter.AddHandler(textBox, _clickEvent, new EventHandler<PointerEvent>((sender, e) =>
                {
                    Debug.WriteLine($"[TextBox] Click handled: {sender}");
                }));
            }

            Debug.WriteLine("事件处理器注册完成");
        }

        /// <summary>
        /// 测试事件路由
        /// </summary>
        private void TestEventRouting()
        {
            Debug.WriteLine("\n=== 事件路由测试 ===");

            if (_eventRouter == null || _clickEvent == null || _mouseOverEvent == null)
                return;

            var device = new InputDevice(InputDeviceType.Mouse, 0, "Mouse");

            // 测试Button1点击（冒泡路由）
            Debug.WriteLine("\n--- Button1 点击测试 ---");
            var button1 = FindElement("Button1");
            if (button1 != null)
            {
                var clickEvent = new PointerEvent(
                    (uint)Environment.TickCount,
                    device,
                    PointerId.Mouse,
                    new Point(70, 70),
                    PointerEventType.Pressed,
                    PointerButton.Primary,
                    PointerButton.Primary);

                var eventArgs = new RoutedEventArgs(_clickEvent, button1);
                _eventRouter.RouteEvent(button1, eventArgs);
            }

            // 测试TextBox点击（被Panel2阻止冒泡）
            Debug.WriteLine("\n--- TextBox 点击测试 ---");
            var textBox = FindElement("TextBox");
            if (textBox != null)
            {
                var clickEvent = new PointerEvent(
                    (uint)Environment.TickCount,
                    device,
                    PointerId.Mouse,
                    new Point(520, 70),
                    PointerEventType.Pressed,
                    PointerButton.Primary,
                    PointerButton.Primary);

                var eventArgs = new RoutedEventArgs(_clickEvent, textBox);
                _eventRouter.RouteEvent(textBox, eventArgs);
            }

            // 测试MouseOver（直接路由）
            Debug.WriteLine("\n--- Button1 MouseOver 测试 ---");
            if (button1 != null)
            {
                var mouseOverEvent = new PointerEvent(
                    (uint)Environment.TickCount,
                    device,
                    PointerId.Mouse,
                    new Point(70, 70),
                    PointerEventType.Moved,
                    PointerButton.None,
                    PointerButton.None);

                var eventArgs = new RoutedEventArgs(_mouseOverEvent, button1);
                _eventRouter.RouteEvent(button1, eventArgs);
            }
        }

        /// <summary>
        /// 测试拦截器
        /// </summary>
        private void TestInterceptors()
        {
            Debug.WriteLine("\n=== 拦截器测试 ===");

            if (_eventRouter == null || _clickEvent == null)
                return;

            // 添加安全拦截器
            var securityInterceptor = InterceptorFactory.CreateSecurity();
            securityInterceptor.BlockEvent("Click");
            _eventRouter.AddInterceptor(securityInterceptor);

            Debug.WriteLine("\n--- 安全拦截器阻止点击 ---");
            var button2 = FindElement("Button2");
            if (button2 != null)
            {
                var device = new InputDevice(InputDeviceType.Mouse, 0, "Mouse");
                var clickEvent = new PointerEvent(
                    (uint)Environment.TickCount,
                    device,
                    PointerId.Mouse,
                    new Point(200, 70),
                    PointerEventType.Pressed,
                    PointerButton.Primary,
                    PointerButton.Primary);

                var eventArgs = new RoutedEventArgs(_clickEvent, button2);
                _eventRouter.RouteEvent(button2, eventArgs);
            }

            // 移除安全拦截器
            _eventRouter.RemoveInterceptor(securityInterceptor);

            // 添加条件拦截器
            var conditionalInterceptor = InterceptorFactory.CreateConditional(context =>
            {
                var elementName = (context.Target as RoutableElement)?.Name ?? "";
                return !elementName.Contains("Button3"); // 阻止Button3的事件
            });
            _eventRouter.AddInterceptor(conditionalInterceptor);

            Debug.WriteLine("\n--- 条件拦截器阻止Button3 ---");
            var button3 = FindElement("Button3");
            if (button3 != null)
            {
                var device = new InputDevice(InputDeviceType.Mouse, 0, "Mouse");
                var clickEvent = new PointerEvent(
                    (uint)Environment.TickCount,
                    device,
                    PointerId.Mouse,
                    new Point(70, 130),
                    PointerEventType.Pressed,
                    PointerButton.Primary,
                    PointerButton.Primary);

                var eventArgs = new RoutedEventArgs(_clickEvent, button3);
                _eventRouter.RouteEvent(button3, eventArgs);
            }
        }

        /// <summary>
        /// 显示性能统计
        /// </summary>
        private void ShowPerformanceStats()
        {
            Debug.WriteLine("\n=== 性能统计 ===");

            if (_eventRouter == null)
                return;

            var cacheStats = _eventRouter.GetCacheStats();
            Debug.WriteLine($"路由缓存统计: {cacheStats}");

            // 获取性能拦截器的报告
            foreach (var interceptor in _eventRouter.GetType().GetField("_interceptors", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_eventRouter) as System.Collections.Generic.List<IEventRoutingInterceptor> ?? new())
            {
                if (interceptor is PerformanceInterceptor perfInterceptor)
                {
                    Debug.WriteLine(perfInterceptor.GetPerformanceReport());
                    break;
                }
            }
        }

        /// <summary>
        /// 查找元素
        /// </summary>
        private RoutableElement? FindElement(string name)
        {
            return FindElementRecursive(_rootElement, name);
        }

        /// <summary>
        /// 递归查找元素
        /// </summary>
        private RoutableElement? FindElementRecursive(RoutableElement? element, string name)
        {
            if (element == null)
                return null;

            if (element.Name == name)
                return element;

            foreach (var child in element.Children)
            {
                if (child is RoutableElement routableChild)
                {
                    var found = FindElementRecursive(routableChild, name);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// 可路由元素
    /// </summary>
    public class RoutableElement : ExampleElement
    {
        /// <summary>
        /// 初始化可路由元素
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="bounds">边界</param>
        public RoutableElement(string name, Rect bounds) : base(name, bounds)
        {
        }

        public override string ToString() => $"RoutableElement({Name})";
    }

    /// <summary>
    /// 事件路由示例程序
    /// </summary>
    public static class EventRoutingExampleProgram
    {
        /// <summary>
        /// 运行示例
        /// </summary>
        public static void RunExample()
        {
            var example = new EventRoutingExample();
            example.Run();
        }
    }
}
