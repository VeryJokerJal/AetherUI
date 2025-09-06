using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AetherUI.Input.Core;

namespace AetherUI.Input.Platform.Windows
{
    /// <summary>
    /// Windows平台输入提供者
    /// </summary>
    public class WindowsInputProvider : IPlatformInputProvider
    {
        private IntPtr _windowHandle;
        private IntPtr _originalWndProc;
        private Win32.WndProc? _wndProcDelegate;
        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// 原始输入事件
        /// </summary>
        public event EventHandler<RawInputEventArgs>? RawInputReceived;

        /// <summary>
        /// 初始化Windows输入提供者
        /// </summary>
        /// <param name="windowHandle">窗口句柄</param>
        public void Initialize(IntPtr windowHandle)
        {
            if (_isInitialized)
                throw new InvalidOperationException("WindowsInputProvider已经初始化");

            if (windowHandle == IntPtr.Zero)
                throw new ArgumentException("窗口句柄不能为空", nameof(windowHandle));

            _windowHandle = windowHandle;

            try
            {
                // 子类化窗口过程
                _wndProcDelegate = WndProc;
                _originalWndProc = Win32.SetWindowLongPtr(_windowHandle, Win32.GWLP_WNDPROC, 
                    Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));

                if (_originalWndProc == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error, $"SetWindowLongPtr失败: {error}");
                }

                // 注册原始输入设备
                RegisterRawInputDevices();

                _isInitialized = true;
                Debug.WriteLine("WindowsInputProvider初始化成功");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WindowsInputProvider初始化失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 关闭Windows输入提供者
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized || _isDisposed)
                return;

