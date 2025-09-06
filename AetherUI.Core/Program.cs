using System;

namespace AetherUI.Core
{
    /// <summary>
    /// 核心功能测试程序
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 程序入口点
        /// </summary>
        public static void Main()
        {
            Console.WriteLine("AetherUI 核心功能测试");
            Console.WriteLine("====================");

            try
            {
                TestBasicInfrastructure();
                DesignTimeTest.RunTests();

                Console.WriteLine("\n所有核心功能测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }

            Console.WriteLine("按任意键退出...");
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
        /// 测试基础设施
        /// </summary>
        private static void TestBasicInfrastructure()
        {
            Console.WriteLine("\n=== 测试基础设施 ===");

            try
            {
                // 测试ViewModelBase
                var viewModel = new TestViewModel();
                Console.WriteLine($"✓ 创建ViewModel成功: {viewModel.Title}");

                // 测试属性变化通知
                bool propertyChanged = false;
                viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "Title")
                        propertyChanged = true;
                };

                viewModel.Title = "新标题";
                Console.WriteLine($"✓ 属性变化通知: {propertyChanged}");

                // 测试命令
                bool commandExecuted = false;
                var command = new RelayCommand(() => commandExecuted = true);
                
                bool canExecute = command.CanExecute(null);
                command.Execute(null);
                
                Console.WriteLine($"✓ 命令执行: CanExecute={canExecute}, Executed={commandExecuted}");

                // 测试依赖属性
                var element = new TestFrameworkElement();
                element.Width = 100;
                element.Height = 200;
                
                Console.WriteLine($"✓ 依赖属性: Width={element.Width}, Height={element.Height}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 基础设施测试失败: {ex.Message}");
            }
        }
    }
}
