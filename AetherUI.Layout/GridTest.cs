using System;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// Grid布局测试类
    /// </summary>
    public static class GridTest
    {
        /// <summary>
        /// 测试基础Grid布局
        /// </summary>
        public static void TestBasicGrid()
        {
            Console.WriteLine("=== 测试基础Grid布局 ===");

            // 创建2x2的Grid
            Grid grid = new Grid();

            // 定义行
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star() });

            // 定义列
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Pixel(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star() });

            // 添加子元素
            TextBlock title = new TextBlock { Text = "标题", FontSize = 16 };
            Grid.SetRow(title, 0);
            Grid.SetColumn(title, 0);
            Grid.SetColumnSpan(title, 2);

            Button button1 = new Button { Content = "按钮1" };
            Grid.SetRow(button1, 1);
            Grid.SetColumn(button1, 0);

            TextBlock content = new TextBlock { Text = "内容区域", FontSize = 12 };
            Grid.SetRow(content, 1);
            Grid.SetColumn(content, 1);

            grid.Children.Add(title);
            grid.Children.Add(button1);
            grid.Children.Add(content);

            PerformLayout(grid, new Size(300, 200));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试复杂Grid布局
        /// </summary>
        public static void TestComplexGrid()
        {
            Console.WriteLine("=== 测试复杂Grid布局 ===");

            // 创建3x3的Grid
            Grid grid = new Grid();

            // 定义行（固定-自动-星号）
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Pixel(50) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star() });

            // 定义列（星号-固定-星号）
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star(2) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Pixel(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star(1) });

            // 添加子元素
            // 标题栏（跨所有列）
            TextBlock header = new TextBlock { Text = "应用程序标题", FontSize = 18 };
            Grid.SetRow(header, 0);
            Grid.SetColumn(header, 0);
            Grid.SetColumnSpan(header, 3);

            // 工具栏（跨所有列）
            StackPanel toolbar = new StackPanel { Orientation = Orientation.Horizontal };
            toolbar.Children.Add(new Button { Content = "新建" });
            toolbar.Children.Add(new Button { Content = "打开" });
            toolbar.Children.Add(new Button { Content = "保存" });
            Grid.SetRow(toolbar, 1);
            Grid.SetColumn(toolbar, 0);
            Grid.SetColumnSpan(toolbar, 3);

            // 主内容区域
            TextBlock mainContent = new TextBlock { Text = "主要内容区域", FontSize = 14 };
            Grid.SetRow(mainContent, 2);
            Grid.SetColumn(mainContent, 0);

            // 侧边栏
            StackPanel sidebar = new StackPanel { Orientation = Orientation.Vertical };
            sidebar.Children.Add(new Button { Content = "选项1" });
            sidebar.Children.Add(new Button { Content = "选项2" });
            Grid.SetRow(sidebar, 2);
            Grid.SetColumn(sidebar, 1);

            // 状态栏
            TextBlock status = new TextBlock { Text = "状态信息", FontSize = 10 };
            Grid.SetRow(status, 2);
            Grid.SetColumn(status, 2);

            grid.Children.Add(header);
            grid.Children.Add(toolbar);
            grid.Children.Add(mainContent);
            grid.Children.Add(sidebar);
            grid.Children.Add(status);

            PerformLayout(grid, new Size(500, 400));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试跨行跨列
        /// </summary>
        public static void TestSpanningGrid()
        {
            Console.WriteLine("=== 测试跨行跨列Grid ===");

            // 创建4x4的Grid
            Grid grid = new Grid();

            // 定义4行4列，都是星号
            for (int i = 0; i < 4; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star() });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star() });
            }

            // 大标题（跨2行2列）
            TextBlock bigTitle = new TextBlock { Text = "大标题", FontSize = 20 };
            Grid.SetRow(bigTitle, 0);
            Grid.SetColumn(bigTitle, 0);
            Grid.SetRowSpan(bigTitle, 2);
            Grid.SetColumnSpan(bigTitle, 2);

            // 右上角按钮
            Button topRightButton = new Button { Content = "右上" };
            Grid.SetRow(topRightButton, 0);
            Grid.SetColumn(topRightButton, 2);
            Grid.SetColumnSpan(topRightButton, 2);

            // 左下角按钮
            Button bottomLeftButton = new Button { Content = "左下" };
            Grid.SetRow(bottomLeftButton, 2);
            Grid.SetColumn(bottomLeftButton, 0);
            Grid.SetRowSpan(bottomLeftButton, 2);

            // 中间区域（跨2行2列）
            TextBlock centerArea = new TextBlock { Text = "中心区域", FontSize = 14 };
            Grid.SetRow(centerArea, 1);
            Grid.SetColumn(centerArea, 2);
            Grid.SetRowSpan(centerArea, 2);
            Grid.SetColumnSpan(centerArea, 2);

            // 底部按钮
            Button bottomButton = new Button { Content = "底部" };
            Grid.SetRow(bottomButton, 3);
            Grid.SetColumn(bottomButton, 1);

            grid.Children.Add(bigTitle);
            grid.Children.Add(topRightButton);
            grid.Children.Add(bottomLeftButton);
            grid.Children.Add(centerArea);
            grid.Children.Add(bottomButton);

            PerformLayout(grid, new Size(400, 300));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试嵌套Grid
        /// </summary>
        public static void TestNestedGrid()
        {
            Console.WriteLine("=== 测试嵌套Grid ===");

            // 主Grid
            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star() });

            // 标题
            TextBlock title = new TextBlock { Text = "嵌套Grid示例", FontSize = 16 };
            Grid.SetRow(title, 0);

            // 嵌套Grid
            Grid nestedGrid = new Grid();
            nestedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star() });
            nestedGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star() });

            Button leftButton = new Button { Content = "左侧" };
            Grid.SetColumn(leftButton, 0);

            Button rightButton = new Button { Content = "右侧" };
            Grid.SetColumn(rightButton, 1);

            nestedGrid.Children.Add(leftButton);
            nestedGrid.Children.Add(rightButton);

            Grid.SetRow(nestedGrid, 1);

            mainGrid.Children.Add(title);
            mainGrid.Children.Add(nestedGrid);

            PerformLayout(mainGrid, new Size(300, 150));
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
        /// 运行所有Grid测试
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("开始Grid布局测试...\n");

            try
            {
                TestBasicGrid();
                TestComplexGrid();
                TestSpanningGrid();
                TestNestedGrid();

                Console.WriteLine("所有Grid测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}
