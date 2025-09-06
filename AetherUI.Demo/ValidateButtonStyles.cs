using System;
using System.Diagnostics;
using AetherUI.Core;
using AetherUI.Layout;

namespace AetherUI.Demo
{
    /// <summary>
    /// 按钮样式验证程序
    /// </summary>
    public static class ValidateButtonStyles
    {
        /// <summary>
        /// 验证按钮样式功能
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("    AetherUI 按钮样式验证程序");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            try
            {
                Console.WriteLine("开始验证按钮样式功能...");
                Console.WriteLine();

                // 验证默认属性
                ValidateDefaultProperties();

                // 验证自定义属性
                ValidateCustomProperties();

                // 验证属性修改
                ValidatePropertyChanges();

                // 验证颜色解析
                ValidateColorParsing();

                Console.WriteLine();
                Console.WriteLine("===========================================");
                Console.WriteLine("    所有验证测试通过！");
                Console.WriteLine("===========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"验证失败: {ex.Message}");
                Console.WriteLine($"详细错误: {ex}");
}

            Console.WriteLine("\n按任意键退出...");
            try
            {
                _ = Console.ReadKey();
            }
            catch { }
        }

        /// <summary>
        /// 验证默认属性
        /// </summary>
        private static void ValidateDefaultProperties()
        {
            Console.WriteLine("1. 验证默认属性值...");

            Button defaultButton = new();

            // 验证默认背景颜色
            string expectedBackground = "#3498DB";
            if (defaultButton.Background == expectedBackground)
            {
                Console.WriteLine($"   ✓ 默认背景颜色: {defaultButton.Background}");
            }
            else
            {
                throw new Exception($"默认背景颜色错误: 期望 {expectedBackground}, 实际 {defaultButton.Background}");
            }

            // 验证默认前景颜色
            string expectedForeground = "#FFFFFF";
            if (defaultButton.Foreground == expectedForeground)
            {
                Console.WriteLine($"   ✓ 默认前景颜色: {defaultButton.Foreground}");
            }
            else
            {
                throw new Exception($"默认前景颜色错误: 期望 {expectedForeground}, 实际 {defaultButton.Foreground}");
            }

            // 验证默认边框颜色
            string expectedBorderBrush = "#2980B9";
            if (defaultButton.BorderBrush == expectedBorderBrush)
            {
                Console.WriteLine($"   ✓ 默认边框颜色: {defaultButton.BorderBrush}");
            }
            else
            {
                throw new Exception($"默认边框颜色错误: 期望 {expectedBorderBrush}, 实际 {defaultButton.BorderBrush}");
            }

            // 验证默认圆角弧度
            double expectedCornerRadius = 7.0;
            if (Math.Abs(defaultButton.CornerRadius - expectedCornerRadius) < 0.001)
            {
                Console.WriteLine($"   ✓ 默认圆角弧度: {defaultButton.CornerRadius}");
            }
            else
            {
                throw new Exception($"默认圆角弧度错误: 期望 {expectedCornerRadius}, 实际 {defaultButton.CornerRadius}");
            }

            Console.WriteLine("   默认属性验证通过！");
            Console.WriteLine();
        }

        /// <summary>
        /// 验证自定义属性
        /// </summary>
        private static void ValidateCustomProperties()
        {
            Console.WriteLine("2. 验证自定义属性设置...");

            Button customButton = new()
            {
                Background = "#E74C3C",
                Foreground = "#FFFFFF",
                BorderBrush = "#C0392B",
                CornerRadius = 12.0
            };

            // 验证自定义背景颜色
            if (customButton.Background == "#E74C3C")
            {
                Console.WriteLine($"   ✓ 自定义背景颜色: {customButton.Background}");
            }
            else
            {
                throw new Exception($"自定义背景颜色设置失败");
            }

            // 验证自定义前景颜色
            if (customButton.Foreground == "#FFFFFF")
            {
                Console.WriteLine($"   ✓ 自定义前景颜色: {customButton.Foreground}");
            }
            else
            {
                throw new Exception($"自定义前景颜色设置失败");
            }

            // 验证自定义边框颜色
            if (customButton.BorderBrush == "#C0392B")
            {
                Console.WriteLine($"   ✓ 自定义边框颜色: {customButton.BorderBrush}");
            }
            else
            {
                throw new Exception($"自定义边框颜色设置失败");
            }

            // 验证自定义圆角弧度
            if (Math.Abs(customButton.CornerRadius - 12.0) < 0.001)
            {
                Console.WriteLine($"   ✓ 自定义圆角弧度: {customButton.CornerRadius}");
            }
            else
            {
                throw new Exception($"自定义圆角弧度设置失败");
            }

            Console.WriteLine("   自定义属性验证通过！");
            Console.WriteLine();
        }

