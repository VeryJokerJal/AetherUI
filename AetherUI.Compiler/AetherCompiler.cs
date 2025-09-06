using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AetherUI.Core;
using AetherUI.Xaml;

namespace AetherUI.Compiler
{
    /// <summary>
    /// AetherUI编译器，将XAML/JSON标记语言编译为C#代码
    /// </summary>
    public class AetherCompiler
    {
        private readonly Dictionary<string, string> _compilationOptions;
        private readonly List<string> _referencePaths;

        #region 构造函数

        /// <summary>
        /// 初始化AetherUI编译器
        /// </summary>
        public AetherCompiler()
        {
            _compilationOptions = new Dictionary<string, string>();
            _referencePaths = new List<string>();

            // 设置默认编译选项
            SetDefaultOptions();
        }

        #endregion

        #region 编译选项

        /// <summary>
        /// 设置编译选项
        /// </summary>
        /// <param name="key">选项键</param>
        /// <param name="value">选项值</param>
        public void SetOption(string key, string value)
        {
            _compilationOptions[key] = value;
        }

        /// <summary>
        /// 获取编译选项
        /// </summary>
        /// <param name="key">选项键</param>
        /// <returns>选项值</returns>
        public string? GetOption(string key)
        {
            return _compilationOptions.TryGetValue(key, out string? value) ? value : null;
        }

        /// <summary>
        /// 添加引用路径
        /// </summary>
        /// <param name="path">引用路径</param>
        public void AddReference(string path)
        {
            if (!_referencePaths.Contains(path))
            {
                _referencePaths.Add(path);
            }
        }

        #endregion

        #region XAML编译

        /// <summary>
        /// 编译XAML文件到C#代码
        /// </summary>
        /// <param name="xamlFilePath">XAML文件路径</param>
        /// <param name="outputPath">输出路径</param>
        /// <returns>编译结果</returns>
        public CompilationResult CompileXamlFile(string xamlFilePath, string? outputPath = null)
        {
            if (!File.Exists(xamlFilePath))
                return CompilationResult.Error($"XAML file not found: {xamlFilePath}");
try
            {
                string xamlContent = File.ReadAllText(xamlFilePath);
                string className = Path.GetFileNameWithoutExtension(xamlFilePath);
                
                return CompileXaml(xamlContent, className, outputPath);
            }
            catch (Exception ex)
            {
                return CompilationResult.Error($"Failed to compile XAML file: {ex.Message}");
            }
        }

        /// <summary>
        /// 编译XAML字符串到C#代码
        /// </summary>
        /// <param name="xamlContent">XAML内容</param>
        /// <param name="className">生成的类名</param>
        /// <param name="outputPath">输出路径</param>
        /// <returns>编译结果</returns>
        public CompilationResult CompileXaml(string xamlContent, string className, string? outputPath = null)
        {
try
            {
                // 解析XAML
                object rootElement = XamlLoader.Load(xamlContent);
                
                // 生成C#代码
                string csharpCode = GenerateCSharpCode(rootElement, className);
                
                // 编译C#代码
                CompilationResult compilationResult = CompileCSharp(csharpCode, className);
                
                // 保存到文件（如果指定了输出路径）
                if (!string.IsNullOrEmpty(outputPath) && compilationResult.Success)
                {
                    string outputFile = Path.Combine(outputPath, $"{className}.g.cs");
                    File.WriteAllText(outputFile, csharpCode);
                    compilationResult.OutputFiles.Add(outputFile);
                }

                compilationResult.GeneratedCode = csharpCode;
                return compilationResult;
            }
            catch (Exception ex)
            {
                return CompilationResult.Error($"XAML compilation failed: {ex.Message}");
            }
        }

        #endregion

        #region JSON编译

        /// <summary>
        /// 编译JSON文件到C#代码
        /// </summary>
        /// <param name="jsonFilePath">JSON文件路径</param>
        /// <param name="outputPath">输出路径</param>
        /// <returns>编译结果</returns>
        public CompilationResult CompileJsonFile(string jsonFilePath, string? outputPath = null)
        {
            if (!File.Exists(jsonFilePath))
                return CompilationResult.Error($"JSON file not found: {jsonFilePath}");
try
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                string className = Path.GetFileNameWithoutExtension(jsonFilePath);
                
                return CompileJson(jsonContent, className, outputPath);
            }
            catch (Exception ex)
            {
                return CompilationResult.Error($"Failed to compile JSON file: {ex.Message}");
            }
        }

