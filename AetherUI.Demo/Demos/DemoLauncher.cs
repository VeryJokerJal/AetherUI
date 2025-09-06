using System;
using System.Diagnostics;
using AetherUI.Core;
using AetherUI.Layout;

namespace AetherUI.Demo.Demos
{
    /// <summary>
    /// 演示启动器，管理各种演示页面
    /// </summary>
    public static class DemoLauncher
    {
        /// <summary>
        /// 当前演示类型
        /// </summary>
        public enum DemoType
        {
            MainDemo,
            ButtonDemo,
            LayoutDemo,
            ControlsDemo
        }

        /// <summary>
        /// 当前活动的演示
        /// </summary>
        private static DemoType _currentDemo = DemoType.MainDemo;

        /// <summary>
        /// 创建演示选择器UI
        /// </summary>
        /// <returns>演示选择器UI元素</returns>
        public static UIElement CreateDemoSelector()
        {
            Console.WriteLine("创建演示选择器...");

            // 主容器
            Border mainBorder = new()
            {
                Background = "White",
                Padding = new Thickness(20)
            };

            StackPanel mainPanel = new()
            {
                Orientation = Orientation.Vertical
            };

            // 标题
            TextBlock title = new()
            {
                Text = "🎯 AetherUI 演示中心",
                FontSize = 28,
                FontFamily = "Microsoft YaHei",
                FontWeight = FontWeight.Bold,
                Foreground = "#2C3E50",
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(title);

            // 描述
            TextBlock description = new()
            {
                Text = "选择下面的演示项目来体验 AetherUI 框架的各种功能特性",
                FontSize = 14,
                FontFamily = "Microsoft YaHei",
                Foreground = "#7F8C8D",
                Margin = new Thickness(0, 0, 0, 30),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(description);

            // 演示选项网格
            Grid demoGrid = new();
            demoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            demoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            demoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            demoGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // 按钮演示卡片
            UIElement buttonDemoCard = CreateDemoCard(
                "🔘 按钮控件演示",
                "展示各种按钮样式、尺寸、颜色主题和交互功能",
                "#3498DB",
                () => SwitchToDemo(DemoType.ButtonDemo)
            );
            Grid.SetRow(buttonDemoCard, 0);
            Grid.SetColumn(buttonDemoCard, 0);
            demoGrid.Children.Add(buttonDemoCard);

            // 布局演示卡片
            UIElement layoutDemoCard = CreateDemoCard(
                "📦 布局容器演示",
                "展示 StackPanel、Grid、Canvas 等布局容器",
                "#2ECC71",
                () => SwitchToDemo(DemoType.LayoutDemo)
            );
            Grid.SetRow(layoutDemoCard, 0);
            Grid.SetColumn(layoutDemoCard, 1);
            demoGrid.Children.Add(layoutDemoCard);

            // 控件演示卡片
            UIElement controlsDemoCard = CreateDemoCard(
                "🎨 基础控件演示",
                "展示 TextBlock、Border、Card 等基础控件",
                "#E74C3C",
                () => SwitchToDemo(DemoType.ControlsDemo)
            );
            Grid.SetRow(controlsDemoCard, 1);
            Grid.SetColumn(controlsDemoCard, 0);
            demoGrid.Children.Add(controlsDemoCard);

            // 主演示卡片
            UIElement mainDemoCard = CreateDemoCard(
                "🏠 主演示页面",
                "返回到主演示页面查看整体功能",
                "#9B59B6",
                () => SwitchToDemo(DemoType.MainDemo)
            );
            Grid.SetRow(mainDemoCard, 1);
            Grid.SetColumn(mainDemoCard, 1);
            demoGrid.Children.Add(mainDemoCard);

            mainPanel.Children.Add(demoGrid);

            // 当前演示信息
            TextBlock currentDemoInfo = new()
            {
                Text = $"当前演示: {GetDemoName(_currentDemo)}",
                FontSize = 12,
                FontFamily = "Microsoft YaHei",
                Foreground = "#95A5A6",
                Margin = new Thickness(0, 30, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(currentDemoInfo);

            mainBorder.Child = mainPanel;
            return mainBorder;
        }

        /// <summary>
        /// 根据当前演示类型获取对应的UI
        /// </summary>
        /// <returns>当前演示的UI元素</returns>
        public static UIElement GetCurrentDemoUI()
        {
            return _currentDemo switch
            {
                DemoType.ButtonDemo => ButtonDemo.CreateButtonDemoPage(),
                DemoType.LayoutDemo => CreatePlaceholderDemo("布局容器演示", "此演示正在开发中..."),
                DemoType.ControlsDemo => CreatePlaceholderDemo("基础控件演示", "此演示正在开发中..."),
                DemoType.MainDemo => CreateDemoSelector(),
                _ => CreateDemoSelector()
            };
        }

        /// <summary>
        /// 切换到指定演示
        /// </summary>
        /// <param name="demoType">演示类型</param>
        private static void SwitchToDemo(DemoType demoType)
        {
            _currentDemo = demoType;
            Console.WriteLine($"切换到演示: {GetDemoName(demoType)}");
            
            // 这里应该触发UI更新，但由于当前架构限制，我们只能在控制台输出
            Console.WriteLine("请重新启动程序以查看新的演示内容");
        }

        /// <summary>
        /// 获取演示名称
        /// </summary>
        /// <param name="demoType">演示类型</param>
        /// <returns>演示名称</returns>
        private static string GetDemoName(DemoType demoType)
        {
            return demoType switch
            {
                DemoType.ButtonDemo => "按钮控件演示",
                DemoType.LayoutDemo => "布局容器演示",
                DemoType.ControlsDemo => "基础控件演示",
                DemoType.MainDemo => "主演示页面",
                _ => "未知演示"
            };
        }

        /// <summary>
        /// 创建演示卡片
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="description">描述</param>
        /// <param name="color">主题颜色</param>
        /// <param name="clickAction">点击动作</param>
        /// <returns>演示卡片UI元素</returns>
        private static UIElement CreateDemoCard(string title, string description, string color, Action clickAction)
        {
            Card demoCard = new()
            {
                Background = "#F8F9FA",
                CornerRadius = 12,
                Elevation = 3,
                Margin = new Thickness(10)
            };

            Border colorBorder = new()
            {
                Background = color,
                CornerRadius = 12
            };

            StackPanel cardContent = new()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(20)
            };

            // 标题
            TextBlock cardTitle = new()
            {
                Text = title,
                FontSize = 18,
                FontFamily = "Microsoft YaHei",
                FontWeight = FontWeight.Bold,
                Foreground = "White",
                Margin = new Thickness(0, 0, 0, 10)
            };
            cardContent.Children.Add(cardTitle);

            // 描述
            TextBlock cardDescription = new()
            {
                Text = description,
                FontSize = 12,
                FontFamily = "Microsoft YaHei",
                Foreground = "#ECF0F1",
                Margin = new Thickness(0, 0, 0, 15)
            };
            cardContent.Children.Add(cardDescription);

            // 按钮
            Button actionButton = new()
            {
                Content = "查看演示",
                Padding = new Thickness(12, 6, 12, 6),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            actionButton.Click += (s, e) => clickAction?.Invoke();

            Border buttonBorder = new()
            {
                Background = "White",
                CornerRadius = 4
            };
            buttonBorder.Child = actionButton;
            cardContent.Children.Add(buttonBorder);

            colorBorder.Child = cardContent;
            demoCard.Content = colorBorder;
            return demoCard;
        }

        /// <summary>
        /// 创建占位符演示
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息</param>
        /// <returns>占位符演示UI元素</returns>
        private static UIElement CreatePlaceholderDemo(string title, string message)
        {
            Border placeholderBorder = new()
            {
                Background = "White",
                Padding = new Thickness(40)
            };

            StackPanel placeholderPanel = new()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock placeholderTitle = new()
            {
                Text = title,
                FontSize = 24,
                FontFamily = "Microsoft YaHei",
                FontWeight = FontWeight.Bold,
                Foreground = "#2C3E50",
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            placeholderPanel.Children.Add(placeholderTitle);

            TextBlock placeholderMessage = new()
            {
                Text = message,
                FontSize = 16,
                FontFamily = "Microsoft YaHei",
                Foreground = "#7F8C8D",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            placeholderPanel.Children.Add(placeholderMessage);

            placeholderBorder.Child = placeholderPanel;
            return placeholderBorder;
        }
    }
}
