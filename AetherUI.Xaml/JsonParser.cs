using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using AetherUI.Core;

namespace AetherUI.Xaml
{
    /// <summary>
    /// JSON解析器，将JSON配置解析为UI元素树
    /// </summary>
    public class JsonParser
    {
        private readonly Dictionary<string, Type> _typeCache;
        private readonly Dictionary<string, string> _namespaceMap;

        #region 构造函数

        /// <summary>
        /// 初始化JSON解析器
        /// </summary>
        public JsonParser()
        {
            _typeCache = new Dictionary<string, Type>();
            _namespaceMap = new Dictionary<string, string>();

            // 注册默认命名空间
            RegisterDefaultNamespaces();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 解析JSON字符串
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns>解析后的UI元素</returns>
        public object Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON content cannot be null or empty", nameof(json));
try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                object result = ParseElement(root);
return result;
            }
            catch (JsonException ex)
            {
                throw new XamlParseException($"JSON parsing error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new XamlParseException($"JSON parsing error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从文件解析JSON
        /// </summary>
        /// <param name="filePath">JSON文件路径</param>
        /// <returns>解析后的UI元素</returns>
        public object ParseFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
try
            {
                string json = System.IO.File.ReadAllText(filePath);
                return Parse(json);
            }
            catch (System.IO.IOException ex)
            {
                throw new XamlParseException($"Failed to read JSON file: {ex.Message}", ex);
            }
        }

        #endregion

        #region 元素解析

        /// <summary>
        /// 解析JSON元素
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <returns>解析后的对象</returns>
        private object ParseElement(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
                throw new XamlParseException("JSON element must be an object");

            // 获取类型名称
            if (!element.TryGetProperty("Type", out JsonElement typeElement))
                throw new XamlParseException("JSON element must have a 'Type' property");

            string typeName = typeElement.GetString() ?? throw new XamlParseException("Type property cannot be null");
            (string namespaceName, string localTypeName) = ParseTypeName(typeName);

            // 获取类型
            Type type = ResolveType(namespaceName, localTypeName);
            if (type == null)
                throw new XamlParseException($"Cannot resolve type: {typeName}");

            // 创建实例
            object instance = CreateInstance(type);
            ParseProperties(element, instance);

            // 解析子元素
            ParseChildren(element, instance);

            return instance;
        }

        /// <summary>
        /// 解析类型名称
        /// </summary>
        /// <param name="typeName">类型名称</param>
        /// <returns>命名空间和类型名</returns>
        private (string namespaceName, string localTypeName) ParseTypeName(string typeName)
        {
            if (typeName.Contains(':'))
            {
                string[] parts = typeName.Split(':');
                return (parts[0], parts[1]);
            }
            else
            {
                return ("", typeName);
            }
        }

        /// <summary>
        /// 解析属性
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <param name="instance">目标实例</param>
        private void ParseProperties(JsonElement element, object instance)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                // 跳过特殊属性
                if (property.Name == "Type" || property.Name == "Children" || property.Name == "Content")
                    continue;
try
                {
                    SetProperty(instance, property.Name, property.Value);
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// 解析子元素
        /// </summary>
        /// <param name="element">JSON元素</param>
        /// <param name="instance">父实例</param>
        private void ParseChildren(JsonElement element, object instance)
        {
            // 处理Children数组
            if (element.TryGetProperty("Children", out JsonElement childrenElement) && 
                childrenElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement childElement in childrenElement.EnumerateArray())
                {
                    object childInstance = ParseElement(childElement);
                    AddChild(instance, childInstance);
                }
            }

            // 处理Content属性
            if (element.TryGetProperty("Content", out JsonElement contentElement))
            {
                if (contentElement.ValueKind == JsonValueKind.Object)
                {
                    // Content是一个对象
                    object contentInstance = ParseElement(contentElement);
                    SetProperty(instance, "Content", contentInstance);
                }
                else if (contentElement.ValueKind == JsonValueKind.String)
                {
                    // Content是一个字符串
                    string contentText = contentElement.GetString() ?? "";
                    SetProperty(instance, "Content", contentText);
                }
            }
        }

        #endregion

        #region 类型解析

        /// <summary>
        /// 解析类型
        /// </summary>
        /// <param name="namespaceName">命名空间名称</param>
        /// <param name="typeName">类型名称</param>
        /// <returns>类型</returns>
        private Type? ResolveType(string namespaceName, string typeName)
        {
            string fullTypeName = string.IsNullOrEmpty(namespaceName) ? typeName : $"{namespaceName}:{typeName}";

            // 检查缓存
            if (_typeCache.TryGetValue(fullTypeName, out Type? cachedType))
                return cachedType;

            // 解析命名空间
            string? clrNamespace = ResolveNamespace(namespaceName);
            if (clrNamespace == null)
                return null;

            // 查找类型
            Type? type = FindType(clrNamespace, typeName);
            if (type != null)
            {
                _typeCache[fullTypeName] = type;
            }

            return type;
        }

        /// <summary>
        /// 解析命名空间
        /// </summary>
        /// <param name="namespaceName">命名空间名称</param>
        /// <returns>CLR命名空间</returns>
        private string? ResolveNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
                return "AetherUI.Layout"; // 默认命名空间

            return _namespaceMap.TryGetValue(namespaceName, out string? clrNamespace) ? clrNamespace : null;
        }