        /// <summary>
        /// 编译JSON字符串到C#代码
        /// </summary>
        /// <param name="jsonContent">JSON内容</param>
        /// <param name="className">生成的类名</param>
        /// <param name="outputPath">输出路径</param>
        /// <returns>编译结果</returns>
        public CompilationResult CompileJson(string jsonContent, string className, string? outputPath = null)
        {
try
            {
                // 解析JSON
                object rootElement = JsonLoader.Load(jsonContent);
                
                // 生成C#代码
                string csharpCode = GenerateCSharpCode(rootElement, className);
                
                // 编译C#代码
                CompilationResult compilationResult = CompileCSharp(csharpCode, className);
                
                // 保存到文件（如果指定了输出路径）
                if (!string.IsNullOrEmpty(outputPath) && compilationResult.Success)
                {
                    string outputFile = Path.Combine(outputPath, $"{className}.g.cs");
                    File.WriteAllText(outputFile, csharpCode);
                    compilationResult.OutputFiles.Add(outputFile);
                }

                compilationResult.GeneratedCode = csharpCode;
                return compilationResult;
            }
            catch (Exception ex)
            {
                return CompilationResult.Error($"JSON compilation failed: {ex.Message}");
            }
        }

        #endregion

        #region C#代码生成

        /// <summary>
        /// 生成C#代码
        /// </summary>
        /// <param name="rootElement">根元素</param>
        /// <param name="className">类名</param>
        /// <returns>C#代码</returns>
        private string GenerateCSharpCode(object rootElement, string className)
        {
            var codeBuilder = new StringBuilder();
            
            // 添加using语句
            codeBuilder.AppendLine("using System;");
            codeBuilder.AppendLine("using AetherUI.Core;");
            codeBuilder.AppendLine("using AetherUI.Layout;");
            codeBuilder.AppendLine();

            // 添加命名空间
            string namespaceName = GetOption("Namespace") ?? "AetherUI.Generated";
            codeBuilder.AppendLine($"namespace {namespaceName}");
            codeBuilder.AppendLine("{");

            // 添加类定义
            codeBuilder.AppendLine($"    /// <summary>");
            codeBuilder.AppendLine($"    /// Generated class for {className}");
            codeBuilder.AppendLine($"    /// </summary>");
            codeBuilder.AppendLine($"    public partial class {className} : {rootElement.GetType().Name}");
            codeBuilder.AppendLine("    {");

            // 添加构造函数
            codeBuilder.AppendLine($"        /// <summary>");
            codeBuilder.AppendLine($"        /// Initializes a new instance of the {className} class");
            codeBuilder.AppendLine($"        /// </summary>");
            codeBuilder.AppendLine($"        public {className}()");
            codeBuilder.AppendLine("        {");
            codeBuilder.AppendLine("            InitializeComponent();");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine();

            // 添加InitializeComponent方法
            codeBuilder.AppendLine("        /// <summary>");
            codeBuilder.AppendLine("        /// Initializes the component");
            codeBuilder.AppendLine("        /// </summary>");
            codeBuilder.AppendLine("        private void InitializeComponent()");
            codeBuilder.AppendLine("        {");
            
            // 生成元素初始化代码
            GenerateElementInitialization(codeBuilder, rootElement, "this", 3);
            
            codeBuilder.AppendLine("        }");

            // 结束类定义
            codeBuilder.AppendLine("    }");

            // 结束命名空间
            codeBuilder.AppendLine("}");

            return codeBuilder.ToString();
        }

        /// <summary>
        /// 生成元素初始化代码
        /// </summary>
        /// <param name="codeBuilder">代码构建器</param>
        /// <param name="element">元素</param>
        /// <param name="variableName">变量名</param>
        /// <param name="indentLevel">缩进级别</param>
        private void GenerateElementInitialization(StringBuilder codeBuilder, object element, string variableName, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            
            // 如果不是根元素，创建变量
            if (variableName != "this")
            {
                codeBuilder.AppendLine($"{indent}var {variableName} = new {element.GetType().Name}();");
            }

            // 设置属性
            GeneratePropertySetters(codeBuilder, element, variableName, indentLevel);

            // 处理子元素
            GenerateChildElements(codeBuilder, element, variableName, indentLevel);
        }

        /// <summary>
        /// 生成属性设置代码
        /// </summary>
        /// <param name="codeBuilder">代码构建器</param>
        /// <param name="element">元素</param>
        /// <param name="variableName">变量名</param>
        /// <param name="indentLevel">缩进级别</param>
        private void GeneratePropertySetters(StringBuilder codeBuilder, object element, string variableName, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            
            // 这里是简化实现，实际应该反射获取所有已设置的属性
            // 为了演示，我们只处理一些常见属性
            
            // 使用反射来设置属性，避免直接引用具体类型
            Type elementType = element.GetType();

            if (elementType.Name == "TextBlock")
            {
                var textProperty = elementType.GetProperty("Text");
                var fontSizeProperty = elementType.GetProperty("FontSize");

                if (textProperty != null)
                {
                    string? text = textProperty.GetValue(element)?.ToString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        codeBuilder.AppendLine($"{indent}{variableName}.Text = \"{EscapeString(text)}\";");
                    }
                }

                if (fontSizeProperty != null)
                {
                    object? fontSize = fontSizeProperty.GetValue(element);
                    if (fontSize != null && (double)fontSize > 0)
                    {
                        codeBuilder.AppendLine($"{indent}{variableName}.FontSize = {fontSize};");
                    }
                }
            }

