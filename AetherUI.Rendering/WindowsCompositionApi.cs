using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AetherUI.Rendering
{
    /// <summary>
    /// Windows组合效果类型
    /// </summary>
    public enum WindowCompositionType
    {
        /// <summary>
        /// 禁用组合效果
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// 亚克力效果（Windows 10 Fall Creators Update及以上）
        /// </summary>
        Acrylic = 1,

        /// <summary>
        /// 云母效果（Windows 11及以上）
        /// </summary>
        Mica = 2,

        /// <summary>
        /// 云母Alt效果（Windows 11及以上）
        /// </summary>
        MicaAlt = 3
    }

    /// <summary>
    /// DWM窗口属性
    /// </summary>
    public enum DwmWindowAttribute
    {
        /// <summary>
        /// 使用沉浸式暗色模式
        /// </summary>
        UseImmersiveDarkMode = 20,

        /// <summary>
        /// 窗口圆角偏好
        /// </summary>
        WindowCornerPreference = 33,

        /// <summary>
        /// 系统背景类型
        /// </summary>
        SystemBackdropType = 38
    }

    /// <summary>
    /// 窗口圆角偏好
    /// </summary>
    public enum WindowCornerPreference
    {
        /// <summary>
        /// 默认圆角
        /// </summary>
        Default = 0,

        /// <summary>
        /// 不使用圆角
        /// </summary>
        DoNotRound = 1,

        /// <summary>
        /// 圆角
        /// </summary>
        Round = 2,

        /// <summary>
        /// 小圆角
        /// </summary>
        RoundSmall = 3
    }

    /// <summary>
    /// 系统背景类型
    /// </summary>
    public enum SystemBackdropType
    {
        /// <summary>
        /// 自动
        /// </summary>
        Auto = 0,

        /// <summary>
        /// 无
        /// </summary>
        None = 1,

        /// <summary>
        /// 云母
        /// </summary>
        Mica = 2,

        /// <summary>
        /// 亚克力
        /// </summary>
        Acrylic = 3,

        /// <summary>
        /// 云母Alt
        /// </summary>
        MicaAlt = 4
    }

    /// <summary>
    /// 窗口组合属性数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    /// <summary>
    /// 窗口组合属性
    /// </summary>
    public enum WindowCompositionAttribute
    {
        /// <summary>
        /// 启用模糊背景
        /// </summary>
        AccentPolicy = 19
    }

    /// <summary>
    /// 重音策略
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AccentPolicy
    {
        public AccentState AccentState;
        public AccentFlags AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    /// <summary>
    /// 重音状态
    /// </summary>
    public enum AccentState
    {
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// 启用渐变
        /// </summary>
        EnableGradient = 1,

        /// <summary>
        /// 启用透明渐变
        /// </summary>
        EnableTransparentGradient = 2,

        /// <summary>
        /// 启用模糊背景
        /// </summary>
        EnableBlurBehind = 3,

        /// <summary>
        /// 启用亚克力模糊背景
        /// </summary>
        EnableAcrylicBlurBehind = 4,

        /// <summary>
        /// 启用主机背景
        /// </summary>
        EnableHostBackdrop = 5
    }

    /// <summary>
    /// 重音标志
    /// </summary>
    [Flags]
    public enum AccentFlags
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,

        /// <summary>
        /// 绘制左边框
        /// </summary>
        DrawLeftBorder = 0x20,

        /// <summary>
        /// 绘制上边框
        /// </summary>
        DrawTopBorder = 0x40,

        /// <summary>
        /// 绘制右边框
        /// </summary>
        DrawRightBorder = 0x80,

        /// <summary>
        /// 绘制下边框
        /// </summary>
        DrawBottomBorder = 0x100,

        /// <summary>
        /// 绘制所有边框
        /// </summary>
        DrawAllBorders = DrawLeftBorder | DrawTopBorder | DrawRightBorder | DrawBottomBorder
    }

    /// <summary>
    /// Windows组合API包装器
    /// </summary>
    public static class WindowsCompositionApi
    {
        #region Windows API声明

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attribute, ref int attributeValue, int attributeSize);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attribute, ref bool attributeValue, int attributeSize);

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);

        [DllImport("kernel32.dll")]
        private static extern uint GetVersion();

        #endregion

        #region 系统版本检测

        /// <summary>
        /// 检查是否支持DWM组合
        /// </summary>
        /// <returns>是否支持</returns>
        public static bool IsCompositionEnabled()
        {
            try
            {
                DwmIsCompositionEnabled(out bool enabled);
                return enabled;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查是否为Windows 10或更高版本
        /// </summary>
        /// <returns>是否为Windows 10+</returns>
        public static bool IsWindows10OrLater()
        {
            try
            {
                var version = Environment.OSVersion.Version;
                return version.Major >= 10;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查是否为Windows 11或更高版本
        /// </summary>
        /// <returns>是否为Windows 11+</returns>
        public static bool IsWindows11OrLater()
        {
            try
            {
                var version = Environment.OSVersion.Version;
                return version.Major >= 10 && version.Build >= 22000;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 背景效果设置

        /// <summary>
        /// 设置窗口背景效果
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <param name="effectType">效果类型</param>
        /// <param name="opacity">透明度（0.0-1.0）</param>
        /// <param name="tintColor">色调颜色</param>
        /// <returns>是否设置成功</returns>
        public static bool SetWindowBackgroundEffect(IntPtr windowHandle, WindowCompositionType effectType, float opacity = 0.8f, uint tintColor = 0x00FFFFFF)
        {
            if (windowHandle == IntPtr.Zero || !IsCompositionEnabled())
            {
return false;
            }

            try
            {
                switch (effectType)
                {
                    case WindowCompositionType.Disabled:
                        return DisableBackgroundEffect(windowHandle);

                    case WindowCompositionType.Acrylic:
                        return SetAcrylicEffect(windowHandle, opacity, tintColor);

                    case WindowCompositionType.Mica:
                        return SetMicaEffect(windowHandle);

                    case WindowCompositionType.MicaAlt:
                        return SetMicaAltEffect(windowHandle);

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
return false;
            }
        }

        /// <summary>
        /// 禁用背景效果
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <returns>是否成功</returns>
        private static bool DisableBackgroundEffect(IntPtr windowHandle)
        {
            try
            {
                // 尝试使用新的DWM API（Windows 11）
                if (IsWindows11OrLater())
                {
                    int backdropType = (int)SystemBackdropType.None;
                    DwmSetWindowAttribute(windowHandle, DwmWindowAttribute.SystemBackdropType, ref backdropType, sizeof(int));
                }

                // 使用旧的组合API
                var accent = new AccentPolicy
                {
                    AccentState = AccentState.Disabled,
                    AccentFlags = AccentFlags.None,
                    GradientColor = 0,
                    AnimationId = 0
                };

                var accentPtr = Marshal.AllocHGlobal(Marshal.SizeOf(accent));
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttributeData
                {
                    Attribute = WindowCompositionAttribute.AccentPolicy,
                    Data = accentPtr,
                    SizeOfData = Marshal.SizeOf(accent)
                };

                int result = SetWindowCompositionAttribute(windowHandle, ref data);
                Marshal.FreeHGlobal(accentPtr);

                return result == 1;
            }
            catch (Exception ex)
            {
return false;
            }
        }

        /// <summary>
        /// 设置亚克力效果
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <param name="opacity">透明度</param>
        /// <param name="tintColor">色调颜色</param>
        /// <returns>是否成功</returns>
        private static bool SetAcrylicEffect(IntPtr windowHandle, float opacity, uint tintColor)
        {
            try
            {
                // 尝试使用新的DWM API（Windows 11）
                if (IsWindows11OrLater())
                {
                    int backdropType = (int)SystemBackdropType.Acrylic;
                    DwmSetWindowAttribute(windowHandle, DwmWindowAttribute.SystemBackdropType, ref backdropType, sizeof(int));
                    return true;
                }

                // 使用旧的组合API（Windows 10）
                if (IsWindows10OrLater())
                {
                    // 计算ARGB颜色值
                    byte alpha = (byte)(opacity * 255);
                    uint gradientColor = (uint)(alpha << 24) | (tintColor & 0x00FFFFFF);

                    var accent = new AccentPolicy
                    {
                        AccentState = AccentState.EnableAcrylicBlurBehind,
                        AccentFlags = AccentFlags.None,
                        GradientColor = gradientColor,
                        AnimationId = 0
                    };

                    var accentPtr = Marshal.AllocHGlobal(Marshal.SizeOf(accent));
                    Marshal.StructureToPtr(accent, accentPtr, false);

                    var data = new WindowCompositionAttributeData
                    {
                        Attribute = WindowCompositionAttribute.AccentPolicy,
                        Data = accentPtr,
                        SizeOfData = Marshal.SizeOf(accent)
                    };

                    int result = SetWindowCompositionAttribute(windowHandle, ref data);
                    Marshal.FreeHGlobal(accentPtr);

                    return result == 1;
                }

                return false;
            }
            catch (Exception ex)
            {
return false;
            }
        }

        /// <summary>
        /// 设置云母效果
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <returns>是否成功</returns>
        private static bool SetMicaEffect(IntPtr windowHandle)
        {
            try
            {
                if (!IsWindows11OrLater())
                {
return false;
                }

                int backdropType = (int)SystemBackdropType.Mica;
                DwmSetWindowAttribute(windowHandle, DwmWindowAttribute.SystemBackdropType, ref backdropType, sizeof(int));
                return true;
            }
            catch (Exception ex)
            {
return false;
            }
        }

        /// <summary>
        /// 设置云母Alt效果
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <returns>是否成功</returns>
        private static bool SetMicaAltEffect(IntPtr windowHandle)
        {
            try
            {
                if (!IsWindows11OrLater())
                {
return false;
                }

                int backdropType = (int)SystemBackdropType.MicaAlt;
                DwmSetWindowAttribute(windowHandle, DwmWindowAttribute.SystemBackdropType, ref backdropType, sizeof(int));
                return true;
            }
            catch (Exception ex)
            {
return false;
            }
        }

        /// <summary>
        /// 设置窗口圆角
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <param name="cornerPreference">圆角偏好</param>
        /// <returns>是否成功</returns>
        public static bool SetWindowCornerPreference(IntPtr windowHandle, WindowCornerPreference cornerPreference)
        {
            try
            {
                if (!IsWindows11OrLater())
                {
                    return false;
                }

                int preference = (int)cornerPreference;
                DwmSetWindowAttribute(windowHandle, DwmWindowAttribute.WindowCornerPreference, ref preference, sizeof(int));
                return true;
            }
            catch (Exception ex)
            {
return false;
            }
        }

        /// <summary>
        /// 设置暗色模式
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        /// <param name="useDarkMode">是否使用暗色模式</param>
        /// <returns>是否成功</returns>
        public static bool SetDarkMode(IntPtr windowHandle, bool useDarkMode)
        {
            try
            {
                if (!IsWindows10OrLater())
                {
                    return false;
                }

                DwmSetWindowAttribute(windowHandle, DwmWindowAttribute.UseImmersiveDarkMode, ref useDarkMode, sizeof(bool));
                return true;
            }
            catch (Exception ex)
            {
return false;
            }
        }

        #endregion
    }
}
