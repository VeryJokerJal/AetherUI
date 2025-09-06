using System;
using System.Diagnostics;
using AetherUI.Core;
using AetherUI.Layout;

namespace AetherUI.Demo.Demos
{
    /// <summary>
    /// 按钮控件完整演示
    /// </summary>
    public static class ButtonDemo
    {
        /// <summary>
        /// 创建按钮演示页面
        /// </summary>
        /// <returns>按钮演示UI元素</returns>
        public static UIElement CreateButtonDemoPage()
        {
            Console.WriteLine("创建按钮控件演示页面...");

            // 主滚动容器
            Border mainBorder = new()
            {
                Background = "White",
                Padding = new Thickness(20)
            };

            StackPanel mainPanel = new()
            {
                Orientation = Orientation.Vertical
            };

            // 页面标题
            TextBlock pageTitle = new()
            {
                Text = "🔘 按钮控件完整演示",
                FontSize = 32,
                FontFamily = "Microsoft YaHei",
                FontWeight = FontWeight.Bold,
                Foreground = "#2C3E50",
                Margin = new Thickness(0, 0, 0, 30),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(pageTitle);

            // 页面描述
            TextBlock pageDescription = new()
            {
                Text = "本页面展示了 AetherUI 框架中按钮控件的各种功能特性，包括不同尺寸、颜色主题、状态和交互效果。" +
                       "所有按钮都支持点击事件、命令绑定和字体渲染优化。",
                FontSize = 14,
                FontFamily = "Microsoft YaHei",
                Foreground = "#7F8C8D",
                Margin = new Thickness(0, 0, 0, 40),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(pageDescription);

            // 创建演示区域网格
            Grid demoGrid = new();
            demoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            demoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // 左列：基础按钮演示
            StackPanel leftColumn = new()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 0, 20, 0)
            };

            // 右列：高级按钮演示
            StackPanel rightColumn = new()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(20, 0, 0, 0)
            };

            // 添加各种演示区域
            leftColumn.Children.Add(CreateBasicButtonsSection());
            leftColumn.Children.Add(CreateSizeVariationsSection());
            leftColumn.Children.Add(CreateColorThemesSection());
            leftColumn.Children.Add(CreateCornerRadiusSection());

            rightColumn.Children.Add(CreateInteractiveButtonsSection());
            rightColumn.Children.Add(CreateSpecialButtonsSection());
            rightColumn.Children.Add(CreatePerformanceTestSection());

            Grid.SetColumn(leftColumn, 0);
            Grid.SetColumn(rightColumn, 1);
            demoGrid.Children.Add(leftColumn);
            demoGrid.Children.Add(rightColumn);

            mainPanel.Children.Add(demoGrid);
            mainBorder.Child = mainPanel;

            Console.WriteLine("按钮控件演示页面创建完成");
            return mainBorder;
        }

        /// <summary>
        /// 创建基础按钮演示区域
        /// </summary>
        /// <returns>基础按钮演示UI元素</returns>
        private static UIElement CreateBasicButtonsSection()
        {
            return CreateDemoSection("基础按钮", "展示基本的按钮样式和文本渲染", () =>
            {
                StackPanel buttonStack = new()
                {
                    Orientation = Orientation.Vertical
                };

                // 标准按钮
                Button standardButton = CreateStyledButton("标准按钮", "#3498DB", "#FFFFFF");
                standardButton.Click += (s, e) => Console.WriteLine("标准按钮被点击");
                buttonStack.Children.Add(WrapInContainer(standardButton, "#3498DB"));

                // 中文按钮
                Button chineseButton = CreateStyledButton("中文按钮测试", "#E74C3C", "#FFFFFF");
                chineseButton.Click += (s, e) => Console.WriteLine("中文按钮被点击");
                buttonStack.Children.Add(WrapInContainer(chineseButton, "#E74C3C"));

                // 英文按钮
                Button englishButton = CreateStyledButton("English Button", "#27AE60", "#FFFFFF");
                englishButton.Click += (s, e) => Console.WriteLine("English button clicked");
                buttonStack.Children.Add(WrapInContainer(englishButton, "#27AE60"));

                // 混合语言按钮
                Button mixedButton = CreateStyledButton("Mixed 混合 Button", "#9B59B6", "#FFFFFF");
                mixedButton.Click += (s, e) => Console.WriteLine("混合语言按钮被点击");
                buttonStack.Children.Add(WrapInContainer(mixedButton, "#9B59B6"));

                return buttonStack;
            });
        }

