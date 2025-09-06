using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace AetherUI.Core
{
    /// <summary>
    /// MVVM模式的ViewModel基类，实现INotifyPropertyChanged接口
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性更改事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发属性更改通知
        /// </summary>
        /// <param name="propertyName">属性名称，自动获取调用者名称</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值并触发更改通知
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称，自动获取调用者名称</param>
        /// <returns>如果值发生更改则返回true</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值并触发更改通知，支持自定义相等性比较
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="comparer">相等性比较器</param>
        /// <param name="propertyName">属性名称，自动获取调用者名称</param>
        /// <returns>如果值发生更改则返回true</returns>
        protected bool SetProperty<T>(ref T field, T value, IEqualityComparer<T> comparer, [CallerMemberName] string? propertyName = null)
        {
            if (comparer.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值并触发更改通知，支持更改前后的回调
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="onChanging">值更改前的回调</param>
        /// <param name="onChanged">值更改后的回调</param>
        /// <param name="propertyName">属性名称，自动获取调用者名称</param>
        /// <returns>如果值发生更改则返回true</returns>
        protected bool SetProperty<T>(ref T field, T value, Action<T>? onChanging, Action<T>? onChanged, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            onChanging?.Invoke(value);
            field = value;
            OnPropertyChanged(propertyName);
            onChanged?.Invoke(value);
            return true;
        }
    }
}
