using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AetherUI.Core;
using AetherUI.Integration.Input;

namespace AetherUI.Integration.Examples
{
    /// <summary>
    /// 布局输入集成示例
    /// </summary>
    public class LayoutInputIntegrationExample : IDisposable
    {
        private LayoutInputManager? _inputManager;
        private ExampleApplication? _application;
        private bool _isDisposed;

        /// <summary>
        /// 运行示例
        /// </summary>
        public async Task RunAsync()
        {
            Debug.WriteLine("开始布局输入集成示例");

            try
            {
                // 初始化输入管理器
                await InitializeInputManagerAsync();

                // 创建示例应用程序
                CreateApplication();

                // 设置事件处理器
                SetupEventHandlers();

                // 模拟用户交互
                await SimulateUserInteractionsAsync();

                // 生成诊断报告
                GenerateDiagnosticReport();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"示例运行失败: {ex.Message}");
            }

            Debug.WriteLine("布局输入集成示例完成");
        }

        /// <summary>
        /// 初始化输入管理器
        /// </summary>
        private async Task InitializeInputManagerAsync()
        {
            Debug.WriteLine("\n=== 初始化输入管理器 ===");

            // 创建高性能配置
            var config = LayoutInputConfiguration.CreateHighPerformance();
            config.EnableDiagnostics = true; // 启用诊断以便查看详细信息

            _inputManager = new LayoutInputManager(config);

            var success = await _inputManager.InitializeAsync();
            if (!success)
            {
                throw new InvalidOperationException("输入管理器初始化失败");
            }

            Debug.WriteLine("输入管理器初始化成功");
        }

        /// <summary>
        /// 创建示例应用程序
        /// </summary>
        private void CreateApplication()
        {
            Debug.WriteLine("\n=== 创建示例应用程序 ===");

            _application = new ExampleApplication();

            // 设置根元素到输入管理器
            _inputManager?.SetRootElement(_application.RootPanel);

            Debug.WriteLine("示例应用程序创建完成");
        }

        /// <summary>
        /// 设置事件处理器
        /// </summary>
        private void SetupEventHandlers()
        {
            Debug.WriteLine("\n=== 设置事件处理器 ===");

            if (_application == null)
                return;

            // 为按钮设置事件处理器
            _application.Button1.MouseDown += (sender, e) =>
            {
                Debug.WriteLine($"[Event] Button1 MouseDown at {e.Position}");
            };

            _application.Button1.MouseUp += (sender, e) =>
            {
                Debug.WriteLine($"[Event] Button1 MouseUp at {e.Position}");
            };

            _application.Button2.MouseEnter += (sender, e) =>
            {
                Debug.WriteLine($"[Event] Button2 MouseEnter at {e.Position}");
            };

            _application.Button2.MouseLeave += (sender, e) =>
            {
                Debug.WriteLine($"[Event] Button2 MouseLeave at {e.Position}");
            };

            // 为文本框设置事件处理器
            _application.TextBox.GotFocus += (sender, e) =>
            {
                Debug.WriteLine("[Event] TextBox GotFocus");
            };

            _application.TextBox.LostFocus += (sender, e) =>
            {
                Debug.WriteLine("[Event] TextBox LostFocus");
            };

            _application.TextBox.KeyDown += (sender, e) =>
            {
                Debug.WriteLine($"[Event] TextBox KeyDown: {e.Key}");
            };

            Debug.WriteLine("事件处理器设置完成");
        }

        /// <summary>
        /// 模拟用户交互
        /// </summary>
        private async Task SimulateUserInteractionsAsync()
        {
            Debug.WriteLine("\n=== 模拟用户交互 ===");

            if (_inputManager == null || _application == null)
                return;

            // 模拟焦点切换
            Debug.WriteLine("\n--- 模拟焦点切换 ---");
            _inputManager.SetFocus(_application.Button1);
            await Task.Delay(100);

            _inputManager.SetFocus(_application.TextBox);
            await Task.Delay(100);

            _inputManager.SetFocus(_application.Button2);
            await Task.Delay(100);

            // 模拟Tab键导航
            Debug.WriteLine("\n--- 模拟Tab键导航 ---");
            _inputManager.FocusManager.MoveFocusNext();
            await Task.Delay(100);

            _inputManager.FocusManager.MoveFocusNext();
            await Task.Delay(100);

            // 模拟鼠标捕获
            Debug.WriteLine("\n--- 模拟鼠标捕获 ---");
            _inputManager.CaptureMouse(_application.Button1);
            await Task.Delay(100);

            _inputManager.ReleaseMouse();
            await Task.Delay(100);

            Debug.WriteLine("用户交互模拟完成");
        }

