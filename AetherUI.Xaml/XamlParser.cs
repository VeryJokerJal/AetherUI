using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml;
using AetherUI.Core;

namespace AetherUI.Xaml
{
    /// <summary>
    /// XAML解析器，将XAML文档解析为UI元素树
    /// </summary>
    public class XamlParser
    {
        private readonly Dictionary<string, Type> _typeCache;
        private readonly Dictionary<string, string> _namespaceMap;

        #region 构造函数

        /// <summary>
        /// 初始化XAML解析器
        /// </summary>
        public XamlParser()
        {
            _typeCache = new Dictionary<string, Type>();
            _namespaceMap = new Dictionary<string, string>();

            // 注册默认命名空间
            RegisterDefaultNamespaces();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 解析XAML字符串
        /// </summary>
        /// <param name="xaml">XAML字符串</param>
        /// <returns>解析后的UI元素</returns>
        public object Parse(string xaml)
        {
            if (string.IsNullOrWhiteSpace(xaml))
                throw new ArgumentException("XAML content cannot be null or empty", nameof(xaml));

            Debug.WriteLine("Parsing XAML content...");

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xaml);

                if (doc.DocumentElement == null)
                    throw new XamlParseException("XAML document has no root element");

                object result = ParseElement(doc.DocumentElement);
                Debug.WriteLine($"XAML parsing completed. Root type: {result.GetType().Name}");

                return result;
            }
            catch (XmlException ex)
            {
                throw new XamlParseException($"XML parsing error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new XamlParseException($"XAML parsing error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从文件解析XAML
        /// </summary>
        /// <param name="filePath">XAML文件路径</param>
        /// <returns>解析后的UI元素</returns>
        public object ParseFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            Debug.WriteLine($"Parsing XAML file: {filePath}");

            try
            {
                string xaml = System.IO.File.ReadAllText(filePath);
                return Parse(xaml);
            }
            catch (System.IO.IOException ex)
            {
                throw new XamlParseException($"Failed to read XAML file: {ex.Message}", ex);
            }
        }

        #endregion

        #region 元素解析

        /// <summary>
        /// 解析XML元素
        /// </summary>
        /// <param name="element">XML元素</param>
        /// <returns>解析后的对象</returns>
        private object ParseElement(XmlElement element)
        {
            Debug.WriteLine($"Parsing element: {element.Name}");

            // 解析命名空间和类型名
            (string namespaceName, string typeName) = ParseElementName(element.Name);

            // 获取类型
            Type type = ResolveType(namespaceName, typeName);
            if (type == null)
                throw new XamlParseException($"Cannot resolve type: {element.Name}");

            // 创建实例
            object instance = CreateInstance(type);
            Debug.WriteLine($"Created instance of type: {type.Name}");

            // 解析属性
            ParseAttributes(element, instance);

            // 解析子元素
            ParseChildren(element, instance);

            return instance;
        }

        /// <summary>
        /// 解析元素名称
        /// </summary>
        /// <param name="elementName">元素名称</param>
        /// <returns>命名空间和类型名</returns>
        private (string namespaceName, string typeName) ParseElementName(string elementName)
        {
            if (elementName.Contains(':'))
            {
                string[] parts = elementName.Split(':');
                return (parts[0], parts[1]);
            }
            else
            {
                return ("", elementName);
            }
        }

        /// <summary>
        /// 解析属性
        /// </summary>
        /// <param name="element">XML元素</param>
        /// <param name="instance">目标实例</param>
        private void ParseAttributes(XmlElement element, object instance)
        {
            foreach (XmlAttribute attribute in element.Attributes)
            {
                // 跳过命名空间声明
                if (attribute.Name.StartsWith("xmlns"))
                    continue;

                Debug.WriteLine($"Setting property: {attribute.Name} = {attribute.Value}");

                try
                {
                    SetProperty(instance, attribute.Name, attribute.Value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to set property {attribute.Name}: {ex.Message}");
                    // 继续处理其他属性
                }
            }
        }

        /// <summary>
        /// 解析子元素
        /// </summary>
        /// <param name="element">XML元素</param>
        /// <param name="instance">父实例</param>
        private void ParseChildren(XmlElement element, object instance)
        {
            foreach (XmlNode childNode in element.ChildNodes)
            {
                if (childNode is XmlElement childElement)
                {
                    // 检查是否是属性元素语法
                    if (childElement.Name.Contains('.'))
                    {
                        ParsePropertyElement(childElement, instance);
                    }
                    else
                    {
                        // 普通子元素
                        object childInstance = ParseElement(childElement);
                        AddChild(instance, childInstance);
                    }
                }
                else if (childNode is XmlText textNode)
                {
                    // 处理文本内容
                    string text = textNode.Value?.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        SetTextContent(instance, text);
                    }
                }
            }
        }

        /// <summary>
        /// 解析属性元素
        /// </summary>
        /// <param name="element">属性元素</param>
        /// <param name="instance">父实例</param>
        private void ParsePropertyElement(XmlElement element, object instance)
        {
            string[] parts = element.Name.Split('.');
            if (parts.Length != 2)
                return;

            string propertyName = parts[1];
            Debug.WriteLine($"Setting complex property: {propertyName}");

            // 解析属性值
            if (element.ChildNodes.Count == 1 && element.FirstChild is XmlElement valueElement)
            {
                object value = ParseElement(valueElement);
                SetProperty(instance, propertyName, value);
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
                Debug.WriteLine($"Property {propertyName} not found or not writable on type {type.Name}");
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
                Debug.WriteLine($"Failed to set property {propertyName}: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置属性值（字符串）
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">属性值字符串</param>
        private void SetProperty(object instance, string propertyName, string value)
        {
            Type type = instance.GetType();
            PropertyInfo? property = type.GetProperty(propertyName);

            if (property == null || !property.CanWrite)
            {
                Debug.WriteLine($"Property {propertyName} not found or not writable on type {type.Name}");
                return;
            }

            try
            {
                // 字符串转换
                object convertedValue = ConvertStringValue(value, property.PropertyType);
                property.SetValue(instance, convertedValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set property {propertyName}: {ex.Message}");
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
        /// 转换字符串值
        /// </summary>
        /// <param name="value">字符串值</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>转换后的值</returns>
        private object ConvertStringValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
                return targetType.IsValueType ? Activator.CreateInstance(targetType)! : null!;

            // 基本类型转换
            if (targetType == typeof(string))
                return value;

            if (targetType.IsValueType)
            {
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }

            return value;
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

        /// <summary>
        /// 设置文本内容
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="text">文本内容</param>
        private void SetTextContent(object instance, string text)
        {
            // 尝试设置Text属性
            PropertyInfo? textProperty = instance.GetType().GetProperty("Text");
            if (textProperty != null && textProperty.CanWrite)
            {
                textProperty.SetValue(instance, text);
                return;
            }

            // 尝试设置Content属性
            PropertyInfo? contentProperty = instance.GetType().GetProperty("Content");
            if (contentProperty != null && contentProperty.CanWrite)
            {
                contentProperty.SetValue(instance, text);
            }
        }

        #endregion
    }
}
