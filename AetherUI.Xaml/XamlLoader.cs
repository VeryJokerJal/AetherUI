using System;
using System.Diagnostics;
using System.IO;
using AetherUI.Core;

namespace AetherUI.Xaml
{
    /// <summary>
    /// XAML加载器，提供便捷的XAML加载方法
    /// </summary>
    public static class XamlLoader
    {
        private static readonly XamlParser _parser = new XamlParser();

        #region 加载方法

        /// <summary>
        /// 从字符串加载XAML
        /// </summary>
        /// <param name="xaml">XAML字符串</param>
        /// <returns>加载的对象</returns>
        public static object Load(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                throw new ArgumentException("XAML content cannot be null or empty", nameof(xaml));

            Debug.WriteLine("Loading XAML from string...");
            return _parser.Parse(xaml);
        }

        /// <summary>
        /// 从字符串加载XAML并转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="xaml">XAML字符串</param>
        /// <returns>加载的对象</returns>
        public static T Load<T>(string xaml) where T : class
        {
            object result = Load(xaml);
            
            if (result is T typedResult)
                return typedResult;
            
            throw new XamlParseException($"XAML root element is not of type {typeof(T).Name}, actual type: {result.GetType().Name}");
        }

        /// <summary>
        /// 从文件加载XAML
        /// </summary>
        /// <param name="filePath">XAML文件路径</param>
        /// <returns>加载的对象</returns>
        public static object LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"XAML file not found: {filePath}");

            Debug.WriteLine($"Loading XAML from file: {filePath}");
            return _parser.ParseFile(filePath);
        }

        /// <summary>
        /// 从文件加载XAML并转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="filePath">XAML文件路径</param>
        /// <returns>加载的对象</returns>
        public static T LoadFromFile<T>(string filePath) where T : class
        {
            object result = LoadFromFile(filePath);
            
            if (result is T typedResult)
                return typedResult;
            
            throw new XamlParseException($"XAML root element is not of type {typeof(T).Name}, actual type: {result.GetType().Name}");
        }

        /// <summary>
        /// 从资源加载XAML
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集（null表示当前程序集）</param>
        /// <returns>加载的对象</returns>
        public static object LoadFromResource(string resourceName, System.Reflection.Assembly? assembly = null)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Resource name cannot be null or empty", nameof(resourceName));

            assembly ??= System.Reflection.Assembly.GetCallingAssembly();

            Debug.WriteLine($"Loading XAML from resource: {resourceName}");

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new XamlParseException($"Resource not found: {resourceName}");

            using StreamReader reader = new StreamReader(stream);
            string xaml = reader.ReadToEnd();

            return Load(xaml);
        }

        /// <summary>
        /// 从资源加载XAML并转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集（null表示当前程序集）</param>
        /// <returns>加载的对象</returns>
        public static T LoadFromResource<T>(string resourceName, System.Reflection.Assembly? assembly = null) where T : class
        {
            object result = LoadFromResource(resourceName, assembly);
            
            if (result is T typedResult)
                return typedResult;
            
            throw new XamlParseException($"XAML root element is not of type {typeof(T).Name}, actual type: {result.GetType().Name}");
        }

        #endregion

        #region UI元素专用方法

        /// <summary>
        /// 加载UI元素
        /// </summary>
        /// <param name="xaml">XAML字符串</param>
        /// <returns>UI元素</returns>
        public static UIElement LoadUIElement(string xaml)
        {
            return Load<UIElement>(xaml);
        }

        /// <summary>
        /// 从文件加载UI元素
        /// </summary>
        /// <param name="filePath">XAML文件路径</param>
        /// <returns>UI元素</returns>
        public static UIElement LoadUIElementFromFile(string filePath)
        {
            return LoadFromFile<UIElement>(filePath);
        }

        /// <summary>
        /// 从资源加载UI元素
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集（null表示当前程序集）</param>
        /// <returns>UI元素</returns>
        public static UIElement LoadUIElementFromResource(string resourceName, System.Reflection.Assembly? assembly = null)
        {
            return LoadFromResource<UIElement>(resourceName, assembly);
        }

        #endregion

        #region 便捷方法

        /// <summary>
        /// 创建简单的XAML字符串
        /// </summary>
        /// <param name="elementName">元素名称</param>
        /// <param name="attributes">属性字典</param>
        /// <param name="content">内容</param>
        /// <returns>XAML字符串</returns>
        public static string CreateSimpleXaml(string elementName, 
            System.Collections.Generic.Dictionary<string, string>? attributes = null, 
            string? content = null)
        {
            var xaml = new System.Text.StringBuilder();
            xaml.Append($"<{elementName}");

            // 添加属性
            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    xaml.Append($" {attr.Key}=\"{attr.Value}\"");
                }
            }

            if (string.IsNullOrEmpty(content))
            {
                xaml.Append(" />");
            }
            else
            {
                xaml.Append($">{content}</{elementName}>");
            }

            return xaml.ToString();
        }

        /// <summary>
        /// 验证XAML语法
        /// </summary>
        /// <param name="xaml">XAML字符串</param>
        /// <returns>是否有效</returns>
        public static bool ValidateXaml(string xaml)
        {
            try
            {
                Load(xaml);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