            try
            {
                // 恢复原始窗口过程
                if (_originalWndProc != IntPtr.Zero && _windowHandle != IntPtr.Zero)
                {
                    Win32.SetWindowLongPtr(_windowHandle, Win32.GWLP_WNDPROC, _originalWndProc);
                }

                _isInitialized = false;
                Debug.WriteLine("WindowsInputProvider已关闭");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WindowsInputProvider关闭失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取当前鼠标位置
        /// </summary>
        /// <returns>鼠标位置</returns>
        public Point GetMousePosition()
        {
            if (Win32.GetCursorPos(out Win32.POINT point))
            {
                // 转换为客户端坐标
                Win32.ScreenToClient(_windowHandle, ref point);
                return new Point(point.X, point.Y);
            }
            return Point.Zero;
        }

        /// <summary>
        /// 获取当前按键状态
        /// </summary>
        /// <param name="key">虚拟键码</param>
        /// <returns>是否按下</returns>
        public bool IsKeyPressed(VirtualKey key)
        {
            short state = Win32.GetAsyncKeyState((int)key);
            return (state & 0x8000) != 0;
        }

        /// <summary>
        /// 获取当前修饰键状态
        /// </summary>
        /// <returns>修饰键状态</returns>
        public ModifierKeys GetModifierKeys()
        {
            ModifierKeys modifiers = ModifierKeys.None;

            if (IsKeyPressed(VirtualKey.Control))
                modifiers |= ModifierKeys.Control;
            if (IsKeyPressed(VirtualKey.Shift))
                modifiers |= ModifierKeys.Shift;
            if (IsKeyPressed(VirtualKey.Alt))
                modifiers |= ModifierKeys.Alt;
            if (IsKeyPressed(VirtualKey.LWin) || IsKeyPressed(VirtualKey.RWin))
                modifiers |= ModifierKeys.Meta;

            return modifiers;
        }

        /// <summary>
        /// 设置鼠标捕获
        /// </summary>
        /// <param name="capture">是否捕获</param>
        public void SetMouseCapture(bool capture)
        {
            if (capture)
            {
                Win32.SetCapture(_windowHandle);
            }
            else
            {
                Win32.ReleaseCapture();
            }
        }

        /// <summary>
        /// 显示/隐藏鼠标光标
        /// </summary>
        /// <param name="visible">是否可见</param>
        public void SetCursorVisible(bool visible)
        {
            Win32.ShowCursor(visible);
        }

        /// <summary>
        /// 设置鼠标光标样式
        /// </summary>
        /// <param name="cursor">光标样式</param>
        public void SetCursor(SystemCursor cursor)
        {
            IntPtr hCursor = cursor switch
            {
                SystemCursor.Arrow => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_ARROW),
                SystemCursor.IBeam => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_IBEAM),
                SystemCursor.Wait => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_WAIT),
                SystemCursor.Cross => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_CROSS),
                SystemCursor.Hand => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_HAND),
                SystemCursor.No => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_NO),
                SystemCursor.SizeNS => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_SIZENS),
                SystemCursor.SizeWE => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_SIZEWE),
                SystemCursor.SizeNWSE => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_SIZENWSE),
                SystemCursor.SizeNESW => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_SIZENESW),
                SystemCursor.SizeAll => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_SIZEALL),
                SystemCursor.Help => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_HELP),
                _ => Win32.LoadCursor(IntPtr.Zero, Win32.IDC_ARROW)
            };

            Win32.SetCursor(hCursor);
        }

        /// <summary>
        /// 注册原始输入设备
        /// </summary>
        private void RegisterRawInputDevices()
        {
            var devices = new Win32.RAWINPUTDEVICE[]
            {
                // 鼠标
                new Win32.RAWINPUTDEVICE
                {
                    usUsagePage = 0x01, // HID_USAGE_PAGE_GENERIC
                    usUsage = 0x02,     // HID_USAGE_GENERIC_MOUSE
                    dwFlags = Win32.RIDEV_INPUTSINK,
                    hwndTarget = _windowHandle
                },
                // 键盘
                new Win32.RAWINPUTDEVICE
                {
                    usUsagePage = 0x01, // HID_USAGE_PAGE_GENERIC
                    usUsage = 0x06,     // HID_USAGE_GENERIC_KEYBOARD
                    dwFlags = Win32.RIDEV_INPUTSINK,
                    hwndTarget = _windowHandle
                }
            };

            if (!Win32.RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf<Win32.RAWINPUTDEVICE>()))
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error, $"注册原始输入设备失败: {error}");
            }
        }

        /// <summary>
        /// 窗口过程
        /// </summary>
        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                switch (msg)
                {
                    case Win32.WM_INPUT:
                        ProcessRawInput(lParam);
                        break;

                    case Win32.WM_MOUSEMOVE:
                    case Win32.WM_LBUTTONDOWN:
                    case Win32.WM_LBUTTONUP:
                    case Win32.WM_RBUTTONDOWN:
                    case Win32.WM_RBUTTONUP:
                    case Win32.WM_MBUTTONDOWN:
                    case Win32.WM_MBUTTONUP:
                    case Win32.WM_MOUSEWHEEL:
                    case Win32.WM_MOUSEHWHEEL:
                        ProcessMouseMessage(msg, wParam, lParam);
                        break;

                    case Win32.WM_KEYDOWN:
                    case Win32.WM_KEYUP:
                    case Win32.WM_SYSKEYDOWN:
                    case Win32.WM_SYSKEYUP:
                    case Win32.WM_CHAR:
                        ProcessKeyboardMessage(msg, wParam, lParam);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WndProc处理消息失败: {ex.Message}");
            }

            // 调用原始窗口过程
            return Win32.CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
        }

        /// <summary>
        /// 处理原始输入
        /// </summary>
        private void ProcessRawInput(IntPtr lParam)
        {
            // 获取原始输入数据大小
            uint size = 0;
            Win32.GetRawInputData(lParam, Win32.RID_INPUT, IntPtr.Zero, ref size, (uint)Marshal.SizeOf<Win32.RAWINPUTHEADER>());

            if (size == 0) return;

            // 分配缓冲区并获取数据
            IntPtr buffer = Marshal.AllocHGlobal((int)size);
            try
            {
                uint actualSize = Win32.GetRawInputData(lParam, Win32.RID_INPUT, buffer, ref size, (uint)Marshal.SizeOf<Win32.RAWINPUTHEADER>());
                if (actualSize != size) return;

                var rawInput = Marshal.PtrToStructure<Win32.RAWINPUT>(buffer);
                ProcessRawInputData(rawInput);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// 处理原始输入数据
        /// </summary>
        private void ProcessRawInputData(Win32.RAWINPUT rawInput)
        {
            uint timestamp = (uint)Environment.TickCount;

            switch (rawInput.header.dwType)
            {
                case Win32.RIM_TYPEMOUSE:
                    ProcessRawMouseData(rawInput.mouse, timestamp);
                    break;

                case Win32.RIM_TYPEKEYBOARD:
                    ProcessRawKeyboardData(rawInput.keyboard, timestamp);
                    break;
            }
        }

        /// <summary>
        /// 处理原始鼠标数据
        /// </summary>
        private void ProcessRawMouseData(Win32.RAWMOUSE mouse, uint timestamp)
        {
            var position = GetMousePosition();
            var delta = new Point(mouse.lLastX, mouse.lLastY);
            var buttonState = GetCurrentMouseButtonState();
            var buttonChanges = GetMouseButtonChanges(mouse.usButtonFlags);
            var wheelDelta = GetWheelDelta(mouse.usButtonFlags, mouse.usButtonData);
            var modifiers = GetModifierKeys();

            var rawEvent = new RawMouseEventArgs(
                timestamp,
                0, // 设备ID
                position,
                delta,
                buttonState,
                buttonChanges,
                wheelDelta,
                modifiers);

            RawInputReceived?.Invoke(this, rawEvent);
        }

        /// <summary>
        /// 处理原始键盘数据
        /// </summary>
        private void ProcessRawKeyboardData(Win32.RAWKEYBOARD keyboard, uint timestamp)
        {
            var key = (VirtualKey)keyboard.VKey;
            var scanCode = keyboard.MakeCode;
            var isPressed = (keyboard.Flags & Win32.RI_KEY_BREAK) == 0;
            var isRepeat = false; // 原始输入不提供重复信息
            var modifiers = GetModifierKeys();

            var rawEvent = new RawKeyboardEventArgs(
                timestamp,
                0, // 设备ID
                key,
                scanCode,
                isPressed,
                isRepeat,
                modifiers);

            RawInputReceived?.Invoke(this, rawEvent);
        }

        /// <summary>
        /// 处理鼠标消息
        /// </summary>
        private void ProcessMouseMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            // 这里可以处理标准的鼠标消息作为备用
            // 主要依赖原始输入，这里只是为了兼容性
        }

        /// <summary>
        /// 处理键盘消息
        /// </summary>
        private void ProcessKeyboardMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            // 处理字符输入消息
            if (msg == Win32.WM_CHAR)
            {
                char character = (char)wParam.ToInt32();
                // 这里可以触发文本输入事件
            }
        }

        /// <summary>
        /// 获取当前鼠标按钮状态
        /// </summary>
        private PointerButton GetCurrentMouseButtonState()
        {
            PointerButton buttons = PointerButton.None;

            if (IsKeyPressed(VirtualKey.LButton))
                buttons |= PointerButton.Primary;
            if (IsKeyPressed(VirtualKey.RButton))
                buttons |= PointerButton.Secondary;
            if (IsKeyPressed(VirtualKey.MButton))
                buttons |= PointerButton.Middle;
            if (IsKeyPressed(VirtualKey.XButton1))
                buttons |= PointerButton.X1;
            if (IsKeyPressed(VirtualKey.XButton2))
                buttons |= PointerButton.X2;

            return buttons;
        }

        /// <summary>
        /// 获取鼠标按钮变化
        /// </summary>
        private PointerButton GetMouseButtonChanges(ushort buttonFlags)
        {
            PointerButton changes = PointerButton.None;

            if ((buttonFlags & Win32.RI_MOUSE_LEFT_BUTTON_DOWN) != 0 || (buttonFlags & Win32.RI_MOUSE_LEFT_BUTTON_UP) != 0)
                changes |= PointerButton.Primary;
            if ((buttonFlags & Win32.RI_MOUSE_RIGHT_BUTTON_DOWN) != 0 || (buttonFlags & Win32.RI_MOUSE_RIGHT_BUTTON_UP) != 0)
                changes |= PointerButton.Secondary;
            if ((buttonFlags & Win32.RI_MOUSE_MIDDLE_BUTTON_DOWN) != 0 || (buttonFlags & Win32.RI_MOUSE_MIDDLE_BUTTON_UP) != 0)
                changes |= PointerButton.Middle;

            return changes;
        }

        /// <summary>
        /// 获取滚轮增量
        /// </summary>
        private Point GetWheelDelta(ushort buttonFlags, ushort buttonData)
        {
            double deltaX = 0, deltaY = 0;

            if ((buttonFlags & Win32.RI_MOUSE_WHEEL) != 0)
            {
                deltaY = (short)buttonData / 120.0; // WHEEL_DELTA = 120
            }
            else if ((buttonFlags & Win32.RI_MOUSE_HWHEEL) != 0)
            {
                deltaX = (short)buttonData / 120.0;
            }

            return new Point(deltaX, deltaY);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            Shutdown();
            _isDisposed = true;
        }
    }

    /// <summary>
    /// 虚拟键码扩展（Windows特定）
    /// </summary>
    public static class WindowsVirtualKeys
    {
        public const VirtualKey LButton = (VirtualKey)0x01;
        public const VirtualKey RButton = (VirtualKey)0x02;
        public const VirtualKey MButton = (VirtualKey)0x04;
        public const VirtualKey XButton1 = (VirtualKey)0x05;
        public const VirtualKey XButton2 = (VirtualKey)0x06;
        public const VirtualKey LWin = (VirtualKey)0x5B;
        public const VirtualKey RWin = (VirtualKey)0x5C;
    }
}
}
