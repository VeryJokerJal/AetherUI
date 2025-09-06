using System;
using System.Diagnostics;
using AetherUI.Core;
using AetherUI.Layout;

namespace AetherUI.Demo
{
    /// <summary>
    /// AetherUI框架简化演示程序
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 程序入口点
        /// </summary>
        public static void Main()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("       AetherUI 框架演示程序");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            try
            {
                // 运行基础功能演示
                RunBasicDemo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"演示程序运行失败: {ex.Message}");
                Console.WriteLine($"详细错误: {ex}");
                Debug.WriteLine($"Demo error: {ex}");
            }

            Console.WriteLine();
            Console.WriteLine("演示程序结束。按任意键退出...");
            try
            {
                Console.ReadKey();
            }
            catch
            {
                // 忽略控制台读取错误
            }
        }

        /// <summary>
        /// 运行基础演示
        /// </summary>
        private static void RunBasicDemo()
        {
            Console.WriteLine("开始AetherUI基础功能演示...");
            Console.WriteLine();

            // 1. 测试基础UI元素创建
            Console.WriteLine("1. 测试基础UI元素创建");
            TestBasicElements();

            // 2. 测试布局容器
            Console.WriteLine("\n2. 测试布局容器");
            TestLayoutContainers();

            // 3. 测试MVVM功能
            Console.WriteLine("\n3. 测试MVVM功能");
            TestMVVMFeatures();

            // 4. 测试设计时功能
            Console.WriteLine("\n4. 测试设计时功能");
            TestDesignTimeFeatures();

            Console.WriteLine("\n所有基础功能测试完成！");
        }

        /// <summary>
        /// 测试基础UI元素
        /// </summary>
        private static void TestBasicElements()
        {
            try
            {
                // 创建基础控件
                var textBlock = new TextBlock { Text = "Hello AetherUI!" };
                var button = new Button { Content = "Test Button" };
                var border = new Border();

                Console.WriteLine($"✓ TextBlock创建成功: {textBlock.Text}");
                Console.WriteLine($"✓ Button创建成功: {button.Content}");
                Console.WriteLine($"✓ Border创建成功");

                // 测试属性设置
                textBlock.Width = 200;
                textBlock.Height = 30;
                button.Width = 100;
                button.Height = 25;

                Console.WriteLine($"✓ 属性设置成功 - TextBlock: {textBlock.Width}x{textBlock.Height}");
                Console.WriteLine($"✓ 属性设置成功 - Button: {button.Width}x{button.Height}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 基础元素测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试布局容器
        /// </summary>
        private static void TestLayoutContainers()
        {
            try
            {
                // 测试StackPanel
                var stackPanel = new StackPanel();
                stackPanel.Children.Add(new TextBlock { Text = "Item 1" });
                stackPanel.Children.Add(new TextBlock { Text = "Item 2" });
                stackPanel.Children.Add(new Button { Content = "Button" });

                Console.WriteLine($"✓ StackPanel创建成功，包含 {stackPanel.Children.Count} 个子元素");

                // 测试Grid
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                Console.WriteLine($"✓ Grid创建成功，{grid.RowDefinitions.Count}行 {grid.ColumnDefinitions.Count}列");

                // 测试Canvas
                var canvas = new Canvas();
                var canvasChild = new TextBlock { Text = "Canvas Child" };
                Canvas.SetLeft(canvasChild, 10);
                Canvas.SetTop(canvasChild, 20);
                canvas.Children.Add(canvasChild);

                Console.WriteLine($"✓ Canvas创建成功，包含 {canvas.Children.Count} 个子元素");

                // 测试其他容器
                var dockPanel = new DockPanel();
                var wrapPanel = new WrapPanel();
                var uniformGrid = new UniformGrid();
                var card = new Card();
                card.Header = new TextBlock { Text = "Test Card" };

                Console.WriteLine("✓ DockPanel创建成功");
                Console.WriteLine("✓ WrapPanel创建成功");
                Console.WriteLine("✓ UniformGrid创建成功");
                Console.WriteLine($"✓ Card创建成功: {card.Header}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 布局容器测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试MVVM功能
        /// </summary>
        private static void TestMVVMFeatures()
        {
            try
            {
                // 创建ViewModel
                var viewModel = new TestViewModel();
                Console.WriteLine($"✓ ViewModel创建成功: {viewModel.Title}");

                // 测试属性变化通知
                bool propertyChanged = false;
                viewModel.PropertyChanged += (s, e) =>
                {
                    propertyChanged = true;
                    Console.WriteLine($"✓ 属性变化通知: {e.PropertyName}");
                };

                viewModel.Title = "Updated Title";
                viewModel.Counter = 42;

                if (propertyChanged)
                {
                    Console.WriteLine("✓ PropertyChanged事件正常工作");
                }

                // 测试命令
                if (viewModel.TestCommand.CanExecute(null))
                {
                    viewModel.TestCommand.Execute(null);
                    Console.WriteLine("✓ 命令执行成功");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ MVVM功能测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试设计时功能
        /// </summary>
        private static void TestDesignTimeFeatures()
        {
            try
            {
                // 测试设计时检测
                bool isDesignTime = DesignTimeHelper.IsInDesignMode;
                Console.WriteLine($"✓ 设计时检测: {(isDesignTime ? "设计时模式" : "运行时模式")}");

                // 测试设计时数据上下文
                var element = new StackPanel();
                var designTimeContext = new DesignTimeDataContext();
                designTimeContext.SetDesignTimeProperty("TestProperty", "TestValue");

                DesignTimeHelper.SetDesignTimeDataContext(element, designTimeContext);
                Console.WriteLine("✓ 设计时数据上下文设置成功");

                // 测试设计时数据生成
                var generatedContext = DesignTimeDataContext.CreateFor<TestViewModel>();
                Console.WriteLine($"✓ 设计时数据生成成功: {generatedContext.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 设计时功能测试失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 测试用的ViewModel
    /// </summary>
    public class TestViewModel : ViewModelBase
    {
        private string _title = "Test ViewModel";
        private int _counter = 0;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public int Counter
        {
            get => _counter;
            set => SetProperty(ref _counter, value);
        }

        public ICommand TestCommand { get; }

        public TestViewModel()
        {
            TestCommand = new RelayCommand(ExecuteTest);
        }

        private void ExecuteTest()
        {
            Counter++;
            Console.WriteLine($"Test command executed, counter: {Counter}");
        }
    }
}
