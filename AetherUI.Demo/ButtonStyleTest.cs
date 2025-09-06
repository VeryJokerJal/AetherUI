using System;
using System.Diagnostics;
using AetherUI.Core;
using AetherUI.Layout;
using AetherUI.Rendering;

namespace AetherUI.Demo
{
    /// <summary>
    /// 按钮样式测试程序
    /// </summary>
    public static class ButtonStyleTest
    {
        /// <summary>
        /// 运行按钮样式测试
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("    AetherUI 按钮样式测试程序");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("测试新增的按钮视觉样式属性...");
            Console.WriteLine("新功能特性：");
            Console.WriteLine("- 默认背景颜色 (#3498DB 蓝色)");
            Console.WriteLine("- 默认文本颜色 (#FFFFFF 白色)");
            Console.WriteLine("- 默认边框颜色 (#2980B9 深蓝色)");
            Console.WriteLine("- 圆角弧度支持 (默认 7 像素)");
            Console.WriteLine("- 动态颜色属性修改");
            Console.WriteLine("- 十六进制颜色值解析");
            Console.WriteLine();

            try
            {
                // 创建按钮样式测试UI
                UIElement styleTestUI = CreateButtonStyleTestPage();

                Console.WriteLine("按钮样式测试UI创建成功");
                Console.WriteLine("启动渲染窗口...");

                // 运行测试窗口
                RunButtonStyleTestWindow(styleTestUI);

                Console.WriteLine("按钮样式测试窗口已关闭。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"按钮样式测试运行失败: {ex.Message}");
                Console.WriteLine($"详细错误: {ex}");
Console.WriteLine("\n按任意键退出...");
                try
                {
                    _ = Console.ReadKey();
                }
                catch { }
            }
        }

        /// <summary>
        /// 创建按钮样式测试页面
        /// </summary>
        /// <returns>测试页面UI元素</returns>
        public static UIElement CreateButtonStyleTestPage()
        {
            Console.WriteLine("创建按钮样式测试页面...");

            // 主容器
            Border mainBorder = new()
            {
                Background = "White",
                Padding = new Thickness(30)
            };

            StackPanel mainPanel = new()
            {
                Orientation = Orientation.Vertical
            };

            // 页面标题
            TextBlock pageTitle = new()
            {
                Text = "🎨 按钮样式属性测试",
                FontSize = 28,
                FontFamily = "Microsoft YaHei",
                FontWeight = FontWeight.Bold,
                Foreground = "#2C3E50",
                Margin = new Thickness(0, 0, 0, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(pageTitle);

            // 测试网格
            Grid testGrid = new();
            testGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            testGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // 左列：默认样式测试
            StackPanel leftColumn = CreateDefaultStyleTests();
            Grid.SetColumn(leftColumn, 0);
            testGrid.Children.Add(leftColumn);

            // 右列：自定义样式测试
            StackPanel rightColumn = CreateCustomStyleTests();
            Grid.SetColumn(rightColumn, 1);
            testGrid.Children.Add(rightColumn);

            mainPanel.Children.Add(testGrid);
            mainBorder.Child = mainPanel;

            Console.WriteLine("按钮样式测试页面创建完成");
            return mainBorder;
        }

        /// <summary>
        /// 创建默认样式测试
        /// </summary>
        /// <returns>默认样式测试UI元素</returns>
        private static StackPanel CreateDefaultStyleTests()
        {
            StackPanel leftColumn = new()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 15, 0)
            };

            // 标题
            TextBlock leftTitle = new()
            {
                Text = "默认样式测试",
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Foreground = "#34495E",
                Margin = new Thickness(0, 0, 0, 20)
            };
            leftColumn.Children.Add(leftTitle);

            // 默认按钮（使用所有默认值）
            Button defaultButton = new()
            {
                Content = "默认样式按钮",
                Padding = new Thickness(20, 10, 20, 10),
                Margin = new Thickness(0, 0, 0, 15)
            };
            defaultButton.Click += (s, e) => Console.WriteLine("默认样式按钮被点击");
            leftColumn.Children.Add(defaultButton);

            // 显示默认属性值
            TextBlock defaultInfo = new()
            {
                Text = $"背景: {defaultButton.Background}\n" +
                       $"前景: {defaultButton.Foreground}\n" +
                       $"边框: {defaultButton.BorderBrush}\n" +
                       $"圆角: {defaultButton.CornerRadius}px",
                FontSize = 12,
                FontFamily = "Consolas",
                Foreground = "#7F8C8D",
                Margin = new Thickness(0, 0, 0, 20)
            };
            leftColumn.Children.Add(defaultInfo);

            // 圆角测试按钮
            Button[] cornerButtons = new[]
            {
                new Button { Content = "圆角 0px", CornerRadius = 0, Margin = new Thickness(0, 0, 0, 8) },
                new Button { Content = "圆角 5px", CornerRadius = 5, Margin = new Thickness(0, 0, 0, 8) },
                new Button { Content = "圆角 10px", CornerRadius = 10, Margin = new Thickness(0, 0, 0, 8) },
                new Button { Content = "圆角 20px", CornerRadius = 20, Margin = new Thickness(0, 0, 0, 8) }
            };

            foreach (Button btn in cornerButtons)
            {
                btn.Click += (s, e) => Console.WriteLine($"圆角按钮被点击: {btn.Content}");
                leftColumn.Children.Add(btn);
            }

            return leftColumn;
        }

