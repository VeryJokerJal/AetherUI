using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace AetherUI.Core
{
    /// <summary>
    /// 设计时助手类，提供设计时检测和数据绑定支持
    /// </summary>
    public static class DesignTimeHelper
    {
        private static bool? _isInDesignMode;

        #region 设计时检测

        /// <summary>
        /// 检查是否在设计时模式
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                if (_isInDesignMode.HasValue)
                    return _isInDesignMode.Value;

                _isInDesignMode = DetectDesignMode();
                return _isInDesignMode.Value;
            }
        }

        /// <summary>
        /// 检测设计时模式
        /// </summary>
        /// <returns>是否在设计时</returns>
        private static bool DetectDesignMode()
        {
            try
            {
                // 检查是否在Visual Studio设计器中
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    Debug.WriteLine("Design mode detected: LicenseManager.UsageMode");
                    return true;
                }

                // 检查进程名称
                string processName = Process.GetCurrentProcess().ProcessName.ToLowerInvariant();
                if (processName.Contains("devenv") || 
                    processName.Contains("blend") || 
                    processName.Contains("xdesproc") ||
                    processName.Contains("designer"))
                {
                    Debug.WriteLine($"Design mode detected: Process name {processName}");
                    return true;
                }

                // 检查是否在调试器中
                if (Debugger.IsAttached)
                {
                    // 进一步检查调试器类型
                    string? debuggerName = Environment.GetEnvironmentVariable("VS_DEBUGGER_CAUSAL_SESSION_ID");
                    if (!string.IsNullOrEmpty(debuggerName))
                    {
                        Debug.WriteLine("Design mode detected: Visual Studio debugger");
                        return true;
                    }
                }

                // 检查特定的环境变量
                string? designModeEnv = Environment.GetEnvironmentVariable("AETHERUI_DESIGN_MODE");
                if (!string.IsNullOrEmpty(designModeEnv) && 
                    (designModeEnv.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                     designModeEnv.Equals("1", StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.WriteLine("Design mode detected: Environment variable");
                    return true;
                }

                Debug.WriteLine("Runtime mode detected");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error detecting design mode: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 强制设置设计时模式（用于测试）
        /// </summary>
        /// <param name="isDesignMode">是否为设计时模式</param>
        public static void SetDesignMode(bool isDesignMode)
        {
            _isInDesignMode = isDesignMode;
            Debug.WriteLine($"Design mode manually set to: {isDesignMode}");
        }

        #endregion

        #region 设计时数据绑定

        /// <summary>
        /// 为元素设置设计时数据上下文
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="dataContext">数据上下文</param>
        public static void SetDesignTimeDataContext(FrameworkElement element, object dataContext)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (IsInDesignMode)
            {
                DesignTimeBinding.SetDesignTimeDataContext(element, dataContext);
                Debug.WriteLine($"Set design time data context for {element.GetType().Name}");
            }
        }

        /// <summary>
        /// 为元素设置设计时数据上下文（从类型生成）
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="viewModelType">ViewModel类型</param>
        public static void SetDesignTimeDataContext(FrameworkElement element, Type viewModelType)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (viewModelType == null)
                throw new ArgumentNullException(nameof(viewModelType));

            if (IsInDesignMode)
            {
                var designTimeContext = DesignTimeDataContext.CreateFor(viewModelType);
                DesignTimeBinding.SetDesignTimeDataContext(element, designTimeContext);
                Debug.WriteLine($"Set generated design time data context for {element.GetType().Name}");
            }
        }

        /// <summary>
        /// 为元素设置设计时数据上下文（泛型版本）
        /// </summary>
        /// <typeparam name="T">ViewModel类型</typeparam>
        /// <param name="element">UI元素</param>
        public static void SetDesignTimeDataContext<T>(FrameworkElement element) where T : class
        {
            SetDesignTimeDataContext(element, typeof(T));
        }

        /// <summary>
        /// 获取设计时属性值
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性值</returns>
        public static object? GetDesignTimeProperty(FrameworkElement element, string propertyName)
        {
            if (element == null || string.IsNullOrEmpty(propertyName))
                return null;

            if (!IsInDesignMode)
                return null;

            object? dataContext = DesignTimeBinding.GetDesignTimeDataContext(element);
            if (dataContext is DesignTimeDataContext designTimeContext)
            {
                return designTimeContext.GetDesignTimeProperty(propertyName);
            }

            return null;
        }

        /// <summary>
        /// 设置设计时属性值
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">属性值</param>
        public static void SetDesignTimeProperty(FrameworkElement element, string propertyName, object? value)
        {
            if (element == null || string.IsNullOrEmpty(propertyName))
                return;

            if (!IsInDesignMode)
                return;

            // 如果没有设计时数据上下文，创建一个
            object? dataContext = DesignTimeBinding.GetDesignTimeDataContext(element);
            if (!(dataContext is DesignTimeDataContext designTimeContext))
            {
                designTimeContext = new DesignTimeDataContext();
                DesignTimeBinding.SetDesignTimeDataContext(element, designTimeContext);
            }

            designTimeContext.SetDesignTimeProperty(propertyName, value);
        }

        #endregion

        #region 设计时资源

        /// <summary>
        /// 获取设计时资源
        /// </summary>
        /// <param name="resourceKey">资源键</param>
        /// <returns>资源值</returns>
        public static object? GetDesignTimeResource(string resourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey))
                return null;

            if (!IsInDesignMode)
                return null;

            // 这里可以实现设计时资源查找逻辑
            // 目前返回模拟值
            return resourceKey switch
            {
                "DesignTimeTitle" => "设计时标题",
                "DesignTimeText" => "设计时文本内容",
                "DesignTimeImage" => "/Images/DesignTime/placeholder.png",
                _ => $"设计时资源: {resourceKey}"
            };
        }

        /// <summary>
        /// 创建设计时命令
        /// </summary>
        /// <param name="commandName">命令名称</param>
        /// <returns>设计时命令</returns>
        public static ICommand CreateDesignTimeCommand(string commandName = "DesignTimeCommand")
        {
            if (!IsInDesignMode)
                return new RelayCommand(() => { });

            return new RelayCommand(
                execute: () => Debug.WriteLine($"Design time command executed: {commandName}"),
                canExecute: () => true);
        }

        #endregion

        #region 设计时样式

        /// <summary>
        /// 应用设计时样式
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="styleName">样式名称</param>
        public static void ApplyDesignTimeStyle(FrameworkElement element, string styleName)
        {
            if (element == null || string.IsNullOrEmpty(styleName))
                return;

            if (!IsInDesignMode)
                return;

            Debug.WriteLine($"Applying design time style '{styleName}' to {element.GetType().Name}");

            // 这里可以实现设计时样式应用逻辑
            // 目前只是记录日志
        }

        /// <summary>
        /// 设置设计时可见性
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="visibility">可见性</param>
        public static void SetDesignTimeVisibility(UIElement element, Visibility visibility)
        {
            if (element == null)
                return;

            if (IsInDesignMode)
            {
                element.Visibility = visibility;
                Debug.WriteLine($"Set design time visibility to {visibility} for {element.GetType().Name}");
            }
        }

        #endregion

        #region 设计时验证

        /// <summary>
        /// 验证设计时数据绑定
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <returns>验证结果</returns>
        public static bool ValidateDesignTimeBinding(FrameworkElement element)
        {
            if (element == null)
                return false;

            if (!IsInDesignMode)
                return true; // 运行时不验证

            try
            {
                // 检查数据上下文
                object? dataContext = DesignTimeBinding.GetDesignTimeDataContext(element);
                if (dataContext == null)
                {
                    Debug.WriteLine($"Warning: No DataContext set for {element.GetType().Name}");
                    return false;
                }

                // 检查绑定表达式（这里是简化实现）
                Debug.WriteLine($"Design time binding validation passed for {element.GetType().Name}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Design time binding validation failed: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
