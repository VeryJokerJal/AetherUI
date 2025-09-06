using System;
using System.IO;

namespace AetherUI.Compiler
{
    /// <summary>
    /// 编译器测试程序
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 程序入口点
        /// </summary>
        public static void Main()
        {
            Console.WriteLine("AetherUI 编译器测试");
            Console.WriteLine("==================");

            try
            {
                TestXamlCompilation();
                TestJsonCompilation();
                TestCodeGeneration();

                Console.WriteLine("\n所有编译器测试完成！");
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
        /// 测试XAML编译
        /// </summary>
        private static void TestXamlCompilation()
        {
            Console.WriteLine("\n=== 测试XAML编译 ===");

            AetherCompiler compiler = new AetherCompiler();

            // 简单XAML
            string simpleXaml = @"<Button Content=""Hello World"" />";
            
            try
            {
                CompilationResult result = compiler.CompileXaml(simpleXaml, "SimpleButton");
                Console.WriteLine($"✓ 简单XAML编译: {result.Success}");
                Console.WriteLine($"  消息: {result.Message}");
                Console.WriteLine($"  编译时间: {result.CompilationTimeMs}ms");
                
                if (!string.IsNullOrEmpty(result.GeneratedCode))
                {
                    Console.WriteLine("  生成的代码片段:");
                    string[] lines = result.GeneratedCode.Split('\n');
                    for (int i = 0; i < Math.Min(5, lines.Length); i++)
                    {
                        Console.WriteLine($"    {lines[i].Trim()}");
                    }
                    if (lines.Length > 5)
                    {
                        Console.WriteLine("    ...");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 简单XAML编译失败: {ex.Message}");
            }

            // 复杂XAML
            string complexXaml = @"
<StackPanel Orientation=""Vertical"">
    <TextBlock Text=""标题"" FontSize=""18"" />
    <Button Content=""按钮1"" />
    <Button Content=""按钮2"" />
</StackPanel>";

            try
            {
                CompilationResult result = compiler.CompileXaml(complexXaml, "ComplexPanel");
                Console.WriteLine($"✓ 复杂XAML编译: {result.Success}");
                Console.WriteLine($"  消息: {result.Message}");
                Console.WriteLine($"  错误数量: {result.Errors.Count}");
                Console.WriteLine($"  警告数量: {result.Warnings.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 复杂XAML编译失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试JSON编译
        /// </summary>
        private static void TestJsonCompilation()
        {
            Console.WriteLine("\n=== 测试JSON编译 ===");

            AetherCompiler compiler = new AetherCompiler();

            // 简单JSON
            string simpleJson = @"{
                ""Type"": ""Button"",
                ""Content"": ""Hello JSON""
            }";
            
            try
            {
                CompilationResult result = compiler.CompileJson(simpleJson, "JsonButton");
                Console.WriteLine($"✓ 简单JSON编译: {result.Success}");
                Console.WriteLine($"  消息: {result.Message}");
                Console.WriteLine($"  编译时间: {result.CompilationTimeMs}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 简单JSON编译失败: {ex.Message}");
            }

            // 复杂JSON
            string complexJson = @"{
                ""Type"": ""StackPanel"",
                ""Orientation"": ""Vertical"",
                ""Children"": [
                    {
                        ""Type"": ""TextBlock"",
                        ""Text"": ""JSON标题"",
                        ""FontSize"": 16
                    },
                    {
                        ""Type"": ""Button"",
                        ""Content"": ""JSON按钮""
                    }
                ]
            }";

            try
            {
                CompilationResult result = compiler.CompileJson(complexJson, "JsonPanel");
                Console.WriteLine($"✓ 复杂JSON编译: {result.Success}");
                Console.WriteLine($"  消息: {result.Message}");
                Console.WriteLine($"  错误数量: {result.Errors.Count}");
                Console.WriteLine($"  警告数量: {result.Warnings.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 复杂JSON编译失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试代码生成
        /// </summary>
        private static void TestCodeGeneration()
        {
            Console.WriteLine("\n=== 测试代码生成 ===");

            AetherCompiler compiler = new AetherCompiler();

            // 设置编译选项
            compiler.SetOption("Namespace", "TestNamespace");
            compiler.SetOption("OutputType", "Library");

            string testXaml = @"<TextBlock Text=""代码生成测试"" FontSize=""14"" />";

            try
            {
                CompilationResult result = compiler.CompileXaml(testXaml, "CodeGenTest");
                
                Console.WriteLine($"✓ 代码生成测试: {result.Success}");
                Console.WriteLine($"  命名空间: {compiler.GetOption("Namespace")}");
                Console.WriteLine($"  输出类型: {compiler.GetOption("OutputType")}");
                
                if (!string.IsNullOrEmpty(result.GeneratedCode))
                {
                    Console.WriteLine("\n  完整生成的代码:");
                    Console.WriteLine("  " + new string('-', 50));
                    
                    string[] lines = result.GeneratedCode.Split('\n');
                    foreach (string line in lines)
                    {
                        Console.WriteLine($"  {line}");
                    }
                    
                    Console.WriteLine("  " + new string('-', 50));
                }

                // 测试保存到文件
                string tempDir = Path.GetTempPath();
                string outputPath = Path.Combine(tempDir, "AetherUITest");
                Directory.CreateDirectory(outputPath);

                CompilationResult fileResult = compiler.CompileXaml(testXaml, "FileTest", outputPath);
                Console.WriteLine($"✓ 文件输出测试: {fileResult.Success}");
                Console.WriteLine($"  输出文件数量: {fileResult.OutputFiles.Count}");
                
                foreach (string outputFile in fileResult.OutputFiles)
                {
                    Console.WriteLine($"  输出文件: {outputFile}");
                    if (File.Exists(outputFile))
                    {
                        Console.WriteLine($"    文件大小: {new FileInfo(outputFile).Length} 字节");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 代码生成测试失败: {ex.Message}");
            }
        }
    }
}