        /// <summary>
        /// 查找类型
        /// </summary>
        /// <param name="namespaceName">命名空间</param>
        /// <param name="typeName">类型名称</param>
        /// <returns>类型</returns>
        private Type? FindType(string namespaceName, string typeName)
        {
            string fullTypeName = $"{namespaceName}.{typeName}";

            // 在当前程序集中查找
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Type? type = currentAssembly.GetType(fullTypeName);
            if (type != null)
                return type;

            // 在引用的程序集中查找
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullTypeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        #endregion

        #region 实例创建和属性设置

        /// <summary>
        /// 创建类型实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>实例</returns>
        private object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type) ?? throw new XamlParseException($"Failed to create instance of type: {type.Name}");
            }
            catch (Exception ex)
            {
                throw new XamlParseException($"Failed to create instance of type {type.Name}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">属性值</param>
        private void SetProperty(object instance, string propertyName, object value)
        {
            Type type = instance.GetType();
            PropertyInfo? property = type.GetProperty(propertyName);

            if (property == null || !property.CanWrite)
            {
return;
            }

            try
            {
                // 类型转换
                object convertedValue = ConvertValue(value, property.PropertyType);
                property.SetValue(instance, convertedValue);
            }
            catch (Exception ex)
            {
}
        }

        /// <summary>
        /// 设置属性值（JSON元素）
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="jsonElement">JSON元素</param>
        private void SetProperty(object instance, string propertyName, JsonElement jsonElement)
        {
            Type type = instance.GetType();
            PropertyInfo? property = type.GetProperty(propertyName);

            if (property == null || !property.CanWrite)
            {
return;
            }

            try
            {
                // JSON元素转换
                object convertedValue = ConvertJsonValue(jsonElement, property.PropertyType);
                property.SetValue(instance, convertedValue);
            }
            catch (Exception ex)
            {
}
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 注册默认命名空间
        /// </summary>
        private void RegisterDefaultNamespaces()
        {
            _namespaceMap[""] = "AetherUI.Layout";
            _namespaceMap["core"] = "AetherUI.Core";
            _namespaceMap["layout"] = "AetherUI.Layout";
        }

        /// <summary>
        /// 转换值类型
        /// </summary>
        /// <param name="value">原始值</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>转换后的值</returns>
        private object ConvertValue(object value, Type targetType)
        {
            if (value == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType)! : null!;

            if (targetType.IsAssignableFrom(value.GetType()))
                return value;

            // 基本类型转换
            if (targetType.IsValueType || targetType == typeof(string))
            {
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }

            return value;
        }

        /// <summary>
        /// 转换JSON值
        /// </summary>
        /// <param name="jsonElement">JSON元素</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>转换后的值</returns>
        private object ConvertJsonValue(JsonElement jsonElement, Type targetType)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.String:
                    string stringValue = jsonElement.GetString() ?? "";
                    if (targetType == typeof(string))
                        return stringValue;
                    if (targetType.IsValueType)
                        return Convert.ChangeType(stringValue, targetType, CultureInfo.InvariantCulture);
                    return stringValue;

                case JsonValueKind.Number:
                    if (targetType == typeof(int))
                        return jsonElement.GetInt32();
                    if (targetType == typeof(double))
                        return jsonElement.GetDouble();
                    if (targetType == typeof(float))
                        return (float)jsonElement.GetDouble();
                    if (targetType == typeof(long))
                        return jsonElement.GetInt64();
                    return Convert.ChangeType(jsonElement.GetDouble(), targetType, CultureInfo.InvariantCulture);

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return jsonElement.GetBoolean();

                case JsonValueKind.Null:
                    return targetType.IsValueType ? Activator.CreateInstance(targetType)! : null!;

                default:
                    return jsonElement.ToString();
            }
        }

        /// <summary>
        /// 添加子元素
        /// </summary>
        /// <param name="parent">父元素</param>
        /// <param name="child">子元素</param>
        private void AddChild(object parent, object child)
        {
            // 尝试添加到Children集合
            PropertyInfo? childrenProperty = parent.GetType().GetProperty("Children");
            if (childrenProperty != null)
            {
                object? children = childrenProperty.GetValue(parent);
                if (children != null)
                {
                    MethodInfo? addMethod = children.GetType().GetMethod("Add");
                    addMethod?.Invoke(children, new[] { child });
                    return;
                }
            }

            // 尝试设置Child属性
            PropertyInfo? childProperty = parent.GetType().GetProperty("Child");
            if (childProperty != null && childProperty.CanWrite)
            {
                childProperty.SetValue(parent, child);
                return;
            }

            // 尝试设置Content属性
            PropertyInfo? contentProperty = parent.GetType().GetProperty("Content");
            if (contentProperty != null && contentProperty.CanWrite)
            {
                contentProperty.SetValue(parent, child);
            }
        }

        #endregion
    }
}
