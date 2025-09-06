using System;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 高级布局容器测试类
    /// </summary>
    public static class AdvancedLayoutTest
    {
        /// <summary>
        /// 测试DockPanel布局
        /// </summary>
        public static void TestDockPanel()
        {
            Console.WriteLine("=== 测试DockPanel布局 ===");

            DockPanel dockPanel = new DockPanel();

            // 顶部工具栏
            TextBlock toolbar = new TextBlock { Text = "工具栏", FontSize = 14 };
            DockPanel.SetDock(toolbar, Dock.Top);

            // 底部状态栏
            TextBlock statusBar = new TextBlock { Text = "状态栏", FontSize = 10 };
            DockPanel.SetDock(statusBar, Dock.Bottom);

            // 左侧导航
            TextBlock leftNav = new TextBlock { Text = "导航", FontSize = 12 };
            DockPanel.SetDock(leftNav, Dock.Left);

            // 右侧面板
            TextBlock rightPanel = new TextBlock { Text = "面板", FontSize = 12 };
            DockPanel.SetDock(rightPanel, Dock.Right);

            // 中心内容（最后一个，自动填充）
            TextBlock mainContent = new TextBlock { Text = "主要内容区域", FontSize = 14 };

            dockPanel.Children.Add(toolbar);
            dockPanel.Children.Add(statusBar);
            dockPanel.Children.Add(leftNav);
            dockPanel.Children.Add(rightPanel);
            dockPanel.Children.Add(mainContent);

            PerformLayout(dockPanel, new Size(500, 400));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试WrapPanel布局
        /// </summary>
        public static void TestWrapPanel()
        {
            Console.WriteLine("=== 测试WrapPanel布局 ===");

            WrapPanel wrapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal
            };

            // 添加多个按钮测试换行
            for (int i = 1; i <= 8; i++)
            {
                Button button = new Button { Content = $"按钮{i}" };
                wrapPanel.Children.Add(button);
            }

            PerformLayout(wrapPanel, new Size(300, 200));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试UniformGrid布局
        /// </summary>
        public static void TestUniformGrid()
        {
            Console.WriteLine("=== 测试UniformGrid布局 ===");

            UniformGrid uniformGrid = new UniformGrid
            {
                Rows = 3,
                Columns = 3,
                FirstColumn = 1 // 从第二列开始
            };

            // 添加7个按钮（第一行第一列空着）
            for (int i = 1; i <= 7; i++)
            {
                Button button = new Button { Content = $"格{i}" };
                uniformGrid.Children.Add(button);
            }

            PerformLayout(uniformGrid, new Size(300, 300));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试Border布局
        /// </summary>
        public static void TestBorder()
        {
            Console.WriteLine("=== 测试Border布局 ===");

            Border border = new Border
            {
                BorderThickness = new Thickness(2),
                Padding = new Thickness(10),
                CornerRadius = 5
            };

            TextBlock content = new TextBlock 
            { 
                Text = "带边框的内容", 
                FontSize = 14 
            };

            border.Child = content;

            PerformLayout(border, new Size(200, 100));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试Card布局
        /// </summary>
        public static void TestCard()
        {
            Console.WriteLine("=== 测试Card布局 ===");

            Card card = new Card
            {
                Elevation = 4,
                CornerRadius = 12
            };

            // 卡片头部
            TextBlock header = new TextBlock 
            { 
                Text = "卡片标题", 
                FontSize = 18 
            };

            // 卡片内容
            TextBlock content = new TextBlock 
            { 
                Text = "这是卡片的主要内容区域，可以包含任意的UI元素。", 
                FontSize = 14 
            };

            // 卡片底部
            StackPanel footer = new StackPanel 
            { 
                Orientation = Orientation.Horizontal 
            };
            footer.Children.Add(new Button { Content = "确定" });
            footer.Children.Add(new Button { Content = "取消" });

            card.Header = header;
            card.Content = content;
            card.Footer = footer;

            PerformLayout(card, new Size(300, 250));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试复合布局
        /// </summary>
        public static void TestComplexLayout()
        {
            Console.WriteLine("=== 测试复合布局 ===");

            // 主DockPanel
            DockPanel mainPanel = new DockPanel();

            // 顶部标题栏
            Border titleBar = new Border
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(10)
            };
            titleBar.Child = new TextBlock { Text = "应用程序标题", FontSize = 16 };
            DockPanel.SetDock(titleBar, Dock.Top);

            // 底部状态栏
            Border statusBar = new Border
            {
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(10, 5, 10, 5)
            };
            statusBar.Child = new TextBlock { Text = "就绪", FontSize = 10 };
            DockPanel.SetDock(statusBar, Dock.Bottom);

            // 左侧工具面板
            WrapPanel toolPanel = new WrapPanel
            {
                Orientation = Orientation.Vertical,
                ItemWidth = 60
            };
            for (int i = 1; i <= 6; i++)
            {
                toolPanel.Children.Add(new Button { Content = $"工具{i}" });
            }
            DockPanel.SetDock(toolPanel, Dock.Left);

            // 主内容区域（Card布局）
            Card contentCard = new Card();
            contentCard.Header = new TextBlock { Text = "工作区", FontSize = 14 };
            
            UniformGrid contentGrid = new UniformGrid { Rows = 2, Columns = 2 };
            for (int i = 1; i <= 4; i++)
            {
                contentGrid.Children.Add(new Button { Content = $"项目{i}" });
            }
            contentCard.Content = contentGrid;

            mainPanel.Children.Add(titleBar);
            mainPanel.Children.Add(statusBar);
            mainPanel.Children.Add(toolPanel);
            mainPanel.Children.Add(contentCard);

            PerformLayout(mainPanel, new Size(600, 500));
            Console.WriteLine();
        }

        /// <summary>
        /// 执行布局测试
        /// </summary>
        /// <param name="element">要测试的元素</param>
        /// <param name="availableSize">可用尺寸</param>
        private static void PerformLayout(UIElement element, Size availableSize)
        {
            Console.WriteLine($"布局元素: {element.GetType().Name}");
            Console.WriteLine($"可用尺寸: {availableSize}");

            // 测量
            element.Measure(availableSize);
            Console.WriteLine($"期望尺寸: {element.DesiredSize}");

            // 排列
            element.Arrange(new Rect(0, 0, availableSize.Width, availableSize.Height));
            Console.WriteLine($"渲染尺寸: {element.RenderSize}");
        }

        /// <summary>
        /// 运行所有高级布局测试
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("开始高级布局容器测试...\n");

            try
            {
                TestDockPanel();
                TestWrapPanel();
                TestUniformGrid();
                TestBorder();
                TestCard();
                TestComplexLayout();

                Console.WriteLine("所有高级布局测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}
