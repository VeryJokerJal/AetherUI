using System;
using AetherUI.Core;
using AetherUI.Layout;

namespace AetherUI.Xaml
{
    /// <summary>
    /// XAML解析器测试程序
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 程序入口点
        /// </summary>
        public static void Main()
        {
            Console.WriteLine("AetherUI XAML解析器测试");
            Console.WriteLine("=======================");

            try
            {
                TestBasicXamlParsing();
                TestComplexXamlParsing();
                TestXamlLoader();
                TestErrorHandling();

                TestBasicJsonParsing();
                TestComplexJsonParsing();
                TestJsonLoader();
                TestJsonConversion();

                Console.WriteLine("\n所有XAML和JSON测试完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试失败: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
            }

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 测试基础XAML解析
        /// </summary>
        private static void TestBasicXamlParsing()
        {
            Console.WriteLine("\n=== 测试基础XAML解析 ===");

            // 简单按钮
            string buttonXaml = @"<Button Content=""点击我"" />";
            
            try
            {
                object button = XamlLoader.Load(buttonXaml);
                Console.WriteLine($"✓ 解析按钮成功: {button.GetType().Name}");
                
                if (button is Button btn)
                {
                    Console.WriteLine($"  按钮内容: {btn.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 解析按钮失败: {ex.Message}");
            }

            // 文本块
            string textXaml = @"<TextBlock Text=""Hello AetherUI"" FontSize=""16"" />";
            
            try
            {
                object text = XamlLoader.Load(textXaml);
                Console.WriteLine($"✓ 解析文本块成功: {text.GetType().Name}");
                
                if (text is TextBlock tb)
                {
                    Console.WriteLine($"  文本内容: {tb.Text}");
                    Console.WriteLine($"  字体大小: {tb.FontSize}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 解析文本块失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试复杂XAML解析
        /// </summary>
        private static void TestComplexXamlParsing()
        {
            Console.WriteLine("\n=== 测试复杂XAML解析 ===");

            // StackPanel布局
            string stackPanelXaml = @"
<StackPanel Orientation=""Vertical"">
    <TextBlock Text=""标题"" FontSize=""18"" />
    <Button Content=""按钮1"" />
    <Button Content=""按钮2"" />
</StackPanel>";

            try
            {
                object stackPanel = XamlLoader.Load(stackPanelXaml);
                Console.WriteLine($"✓ 解析StackPanel成功: {stackPanel.GetType().Name}");
                
                if (stackPanel is StackPanel sp)
                {
                    Console.WriteLine($"  方向: {sp.Orientation}");
                    Console.WriteLine($"  子元素数量: {sp.Children.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 解析StackPanel失败: {ex.Message}");
            }

            // Grid布局
            string gridXaml = @"
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height=""Auto"" />
        <RowDefinition Height=""*"" />
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width=""100"" />
        <ColumnDefinition Width=""*"" />
    </Grid.ColumnDefinitions>
    
    <TextBlock Text=""标题"" Grid.Row=""0"" Grid.Column=""0"" Grid.ColumnSpan=""2"" />
    <Button Content=""左侧"" Grid.Row=""1"" Grid.Column=""0"" />
    <TextBlock Text=""右侧内容"" Grid.Row=""1"" Grid.Column=""1"" />
</Grid>";

            try
            {
                object grid = XamlLoader.Load(gridXaml);
                Console.WriteLine($"✓ 解析Grid成功: {grid.GetType().Name}");
                
                if (grid is Grid g)
                {
                    Console.WriteLine($"  行定义数量: {g.RowDefinitions.Count}");
                    Console.WriteLine($"  列定义数量: {g.ColumnDefinitions.Count}");
                    Console.WriteLine($"  子元素数量: {g.Children.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 解析Grid失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试XAML加载器
        /// </summary>
        private static void TestXamlLoader()
        {
            Console.WriteLine("\n=== 测试XAML加载器 ===");

            // 测试类型化加载
            string buttonXaml = @"<Button Content=""类型化按钮"" />";
            
            try
            {
                Button button = XamlLoader.Load<Button>(buttonXaml);
                Console.WriteLine($"✓ 类型化加载成功: {button.Content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 类型化加载失败: {ex.Message}");
            }

            // 测试UI元素加载
            string uiElementXaml = @"<StackPanel><Button Content=""UI元素"" /></StackPanel>";
            
            try
            {
                UIElement element = XamlLoader.LoadUIElement(uiElementXaml);
                Console.WriteLine($"✓ UI元素加载成功: {element.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ UI元素加载失败: {ex.Message}");
            }

            // 测试简单XAML创建
            var attributes = new System.Collections.Generic.Dictionary<string, string>
            {
                ["Content"] = "动态按钮",
                ["FontSize"] = "14"
            };
            
            string dynamicXaml = XamlLoader.CreateSimpleXaml("Button", attributes);
            Console.WriteLine($"✓ 动态XAML创建: {dynamicXaml}");

            try
            {
                Button dynamicButton = XamlLoader.Load<Button>(dynamicXaml);
                Console.WriteLine($"✓ 动态按钮解析成功: {dynamicButton.Content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 动态按钮解析失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试错误处理
        /// </summary>
        private static void TestErrorHandling()
        {
            Console.WriteLine("\n=== 测试错误处理 ===");

            // 测试无效XML
            string invalidXml = @"<Button Content=""未闭合标签""";
            
            try
            {
                XamlLoader.Load(invalidXml);
                Console.WriteLine("✗ 应该抛出异常但没有");
            }
            catch (XamlParseException ex)
            {
                Console.WriteLine($"✓ 正确捕获XML错误: {ex.Message}");
            }

            // 测试未知类型
            string unknownType = @"<UnknownElement />";
            
            try
            {
                XamlLoader.Load(unknownType);
                Console.WriteLine("✗ 应该抛出异常但没有");
            }
            catch (XamlParseException ex)
            {
                Console.WriteLine($"✓ 正确捕获类型错误: {ex.Message}");
            }

            // 测试类型不匹配
            string textXaml = @"<TextBlock Text=""文本"" />";
            
            try
            {
                Button button = XamlLoader.Load<Button>(textXaml);
                Console.WriteLine("✗ 应该抛出异常但没有");
            }
            catch (XamlParseException ex)
            {
                Console.WriteLine($"✓ 正确捕获类型不匹配错误: {ex.Message}");
            }

            // 测试XAML验证
            bool validXaml = XamlLoader.ValidateXaml(@"<Button Content=""有效"" />");
            bool invalidXaml = XamlLoader.ValidateXaml(@"<Button Content=""无效");
            
            Console.WriteLine($"✓ XAML验证 - 有效: {validXaml}, 无效: {invalidXaml}");
        }

        /// <summary>
        /// 测试基础JSON解析
        /// </summary>
        private static void TestBasicJsonParsing()
        {
            Console.WriteLine("\n=== 测试基础JSON解析 ===");

            // 简单按钮
            string buttonJson = @"{
                ""Type"": ""Button"",
                ""Content"": ""点击我""
            }";

            try
            {
                object button = JsonLoader.Load(buttonJson);
                Console.WriteLine($"✓ 解析按钮成功: {button.GetType().Name}");

                if (button is Button btn)
                {
                    Console.WriteLine($"  按钮内容: {btn.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 解析按钮失败: {ex.Message}");
            }

            // 文本块
            string textJson = @"{
                ""Type"": ""TextBlock"",
                ""Text"": ""Hello AetherUI"",
                ""FontSize"": 16
            }";

            try
            {
                object text = JsonLoader.Load(textJson);
                Console.WriteLine($"✓ 解析文本块成功: {text.GetType().Name}");

                if (text is TextBlock tb)
                {
                    Console.WriteLine($"  文本内容: {tb.Text}");
                    Console.WriteLine($"  字体大小: {tb.FontSize}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 解析文本块失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试复杂JSON解析
        /// </summary>
        private static void TestComplexJsonParsing()
        {
            Console.WriteLine("\n=== 测试复杂JSON解析 ===");

            // StackPanel布局
            string stackPanelJson = @"{
                ""Type"": ""StackPanel"",
                ""Orientation"": ""Vertical"",
                ""Children"": [
                    {
                        ""Type"": ""TextBlock"",
                        ""Text"": ""标题"",
                        ""FontSize"": 18
                    },
                    {
                        ""Type"": ""Button"",
                        ""Content"": ""按钮1""
                    },
                    {
                        ""Type"": ""Button"",
                        ""Content"": ""按钮2""
                    }
                ]
            }";

            try
            {
                object stackPanel = JsonLoader.Load(stackPanelJson);
                Console.WriteLine($"✓ 解析StackPanel成功: {stackPanel.GetType().Name}");

                if (stackPanel is StackPanel sp)
                {
                    Console.WriteLine($"  方向: {sp.Orientation}");
                    Console.WriteLine($"  子元素数量: {sp.Children.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 解析StackPanel失败: {ex.Message}");
            }

            // Border容器
            string borderJson = @"{
                ""Type"": ""Border"",
                ""BorderThickness"": ""2"",
                ""Padding"": ""10"",
                ""CornerRadius"": 5,
                ""Content"": {
                    ""Type"": ""TextBlock"",
                    ""Text"": ""带边框的内容""
                }
            }";

            try
            {
                object border = JsonLoader.Load(borderJson);
                Console.WriteLine($"✓ 解析Border成功: {border.GetType().Name}");

                if (border is Border b)
                {
                    Console.WriteLine($"  圆角半径: {b.CornerRadius}");
                    Console.WriteLine($"  内容类型: {b.Child?.GetType().Name ?? "null"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 解析Border失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试JSON加载器
        /// </summary>
        private static void TestJsonLoader()
        {
            Console.WriteLine("\n=== 测试JSON加载器 ===");

            // 测试类型化加载
            string buttonJson = @"{
                ""Type"": ""Button"",
                ""Content"": ""类型化按钮""
            }";

            try
            {
                Button button = JsonLoader.Load<Button>(buttonJson);
                Console.WriteLine($"✓ 类型化加载成功: {button.Content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 类型化加载失败: {ex.Message}");
            }

            // 测试UI元素加载
            string uiElementJson = @"{
                ""Type"": ""StackPanel"",
                ""Children"": [
                    {
                        ""Type"": ""Button"",
                        ""Content"": ""UI元素""
                    }
                ]
            }";

            try
            {
                UIElement element = JsonLoader.LoadUIElement(uiElementJson);
                Console.WriteLine($"✓ UI元素加载成功: {element.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ UI元素加载失败: {ex.Message}");
            }

            // 测试简单JSON创建
            var properties = new Dictionary<string, object>
            {
                ["Content"] = "动态按钮",
                ["FontSize"] = 14
            };

            string dynamicJson = JsonLoader.CreateSimpleJson("Button", properties);
            Console.WriteLine($"✓ 动态JSON创建: {dynamicJson.Replace("\n", " ").Replace("  ", " ")}");

            try
            {
                Button dynamicButton = JsonLoader.Load<Button>(dynamicJson);
                Console.WriteLine($"✓ 动态按钮解析成功: {dynamicButton.Content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 动态按钮解析失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试JSON转换
        /// </summary>
        private static void TestJsonConversion()
        {
            Console.WriteLine("\n=== 测试JSON转换 ===");

            // 测试JSON验证
            string validJson = @"{""Type"": ""Button"", ""Content"": ""有效""}";
            string invalidJson = @"{""Type"": ""Button"", ""Content"":";

            bool validResult = JsonLoader.ValidateJson(validJson);
            bool invalidResult = JsonLoader.ValidateJson(invalidJson);

            Console.WriteLine($"✓ JSON验证 - 有效: {validResult}, 无效: {invalidResult}");

            // 测试对象转JSON
            var testObject = new { Type = "Button", Content = "测试按钮", FontSize = 12 };
            string objectJson = JsonLoader.ToJson(testObject);
            Console.WriteLine($"✓ 对象转JSON: {objectJson.Replace("\n", " ").Replace("  ", " ")}");

            // 测试XAML转JSON（简化）
            try
            {
                string xaml = @"<Button Content=""XAML按钮"" />";
                string convertedJson = JsonLoader.ConvertFromXaml(xaml);
                Console.WriteLine($"✓ XAML转JSON: {convertedJson}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ XAML转JSON失败: {ex.Message}");
            }

            // 测试JSON转XAML（简化）
            try
            {
                string json = @"{""Type"": ""Button""}";
                string convertedXaml = JsonLoader.ConvertToXaml(json);
                Console.WriteLine($"✓ JSON转XAML: {convertedXaml}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ JSON转XAML失败: {ex.Message}");
            }
        }
    }
}