        /// <summary>
        /// 创建尺寸变化演示区域
        /// </summary>
        /// <returns>尺寸变化演示UI元素</returns>
        private static UIElement CreateSizeVariationsSection()
        {
            return CreateDemoSection("尺寸变化", "展示不同尺寸的按钮", () =>
            {
                StackPanel sizeStack = new()
                {
                    Orientation = Orientation.Vertical
                };

                // 小按钮
                Button smallButton = CreateStyledButton("小按钮", "#F39C12", "#FFFFFF");
                smallButton.Padding = new Thickness(8, 4, 8, 4);
                smallButton.Click += (s, e) => Console.WriteLine("小按钮被点击");
                sizeStack.Children.Add(WrapInContainer(smallButton, "#F39C12"));

                // 中等按钮
                Button mediumButton = CreateStyledButton("中等按钮", "#E67E22", "#FFFFFF");
                mediumButton.Padding = new Thickness(12, 8, 12, 8);
                mediumButton.Click += (s, e) => Console.WriteLine("中等按钮被点击");
                sizeStack.Children.Add(WrapInContainer(mediumButton, "#E67E22"));

                // 大按钮
                Button largeButton = CreateStyledButton("大按钮", "#D35400", "#FFFFFF");
                largeButton.Padding = new Thickness(20, 12, 20, 12);
                largeButton.Click += (s, e) => Console.WriteLine("大按钮被点击");
                sizeStack.Children.Add(WrapInContainer(largeButton, "#D35400"));

                return sizeStack;
            });
        }

        /// <summary>
        /// 创建颜色主题演示区域
        /// </summary>
        /// <returns>颜色主题演示UI元素</returns>
        private static UIElement CreateColorThemesSection()
        {
            return CreateDemoSection("颜色主题", "展示不同颜色主题的按钮", () =>
            {
                StackPanel colorStack = new()
                {
                    Orientation = Orientation.Vertical
                };

                var themes = new[]
                {
                    new { Name = "主要", Color = "#3498DB", Text = "#FFFFFF" },
                    new { Name = "成功", Color = "#2ECC71", Text = "#FFFFFF" },
                    new { Name = "警告", Color = "#F1C40F", Text = "#2C3E50" },
                    new { Name = "危险", Color = "#E74C3C", Text = "#FFFFFF" },
                    new { Name = "信息", Color = "#17A2B8", Text = "#FFFFFF" }
                };

                foreach (var theme in themes)
                {
                    Button themeButton = CreateStyledButton($"{theme.Name}按钮", theme.Color, theme.Text);
                    themeButton.Click += (s, e) => Console.WriteLine($"{theme.Name}按钮被点击");
                    colorStack.Children.Add(WrapInContainer(themeButton, theme.Color));
                }

                return colorStack;
            });
        }

