using System;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 布局测试类，用于验证布局容器的功能
    /// </summary>
    public static class LayoutTest
    {
        /// <summary>
        /// 测试StackPanel的垂直布局
        /// </summary>
        public static void TestVerticalStackPanel()
        {
            Console.WriteLine("=== Testing Vertical StackPanel ===");

            // 创建垂直StackPanel
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Width = 200,
                Height = 300
            };

            // 添加子元素
            TextBlock title = new TextBlock
            {
                Text = "标题文本",
                FontSize = 16
            };

            Button button1 = new Button
            {
                Content = "按钮1",
                Margin = new Thickness(0, 5, 0, 5)
            };

            Button button2 = new Button
            {
                Content = "按钮2",
                Margin = new Thickness(0, 5, 0, 5)
            };

            TextBlock description = new TextBlock
            {
                Text = "这是一个描述文本",
                FontSize = 12
            };

            // 添加到StackPanel
            stackPanel.Children.Add(title);
            stackPanel.Children.Add(button1);
            stackPanel.Children.Add(button2);
            stackPanel.Children.Add(description);

            // 执行布局
            PerformLayout(stackPanel, new Size(200, 300));

            Console.WriteLine("=== Vertical StackPanel Test Complete ===\n");
        }

        /// <summary>
        /// 测试StackPanel的水平布局
        /// </summary>
        public static void TestHorizontalStackPanel()
        {
            Console.WriteLine("=== Testing Horizontal StackPanel ===");

            // 创建水平StackPanel
            StackPanel stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Width = 400,
                Height = 50
            };

            // 添加子元素
            Button button1 = new Button
            {
                Content = "确定",
                Margin = new Thickness(5, 0, 5, 0)
            };

            Button button2 = new Button
            {
                Content = "取消",
                Margin = new Thickness(5, 0, 5, 0)
            };

            Button button3 = new Button
            {
                Content = "应用",
                Margin = new Thickness(5, 0, 5, 0)
            };

            // 添加到StackPanel
            stackPanel.Children.Add(button1);
            stackPanel.Children.Add(button2);
            stackPanel.Children.Add(button3);

            // 执行布局
            PerformLayout(stackPanel, new Size(400, 50));
}

        /// <summary>
        /// 测试嵌套StackPanel
        /// </summary>
        public static void TestNestedStackPanel()
        {
            StackPanel mainPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Width = 300,
                Height = 200
            };

            // 添加标题
            TextBlock title = new TextBlock
            {
                Text = "嵌套布局测试",
                FontSize = 14
            };

            // 创建水平按钮面板
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 10)
            };

            Button okButton = new Button
            {
                Content = "确定",
                Margin = new Thickness(0, 0, 10, 0)
            };

            Button cancelButton = new Button
            {
                Content = "取消"
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            // 添加描述
            TextBlock description = new TextBlock
            {
                Text = "这是一个嵌套布局的示例",
                FontSize = 10
            };

            // 组装主面板
            mainPanel.Children.Add(title);
            mainPanel.Children.Add(buttonPanel);
            mainPanel.Children.Add(description);

            // 执行布局
            PerformLayout(mainPanel, new Size(300, 200));
}

        /// <summary>
        /// 执行布局测试
        /// </summary>
        /// <param name="element">要测试的元素</param>
        /// <param name="availableSize">可用尺寸</param>
        private static void PerformLayout(UIElement element, Size availableSize)
        {
Debug.WriteLine($"Available size: {availableSize}");

            // 测量阶段
            element.Measure(availableSize);
            Rect finalRect = new Rect(0, 0, availableSize.Width, availableSize.Height);
            element.Arrange(finalRect);
Debug.WriteLine("Layout complete");
        }

        /// <summary>
        /// 运行所有测试
        /// </summary>
        public static void RunAllTests()
        {
try
            {
                TestVerticalStackPanel();
                TestHorizontalStackPanel();
                TestNestedStackPanel();
}
            catch (Exception ex)
            {
Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
