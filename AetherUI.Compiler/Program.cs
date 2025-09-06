using System;
using System.IO;
using AetherUI.Xaml;

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
                TestHotReload();

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

        /// <summary>
        /// 测试热重载功能
        /// </summary>
        private static void TestHotReload()
        {
            Console.WriteLine("\n=== 测试热重载功能 ===");

            try
            {
                using HotReloadManager hotReloadManager = new HotReloadManager();

                // 设置事件处理
                hotReloadManager.HotReloadCompleted += (sender, e) =>
                {
                    Console.WriteLine($"✓ 热重载完成: {Path.GetFileName(e.FilePath)}");
                    Console.WriteLine($"  新元素类型: {e.NewElement?.GetType().Name ?? "null"}");
                    Console.WriteLine($"  编译成功: {e.CompilationResult?.Success ?? false}");
                };

                hotReloadManager.HotReloadError += (sender, e) =>
                {
                    Console.WriteLine($"✗ 热重载错误: {Path.GetFileName(e.FilePath)}");
                    Console.WriteLine($"  错误信息: {e.Error.Message}");
                };

                // 创建临时测试目录
                string tempDir = Path.Combine(Path.GetTempPath(), "AetherUIHotReloadTest");
                Directory.CreateDirectory(tempDir);

                Console.WriteLine($"✓ 创建测试目录: {tempDir}");

                // 创建测试XAML文件
                string testXamlPath = Path.Combine(tempDir, "TestButton.xaml");
                string initialXaml = @"<Button Content=""初始按钮"" />";
                File.WriteAllText(testXamlPath, initialXaml);

                Console.WriteLine($"✓ 创建测试XAML文件: {Path.GetFileName(testXamlPath)}");

                // 注册元素
                try
                {
                    object initialElement = XamlLoader.Load(initialXaml);
                    hotReloadManager.RegisterElement(testXamlPath, initialElement);
                    Console.WriteLine($"✓ 注册初始元素: {initialElement.GetType().Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ 注册初始元素失败: {ex.Message}");
                }

                // 开始监控
                hotReloadManager.StartWatching(tempDir);
                Console.WriteLine("✓ 开始热重载监控");

                // 模拟文件修改
                Console.WriteLine("✓ 模拟文件修改...");
                System.Threading.Thread.Sleep(1000); // 等待监控启动

                string modifiedXaml = @"<Button Content=""修改后的按钮"" />";
                File.WriteAllText(testXamlPath, modifiedXaml);

                // 等待热重载处理
                System.Threading.Thread.Sleep(2000);

                // 创建测试JSON文件
                string testJsonPath = Path.Combine(tempDir, "TestPanel.json");
                string testJson = @"{
                    ""Type"": ""StackPanel"",
                    ""Children"": [
                        {
                            ""Type"": ""TextBlock"",
                            ""Text"": ""JSON热重载测试""
                        }
                    ]
                }";
                File.WriteAllText(testJsonPath, testJson);

                Console.WriteLine($"✓ 创建测试JSON文件: {Path.GetFileName(testJsonPath)}");

                // 注册JSON元素
                try
                {
                    object jsonElement = JsonLoader.Load(testJson);
                    hotReloadManager.RegisterElement(testJsonPath, jsonElement);
                    Console.WriteLine($"✓ 注册JSON元素: {jsonElement.GetType().Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ 注册JSON元素失败: {ex.Message}");
                }

                // 等待处理
                System.Threading.Thread.Sleep(1000);

                // 修改JSON文件
                string modifiedJson = @"{
                    ""Type"": ""StackPanel"",
                    ""Children"": [
                        {
                            ""Type"": ""TextBlock"",
                            ""Text"": ""JSON热重载测试 - 已修改""
                        },
                        {
                            ""Type"": ""Button"",
                            ""Content"": ""新增按钮""
                        }
                    ]
                }";
                File.WriteAllText(testJsonPath, modifiedJson);

                // 等待热重载处理
                System.Threading.Thread.Sleep(2000);

                // 停止监控
                hotReloadManager.StopWatching();
                Console.WriteLine("✓ 停止热重载监控");

                // 清理测试文件
                try
                {
                    Directory.Delete(tempDir, true);
                    Console.WriteLine("✓ 清理测试文件");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ 清理测试文件失败: {ex.Message}");
                }

                Console.WriteLine("✓ 热重载功能测试完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 热重载测试失败: {ex.Message}");
            }
        }
    }
}