        /// <summary>
        /// 验证属性修改
        /// </summary>
        private static void ValidatePropertyChanges()
        {
            Console.WriteLine("3. 验证属性动态修改...");

            Button button = new();

            // 修改背景颜色
            button.Background = "#2ECC71";
            if (button.Background == "#2ECC71")
            {
                Console.WriteLine($"   ✓ 背景颜色动态修改: {button.Background}");
            }
            else
            {
                throw new Exception("背景颜色动态修改失败");
            }

            // 修改前景颜色
            button.Foreground = "#2C3E50";
            if (button.Foreground == "#2C3E50")
            {
                Console.WriteLine($"   ✓ 前景颜色动态修改: {button.Foreground}");
            }
            else
            {
                throw new Exception("前景颜色动态修改失败");
            }

            // 修改边框颜色
            button.BorderBrush = "#27AE60";
            if (button.BorderBrush == "#27AE60")
            {
                Console.WriteLine($"   ✓ 边框颜色动态修改: {button.BorderBrush}");
            }
            else
            {
                throw new Exception("边框颜色动态修改失败");
            }

            // 修改圆角弧度
            button.CornerRadius = 20.0;
            if (Math.Abs(button.CornerRadius - 20.0) < 0.001)
            {
                Console.WriteLine($"   ✓ 圆角弧度动态修改: {button.CornerRadius}");
            }
            else
            {
                throw new Exception("圆角弧度动态修改失败");
            }

            Console.WriteLine("   属性动态修改验证通过！");
            Console.WriteLine();
        }

        /// <summary>
        /// 验证颜色解析
        /// </summary>
        private static void ValidateColorParsing()
        {
            Console.WriteLine("4. 验证颜色解析功能...");

            // 测试各种颜色格式
            var colorTests = new[]
            {
                "#FF0000",  // 红色
                "#00FF00",  // 绿色
                "#0000FF",  // 蓝色
                "#FFFFFF",  // 白色
                "#000000",  // 黑色
                "#3498DB",  // 蓝色
                "#E74C3C",  // 红色
                "#2ECC71",  // 绿色
                "#F39C12",  // 橙色
                "#9B59B6"   // 紫色
            };

            foreach (string color in colorTests)
            {
                try
                {
                    Button testButton = new()
                    {
                        Background = color,
                        Foreground = color,
                        BorderBrush = color
                    };

                    if (testButton.Background == color &&
                        testButton.Foreground == color &&
                        testButton.BorderBrush == color)
                    {
                        Console.WriteLine($"   ✓ 颜色解析成功: {color}");
                    }
                    else
                    {
                        throw new Exception($"颜色解析失败: {color}");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"颜色 {color} 解析时发生错误: {ex.Message}");
                }
            }

            // 测试圆角弧度范围
            var radiusTests = new[] { 0.0, 1.0, 5.0, 10.0, 15.0, 20.0, 25.0, 50.0 };

            foreach (double radius in radiusTests)
            {
                try
                {
                    Button testButton = new()
                    {
                        CornerRadius = radius
                    };

                    if (Math.Abs(testButton.CornerRadius - radius) < 0.001)
                    {
                        Console.WriteLine($"   ✓ 圆角弧度设置成功: {radius}px");
                    }
                    else
                    {
                        throw new Exception($"圆角弧度设置失败: {radius}");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"圆角弧度 {radius} 设置时发生错误: {ex.Message}");
                }
            }

            Console.WriteLine("   颜色解析功能验证通过！");
            Console.WriteLine();
        }
    }
}
