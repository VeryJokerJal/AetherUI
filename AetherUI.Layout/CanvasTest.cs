using System;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// Canvas布局测试类
    /// </summary>
    public static class CanvasTest
    {
        /// <summary>
        /// 测试基础Canvas布局
        /// </summary>
        public static void TestBasicCanvas()
        {
            Console.WriteLine("=== 测试基础Canvas布局 ===");

            Canvas canvas = new Canvas();

            // 左上角按钮
            Button topLeftButton = new Button { Content = "左上角" };
            Canvas.SetLeft(topLeftButton, 10);
            Canvas.SetTop(topLeftButton, 10);

            // 右上角文本
            TextBlock topRightText = new TextBlock { Text = "右上角文本", FontSize = 14 };
            Canvas.SetRight(topRightText, 10);
            Canvas.SetTop(topRightText, 10);

            // 左下角文本
            TextBlock bottomLeftText = new TextBlock { Text = "左下角", FontSize = 12 };
            Canvas.SetLeft(bottomLeftText, 10);
            Canvas.SetBottom(bottomLeftText, 10);

            // 右下角按钮
            Button bottomRightButton = new Button { Content = "右下角" };
            Canvas.SetRight(bottomRightButton, 10);
            Canvas.SetBottom(bottomRightButton, 10);

            // 中心元素
            TextBlock centerText = new TextBlock { Text = "中心位置", FontSize = 16 };
            Canvas.SetLeft(centerText, 150);
            Canvas.SetTop(centerText, 100);

            canvas.Children.Add(topLeftButton);
            canvas.Children.Add(topRightText);
            canvas.Children.Add(bottomLeftText);
            canvas.Children.Add(bottomRightButton);
            canvas.Children.Add(centerText);

            PerformLayout(canvas, new Size(400, 300));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试Canvas拉伸布局
        /// </summary>
        public static void TestCanvasStretching()
        {
            Console.WriteLine("=== 测试Canvas拉伸布局 ===");

            Canvas canvas = new Canvas();

            // 拉伸到整个宽度的标题栏
            TextBlock titleBar = new TextBlock { Text = "标题栏", FontSize = 18 };
            Canvas.SetLeft(titleBar, 0);
            Canvas.SetTop(titleBar, 0);
            Canvas.SetRight(titleBar, 0);

            // 拉伸到整个高度的侧边栏
            TextBlock sidebar = new TextBlock { Text = "侧边栏", FontSize = 12 };
            Canvas.SetLeft(sidebar, 0);
            Canvas.SetTop(sidebar, 30);
            Canvas.SetBottom(sidebar, 30);

            // 填充剩余空间的主内容区
            TextBlock mainContent = new TextBlock { Text = "主内容区域", FontSize = 14 };
            Canvas.SetLeft(mainContent, 80);
            Canvas.SetTop(mainContent, 30);
            Canvas.SetRight(mainContent, 10);
            Canvas.SetBottom(mainContent, 30);

            // 底部状态栏
            TextBlock statusBar = new TextBlock { Text = "状态栏", FontSize = 10 };
            Canvas.SetLeft(statusBar, 0);
            Canvas.SetRight(statusBar, 0);
            Canvas.SetBottom(statusBar, 0);

            canvas.Children.Add(titleBar);
            canvas.Children.Add(sidebar);
            canvas.Children.Add(mainContent);
            canvas.Children.Add(statusBar);

            PerformLayout(canvas, new Size(500, 400));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试Canvas Z轴顺序
        /// </summary>
        public static void TestCanvasZIndex()
        {
            Console.WriteLine("=== 测试Canvas Z轴顺序 ===");

            Canvas canvas = new Canvas();

            // 背景矩形（Z轴最低）
            TextBlock background = new TextBlock { Text = "背景层", FontSize = 20 };
            Canvas.SetLeft(background, 50);
            Canvas.SetTop(background, 50);
            Canvas.SetZIndex(background, 0);

            // 中间层按钮
            Button middleButton = new Button { Content = "中间层" };
            Canvas.SetLeft(middleButton, 80);
            Canvas.SetTop(middleButton, 80);
            Canvas.SetZIndex(middleButton, 1);

            // 前景文本（Z轴最高）
            TextBlock foreground = new TextBlock { Text = "前景层", FontSize = 14 };
            Canvas.SetLeft(foreground, 110);
            Canvas.SetTop(foreground, 110);
            Canvas.SetZIndex(foreground, 2);

            // 重叠测试元素
            Button overlappingButton = new Button { Content = "重叠测试" };
            Canvas.SetLeft(overlappingButton, 70);
            Canvas.SetTop(overlappingButton, 70);
            Canvas.SetZIndex(overlappingButton, -1); // 负Z轴，应该在背景之后

            canvas.Children.Add(background);
            canvas.Children.Add(middleButton);
            canvas.Children.Add(foreground);
            canvas.Children.Add(overlappingButton);

            PerformLayout(canvas, new Size(300, 250));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试Canvas动态定位
        /// </summary>
        public static void TestCanvasDynamicPositioning()
        {
            Console.WriteLine("=== 测试Canvas动态定位 ===");

            Canvas canvas = new Canvas();

            // 创建一些按钮进行动态定位测试
            for (int i = 0; i < 5; i++)
            {
                Button button = new Button { Content = $"按钮{i + 1}" };
                
                // 计算螺旋位置
                double angle = i * Math.PI / 2; // 90度间隔
                double radius = 50 + i * 20;
                double x = 150 + radius * Math.Cos(angle);
                double y = 100 + radius * Math.Sin(angle);

                Canvas.SetLeft(button, x);
                Canvas.SetTop(button, y);
                Canvas.SetZIndex(button, i); // 按顺序设置Z轴

                canvas.Children.Add(button);

                Console.WriteLine($"按钮{i + 1} 位置: ({x:F1}, {y:F1}), Z轴: {i}");
            }

            PerformLayout(canvas, new Size(400, 300));
            Console.WriteLine();
        }

        /// <summary>
        /// 测试嵌套Canvas
        /// </summary>
        public static void TestNestedCanvas()
        {
            Console.WriteLine("=== 测试嵌套Canvas ===");

            // 主Canvas
            Canvas mainCanvas = new Canvas();

            // 标题
            TextBlock title = new TextBlock { Text = "嵌套Canvas示例", FontSize = 16 };
            Canvas.SetLeft(title, 10);
            Canvas.SetTop(title, 10);

            // 嵌套的子Canvas
            Canvas nestedCanvas = new Canvas();
            Canvas.SetLeft(nestedCanvas, 50);
            Canvas.SetTop(nestedCanvas, 50);
            Canvas.SetRight(nestedCanvas, 50);
            Canvas.SetBottom(nestedCanvas, 50);

            // 在嵌套Canvas中添加元素
            Button nestedButton1 = new Button { Content = "嵌套1" };
            Canvas.SetLeft(nestedButton1, 10);
            Canvas.SetTop(nestedButton1, 10);

            Button nestedButton2 = new Button { Content = "嵌套2" };
            Canvas.SetRight(nestedButton2, 10);
            Canvas.SetBottom(nestedButton2, 10);

            TextBlock nestedCenter = new TextBlock { Text = "嵌套中心", FontSize = 12 };
            Canvas.SetLeft(nestedCenter, 50);
            Canvas.SetTop(nestedCenter, 50);

            nestedCanvas.Children.Add(nestedButton1);
            nestedCanvas.Children.Add(nestedButton2);
            nestedCanvas.Children.Add(nestedCenter);

            mainCanvas.Children.Add(title);
            mainCanvas.Children.Add(nestedCanvas);

            PerformLayout(mainCanvas, new Size(400, 300));
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
        /// 运行所有Canvas测试
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("开始Canvas布局测试...\n");

            try
            {
                TestBasicCanvas();
                TestCanvasStretching();
                TestCanvasZIndex();
                TestCanvasDynamicPositioning();
                TestNestedCanvas();

                Console.WriteLine("所有Canvas测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }
        }
    }
}
