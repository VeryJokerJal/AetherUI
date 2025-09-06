using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace AetherUI.Core
{
    /// <summary>
    /// 设计时数据上下文，为设计器提供模拟数据
    /// </summary>
    public class DesignTimeDataContext : ViewModelBase
    {
        private readonly Dictionary<string, object?> _designTimeData;
        private readonly Dictionary<string, Type> _propertyTypes;

        #region 构造函数

        /// <summary>
        /// 初始化设计时数据上下文
        /// </summary>
        public DesignTimeDataContext()
        {
            _designTimeData = new Dictionary<string, object?>();
            _propertyTypes = new Dictionary<string, Type>();
}

        #endregion

        #region 设计时数据管理

        /// <summary>
        /// 设置设计时属性值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">属性值</param>
        /// <param name="propertyType">属性类型</param>
        public void SetDesignTimeProperty(string propertyName, object? value, Type? propertyType = null)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Property name cannot be null or empty", nameof(propertyName));

            _designTimeData[propertyName] = value;
            
            if (propertyType != null)
            {
                _propertyTypes[propertyName] = propertyType;
            }
            else if (value != null)
            {
                _propertyTypes[propertyName] = value.GetType();
            }
OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// 获取设计时属性值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性值</returns>
        public object? GetDesignTimeProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;

            return _designTimeData.TryGetValue(propertyName, out object? value) ? value : null;
        }

        /// <summary>
        /// 获取设计时属性值（泛型版本）
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性值</returns>
        public T? GetDesignTimeProperty<T>(string propertyName)
        {
            object? value = GetDesignTimeProperty(propertyName);
            
            if (value is T typedValue)
                return typedValue;
            
            if (value == null)
                return default(T);

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 检查是否有设计时属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns>是否存在</returns>
        public bool HasDesignTimeProperty(string propertyName)
        {
            return !string.IsNullOrEmpty(propertyName) && _designTimeData.ContainsKey(propertyName);
        }

        /// <summary>
        /// 移除设计时属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        public void RemoveDesignTimeProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            if (_designTimeData.Remove(propertyName))
            {
                _propertyTypes.Remove(propertyName);
OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// 清除所有设计时属性
        /// </summary>
        public void ClearDesignTimeProperties()
        {
            var propertyNames = new List<string>(_designTimeData.Keys);
            
            _designTimeData.Clear();
            _propertyTypes.Clear();
foreach (string propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        #endregion

        #region 自动生成设计时数据

        /// <summary>
        /// 从类型自动生成设计时数据
        /// </summary>
        /// <param name="viewModelType">ViewModel类型</param>
        public void GenerateFromType(Type viewModelType)
        {
            if (viewModelType == null)
                throw new ArgumentNullException(nameof(viewModelType));
PropertyInfo[] properties = viewModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead)
                    continue;

                // 跳过索引器
                if (property.GetIndexParameters().Length > 0)
                    continue;

                // 生成模拟数据
                object? mockValue = GenerateMockValue(property.PropertyType, property.Name);
                SetDesignTimeProperty(property.Name, mockValue, property.PropertyType);
            }
}

        /// <summary>
        /// 生成模拟值
        /// </summary>
        /// <param name="propertyType">属性类型</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>模拟值</returns>
        private object? GenerateMockValue(Type propertyType, string propertyName)
        {
            // 处理可空类型
            Type actualType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            // 基本类型的模拟数据
            if (actualType == typeof(string))
            {
                return GenerateMockString(propertyName);
            }
            else if (actualType == typeof(int))
            {
                return GenerateMockInt(propertyName);
            }
            else if (actualType == typeof(double))
            {
                return GenerateMockDouble(propertyName);
            }
            else if (actualType == typeof(bool))
            {
                return GenerateMockBool(propertyName);
            }
            else if (actualType == typeof(DateTime))
            {
                return DateTime.Now;
            }
            else if (actualType.IsEnum)
            {
                Array enumValues = Enum.GetValues(actualType);
                return enumValues.Length > 0 ? enumValues.GetValue(0) : null;
            }
            else if (actualType == typeof(decimal))
            {
                return (decimal)GenerateMockDouble(propertyName);
            }
            else if (actualType == typeof(float))
            {
                return (float)GenerateMockDouble(propertyName);
            }

            // 对于复杂类型，返回null或尝试创建实例
            try
            {
                if (actualType.GetConstructor(Type.EmptyTypes) != null)
                {
                    return Activator.CreateInstance(actualType);
                }
            }
            catch
            {
                // 忽略创建失败
            }

            return null;
        }

        /// <summary>
        /// 生成模拟字符串
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns>模拟字符串</returns>
        private string GenerateMockString(string propertyName)
        {
            string lowerName = propertyName.ToLowerInvariant();

            if (lowerName.Contains("name"))
                return "示例名称";
            else if (lowerName.Contains("title"))
                return "示例标题";
            else if (lowerName.Contains("description"))
                return "这是一个示例描述文本";
            else if (lowerName.Contains("email"))
                return "example@example.com";
            else if (lowerName.Contains("phone"))
                return "123-456-7890";
            else if (lowerName.Contains("address"))
                return "示例地址";
            else if (lowerName.Contains("url") || lowerName.Contains("link"))
                return "https://example.com";
            else
                return $"示例{propertyName}";
        }

        /// <summary>
        /// 生成模拟整数
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns>模拟整数</returns>
        private int GenerateMockInt(string propertyName)
        {
            string lowerName = propertyName.ToLowerInvariant();

            if (lowerName.Contains("count") || lowerName.Contains("number"))
                return 42;
            else if (lowerName.Contains("age"))
                return 25;
            else if (lowerName.Contains("year"))
                return DateTime.Now.Year;
            else if (lowerName.Contains("id"))
                return 1001;
            else
                return 100;
        }

        /// <summary>
        /// 生成模拟双精度数
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns>模拟双精度数</returns>
        private double GenerateMockDouble(string propertyName)
        {
            string lowerName = propertyName.ToLowerInvariant();

            if (lowerName.Contains("price") || lowerName.Contains("cost"))
                return 99.99;
            else if (lowerName.Contains("percentage") || lowerName.Contains("percent"))
                return 75.5;
            else if (lowerName.Contains("width") || lowerName.Contains("height"))
                return 200.0;
            else if (lowerName.Contains("weight"))
                return 70.5;
            else
                return 123.45;
        }

        /// <summary>
        /// 生成模拟布尔值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns>模拟布尔值</returns>
        private bool GenerateMockBool(string propertyName)
        {
            string lowerName = propertyName.ToLowerInvariant();

            if (lowerName.Contains("is") || lowerName.Contains("has") || lowerName.Contains("can"))
                return true;
            else if (lowerName.Contains("not") || lowerName.Contains("disabled"))
                return false;
            else
                return true;
        }

        #endregion

        #region 静态工厂方法

        /// <summary>
        /// 创建设计时数据上下文
        /// </summary>
        /// <param name="viewModelType">ViewModel类型</param>
        /// <returns>设计时数据上下文</returns>
        public static DesignTimeDataContext CreateFor(Type viewModelType)
        {
            var context = new DesignTimeDataContext();
            context.GenerateFromType(viewModelType);
            return context;
        }

        /// <summary>
        /// 创建设计时数据上下文
        /// </summary>
        /// <typeparam name="T">ViewModel类型</typeparam>
        /// <returns>设计时数据上下文</returns>
        public static DesignTimeDataContext CreateFor<T>() where T : class
        {
            return CreateFor(typeof(T));
        }

        #endregion
    }
}
