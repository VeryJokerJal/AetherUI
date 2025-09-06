using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using AetherUI.Core;

namespace AetherUI.Xaml
{
    /// <summary>
    /// JSON加载器，提供便捷的JSON UI配置加载方法
    /// </summary>
    public static class JsonLoader
    {
        private static readonly JsonParser _parser = new JsonParser();

        #region 加载方法

        /// <summary>
        /// 从字符串加载JSON
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>加载的对象</returns>
        public static object Load(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON content cannot be null or empty", nameof(json));

            Debug.WriteLine("Loading JSON from string...");
            return _parser.Parse(json);
        }

        /// <summary>
        /// 从字符串加载JSON并转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON字符串</param>
        /// <returns>加载的对象</returns>
        public static T Load<T>(string json) where T : class
        {
            object result = Load(json);
            
            if (result is T typedResult)
                return typedResult;
            
            throw new XamlParseException($"JSON root element is not of type {typeof(T).Name}, actual type: {result.GetType().Name}");
        }

        /// <summary>
        /// 从文件加载JSON
        /// </summary>
        /// <param name="filePath">JSON文件路径</param>
        /// <returns>加载的对象</returns>
        public static object LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"JSON file not found: {filePath}");

            Debug.WriteLine($"Loading JSON from file: {filePath}");
            return _parser.ParseFile(filePath);
        }

        /// <summary>
        /// 从文件加载JSON并转换为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="filePath">JSON文件路径</param>
        /// <returns>加载的对象</returns>
        public static T LoadFromFile<T>(string filePath) where T : class
        {
            object result = LoadFromFile(filePath);
            
            if (result is T typedResult)
                return typedResult;
            
            throw new XamlParseException($"JSON root element is not of type {typeof(T).Name}, actual type: {result.GetType().Name}");
        }

        /// <summary>
        /// 从资源加载JSON
        /// </summary>
        /// <param name="resourceName">资源名称</param>
        /// <param name="assembly">程序集（null表示当前程序集）</param>
        /// <returns>加载的对象</returns>
        public static object LoadFromResource(string resourceName, System.Reflection.Assembly? assembly = null)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Resource name cannot be null or empty", nameof(resourceName));

            assembly ??= System.Reflection.Assembly.GetCallingAssembly();

            Debug.WriteLine($"Loading JSON from resource: {resourceName}");

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new XamlParseException($"Resource not found: {resourceName}");

            using StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            return Load(json);
        }

        /// <summary>
        /// 从资源加载JSON并转换为指定类型
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
            
            throw new XamlParseException($"JSON root element is not of type {typeof(T).Name}, actual type: {result.GetType().Name}");
        }

        #endregion

        #region UI元素专用方法

        /// <summary>
        /// 加载UI元素
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>UI元素</returns>
        public static UIElement LoadUIElement(string json)
        {
            return Load<UIElement>(json);
        }

        /// <summary>
        /// 从文件加载UI元素
        /// </summary>
        /// <param name="filePath">JSON文件路径</param>
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
        /// 创建简单的JSON字符串
        /// </summary>
        /// <param name="typeName">类型名称</param>
        /// <param name="properties">属性字典</param>
        /// <param name="children">子元素列表</param>
        /// <returns>JSON字符串</returns>
        public static string CreateSimpleJson(string typeName, 
            Dictionary<string, object>? properties = null, 
            List<object>? children = null)
        {
            var jsonObject = new Dictionary<string, object>
            {
                ["Type"] = typeName
            };

            // 添加属性
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    jsonObject[prop.Key] = prop.Value;
                }
            }

            // 添加子元素
            if (children != null && children.Count > 0)
            {
                jsonObject["Children"] = children;
            }

            return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }

        /// <summary>
        /// 验证JSON语法
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>是否有效</returns>
        public static bool ValidateJson(string json)
        {
            try
            {
                Load(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将对象转换为JSON字符串
        /// </summary>
        /// <param name="obj">要转换的对象</param>
        /// <param name="indented">是否格式化</param>
        /// <returns>JSON字符串</returns>
        public static string ToJson(object obj, bool indented = true)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indented,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// 从XAML转换为JSON
        /// </summary>
        /// <param name="xaml">XAML字符串</param>
        /// <returns>JSON字符串</returns>
        public static string ConvertFromXaml(string xaml)
        {
            try
            {
                // 使用XAML解析器解析
                object element = XamlLoader.Load(xaml);
                
                // 转换为JSON（这里是简化实现）
                return ToJson(new { Type = element.GetType().Name });
            }
            catch (Exception ex)
            {
                throw new XamlParseException($"Failed to convert XAML to JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从JSON转换为XAML
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>XAML字符串</returns>
        public static string ConvertToXaml(string json)
        {
            try
            {
                // 解析JSON
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                // 转换为XAML（这里是简化实现）
                if (root.TryGetProperty("Type", out JsonElement typeElement))
                {
                    string typeName = typeElement.GetString() ?? "UIElement";
                    return $"<{typeName} />";
                }

                return "<UIElement />";
            }
            catch (Exception ex)
            {
                throw new XamlParseException($"Failed to convert JSON to XAML: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
