using System;
using System.ComponentModel;
using System.Diagnostics;

namespace AetherUI.Core
{
    /// <summary>
    /// 设计时数据绑定测试类
    /// </summary>
    public static class DesignTimeTest
    {
        /// <summary>
        /// 运行设计时数据绑定测试
        /// </summary>
        public static void RunTests()
        {
            Console.WriteLine("AetherUI 设计时数据绑定测试");
            Console.WriteLine("============================");

            try
            {
                TestDesignTimeDetection();
                TestDesignTimeDataContext();
                TestDesignTimeBinding();
                TestDesignTimeHelper();

                Console.WriteLine("\n所有设计时数据绑定测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 测试设计时检测
        /// </summary>
        private static void TestDesignTimeDetection()
        {
            Console.WriteLine("\n=== 测试设计时检测 ===");

            // 测试默认检测
            bool isDesignMode = DesignTimeHelper.IsInDesignMode;
            Console.WriteLine($"✓ 当前设计时模式: {isDesignMode}");

            // 测试手动设置
            DesignTimeHelper.SetDesignMode(true);
            bool manualDesignMode = DesignTimeHelper.IsInDesignMode;
            Console.WriteLine($"✓ 手动设置设计时模式: {manualDesignMode}");

            // 测试运行时模式
            DesignTimeHelper.SetDesignMode(false);
            bool runtimeMode = DesignTimeHelper.IsInDesignMode;
            Console.WriteLine($"✓ 运行时模式: {!runtimeMode}");

            // 恢复设计时模式用于后续测试
            DesignTimeHelper.SetDesignMode(true);
        }

        /// <summary>
        /// 测试设计时数据上下文
        /// </summary>
        private static void TestDesignTimeDataContext()
        {
            Console.WriteLine("\n=== 测试设计时数据上下文 ===");

            try
            {
                // 创建设计时数据上下文
                var dataContext = new DesignTimeDataContext();
                Console.WriteLine("✓ 创建设计时数据上下文成功");

                // 设置属性
                dataContext.SetDesignTimeProperty("Title", "设计时标题");
                dataContext.SetDesignTimeProperty("Count", 42);
                dataContext.SetDesignTimeProperty("IsEnabled", true);
                Console.WriteLine("✓ 设置设计时属性成功");

                // 获取属性
                string? title = dataContext.GetDesignTimeProperty<string>("Title");
                int count = dataContext.GetDesignTimeProperty<int>("Count");
                bool isEnabled = dataContext.GetDesignTimeProperty<bool>("IsEnabled");

                Console.WriteLine($"  标题: {title}");
                Console.WriteLine($"  计数: {count}");
                Console.WriteLine($"  启用: {isEnabled}");

                // 测试属性存在性
                bool hasTitle = dataContext.HasDesignTimeProperty("Title");
                bool hasNonExistent = dataContext.HasDesignTimeProperty("NonExistent");
                Console.WriteLine($"✓ 属性存在性检查: Title={hasTitle}, NonExistent={hasNonExistent}");

                // 测试从类型生成
                var generatedContext = DesignTimeDataContext.CreateFor<TestViewModel>();
                Console.WriteLine("✓ 从类型生成设计时数据上下文成功");

                // 清除属性
                dataContext.ClearDesignTimeProperties();
                bool hasTitleAfterClear = dataContext.HasDesignTimeProperty("Title");
                Console.WriteLine($"✓ 清除属性后: Title={hasTitleAfterClear}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 设计时数据上下文测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试设计时绑定
        /// </summary>
        private static void TestDesignTimeBinding()
        {
            Console.WriteLine("\n=== 测试设计时绑定 ===");

            try
            {
                // 设置设计时值
                DesignTimeBinding.SetDesignTimeValue("TestKey", "测试值");
                DesignTimeBinding.SetDesignTimeValue("NumberKey", 123);
                Console.WriteLine("✓ 设置设计时值成功");

                // 获取设计时值
                string? testValue = DesignTimeBinding.GetDesignTimeValue<string>("TestKey");
                int numberValue = DesignTimeBinding.GetDesignTimeValue<int>("NumberKey");
                string? nonExistentValue = DesignTimeBinding.GetDesignTimeValue<string>("NonExistent");

                Console.WriteLine($"  测试值: {testValue}");
                Console.WriteLine($"  数字值: {numberValue}");
                Console.WriteLine($"  不存在的值: {nonExistentValue ?? "null"}");

                // 清除设计时值
                DesignTimeBinding.ClearDesignTimeValues();
                string? clearedValue = DesignTimeBinding.GetDesignTimeValue<string>("TestKey");
                Console.WriteLine($"✓ 清除后的值: {clearedValue ?? "null"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 设计时绑定测试失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试设计时助手
        /// </summary>
        private static void TestDesignTimeHelper()
        {
            Console.WriteLine("\n=== 测试设计时助手 ===");

            try
            {
                // 创建测试元素
                var testElement = new TestFrameworkElement();
                Console.WriteLine("✓ 创建测试元素成功");

                // 测试设计时数据上下文设置
                var testViewModel = new TestViewModel
                {
                    Title = "运行时标题",
                    Count = 100,
                    IsEnabled = false
                };

                DesignTimeHelper.SetDesignTimeDataContext(testElement, testViewModel);
                Console.WriteLine("✓ 设置设计时数据上下文成功");

                // 测试从类型生成数据上下文
                DesignTimeHelper.SetDesignTimeDataContext<TestViewModel>(testElement);
                Console.WriteLine("✓ 从类型生成设计时数据上下文成功");

                // 测试设计时属性
                DesignTimeHelper.SetDesignTimeProperty(testElement, "CustomProperty", "自定义值");
                object? customValue = DesignTimeHelper.GetDesignTimeProperty(testElement, "CustomProperty");
                Console.WriteLine($"✓ 设计时属性: {customValue}");

                // 测试设计时资源
                object? resource = DesignTimeHelper.GetDesignTimeResource("DesignTimeTitle");
                Console.WriteLine($"✓ 设计时资源: {resource}");

                // 测试设计时命令
                var command = DesignTimeHelper.CreateDesignTimeCommand("TestCommand");
                bool canExecute = command.CanExecute(null);
                Console.WriteLine($"✓ 设计时命令可执行: {canExecute}");

                if (canExecute)
                {
                    command.Execute(null);
                    Console.WriteLine("✓ 设计时命令执行成功");
                }

                // 测试绑定验证
                bool isValid = DesignTimeHelper.ValidateDesignTimeBinding(testElement);
                Console.WriteLine($"✓ 设计时绑定验证: {isValid}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 设计时助手测试失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 测试ViewModel类
    /// </summary>
    public class TestViewModel : ViewModelBase
    {
        private string _title = "默认标题";
        private int _count = 0;
        private bool _isEnabled = true;

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 计数
        /// </summary>
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        /// <summary>
        /// 测试命令
        /// </summary>
        public ICommand TestCommand { get; }

        /// <summary>
        /// 初始化测试ViewModel
        /// </summary>
        public TestViewModel()
        {
            TestCommand = new RelayCommand(ExecuteTest, CanExecuteTest);
        }

        /// <summary>
        /// 执行测试命令
        /// </summary>
        private void ExecuteTest()
        {
            Count++;
            Debug.WriteLine($"Test command executed, count: {Count}");
        }

        /// <summary>
        /// 测试命令是否可执行
        /// </summary>
        /// <returns>是否可执行</returns>
        private bool CanExecuteTest()
        {
            return IsEnabled;
        }
    }

    /// <summary>
    /// 测试FrameworkElement类
    /// </summary>
    public class TestFrameworkElement : FrameworkElement
    {
        /// <summary>
        /// 初始化测试FrameworkElement
        /// </summary>
        public TestFrameworkElement()
        {
            Debug.WriteLine("TestFrameworkElement created");
        }
    }
}
