using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace AetherUI.Core
{
    /// <summary>
    /// 依赖属性系统，类似于WPF的DependencyProperty
    /// </summary>
    public sealed class DependencyProperty
    {
        private static readonly ConcurrentDictionary<string, DependencyProperty> _registeredProperties = new();
        private static int _nextGlobalIndex = 0;

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 属性类型
        /// </summary>
        public Type PropertyType { get; }

        /// <summary>
        /// 所有者类型
        /// </summary>
        public Type OwnerType { get; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object? DefaultValue { get; }

        /// <summary>
        /// 全局索引
        /// </summary>
        public int GlobalIndex { get; }

        /// <summary>
        /// 属性元数据
        /// </summary>
        public PropertyMetadata? Metadata { get; }

        private DependencyProperty(string name, Type propertyType, Type ownerType, PropertyMetadata? metadata)
        {
            Name = name;
            PropertyType = propertyType;
            OwnerType = ownerType;
            Metadata = metadata;
            DefaultValue = metadata?.DefaultValue;
            GlobalIndex = System.Threading.Interlocked.Increment(ref _nextGlobalIndex);
        }

        /// <summary>
        /// 注册依赖属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="ownerType">所有者类型</param>
        /// <param name="metadata">属性元数据</param>
        /// <returns>注册的依赖属性</returns>
        public static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata? metadata = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Property name cannot be null or empty", nameof(name));
            if (propertyType == null)
                throw new ArgumentNullException(nameof(propertyType));
            if (ownerType == null)
                throw new ArgumentNullException(nameof(ownerType));

            string key = $"{ownerType.FullName}.{name}";
            
            if (_registeredProperties.ContainsKey(key))
                throw new ArgumentException($"Property '{name}' is already registered for type '{ownerType.FullName}'");

            DependencyProperty property = new DependencyProperty(name, propertyType, ownerType, metadata);
            _registeredProperties[key] = property;

            Debug.WriteLine($"Registered dependency property: {key}");
            return property;
        }

        /// <summary>
        /// 注册只读依赖属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="propertyType">属性类型</param>
        /// <param name="ownerType">所有者类型</param>
        /// <param name="metadata">属性元数据</param>
        /// <returns>只读依赖属性键</returns>
        public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata? metadata = null)
        {
            DependencyProperty property = Register(name, propertyType, ownerType, metadata);
            return new DependencyPropertyKey(property);
        }

        /// <summary>
        /// 获取已注册的依赖属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="ownerType">所有者类型</param>
        /// <returns>依赖属性，如果未找到则返回null</returns>
        public static DependencyProperty? GetProperty(string name, Type ownerType)
        {
            string key = $"{ownerType.FullName}.{name}";
            _registeredProperties.TryGetValue(key, out DependencyProperty? property);
            return property;
        }

        public override string ToString()
        {
            return $"{OwnerType.Name}.{Name}";
        }
    }

    /// <summary>
    /// 只读依赖属性键
    /// </summary>
    public sealed class DependencyPropertyKey
    {
        /// <summary>
        /// 依赖属性
        /// </summary>
        public DependencyProperty DependencyProperty { get; }

        internal DependencyPropertyKey(DependencyProperty dependencyProperty)
        {
            DependencyProperty = dependencyProperty;
        }
    }

    /// <summary>
    /// 依赖属性元数据
    /// </summary>
    public class PropertyMetadata
    {
        /// <summary>
        /// 默认值
        /// </summary>
        public object? DefaultValue { get; }

        /// <summary>
        /// 属性更改回调
        /// </summary>
        public PropertyChangedCallback? PropertyChangedCallback { get; }

        /// <summary>
        /// 强制值回调
        /// </summary>
        public CoerceValueCallback? CoerceValueCallback { get; }

        /// <summary>
        /// 初始化属性元数据
        /// </summary>
        /// <param name="defaultValue">默认值</param>
        /// <param name="propertyChangedCallback">属性更改回调</param>
        /// <param name="coerceValueCallback">强制值回调</param>
        public PropertyMetadata(object? defaultValue = null, PropertyChangedCallback? propertyChangedCallback = null, CoerceValueCallback? coerceValueCallback = null)
        {
            DefaultValue = defaultValue;
            PropertyChangedCallback = propertyChangedCallback;
            CoerceValueCallback = coerceValueCallback;
        }
    }

    /// <summary>
    /// 属性更改回调委托
    /// </summary>
    /// <param name="d">依赖对象</param>
    /// <param name="e">属性更改事件参数</param>
    public delegate void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e);

    /// <summary>
    /// 强制值回调委托
    /// </summary>
    /// <param name="d">依赖对象</param>
    /// <param name="baseValue">基础值</param>
    /// <returns>强制后的值</returns>
    public delegate object? CoerceValueCallback(DependencyObject d, object? baseValue);

    /// <summary>
    /// 依赖属性更改事件参数
    /// </summary>
    public class DependencyPropertyChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 依赖属性
        /// </summary>
        public DependencyProperty Property { get; }

        /// <summary>
        /// 旧值
        /// </summary>
        public object? OldValue { get; }

        /// <summary>
        /// 新值
        /// </summary>
        public object? NewValue { get; }

        /// <summary>
        /// 初始化依赖属性更改事件参数
        /// </summary>
        /// <param name="property">依赖属性</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        public DependencyPropertyChangedEventArgs(DependencyProperty property, object? oldValue, object? newValue)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