            if (elementType.Name == "Button")
            {
                var contentProperty = elementType.GetProperty("Content");
                if (contentProperty != null)
                {
                    object? content = contentProperty.GetValue(element);
                    if (content != null)
                    {
                        codeBuilder.AppendLine($"{indent}{variableName}.Content = \"{EscapeString(content.ToString())}\";");
                    }
                }
            }
        }

        /// <summary>
        /// 生成子元素代码
        /// </summary>
        /// <param name="codeBuilder">代码构建器</param>
        /// <param name="element">元素</param>
        /// <param name="variableName">变量名</param>
        /// <param name="indentLevel">缩进级别</param>
        private void GenerateChildElements(StringBuilder codeBuilder, object element, string variableName, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            
            // 处理Panel的Children
            if (element is Panel panel)
            {
                for (int i = 0; i < panel.Children.Count; i++)
                {
                    string childVariableName = $"child{i}";
                    GenerateElementInitialization(codeBuilder, panel.Children[i], childVariableName, indentLevel);
                    codeBuilder.AppendLine($"{indent}{variableName}.Children.Add({childVariableName});");
                }
            }
        }

        /// <summary>
        /// 转义字符串
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>转义后的字符串</returns>
        private string EscapeString(string? str)
        {
            if (str == null) return "";
            return str.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        #endregion

        #region C#编译

        /// <summary>
        /// 编译C#代码
        /// </summary>
        /// <param name="csharpCode">C#代码</param>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns>编译结果</returns>
        private CompilationResult CompileCSharp(string csharpCode, string assemblyName)
        {
try
            {
                // 解析语法树
                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(csharpCode);

                // 创建编译选项
                CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Debug);

                // 添加引用
                List<MetadataReference> references = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(UIElement).Assembly.Location)
                };

                // 创建编译
                CSharpCompilation compilation = CSharpCompilation.Create(
                    assemblyName,
                    new[] { syntaxTree },
                    references,
                    compilationOptions);

                // 编译到内存
                using var memoryStream = new MemoryStream();
                var emitResult = compilation.Emit(memoryStream);

                if (emitResult.Success)
                {
                    return new CompilationResult(true, $"Compilation successful for {assemblyName}");
                }
                else
                {
                    var errors = new List<string>();
                    foreach (var diagnostic in emitResult.Diagnostics)
                    {
                        if (diagnostic.Severity == DiagnosticSeverity.Error)
                        {
                            errors.Add(diagnostic.ToString());
                        }
                    }
                    return new CompilationResult(false, $"Compilation failed: {string.Join("; ", errors)}");
                }
            }
            catch (Exception ex)
            {
                return CompilationResult.Error($"C# compilation error: {ex.Message}");
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 设置默认编译选项
        /// </summary>
        private void SetDefaultOptions()
        {
            SetOption("Namespace", "AetherUI.Generated");
            SetOption("OutputType", "Library");
            SetOption("TargetFramework", "net9.0");
        }

        #endregion
    }

    /// <summary>
    /// 编译结果
    /// </summary>
    public class CompilationResult
    {
        /// <summary>
        /// 编译是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 编译消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 错误列表
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// 警告列表
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// 生成的C#代码
        /// </summary>
        public string? GeneratedCode { get; set; }

        /// <summary>
        /// 输出文件列表
        /// </summary>
        public List<string> OutputFiles { get; set; } = new List<string>();

        /// <summary>
        /// 编译时间（毫秒）
        /// </summary>
        public long CompilationTimeMs { get; set; }

        /// <summary>
        /// 编译开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 编译结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 初始化编译结果
        /// </summary>
        public CompilationResult()
        {
            StartTime = DateTime.Now;
        }

        /// <summary>
        /// 初始化编译结果
        /// </summary>
        /// <param name="success">是否成功</param>
        /// <param name="message">消息</param>
        public CompilationResult(bool success, string message) : this()
        {
            Success = success;
            Message = message;
            EndTime = DateTime.Now;
            CompilationTimeMs = (long)(EndTime - StartTime).TotalMilliseconds;
        }

        /// <summary>
        /// 创建失败的编译结果
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <returns>编译结果</returns>
        public static CompilationResult Error(string message)
        {
            var result = new CompilationResult(false, message);
            result.Errors.Add(message);
            return result;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString()
        {
            return $"CompilationResult: {(Success ? "Success" : "Failed")} - {Message}";
        }
    }
}
