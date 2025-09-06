using System;
using System.Diagnostics;
using AetherUI.Core;
using AetherUI.Layout;
using AetherUI.Rendering;

namespace AetherUI.Demo
{
    /// <summary>
    /// AetherUI框架可视化演示程序
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 程序入口点
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("    AetherUI 框架可视化演示程序");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("正在启动图形界面演示...");
            Console.WriteLine("窗口控制：");
            Console.WriteLine("- ESC键：退出程序");
            Console.WriteLine("- 鼠标：点击和悬停交互");
            Console.WriteLine();

            try
            {
                // 创建演示UI
                UIElement demoUI = CreateDemoUI();

                Console.WriteLine("启动AetherUI现代化窗口演示...");
                Console.WriteLine("功能特性：");
                Console.WriteLine("- 真实字体渲染系统");
                Console.WriteLine("- 现代化背景效果（亚克力/云母）");
                Console.WriteLine("- 响应式窗口大小变化");
                Console.WriteLine("- ESC键退出程序");
                Console.WriteLine();

                // 运行图形渲染演示
                RunModernWindowDemo(demoUI);

                Console.WriteLine("演示窗口已关闭。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"演示程序运行失败: {ex.Message}");
                Console.WriteLine($"详细错误: {ex}");
                Debug.WriteLine($"Demo error: {ex}");

                Console.WriteLine("\n按任意键退出...");
                try
                {
                    Console.ReadKey();
                }
                catch { }
            }
        }

        /// <summary>
        /// 运行现代化窗口演示
        /// </summary>
        /// <param name="demoUI">演示UI</param>
        private static void RunModernWindowDemo(UIElement demoUI)
        {
            // 创建窗口
            var window = new AetherUI.Rendering.Window(1200, 800, "AetherUI 现代化窗口演示");

            // 设置根UI元素
            window.RootElement = demoUI;

            // 配置现代化背景效果
            var backgroundConfig = new AetherUI.Rendering.BackgroundEffectConfig
            {
                Type = AetherUI.Rendering.BackgroundEffectType.Acrylic,
                Opacity = 0.85f,
                TintColor = new OpenTK.Mathematics.Vector4(0.95f, 0.95f, 0.98f, 1.0f)
            };

            // 添加窗口大小变化监听器
            window.AddResizeListener(new WindowResizeLogger());

            // 添加窗口大小变化事件处理
            window.WindowResized += (sender, args) =>
            {
                Console.WriteLine($"窗口大小变化: {args.OldSize} -> {args.NewSize}");
                Console.WriteLine($"变化量: {args.Delta}");
            };

            // 在窗口加载后设置背景效果
            window.Load += () =>
            {
                // 设置Windows原生背景效果
                window.SetBackgroundEffect(backgroundConfig);
                Console.WriteLine("Windows原生背景效果已启用：亚克力效果");
                Console.WriteLine("窗口大小变化响应系统已启用");
                Console.WriteLine("尝试调整窗口大小来测试响应式布局！");
                Console.WriteLine();

                // 显示系统兼容性信息
                ShowSystemCompatibilityInfo();
            };

            // 运行窗口
            window.Run();
        }

        /// <summary>
        /// 显示系统兼容性信息
        /// </summary>
        private static void ShowSystemCompatibilityInfo()
        {
            Console.WriteLine("=== 系统兼容性信息 ===");

            bool compositionEnabled = AetherUI.Rendering.WindowsCompositionApi.IsCompositionEnabled();
            bool isWindows10 = AetherUI.Rendering.WindowsCompositionApi.IsWindows10OrLater();
            bool isWindows11 = AetherUI.Rendering.WindowsCompositionApi.IsWindows11OrLater();

            Console.WriteLine($"DWM组合效果: {(compositionEnabled ? "✓ 支持" : "✗ 不支持")}");
            Console.WriteLine($"Windows 10+: {(isWindows10 ? "✓ 支持" : "✗ 不支持")}");
            Console.WriteLine($"Windows 11+: {(isWindows11 ? "✓ 支持" : "✗ 不支持")}");

            Console.WriteLine();
            Console.WriteLine("支持的背景效果:");
            Console.WriteLine($"- 亚克力效果: {(isWindows10 ? "✓ 支持" : "✗ 需要Windows 10+")}");
            Console.WriteLine($"- 云母效果: {(isWindows11 ? "✓ 支持" : "✗ 需要Windows 11+")}");
            Console.WriteLine($"- 渐变效果: ✓ 支持（OpenGL实现）");
            Console.WriteLine();
        }