        /// <summary>
        /// 创建样式化按钮
        /// </summary>
        /// <param name="text">按钮文本</param>
        /// <param name="backgroundColor">背景颜色</param>
        /// <param name="textColor">文本颜色</param>
        /// <returns>样式化的按钮</returns>
        private static Button CreateStyledButton(string text, string backgroundColor, string textColor)
        {
            return new Button
            {
                Content = text,
                Background = backgroundColor,      // 使用新的背景颜色属性
                Foreground = textColor,           // 使用新的前景颜色属性
                BorderBrush = GetDarkerColor(backgroundColor), // 自动生成较深的边框颜色
                CornerRadius = 7.0,               // 使用默认圆角
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
        }

        /// <summary>
        /// 将按钮包装在带颜色的容器中
        /// </summary>
        /// <param name="button">按钮</param>
        /// <param name="borderColor">边框颜色</param>
        /// <returns>包装后的容器</returns>
        private static UIElement WrapInContainer(Button button, string borderColor)
        {
            Border container = new()
            {
                Background = "Transparent",  // 使用透明背景，让按钮自己的样式显示
                CornerRadius = 8,            // 稍微大一点的圆角来包围按钮
                Margin = new Thickness(0, 0, 0, 8)
            };

            container.Child = button;
            return container;
        }

        /// <summary>
        /// 获取较深的颜色用作边框
        /// </summary>
        /// <param name="hexColor">十六进制颜色值</param>
        /// <returns>较深的颜色</returns>
        private static string GetDarkerColor(string hexColor)
        {
            try
            {
                if (!hexColor.StartsWith("#") || hexColor.Length != 7)
                    return "#2C3E50"; // 默认深色

                // 解析RGB值
                int r = Convert.ToInt32(hexColor.Substring(1, 2), 16);
                int g = Convert.ToInt32(hexColor.Substring(3, 2), 16);
                int b = Convert.ToInt32(hexColor.Substring(5, 2), 16);

                // 使颜色变深（乘以0.8）
                r = Math.Max(0, (int)(r * 0.8));
                g = Math.Max(0, (int)(g * 0.8));
                b = Math.Max(0, (int)(b * 0.8));

                return $"#{r:X2}{g:X2}{b:X2}";
            }
            catch
            {
                return "#2C3E50"; // 出错时返回默认深色
            }
        }

        /// <summary>
        /// 创建圆角弧度演示区域
        /// </summary>
        /// <returns>圆角弧度演示UI元素</returns>
        private static UIElement CreateCornerRadiusSection()
        {
            return CreateDemoSection("圆角弧度", "展示不同圆角弧度的按钮效果", () =>
            {
                StackPanel radiusStack = new()
                {
                    Orientation = Orientation.Vertical
                };

                var radiusOptions = new[]
                {
                    new { Name = "无圆角", Radius = 0.0, Color = "#E74C3C" },
                    new { Name = "小圆角", Radius = 3.0, Color = "#F39C12" },
                    new { Name = "默认圆角", Radius = 7.0, Color = "#3498DB" },
                    new { Name = "大圆角", Radius = 15.0, Color = "#2ECC71" },
                    new { Name = "超大圆角", Radius = 25.0, Color = "#9B59B6" }
                };

                foreach (var option in radiusOptions)
                {
                    Button radiusButton = new Button
                    {
                        Content = $"{option.Name} ({option.Radius}px)",
                        Background = option.Color,
                        Foreground = "#FFFFFF",
                        BorderBrush = GetDarkerColor(option.Color),
                        CornerRadius = option.Radius,
                        Padding = new Thickness(15, 8, 15, 8),
                        Margin = new Thickness(0, 0, 0, 8),
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };

                    radiusButton.Click += (s, e) =>
                        Console.WriteLine($"圆角按钮被点击: {option.Name} (半径: {option.Radius}px)");

                    radiusStack.Children.Add(WrapInContainer(radiusButton, option.Color));
                }

                return radiusStack;
            });
        }

        /// <summary>
        /// 创建交互按钮演示区域
        /// </summary>
        /// <returns>交互按钮演示UI元素</returns>
        private static UIElement CreateInteractiveButtonsSection()
        {
            return CreateDemoSection("交互按钮", "展示按钮的交互功能和事件处理", () =>
            {
                StackPanel interactiveStack = new()
                {
                    Orientation = Orientation.Vertical
                };

                // 计数器按钮
                int clickCount = 0;
                Button counterButton = CreateStyledButton("点击计数: 0", "#8E44AD", "#FFFFFF");
                counterButton.Click += (s, e) =>
                {
                    clickCount++;
                    if (s is Button btn)
                    {
                        btn.Content = $"点击计数: {clickCount}";
                    }
                    Console.WriteLine($"计数器按钮被点击，当前计数: {clickCount}");
                };
                interactiveStack.Children.Add(WrapInContainer(counterButton, "#8E44AD"));

                // 状态切换按钮
                bool isToggled = false;
                Button toggleButton = CreateStyledButton("状态: 关闭", "#34495E", "#FFFFFF");
                toggleButton.Click += (s, e) =>
                {
                    isToggled = !isToggled;
                    if (s is Button btn)
                    {
                        btn.Content = $"状态: {(isToggled ? "开启" : "关闭")}";
                    }
                    Console.WriteLine($"切换按钮状态: {(isToggled ? "开启" : "关闭")}");
                };
                interactiveStack.Children.Add(WrapInContainer(toggleButton, "#34495E"));

                // 命令按钮（使用RelayCommand）
                RelayCommand testCommand = new(() =>
                {
                    Console.WriteLine("命令按钮执行了RelayCommand");
});

                Button commandButton = CreateStyledButton("命令按钮", "#16A085", "#FFFFFF");
                commandButton.Command = testCommand;
                interactiveStack.Children.Add(WrapInContainer(commandButton, "#16A085"));

                return interactiveStack;
            });
        }

        /// <summary>
        /// 创建特殊按钮演示区域
        /// </summary>
        /// <returns>特殊按钮演示UI元素</returns>
        private static UIElement CreateSpecialButtonsSection()
        {
            return CreateDemoSection("特殊按钮", "展示特殊功能和多语言支持", () =>
            {
                StackPanel specialStack = new()
                {
                    Orientation = Orientation.Vertical
                };

                // Unicode多语言按钮
                var languages = new[]
                {
                    new { Text = "日本語ボタン", Lang = "Japanese" },
                    new { Text = "한국어 버튼", Lang = "Korean" },
                    new { Text = "Русская кнопка", Lang = "Russian" },
                    new { Text = "العربية زر", Lang = "Arabic" },
                    new { Text = "Français bouton", Lang = "French" }
                };

                string[] colors = { "#E74C3C", "#3498DB", "#2ECC71", "#F39C12", "#9B59B6" };

                for (int i = 0; i < languages.Length; i++)
                {
                    var lang = languages[i];
                    Button langButton = CreateStyledButton(lang.Text, colors[i], "#FFFFFF");
                    langButton.Click += (s, e) => Console.WriteLine($"{lang.Lang} button clicked: {lang.Text}");
                    specialStack.Children.Add(WrapInContainer(langButton, colors[i]));
                }

                return specialStack;
            });
        }

        /// <summary>
        /// 创建性能测试演示区域
        /// </summary>
        /// <returns>性能测试演示UI元素</returns>
        private static UIElement CreatePerformanceTestSection()
        {
            return CreateDemoSection("性能测试", "测试字体渲染和窗口调整性能", () =>
            {
                StackPanel performanceStack = new()
                {
                    Orientation = Orientation.Vertical
                };

                // 字体渲染测试按钮
                Button fontTestButton = CreateStyledButton("字体渲染测试", "#C0392B", "#FFFFFF");
                fontTestButton.Click += (s, e) =>
                {
                    Console.WriteLine("=== 字体渲染性能测试 ===");
                    Console.WriteLine("测试抗锯齿效果: AntiAliasGridFit");
                    Console.WriteLine("测试表情符号支持: 🎉🚀💡⭐🔥");
                    Console.WriteLine("测试混合字符: ABC中文123日本語한국어");
                    Console.WriteLine("字体渲染测试完成");
                };
                performanceStack.Children.Add(WrapInContainer(fontTestButton, "#C0392B"));

                // 窗口调整测试按钮
                Button resizeTestButton = CreateStyledButton("窗口调整测试", "#8E44AD", "#FFFFFF");
                resizeTestButton.Click += (s, e) =>
                {
                    Console.WriteLine("=== 窗口调整性能测试 ===");
                    Console.WriteLine("请尝试调整窗口大小来测试:");
                    Console.WriteLine("- UI元素重新布局");
                    Console.WriteLine("- 字体缓存清理");
                    Console.WriteLine("- 渲染性能优化");
                    Console.WriteLine("观察控制台输出以查看详细信息");
                };
                performanceStack.Children.Add(WrapInContainer(resizeTestButton, "#8E44AD"));

                // 批量按钮测试
                Button batchTestButton = CreateStyledButton("批量渲染测试", "#D35400", "#FFFFFF");
                batchTestButton.Click += (s, e) =>
                {
                    Console.WriteLine("=== 批量按钮渲染测试 ===");
                    for (int i = 1; i <= 10; i++)
                    {
                        Console.WriteLine($"模拟按钮 {i} 渲染完成");
                    }
                    Console.WriteLine("批量渲染测试完成 - 所有按钮渲染正常");
                };
                performanceStack.Children.Add(WrapInContainer(batchTestButton, "#D35400"));

                return performanceStack;
            });
        }

        /// <summary>
        /// 创建演示区域
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="description">描述</param>
        /// <param name="contentFactory">内容创建工厂</param>
        /// <returns>演示区域UI元素</returns>
        private static UIElement CreateDemoSection(string title, string description, Func<UIElement> contentFactory)
        {
            Card sectionCard = new()
            {
                Background = "#F8F9FA",
                CornerRadius = 8,
                Elevation = 2,
                Margin = new Thickness(0, 0, 0, 25)
            };

            StackPanel sectionContent = new()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(20)
            };

            // 区域标题
            TextBlock sectionTitle = new()
            {
                Text = title,
                FontSize = 18,
                FontFamily = "Microsoft YaHei",
                FontWeight = FontWeight.Bold,
                Foreground = "#2C3E50",
                Margin = new Thickness(0, 0, 0, 8)
            };
            sectionContent.Children.Add(sectionTitle);

            // 区域描述
            TextBlock sectionDescription = new()
            {
                Text = description,
                FontSize = 12,
                FontFamily = "Microsoft YaHei",
                Foreground = "#7F8C8D",
                Margin = new Thickness(0, 0, 0, 15)
            };
            sectionContent.Children.Add(sectionDescription);

            // 添加内容
            sectionContent.Children.Add(contentFactory());

            sectionCard.Content = sectionContent;
            return sectionCard;
        }
    }
}