        /// <summary>
        /// 创建自定义样式测试
        /// </summary>
        /// <returns>自定义样式测试UI元素</returns>
        private static StackPanel CreateCustomStyleTests()
        {
            StackPanel rightColumn = new()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(15, 0, 0, 0)
            };

            // 标题
            TextBlock rightTitle = new()
            {
                Text = "自定义样式测试",
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Foreground = "#34495E",
                Margin = new Thickness(0, 0, 0, 20)
            };
            rightColumn.Children.Add(rightTitle);

            // 自定义颜色按钮
            var customButtons = new[]
            {
                new { Text = "成功按钮", Bg = "#2ECC71", Fg = "#FFFFFF", Border = "#27AE60" },
                new { Text = "警告按钮", Bg = "#F39C12", Fg = "#FFFFFF", Border = "#E67E22" },
                new { Text = "危险按钮", Bg = "#E74C3C", Fg = "#FFFFFF", Border = "#C0392B" },
                new { Text = "信息按钮", Bg = "#3498DB", Fg = "#FFFFFF", Border = "#2980B9" },
                new { Text = "深色按钮", Bg = "#34495E", Fg = "#ECF0F1", Border = "#2C3E50" }
            };

            foreach (var btnConfig in customButtons)
            {
                Button customButton = new()
                {
                    Content = btnConfig.Text,
                    Background = btnConfig.Bg,
                    Foreground = btnConfig.Fg,
                    BorderBrush = btnConfig.Border,
                    CornerRadius = 8,
                    Padding = new Thickness(18, 10, 18, 10),
                    Margin = new Thickness(0, 0, 0, 12)
                };

                customButton.Click += (s, e) => 
                    Console.WriteLine($"自定义按钮被点击: {btnConfig.Text} (背景: {btnConfig.Bg})");

                rightColumn.Children.Add(customButton);
            }

            // 动态样式修改测试
            Button dynamicButton = new()
            {
                Content = "点击改变样式",
                Background = "#9B59B6",
                Foreground = "#FFFFFF",
                BorderBrush = "#8E44AD",
                CornerRadius = 12,
                Padding = new Thickness(20, 12, 20, 12),
                Margin = new Thickness(0, 20, 0, 0)
            };

            bool isDynamicToggled = false;
            dynamicButton.Click += (s, e) =>
            {
                isDynamicToggled = !isDynamicToggled;
                if (isDynamicToggled)
                {
                    dynamicButton.Background = "#E67E22";
                    dynamicButton.BorderBrush = "#D35400";
                    dynamicButton.CornerRadius = 25;
                    dynamicButton.Content = "样式已改变";
                }
                else
                {
                    dynamicButton.Background = "#9B59B6";
                    dynamicButton.BorderBrush = "#8E44AD";
                    dynamicButton.CornerRadius = 12;
                    dynamicButton.Content = "点击改变样式";
                }
                Console.WriteLine($"动态按钮样式切换: {(isDynamicToggled ? "橙色" : "紫色")}");
            };

            rightColumn.Children.Add(dynamicButton);

            return rightColumn;
        }

        /// <summary>
        /// 运行按钮样式测试窗口
        /// </summary>
        /// <param name="styleTestUI">样式测试UI</param>
        private static void RunButtonStyleTestWindow(UIElement styleTestUI)
        {
            // 创建窗口
            Window window = new(1200, 800, "AetherUI 按钮样式测试")
            {
                RootElement = styleTestUI
            };

            // 配置背景效果
            BackgroundEffectConfig backgroundConfig = new()
            {
                Type = BackgroundEffectType.Acrylic,
                Opacity = 0.92f,
                TintColor = new OpenTK.Mathematics.Vector4(0.97f, 0.97f, 0.99f, 1.0f)
            };

            // 窗口加载事件
            window.Load += () =>
            {
                window.SetBackgroundEffect(backgroundConfig);
                Console.WriteLine("按钮样式测试背景效果已启用");
                Console.WriteLine();
                Console.WriteLine("=== 测试说明 ===");
                Console.WriteLine("1. 观察默认样式按钮的渲染效果");
                Console.WriteLine("2. 测试不同圆角弧度的视觉差异");
                Console.WriteLine("3. 验证自定义颜色属性的正确解析");
                Console.WriteLine("4. 点击动态按钮测试样式实时修改");
                Console.WriteLine("5. 观察控制台输出的调试信息");
                Console.WriteLine("6. 按ESC键退出测试");
                Console.WriteLine();
            };

            // 运行窗口
            window.Run();
        }
    }
}