        /// <summary>
        /// 创建演示UI
        /// </summary>
        /// <returns>演示UI的根元素</returns>
        private static UIElement CreateDemoUI()
        {
            Console.WriteLine("创建演示UI界面...");

            // 创建主容器 - 使用Grid布局
            var mainGrid = new Grid();

            // 定义行和列
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80, GridUnitType.Pixel) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40, GridUnitType.Pixel) });

            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250, GridUnitType.Pixel) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // 标题栏
            var titleBar = CreateTitleBar();
            Grid.SetRow(titleBar, 0);
            Grid.SetColumnSpan(titleBar, 2);
            mainGrid.Children.Add(titleBar);

            // 左侧导航面板
            var navigationPanel = CreateNavigationPanel();
            Grid.SetRow(navigationPanel, 1);
            Grid.SetColumn(navigationPanel, 0);
            mainGrid.Children.Add(navigationPanel);

            // 主内容区域
            var contentArea = CreateContentArea();
            Grid.SetRow(contentArea, 1);
            Grid.SetColumn(contentArea, 1);
            mainGrid.Children.Add(contentArea);

            // 状态栏
            var statusBar = CreateStatusBar();
            Grid.SetRow(statusBar, 2);
            Grid.SetColumnSpan(statusBar, 2);
            mainGrid.Children.Add(statusBar);

            Console.WriteLine("演示UI创建完成");
            return mainGrid;
        }

        /// <summary>
        /// 创建标题栏
        /// </summary>
        /// <returns>标题栏UI元素</returns>
        private static UIElement CreateTitleBar()
        {
            var titleBorder = new Border
            {
                Background = "DarkSlateBlue"
            };

            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 15, 20, 15)
            };

            // 应用图标（使用TextBlock模拟）
            var iconBorder = new Border
            {
                Width = 40,
                Height = 40,
                Background = "Gold",
                CornerRadius = 20,
                Margin = new Thickness(0, 0, 15, 0)
            };

            var iconText = new TextBlock
            {
                Text = "字",
                FontSize = 20,
                FontFamily = "Microsoft YaHei",
                FontWeight = FontWeight.Bold,
                Foreground = "#2C3E50",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            iconBorder.Child = iconText;
            titlePanel.Children.Add(iconBorder);

            // 标题文本区域
            var titleTextPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            var mainTitle = new TextBlock
            {
                Text = "AetherUI 字体渲染演示",
                FontSize = 24,
                FontFamily = "Microsoft YaHei",
                FontWeight = FontWeight.Bold,
                Foreground = "White"
            };

            var subTitle = new TextBlock
            {
                Text = "现代化跨平台UI框架 - 完整字体系统支持",
                FontSize = 14,
                FontFamily = "Microsoft YaHei",
                FontStyle = FontStyle.Italic,
                Foreground = "LightGray"
            };

            titleTextPanel.Children.Add(mainTitle);
            titleTextPanel.Children.Add(subTitle);
            titlePanel.Children.Add(titleTextPanel);

            titleBorder.Child = titlePanel;
            return titleBorder;
        }

        /// <summary>
        /// 创建导航面板
        /// </summary>
        /// <returns>导航面板UI元素</returns>
        private static UIElement CreateNavigationPanel()
        {
            var navBorder = new Border
            {
                Background = "LightGray",
                BorderBrush = "Gray",
                BorderThickness = new Thickness(0, 0, 1, 0)
            };

            var navPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(15)
            };

            // 导航标题
            var navTitle = new TextBlock
            {
                Text = "功能演示",
                FontSize = 18,
                Margin = new Thickness(0, 0, 0, 20)
            };
            navPanel.Children.Add(navTitle);

            // 导航项目
            var navItems = new[]
            {
                "🏠 主页概览",
                "📦 布局容器",
                "🎨 基础控件",
                "⚡ 事件系统",
                "🔗 数据绑定",
                "🎯 渲染效果",
                "⚙️ 系统信息"
            };

            foreach (var item in navItems)
            {
                // 使用Border包装Button来实现样式
                var buttonBorder = new Border
                {
                    Background = "White",
                    BorderBrush = "DodgerBlue",
                    BorderThickness = new Thickness(1),
                    CornerRadius = 3,
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var navButton = new Button
                {
                    Content = item,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Padding = new Thickness(12, 8, 12, 8)
                };

                buttonBorder.Child = navButton;
                navPanel.Children.Add(buttonBorder);
            }

            navBorder.Child = navPanel;
            return navBorder;
        }

        /// <summary>
        /// 创建主内容区域
        /// </summary>
        /// <returns>内容区域UI元素</returns>
        private static UIElement CreateContentArea()
        {
            var contentBorder = new Border
            {
                Background = "White",
                Margin = new Thickness(10)
            };

            // 使用TabControl样式的内容切换（简化版）
            var contentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(20)
            };

            // 内容标题
            var contentTitle = new TextBlock
            {
                Text = "🏠 主页概览",
                FontSize = 28,
                Margin = new Thickness(0, 0, 0, 30)
            };
            contentPanel.Children.Add(contentTitle);

            // 欢迎信息
            var welcomeCard = CreateWelcomeCard();
            contentPanel.Children.Add(welcomeCard);

            // 功能展示区域
            var featuresArea = CreateFeaturesArea();
            contentPanel.Children.Add(featuresArea);

            contentBorder.Child = contentPanel;
            return contentBorder;
        }

        /// <summary>
        /// 创建欢迎卡片
        /// </summary>
        /// <returns>欢迎卡片UI元素</returns>
        private static UIElement CreateWelcomeCard()
        {
            var card = new Card
            {
                Background = "AliceBlue",
                CornerRadius = 8,
                Elevation = 2,
                Margin = new Thickness(0, 0, 0, 30)
            };

            var cardContent = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(25)
            };

            // 欢迎标题
            var welcomeTitle = new TextBlock
            {
                Text = "欢迎使用 AetherUI 字体渲染系统！",
                FontSize = 20,
                FontFamily = "Microsoft YaHei",
                FontWeight = FontWeight.Bold,
                Foreground = "#2C3E50",
                Margin = new Thickness(0, 0, 0, 15)
            };
            cardContent.Children.Add(welcomeTitle);

            // 描述文本
            var description = new TextBlock
            {
                Text = "AetherUI 是一个现代化的跨平台UI框架，基于OpenGL渲染，支持完整的字体系统。" +
                       "本演示展示了真实的字体渲染功能，包括中文字体、字体样式、颜色和大小控制。",
                FontSize = 14,
                FontFamily = "Microsoft YaHei",
                Foreground = "#34495E",
                Margin = new Thickness(0, 0, 0, 20)
            };
            cardContent.Children.Add(description);

            // 字体演示区域
            var fontDemoTitle = new TextBlock
            {
                Text = "字体渲染演示：",
                FontSize = 16,
                FontFamily = "Microsoft YaHei",
                FontWeight = FontWeight.SemiBold,
                Foreground = "#2980B9",
                Margin = new Thickness(0, 0, 0, 10)
            };
            cardContent.Children.Add(fontDemoTitle);

            // 不同字体样式演示
            var fontSamples = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(20, 0, 0, 20)
            };

            // 添加各种字体样式示例
            var samples = new[]
            {
                new { Text = "默认字体：Microsoft YaHei 14px", FontSize = 14.0, FontWeight = FontWeight.Normal, FontStyle = FontStyle.Normal, Color = "#000000" },
                new { Text = "粗体文本：Bold Weight", FontSize = 14.0, FontWeight = FontWeight.Bold, FontStyle = FontStyle.Normal, Color = "#2C3E50" },
                new { Text = "斜体文本：Italic Style", FontSize = 14.0, FontWeight = FontWeight.Normal, FontStyle = FontStyle.Italic, Color = "#8E44AD" },
                new { Text = "大号文本：Large Size 18px", FontSize = 18.0, FontWeight = FontWeight.Normal, FontStyle = FontStyle.Normal, Color = "#E74C3C" },
                new { Text = "小号文本：Small Size 12px", FontSize = 12.0, FontWeight = FontWeight.Normal, FontStyle = FontStyle.Normal, Color = "#27AE60" },
                new { Text = "中文测试：这是中文字体渲染测试", FontSize = 16.0, FontWeight = FontWeight.Medium, FontStyle = FontStyle.Normal, Color = "#F39C12" },
                new { Text = "混合文本：Mixed Chinese 中文 and English", FontSize = 15.0, FontWeight = FontWeight.SemiBold, FontStyle = FontStyle.Normal, Color = "#9B59B6" }
            };

            foreach (var sample in samples)
            {
                var sampleText = new TextBlock
                {
                    Text = sample.Text,
                    FontSize = sample.FontSize,
                    FontFamily = "Microsoft YaHei",
                    FontWeight = sample.FontWeight,
                    FontStyle = sample.FontStyle,
                    Foreground = sample.Color,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                fontSamples.Children.Add(sampleText);
            }

            cardContent.Children.Add(fontSamples);

            // 功能亮点
            var featuresPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var features = new[]
            {
                new { Text = "字体渲染", Color = "#E74C3C" },
                new { Text = "中文支持", Color = "#2ECC71" },
                new { Text = "OpenGL加速", Color = "#3498DB" },
                new { Text = "MVVM架构", Color = "#9B59B6" }
            };

            foreach (var feature in features)
            {
                var featureBorder = new Border
                {
                    Background = "LightBlue",
                    CornerRadius = 15,
                    Padding = new Thickness(12, 6, 12, 6),
                    Margin = new Thickness(0, 0, 10, 0)
                };

                var featureText = new TextBlock
                {
                    Text = feature.Text,
                    FontSize = 12,
                    FontFamily = "Microsoft YaHei",
                    FontWeight = FontWeight.Medium,
                    Foreground = feature.Color
                };

                featureBorder.Child = featureText;
                featuresPanel.Children.Add(featureBorder);
            }

            cardContent.Children.Add(featuresPanel);
            card.Content = cardContent;
            return card;
        }

        /// <summary>
        /// 创建功能展示区域
        /// </summary>
        /// <returns>功能展示区域UI元素</returns>
        private static UIElement CreateFeaturesArea()
        {
            var featuresGrid = new Grid();

            // 定义2x2网格
            featuresGrid.RowDefinitions.Add(new RowDefinition());
            featuresGrid.RowDefinitions.Add(new RowDefinition());
            featuresGrid.ColumnDefinitions.Add(new ColumnDefinition());
            featuresGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // 布局容器演示
            var layoutDemo = CreateLayoutDemo();
            Grid.SetRow(layoutDemo, 0);
            Grid.SetColumn(layoutDemo, 0);
            featuresGrid.Children.Add(layoutDemo);

            // 控件演示
            var controlsDemo = CreateControlsDemo();
            Grid.SetRow(controlsDemo, 0);
            Grid.SetColumn(controlsDemo, 1);
            featuresGrid.Children.Add(controlsDemo);

            // 事件演示
            var eventsDemo = CreateEventsDemo();
            Grid.SetRow(eventsDemo, 1);
            Grid.SetColumn(eventsDemo, 0);
            featuresGrid.Children.Add(eventsDemo);

            // 渲染演示
            var renderingDemo = CreateRenderingDemo();
            Grid.SetRow(renderingDemo, 1);
            Grid.SetColumn(renderingDemo, 1);
            featuresGrid.Children.Add(renderingDemo);

            return featuresGrid;
        }

        /// <summary>
        /// 创建状态栏
        /// </summary>
        /// <returns>状态栏UI元素</returns>
        private static UIElement CreateStatusBar()
        {
            var statusBorder = new Border
            {
                Background = "LightGray",
                BorderBrush = "Gray",
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(15, 8, 15, 8)
            };

            var statusPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // 状态信息
            var statusText = new TextBlock
            {
                Text = "就绪 | AetherUI v1.0 | OpenGL 渲染 | 实时演示模式",
                FontSize = 12
            };

            statusPanel.Children.Add(statusText);
            statusBorder.Child = statusPanel;
            return statusBorder;
        }

        /// <summary>
        /// 创建布局演示
        /// </summary>
        /// <returns>布局演示UI元素</returns>
        private static UIElement CreateLayoutDemo()
        {
            var demoCard = new Card
            {
                Background = "LightYellow",
                CornerRadius = 6,
                Margin = new Thickness(5),
                Elevation = 1
            };

            var cardContent = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(15)
            };

            // 标题
            var title = new TextBlock
            {
                Text = "📦 布局容器",
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 15)
            };
            cardContent.Children.Add(title);

            // StackPanel演示
            var stackDemo = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            for (int i = 1; i <= 3; i++)
            {
                var stackItem = new Border
                {
                    Background = "Orange",
                    Width = 30,
                    Height = 20,
                    Margin = new Thickness(2),
                    CornerRadius = 3
                };

                var stackText = new TextBlock
                {
                    Text = i.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 10
                };

                stackItem.Child = stackText;
                stackDemo.Children.Add(stackItem);
            }

            cardContent.Children.Add(stackDemo);

            // Grid演示
            var gridDemo = new Grid
            {
                Margin = new Thickness(0, 0, 0, 10)
            };

            gridDemo.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25, GridUnitType.Pixel) });
            gridDemo.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25, GridUnitType.Pixel) });
            gridDemo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40, GridUnitType.Pixel) });
            gridDemo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40, GridUnitType.Pixel) });

            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 2; col++)
                {
                    var gridItem = new Border
                    {
                        Background = "LightGreen",
                        Margin = new Thickness(1),
                        CornerRadius = 2
                    };

                    var gridText = new TextBlock
                    {
                        Text = $"{row},{col}",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 9
                    };

                    gridItem.Child = gridText;
                    Grid.SetRow(gridItem, row);
                    Grid.SetColumn(gridItem, col);
                    gridDemo.Children.Add(gridItem);
                }
            }

            cardContent.Children.Add(gridDemo);

            // 说明文本
            var description = new TextBlock
            {
                Text = "StackPanel + Grid 布局演示",
                FontSize = 11,
                Margin = new Thickness(0, 5, 0, 0)
            };
            cardContent.Children.Add(description);

            demoCard.Content = cardContent;
            return demoCard;
        }

        /// <summary>
        /// 创建控件演示
        /// </summary>
        /// <returns>控件演示UI元素</returns>
        private static UIElement CreateControlsDemo()
        {
            var demoCard = new Card
            {
                Background = "LightCyan",
                CornerRadius = 6,
                Margin = new Thickness(5),
                Elevation = 1
            };

            var cardContent = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(15)
            };

            // 标题
            var title = new TextBlock
            {
                Text = "🎨 基础控件",
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 15)
            };
            cardContent.Children.Add(title);

            // 按钮演示
            var buttonBorder = new Border
            {
                Background = "DodgerBlue",
                CornerRadius = 4,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var demoButton = new Button
            {
                Content = "演示按钮",
                Padding = new Thickness(15, 8, 15, 8)
            };

            buttonBorder.Child = demoButton;
            cardContent.Children.Add(buttonBorder);

            // 文本演示
            var textDemo = new TextBlock
            {
                Text = "这是一个TextBlock控件",
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 8)
            };
            cardContent.Children.Add(textDemo);

            // 边框演示
            var borderDemo = new Border
            {
                Background = "LightPink",
                BorderBrush = "DeepPink",
                BorderThickness = new Thickness(2),
                CornerRadius = 5,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var borderText = new TextBlock
            {
                Text = "Border容器",
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            borderDemo.Child = borderText;
            cardContent.Children.Add(borderDemo);

            demoCard.Content = cardContent;
            return demoCard;
        }

        /// <summary>
        /// 创建事件演示
        /// </summary>
        /// <returns>事件演示UI元素</returns>
        private static UIElement CreateEventsDemo()
        {
            var demoCard = new Card
            {
                Background = "LightGreen",
                CornerRadius = 6,
                Margin = new Thickness(5),
                Elevation = 1
            };

            var cardContent = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(15)
            };

            // 标题
            var title = new TextBlock
            {
                Text = "⚡ 事件系统",
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 15)
            };
            cardContent.Children.Add(title);

            // 交互按钮
            var interactiveBorder = new Border
            {
                Background = "Orange",
                CornerRadius = 4,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var interactiveButton = new Button
            {
                Content = "点击我！",
                Padding = new Thickness(12, 6, 12, 6)
            };

            // 添加点击事件（简化版）
            interactiveButton.Click += (s, e) =>
            {
                Console.WriteLine("按钮被点击了！");
            };

            interactiveBorder.Child = interactiveButton;
            cardContent.Children.Add(interactiveBorder);

            // 状态文本
            var statusText = new TextBlock
            {
                Text = "支持鼠标和键盘事件",
                FontSize = 11,
                Margin = new Thickness(0, 5, 0, 0)
            };
            cardContent.Children.Add(statusText);

            demoCard.Content = cardContent;
            return demoCard;
        }

        /// <summary>
        /// 创建渲染演示
        /// </summary>
        /// <returns>渲染演示UI元素</returns>
        private static UIElement CreateRenderingDemo()
        {
            var demoCard = new Card
            {
                Background = "Lavender",
                CornerRadius = 6,
                Margin = new Thickness(5),
                Elevation = 1
            };

            var cardContent = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(15)
            };

            // 标题
            var title = new TextBlock
            {
                Text = "🎯 渲染效果",
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 15)
            };
            cardContent.Children.Add(title);

            // 渐变演示（使用多个颜色块模拟）
            var gradientPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var colors = new[] { "Red", "Orange", "Yellow", "Green", "Blue", "Purple" };
            foreach (var color in colors)
            {
                var colorBlock = new Border
                {
                    Background = color,
                    Width = 15,
                    Height = 20,
                    Margin = new Thickness(1)
                };
                gradientPanel.Children.Add(colorBlock);
            }

            cardContent.Children.Add(gradientPanel);

            // 说明文本
            var description = new TextBlock
            {
                Text = "OpenGL硬件加速渲染",
                FontSize = 11,
                Margin = new Thickness(0, 5, 0, 0)
            };
            cardContent.Children.Add(description);

            demoCard.Content = cardContent;
            return demoCard;
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

    /// <summary>
    /// 窗口大小变化日志记录器
    /// </summary>
    public class WindowResizeLogger : AetherUI.Rendering.IWindowResizeListener
    {
        /// <summary>
        /// 处理窗口大小变化
        /// </summary>
        /// <param name="args">事件参数</param>
        public void OnWindowResize(AetherUI.Rendering.WindowResizeEventArgs args)
        {
            Console.WriteLine($"[ResizeLogger] 窗口尺寸: {args.NewSize.Width:F0}x{args.NewSize.Height:F0}");

            // 根据窗口大小给出建议
            if (args.NewSize.Width < 800 || args.NewSize.Height < 600)
            {
                Console.WriteLine("[ResizeLogger] 提示：窗口较小，某些UI元素可能显示不完整");
            }
            else if (args.NewSize.Width > 1600 && args.NewSize.Height > 1000)
            {
                Console.WriteLine("[ResizeLogger] 提示：大屏幕模式，UI布局已优化");
            }
        }
    }
}
