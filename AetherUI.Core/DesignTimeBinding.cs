using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace AetherUI.Core
{
    /// <summary>
    /// 设计时绑定扩展，提供设计时数据绑定支持
    /// </summary>
    public static class DesignTimeBinding
    {
        private static readonly Dictionary<string, object?> _designTimeValues = new Dictionary<string, object?>();

        #region 设计时属性存储

        private static readonly Dictionary<DependencyObject, Dictionary<string, object?>> _designTimeProperties =
            new Dictionary<DependencyObject, Dictionary<string, object?>>();

        #endregion

        #region 设计时属性访问器

        /// <summary>
        /// 获取设计时属性
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性值</returns>
        public static object? GetDesignTimeProperty(DependencyObject element, string propertyName)
        {
            if (element == null || string.IsNullOrEmpty(propertyName))
                return null;

            if (_designTimeProperties.TryGetValue(element, out var properties))
            {
                return properties.TryGetValue(propertyName, out var value) ? value : null;
            }

            return null;
        }

        /// <summary>
        /// 设置设计时属性
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">属性值</param>
        public static void SetDesignTimeProperty(DependencyObject element, string propertyName, object? value)
        {
            if (element == null || string.IsNullOrEmpty(propertyName))
                return;

            if (!_designTimeProperties.TryGetValue(element, out var properties))
            {
                properties = new Dictionary<string, object?>();
                _designTimeProperties[element] = properties;
            }

            properties[propertyName] = value;
}

        /// <summary>
        /// 获取设计时数据上下文
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <returns>设计时数据上下文</returns>
        public static object? GetDesignTimeDataContext(DependencyObject element)
        {
            return GetDesignTimeProperty(element, "DataContext");
        }

        /// <summary>
        /// 设置设计时数据上下文
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="value">设计时数据上下文</param>
        public static void SetDesignTimeDataContext(DependencyObject element, object? value)
        {
            SetDesignTimeProperty(element, "DataContext", value);
        }

        #endregion



        #region 辅助方法

        /// <summary>
        /// 设置元素文本
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="text">文本内容</param>
        private static void SetElementText(FrameworkElement element, string text)
        {
            try
            {
                // 尝试通过反射设置Text属性
                PropertyInfo? textProperty = element.GetType().GetProperty("Text");
                if (textProperty != null && textProperty.CanWrite)
                {
                    textProperty.SetValue(element, text);
                    return;
                }

                // 尝试通过反射设置Content属性
                PropertyInfo? contentProperty = element.GetType().GetProperty("Content");
                if (contentProperty != null && contentProperty.CanWrite)
                {
                    contentProperty.SetValue(element, text);
                    return;
                }
}
            catch (Exception ex)
            {
}
        }

        /// <summary>
        /// 加载设计时源
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="source">源路径</param>
        private static void LoadDesignTimeSource(FrameworkElement element, string source)
        {
            try
            {
                // 这里可以实现设计时资源加载逻辑
                // 例如加载图片、样式等
                if (source.EndsWith(".png") || source.EndsWith(".jpg") || source.EndsWith(".jpeg"))
                {
                    // 图片资源
                    SetElementText(element, $"[图片: {source}]");
                }
                else if (source.EndsWith(".xaml"))
                {
                    // XAML资源
                    SetElementText(element, $"[XAML: {source}]");
                }
                else
                {
                    // 其他资源
                    SetElementText(element, $"[资源: {source}]");
                }
            }
            catch (Exception ex)
            {
}
        }

        #endregion

        #region 设计时值管理

        /// <summary>
        /// 设置设计时值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public static void SetDesignTimeValue(string key, object? value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            _designTimeValues[key] = value;
}

        /// <summary>
        /// 获取设计时值
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public static object? GetDesignTimeValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            return _designTimeValues.TryGetValue(key, out object? value) ? value : null;
        }

        /// <summary>
        /// 获取设计时值（泛型版本）
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public static T? GetDesignTimeValue<T>(string key)
        {
            object? value = GetDesignTimeValue(key);
            
            if (value is T typedValue)
                return typedValue;
            
            return default(T);
        }

        /// <summary>
        /// 清除所有设计时值
        /// </summary>
        public static void ClearDesignTimeValues()
        {
            _designTimeValues.Clear();
}

        #endregion
    }
}
