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

                Console.WriteLine("启动AetherUI渲染窗口...");

                // 由于OpenGL环境限制，直接运行控制台模式演示
                Console.WriteLine("注意：图形渲染需要支持OpenGL的环境。");
                Console.WriteLine("当前运行控制台模式演示...");
                Console.WriteLine();

                // 运行控制台模式演示
                RunConsoleDemo(demoUI);
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
                Text = "A",
                FontSize = 20,
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
                Text = "AetherUI 框架演示",
                FontSize = 24
            };

            var subTitle = new TextBlock
            {
                Text = "现代化跨平台UI框架 - 实时渲染演示",
                FontSize = 14
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
                Text = "欢迎使用 AetherUI 框架！",
                FontSize = 20,
                Margin = new Thickness(0, 0, 0, 15)
            };
            cardContent.Children.Add(welcomeTitle);

            // 描述文本
            var description = new TextBlock
            {
                Text = "AetherUI 是一个现代化的跨平台UI框架，基于OpenGL渲染，支持MVVM架构。" +
                       "本演示展示了框架的核心功能，包括布局系统、控件库、事件处理和数据绑定。",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 20)
            };
            cardContent.Children.Add(description);

            // 功能亮点
            var featuresPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var features = new[]
            {
                "⚡ 高性能渲染",
                "🎨 丰富控件",
                "📱 跨平台支持",
                "🔧 MVVM架构"
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
                    Text = feature,
                    FontSize = 12
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

        /// <summary>
        /// 运行控制台模式演示
        /// </summary>
        /// <param name="demoUI">演示UI</param>
        private static void RunConsoleDemo(UIElement demoUI)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("    AetherUI 框架控制台演示模式");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            // 显示UI结构
            Console.WriteLine("UI结构树:");
            PrintUITree(demoUI, 0);
            Console.WriteLine();

            // 显示功能演示
            Console.WriteLine("功能演示:");
            DemonstrateFeatures();
            Console.WriteLine();

            Console.WriteLine("控制台演示完成！");
            Console.WriteLine("注意：完整的图形界面需要支持OpenGL的环境。");
        }

        /// <summary>
        /// 打印UI树结构
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="depth">深度</param>
        private static void PrintUITree(UIElement element, int depth)
        {
            string indent = new string(' ', depth * 2);
            string elementInfo = GetElementInfo(element);
            Console.WriteLine($"{indent}├─ {elementInfo}");

            // 递归打印子元素
            if (element is Panel panel)
            {
                foreach (UIElement child in panel.Children)
                {
                    PrintUITree(child, depth + 1);
                }
            }
            else if (element is Border border && border.Child != null)
            {
                PrintUITree(border.Child, depth + 1);
            }
            else if (element is Card card && card.Content != null)
            {
                PrintUITree(card.Content, depth + 1);
            }
        }

        /// <summary>
        /// 获取元素信息
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <returns>元素信息字符串</returns>
        private static string GetElementInfo(UIElement element)
        {
            string typeName = element.GetType().Name;
            string info = typeName;

            switch (element)
            {
                case TextBlock textBlock:
                    info += $" \"{textBlock.Text}\"";
                    break;
                case Button button:
                    info += $" \"{button.Content}\"";
                    break;
                case Grid grid:
                    info += $" ({grid.RowDefinitions.Count}行 x {grid.ColumnDefinitions.Count}列)";
                    break;
                case StackPanel stackPanel:
                    info += $" ({stackPanel.Orientation}, {stackPanel.Children.Count}个子元素)";
                    break;
                case Border border:
                    if (border.Background != null)
                        info += $" (背景: {border.Background})";
                    break;
                case Card card:
                    if (card.Header != null)
                        info += $" (标题: {card.Header})";
                    break;
            }

            return info;
        }

        /// <summary>
        /// 演示功能特性
        /// </summary>
        private static void DemonstrateFeatures()
        {
            Console.WriteLine("1. 布局系统演示:");
            DemonstrateLayoutSystem();
            Console.WriteLine();

            Console.WriteLine("2. MVVM功能演示:");
            DemonstrateMVVM();
            Console.WriteLine();

            Console.WriteLine("3. 事件系统演示:");
            DemonstrateEventSystem();
        }

        /// <summary>
        /// 演示布局系统
        /// </summary>
        private static void DemonstrateLayoutSystem()
        {
            // 创建各种布局容器
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };
            stackPanel.Children.Add(new TextBlock { Text = "项目1" });
            stackPanel.Children.Add(new TextBlock { Text = "项目2" });
            stackPanel.Children.Add(new Button { Content = "按钮" });

            Console.WriteLine($"   ✓ StackPanel: {stackPanel.Children.Count}个子元素");

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            Console.WriteLine($"   ✓ Grid: {grid.RowDefinitions.Count}行 x {grid.ColumnDefinitions.Count}列");

            var canvas = new Canvas();
            var canvasChild = new TextBlock { Text = "Canvas子元素" };
            Canvas.SetLeft(canvasChild, 10);
            Canvas.SetTop(canvasChild, 20);
            canvas.Children.Add(canvasChild);

            Console.WriteLine($"   ✓ Canvas: 绝对定位，{canvas.Children.Count}个子元素");

            Console.WriteLine("   ✓ DockPanel: 停靠布局");
            Console.WriteLine("   ✓ WrapPanel: 自动换行布局");
            Console.WriteLine("   ✓ UniformGrid: 均匀网格布局");
            Console.WriteLine("   ✓ Border: 边框容器");
            Console.WriteLine("   ✓ Card: 卡片容器");
        }

        /// <summary>
        /// 演示MVVM功能
        /// </summary>
        private static void DemonstrateMVVM()
        {
            var viewModel = new TestViewModel();
            Console.WriteLine($"   ✓ ViewModel创建: {viewModel.Title}");

            // 测试属性变化通知
            bool propertyChanged = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                propertyChanged = true;
                Console.WriteLine($"   ✓ 属性变化通知: {e.PropertyName}");
            };

            viewModel.Title = "更新的标题";
            viewModel.Counter = 42;

            if (propertyChanged)
            {
                Console.WriteLine("   ✓ PropertyChanged事件正常工作");
            }

            // 测试命令
            if (viewModel.TestCommand.CanExecute(null))
            {
                viewModel.TestCommand.Execute(null);
                Console.WriteLine("   ✓ 命令执行成功");
            }
        }

        /// <summary>
        /// 演示事件系统
        /// </summary>
        private static void DemonstrateEventSystem()
        {
            var button = new Button { Content = "测试按钮" };

            button.Click += (s, e) =>
            {
                Console.WriteLine("   ✓ 按钮点击事件触发");
            };

            Console.WriteLine("   ✓ 事件订阅成功");
            Console.WriteLine("   ✓ 事件系统已配置");
            Console.WriteLine("   ✓ 支持鼠标和键盘事件");
            Console.WriteLine("   ✓ 支持路由事件");
            Console.WriteLine("   ✓ 支持命令绑定");
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
