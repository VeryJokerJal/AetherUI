using System;
using AetherUI.Core;
using AetherUI.Layout;

namespace AetherUI.Layout
{
    /// <summary>
    /// 程序入口点，用于测试StackPanel布局
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 主函数
        /// </summary>
        /// <param name="args">命令行参数</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("AetherUI StackPanel 布局测试");
            Console.WriteLine("============================");
            Console.WriteLine();

            try
            {
                // 运行StackPanel测试
                Console.WriteLine("StackPanel 测试:");
                Console.WriteLine("================");
                TestVerticalStackPanel();
                TestHorizontalStackPanel();
                TestNestedStackPanel();

                Console.WriteLine("\nGrid 测试:");
                Console.WriteLine("==========");
                // 运行Grid测试
                GridTest.RunAllTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试过程中发生错误: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("测试完成！");
        }

        private static void TestVerticalStackPanel()
        {
            Console.WriteLine("=== 测试垂直StackPanel ===");

            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // 添加多个子元素
            stackPanel.Children.Add(new TextBlock { Text = "标题", FontSize = 16 });
            stackPanel.Children.Add(new Button { Content = "按钮1" });
            stackPanel.Children.Add(new Button { Content = "按钮2" });
            stackPanel.Children.Add(new TextBlock { Text = "描述文本", FontSize = 10 });

            PerformLayout(stackPanel, new AetherUI.Core.Size(200, 300));
            Console.WriteLine();
        }

        private static void TestHorizontalStackPanel()
        {
            Console.WriteLine("=== 测试水平StackPanel ===");

            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // 添加水平排列的按钮
            stackPanel.Children.Add(new Button { Content = "确定" });
            stackPanel.Children.Add(new Button { Content = "取消" });
            stackPanel.Children.Add(new Button { Content = "应用" });

            PerformLayout(stackPanel, new AetherUI.Core.Size(400, 50));
            Console.WriteLine();
        }

        private static void TestNestedStackPanel()
        {
            Console.WriteLine("=== 测试嵌套StackPanel ===");

            // 主垂直面板
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // 添加标题
            mainPanel.Children.Add(new TextBlock { Text = "嵌套布局示例", FontSize = 14 });

            // 创建水平按钮面板
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            buttonPanel.Children.Add(new Button { Content = "确定" });
            buttonPanel.Children.Add(new Button { Content = "取消" });

            // 添加到主面板
            mainPanel.Children.Add(buttonPanel);
            mainPanel.Children.Add(new TextBlock { Text = "底部说明", FontSize = 10 });

            PerformLayout(mainPanel, new AetherUI.Core.Size(300, 200));
            Console.WriteLine();
        }

        private static void PerformLayout(UIElement element, AetherUI.Core.Size availableSize)
        {
            Console.WriteLine($"布局元素: {element.GetType().Name}");
            Console.WriteLine($"可用尺寸: {availableSize}");

            // 测量
            element.Measure(availableSize);
            Console.WriteLine($"期望尺寸: {element.DesiredSize}");

            // 排列
            element.Arrange(new AetherUI.Core.Rect(0, 0, availableSize.Width, availableSize.Height));
            Console.WriteLine($"渲染尺寸: {element.RenderSize}");
        }
    }
}
