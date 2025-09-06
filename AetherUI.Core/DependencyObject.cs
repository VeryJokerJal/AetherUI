using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace AetherUI.Core
{
    /// <summary>
    /// 依赖对象基类，支持依赖属性系统
    /// </summary>
    public abstract class DependencyObject
    {
        private readonly Dictionary<DependencyProperty, object?> _values = new();
        private readonly object _valuesLock = new object();

        /// <summary>
        /// 获取依赖属性的值
        /// </summary>
        /// <param name="dp">依赖属性</param>
        /// <returns>属性值</returns>
        public object? GetValue(DependencyProperty dp)
        {
            if (dp == null)
                throw new ArgumentNullException(nameof(dp));

            lock (_valuesLock)
            {
                if (_values.TryGetValue(dp, out object? value))
                {
                    Debug.WriteLine($"GetValue: {dp.Name} = {value}");
                    return value;
                }
            }

            object? defaultValue = dp.DefaultValue;
            Debug.WriteLine($"GetValue: {dp.Name} = {defaultValue} (default)");
            return defaultValue;
        }

        /// <summary>
        /// 设置依赖属性的值
        /// </summary>
        /// <param name="dp">依赖属性</param>
        /// <param name="value">新值</param>
        public void SetValue(DependencyProperty dp, object? value)
        {
            if (dp == null)
                throw new ArgumentNullException(nameof(dp));

            SetValueInternal(dp, value, false);
        }

        /// <summary>
        /// 设置只读依赖属性的值
        /// </summary>
        /// <param name="key">只读依赖属性键</param>
        /// <param name="value">新值</param>
        public void SetValue(DependencyPropertyKey key, object? value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            SetValueInternal(key.DependencyProperty, value, true);
        }

        /// <summary>
        /// 清除依赖属性的本地值
        /// </summary>
        /// <param name="dp">依赖属性</param>
        public void ClearValue(DependencyProperty dp)
        {
            if (dp == null)
                throw new ArgumentNullException(nameof(dp));

            object? oldValue;
            lock (_valuesLock)
            {
                if (!_values.TryGetValue(dp, out oldValue))
                {
                    return; // 没有本地值，无需清除
                }

                _values.Remove(dp);
            }

            object? newValue = dp.DefaultValue;
            Debug.WriteLine($"ClearValue: {dp.Name}, old={oldValue}, new={newValue}");

            // 触发属性更改事件
            OnPropertyChanged(new DependencyPropertyChangedEventArgs(dp, oldValue, newValue));
        }

        /// <summary>
        /// 检查依赖属性是否有本地值
        /// </summary>
        /// <param name="dp">依赖属性</param>
        /// <returns>如果有本地值则返回true</returns>
        public bool HasLocalValue(DependencyProperty dp)
        {
            if (dp == null)
                throw new ArgumentNullException(nameof(dp));

            lock (_valuesLock)
            {
                return _values.ContainsKey(dp);
            }
        }

        /// <summary>
        /// 获取所有本地值
        /// </summary>
        /// <returns>本地值的枚举</returns>
        public IEnumerable<KeyValuePair<DependencyProperty, object?>> GetLocalValues()
        {
            lock (_valuesLock)
            {
                return new Dictionary<DependencyProperty, object?>(_values);
            }
        }

        /// <summary>
        /// 内部设置值方法
        /// </summary>
        /// <param name="dp">依赖属性</param>
        /// <param name="value">新值</param>
        /// <param name="isReadOnlyKey">是否为只读属性键设置</param>
        private void SetValueInternal(DependencyProperty dp, object? value, bool isReadOnlyKey)
        {
            // 验证值类型
            if (value != null && !dp.PropertyType.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException($"Value of type '{value.GetType()}' cannot be assigned to property of type '{dp.PropertyType}'");
            }

            // 强制值回调
            if (dp.Metadata?.CoerceValueCallback != null)
            {
                value = dp.Metadata.CoerceValueCallback(this, value);
            }

            object? oldValue;
            bool hasChanged;

            lock (_valuesLock)
            {
                _values.TryGetValue(dp, out oldValue);
                hasChanged = !Equals(oldValue, value);

                if (hasChanged)
                {
                    if (value == null || Equals(value, dp.DefaultValue))
                    {
                        _values.Remove(dp);
                    }
                    else
                    {
                        _values[dp] = value;
                    }
                }
            }

            if (hasChanged)
            {
                Debug.WriteLine($"SetValue: {dp.Name}, old={oldValue}, new={value}");

                // 触发属性更改事件
                DependencyPropertyChangedEventArgs args = new DependencyPropertyChangedEventArgs(dp, oldValue, value);
                OnPropertyChanged(args);

                // 调用属性更改回调
                dp.Metadata?.PropertyChangedCallback?.Invoke(this, args);
            }
        }

        /// <summary>
        /// 属性更改时调用
        /// </summary>
        /// <param name="e">属性更改事件参数</param>
        protected virtual void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            // 子类可以重写此方法来处理属性更改
        }
    }
}