        /// <summary>
        /// 生成诊断报告
        /// </summary>
        private void GenerateDiagnosticReport()
        {
            Debug.WriteLine("\n=== 生成诊断报告 ===");

            if (_inputManager == null)
                return;

            var diagnosticInfo = _inputManager.GetDiagnosticInfo();
            Debug.WriteLine(diagnosticInfo);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _inputManager?.Dispose();
            _application?.Dispose();

            Debug.WriteLine("布局输入集成示例已释放");
        }
    }

    /// <summary>
    /// 示例应用程序
    /// </summary>
    public class ExampleApplication : IDisposable
    {
        /// <summary>
        /// 根面板
        /// </summary>
        public ExamplePanel RootPanel { get; }

        /// <summary>
        /// 按钮1
        /// </summary>
        public ExampleButton Button1 { get; }

        /// <summary>
        /// 按钮2
        /// </summary>
        public ExampleButton Button2 { get; }

        /// <summary>
        /// 文本框
        /// </summary>
        public ExampleTextBox TextBox { get; }

        /// <summary>
        /// 初始化示例应用程序
        /// </summary>
        public ExampleApplication()
        {
            // 创建根面板
            RootPanel = new ExamplePanel("RootPanel");
            RootPanel.Width = 800;
            RootPanel.Height = 600;

            // 创建按钮1
            Button1 = new ExampleButton("Button1");
            Button1.Width = 100;
            Button1.Height = 30;
            Button1.Margin = new Thickness(10);
            Button1.Focusable = true;

            // 创建按钮2
            Button2 = new ExampleButton("Button2");
            Button2.Width = 100;
            Button2.Height = 30;
            Button2.Margin = new Thickness(120, 10, 10, 10);
            Button2.Focusable = true;

            // 创建文本框
            TextBox = new ExampleTextBox("TextBox");
            TextBox.Width = 200;
            TextBox.Height = 25;
            TextBox.Margin = new Thickness(10, 50, 10, 10);
            TextBox.Focusable = true;

            // 添加到根面板
            RootPanel.Children.Add(Button1);
            RootPanel.Children.Add(Button2);
            RootPanel.Children.Add(TextBox);

            Debug.WriteLine("示例应用程序已创建");
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            // 清理资源
            Debug.WriteLine("示例应用程序已释放");
        }
    }

    /// <summary>
    /// 示例面板
    /// </summary>
    public class ExamplePanel : Panel
    {
        /// <summary>
        /// 面板名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 初始化示例面板
        /// </summary>
        /// <param name="name">面板名称</param>
        public ExamplePanel(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString() => $"ExamplePanel({Name})";
    }

    /// <summary>
    /// 示例按钮
    /// </summary>
    public class ExampleButton : FrameworkElement
    {
        /// <summary>
        /// 按钮名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 初始化示例按钮
        /// </summary>
        /// <param name="name">按钮名称</param>
        public ExampleButton(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsHitTestVisible = true;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(Math.Min(Width, availableSize.Width), Math.Min(Height, availableSize.Height));
        }

        public override string ToString() => $"ExampleButton({Name})";
    }

    /// <summary>
    /// 示例文本框
    /// </summary>
    public class ExampleTextBox : FrameworkElement
    {
        /// <summary>
        /// 文本框名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// 初始化示例文本框
        /// </summary>
        /// <param name="name">文本框名称</param>
        public ExampleTextBox(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsHitTestVisible = true;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(Math.Min(Width, availableSize.Width), Math.Min(Height, availableSize.Height));
        }

        public override string ToString() => $"ExampleTextBox({Name})";
    }

    /// <summary>
    /// 布局输入集成示例程序
    /// </summary>
    public static class LayoutInputIntegrationExampleProgram
    {
        /// <summary>
        /// 运行示例
        /// </summary>
        public static async Task RunExampleAsync()
        {
            using var example = new LayoutInputIntegrationExample();
            await example.RunAsync();
        }

        /// <summary>
        /// 主入口点
        /// </summary>
        public static async Task Main(string[] args)
        {
            Debug.WriteLine("=== AetherUI 布局输入集成示例 ===");
            Debug.WriteLine($"启动时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Debug.WriteLine();

            try
            {
                await RunExampleAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"程序异常: {ex}");
            }

            Debug.WriteLine();
            Debug.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}
