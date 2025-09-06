using System;
using AetherUI.Core;
using AetherUI.Layout;

namespace AetherUI.Rendering
{
    /// <summary>
    /// 渲染系统测试程序
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 程序入口点
        /// </summary>
        public static void Main()
        {
            Console.WriteLine("AetherUI 渲染系统测试");
            Console.WriteLine("====================");

            try
            {
                // 创建测试UI
                UIElement rootElement = CreateTestUI();

                // 运行应用程序
                AetherApplication.RunSimple("AetherUI 渲染测试", rootElement);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序错误: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }

            Console.WriteLine("程序结束，按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 创建测试UI
        /// </summary>
        /// <returns>根UI元素</returns>
        private static UIElement CreateTestUI()
        {
            // 创建主DockPanel
            DockPanel mainPanel = new DockPanel();

            // 顶部标题栏
            Border titleBar = new Border
            {
                BorderThickness = new Thickness(0, 0, 0, 2),
                Padding = new Thickness(10),
                Background = "TitleBar"
            };
            TextBlock title = new TextBlock 
            { 
                Text = "AetherUI 渲染系统演示", 
                FontSize = 18 
            };
            titleBar.Child = title;
            DockPanel.SetDock(titleBar, Dock.Top);

            // 底部状态栏
            Border statusBar = new Border
            {
                BorderThickness = new Thickness(0, 2, 0, 0),
                Padding = new Thickness(10, 5, 10, 5),
                Background = "StatusBar"
            };
            TextBlock status = new TextBlock 
            { 
                Text = "就绪 - OpenTK渲染引擎", 
                FontSize = 12 
            };
            statusBar.Child = status;
            DockPanel.SetDock(statusBar, Dock.Bottom);

            // 左侧工具面板
            Border leftPanel = new Border
            {
                BorderThickness = new Thickness(0, 0, 2, 0),
                Padding = new Thickness(5),
                Background = "ToolPanel"
            };
            StackPanel toolStack = new StackPanel 
            { 
                Orientation = Orientation.Vertical 
            };
            
            // 添加工具按钮
            for (int i = 1; i <= 5; i++)
            {
                Button toolButton = new Button 
                { 
                    Content = $"工具 {i}",
                    Margin = new Thickness(2)
                };
                toolStack.Children.Add(toolButton);
            }
            
            leftPanel.Child = toolStack;
            DockPanel.SetDock(leftPanel, Dock.Left);

            // 主内容区域
            Card contentCard = new Card
            {
                Elevation = 4,
                CornerRadius = 8,
                Background = "ContentArea"
            };

            // 卡片头部
            TextBlock cardHeader = new TextBlock 
            { 
                Text = "布局容器演示", 
                FontSize = 16 
            };

            // 卡片内容 - Grid布局
            Grid contentGrid = new Grid();
            
            // 定义Grid行列
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star() });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star() });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star() });

            // Grid内容
            TextBlock gridTitle = new TextBlock 
            { 
                Text = "网格布局示例", 
                FontSize = 14 
            };
            Grid.SetRow(gridTitle, 0);
            Grid.SetColumn(gridTitle, 0);
            Grid.SetColumnSpan(gridTitle, 2);

            // 左侧Canvas
            Canvas leftCanvas = new Canvas();
            
            // Canvas中的元素
            Button canvasBtn1 = new Button { Content = "Canvas 1" };
            Canvas.SetLeft(canvasBtn1, 10);
            Canvas.SetTop(canvasBtn1, 10);
            
            Button canvasBtn2 = new Button { Content = "Canvas 2" };
            Canvas.SetRight(canvasBtn2, 10);
            Canvas.SetBottom(canvasBtn2, 10);
            
            leftCanvas.Children.Add(canvasBtn1);
            leftCanvas.Children.Add(canvasBtn2);
            
            Grid.SetRow(leftCanvas, 1);
            Grid.SetColumn(leftCanvas, 0);

            // 右侧WrapPanel
            WrapPanel rightWrap = new WrapPanel
            {
                Orientation = Orientation.Horizontal
            };
            
            // WrapPanel中的按钮
            for (int i = 1; i <= 8; i++)
            {
                Button wrapButton = new Button 
                { 
                    Content = $"Wrap {i}",
                    Margin = new Thickness(2)
                };
                rightWrap.Children.Add(wrapButton);
            }
            
            Grid.SetRow(rightWrap, 1);
            Grid.SetColumn(rightWrap, 1);

            // 组装Grid
            contentGrid.Children.Add(gridTitle);
            contentGrid.Children.Add(leftCanvas);
            contentGrid.Children.Add(rightWrap);

            // 卡片底部
            StackPanel cardFooter = new StackPanel 
            { 
                Orientation = Orientation.Horizontal 
            };
            cardFooter.Children.Add(new Button { Content = "刷新" });
            cardFooter.Children.Add(new Button { Content = "设置" });
            cardFooter.Children.Add(new Button { Content = "关于" });

            // 组装Card
            contentCard.Header = cardHeader;
            contentCard.Content = contentGrid;
            contentCard.Footer = cardFooter;

            // 组装主面板
            mainPanel.Children.Add(titleBar);
            mainPanel.Children.Add(statusBar);
            mainPanel.Children.Add(leftPanel);
            mainPanel.Children.Add(contentCard);

            Console.WriteLine("测试UI创建完成:");
            Console.WriteLine("- DockPanel主布局");
            Console.WriteLine("- Border装饰容器");
            Console.WriteLine("- Card卡片容器");
            Console.WriteLine("- Grid网格布局");
            Console.WriteLine("- Canvas绝对定位");
            Console.WriteLine("- WrapPanel自动换行");
            Console.WriteLine("- StackPanel线性布局");

            return mainPanel;
        }
    }
}
