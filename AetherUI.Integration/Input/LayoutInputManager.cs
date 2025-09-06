using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AetherUI.Core;
using AetherUI.Input.Events;

namespace AetherUI.Integration.Input
{
    /// <summary>
    /// 布局输入管理器 - 管理整个布局系统的输入处理
    /// </summary>
    public class LayoutInputManager : IDisposable
    {
        private readonly LayoutInputAdapter _inputAdapter;
        private readonly FocusManager _focusManager;
        private readonly Dictionary<UIElement, InputElementState> _elementStates = new();
        private UIElement? _mouseOverElement;
        private UIElement? _capturedElement;
        private bool _isDisposed;

        /// <summary>
        /// 输入适配器
        /// </summary>
        public LayoutInputAdapter InputAdapter => _inputAdapter;

        /// <summary>
        /// 焦点管理器
        /// </summary>
        public FocusManager FocusManager => _focusManager;

        /// <summary>
        /// 当前鼠标悬停元素
        /// </summary>
        public UIElement? MouseOverElement => _mouseOverElement;

        /// <summary>
        /// 当前捕获鼠标的元素
        /// </summary>
        public UIElement? CapturedElement => _capturedElement;

        /// <summary>
        /// 初始化布局输入管理器
        /// </summary>
        /// <param name="configuration">配置</param>
        public LayoutInputManager(LayoutInputConfiguration? configuration = null)
        {
            _inputAdapter = new LayoutInputAdapter(configuration);
            _focusManager = new FocusManager();

            // 订阅输入适配器事件
            _inputAdapter.InputEvent += OnInputEvent;

            Debug.WriteLine("布局输入管理器已初始化");
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        /// <returns>是否成功初始化</returns>
        public async Task<bool> InitializeAsync()
        {
            var success = await _inputAdapter.InitializeAsync();
            if (success)
            {
                Debug.WriteLine("布局输入管理器初始化成功");
            }
            else
            {
                Debug.WriteLine("布局输入管理器初始化失败");
            }
            return success;
        }

        /// <summary>
        /// 设置根元素
        /// </summary>
        /// <param name="rootElement">根元素</param>
        public void SetRootElement(UIElement rootElement)
        {
            _inputAdapter.SetRootElement(rootElement);
            _focusManager.SetRootElement(rootElement);
        }

        /// <summary>
        /// 捕获鼠标
        /// </summary>
        /// <param name="element">要捕获鼠标的元素</param>
        /// <returns>是否成功捕获</returns>
        public bool CaptureMouse(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (_capturedElement == element)
                return true;

            // 释放之前的捕获
            if (_capturedElement != null)
            {
                ReleaseMouse();
            }

            _capturedElement = element;
            Debug.WriteLine($"鼠标已被捕获: {element.GetType().Name}");
            return true;
        }

        /// <summary>
        /// 释放鼠标捕获
        /// </summary>
        public void ReleaseMouse()
        {
            if (_capturedElement != null)
            {
                var element = _capturedElement;
                _capturedElement = null;
                Debug.WriteLine($"鼠标捕获已释放: {element.GetType().Name}");
            }
        }

        /// <summary>
        /// 设置焦点
        /// </summary>
        /// <param name="element">要获得焦点的元素</param>
        /// <returns>是否成功设置焦点</returns>
        public bool SetFocus(UIElement? element)
        {
            return _focusManager.SetFocus(element);
        }

        /// <summary>
        /// 获取元素状态
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>元素状态</returns>
        public InputElementState GetElementState(UIElement element)
        {
            if (!_elementStates.TryGetValue(element, out InputElementState? state))
            {
                state = new InputElementState();
                _elementStates[element] = state;
            }
            return state;
        }

        /// <summary>
        /// 处理输入事件
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        private void OnInputEvent(object? sender, LayoutInputEventArgs e)
        {
            try
            {
                switch (e.InputEvent)
                {
                    case PointerEvent pointerEvent:
                        HandlePointerEvent(e.TargetElement, pointerEvent);
                        break;

                    case KeyboardEvent keyboardEvent:
                        HandleKeyboardEvent(e.TargetElement, keyboardEvent);
                        break;

                    case TextInputEvent textInputEvent:
                        HandleTextInputEvent(e.TargetElement, textInputEvent);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理输入事件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理指针事件
        /// </summary>
        /// <param name="targetElement">目标元素</param>
        /// <param name="pointerEvent">指针事件</param>
        private void HandlePointerEvent(UIElement targetElement, PointerEvent pointerEvent)
        {
            switch (pointerEvent.EventType)
            {
                case PointerEventType.Moved:
                    HandleMouseMove(targetElement, pointerEvent);
                    break;

                case PointerEventType.Pressed:
                    HandleMouseDown(targetElement, pointerEvent);
                    break;

                case PointerEventType.Released:
                    HandleMouseUp(targetElement, pointerEvent);
                    break;

                case PointerEventType.Entered:
                    HandleMouseEnter(targetElement, pointerEvent);
                    break;

                case PointerEventType.Exited:
                    HandleMouseLeave(targetElement, pointerEvent);
                    break;
            }
        }

        /// <summary>
        /// 处理鼠标移动
        /// </summary>
        private void HandleMouseMove(UIElement targetElement, PointerEvent pointerEvent)
        {
            // 更新鼠标悬停元素
            if (_mouseOverElement != targetElement)
            {
                if (_mouseOverElement != null)
                {
                    HandleMouseLeave(_mouseOverElement, pointerEvent);
                }

                _mouseOverElement = targetElement;
                HandleMouseEnter(targetElement, pointerEvent);
            }

            // 触发鼠标移动事件
            var mouseArgs = InputEventConverter.ConvertToMouseEventArgs(pointerEvent);
            targetElement.MouseMove?.Invoke(targetElement, mouseArgs);

            // 更新元素状态
            var state = GetElementState(targetElement);
            state.LastMousePosition = new Point(pointerEvent.Position.X, pointerEvent.Position.Y);
        }

        /// <summary>
        /// 处理鼠标按下
        /// </summary>
        private void HandleMouseDown(UIElement targetElement, PointerEvent pointerEvent)
        {
            // 设置焦点
            if (targetElement.Focusable)
            {
                SetFocus(targetElement);
            }

            // 触发鼠标按下事件
            var mouseButtonArgs = InputEventConverter.ConvertToMouseButtonEventArgs(pointerEvent);
            targetElement.MouseDown?.Invoke(targetElement, mouseButtonArgs);

            // 更新元素状态
            var state = GetElementState(targetElement);
            state.IsMouseDown = true;
            state.MouseDownPosition = new Point(pointerEvent.Position.X, pointerEvent.Position.Y);
        }

        /// <summary>
        /// 处理鼠标抬起
        /// </summary>
        private void HandleMouseUp(UIElement targetElement, PointerEvent pointerEvent)
        {
            // 触发鼠标抬起事件
            var mouseButtonArgs = InputEventConverter.ConvertToMouseButtonEventArgs(pointerEvent);
            targetElement.MouseUp?.Invoke(targetElement, mouseButtonArgs);

            // 更新元素状态
            var state = GetElementState(targetElement);
            state.IsMouseDown = false;

            // 释放鼠标捕获（如果是这个元素捕获的）
            if (_capturedElement == targetElement)
            {
                ReleaseMouse();
            }
        }

        /// <summary>
        /// 处理鼠标进入
        /// </summary>
        private void HandleMouseEnter(UIElement targetElement, PointerEvent pointerEvent)
        {
            var mouseArgs = InputEventConverter.ConvertToMouseEventArgs(pointerEvent);
            targetElement.MouseEnter?.Invoke(targetElement, mouseArgs);

            var state = GetElementState(targetElement);
            state.IsMouseOver = true;
        }

        /// <summary>
        /// 处理鼠标离开
        /// </summary>
        private void HandleMouseLeave(UIElement targetElement, PointerEvent pointerEvent)
        {
            var mouseArgs = InputEventConverter.ConvertToMouseEventArgs(pointerEvent);
            targetElement.MouseLeave?.Invoke(targetElement, mouseArgs);

            var state = GetElementState(targetElement);
            state.IsMouseOver = false;
        }

        /// <summary>
        /// 处理键盘事件
        /// </summary>
        /// <param name="targetElement">目标元素</param>
        /// <param name="keyboardEvent">键盘事件</param>
        private void HandleKeyboardEvent(UIElement targetElement, KeyboardEvent keyboardEvent)
        {
            var keyArgs = InputEventConverter.ConvertToKeyEventArgs(keyboardEvent);

            switch (keyboardEvent.EventType)
            {
                case KeyboardEventType.KeyDown:
                    targetElement.KeyDown?.Invoke(targetElement, keyArgs);
                    break;

                case KeyboardEventType.KeyUp:
                    targetElement.KeyUp?.Invoke(targetElement, keyArgs);
                    break;
            }
        }

        /// <summary>
        /// 处理文本输入事件
        /// </summary>
        /// <param name="targetElement">目标元素</param>
        /// <param name="textInputEvent">文本输入事件</param>
        private void HandleTextInputEvent(UIElement targetElement, TextInputEvent textInputEvent)
        {
            // 这里可以处理文本输入事件
            // 例如：转发给文本控件
            Debug.WriteLine($"文本输入: '{textInputEvent.Text}' -> {targetElement.GetType().Name}");
        }

        /// <summary>
        /// 获取诊断信息
        /// </summary>
        /// <returns>诊断信息</returns>
        public string GetDiagnosticInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("=== 布局输入管理器诊断信息 ===");
            info.AppendLine($"鼠标悬停元素: {_mouseOverElement?.GetType().Name ?? "无"}");
            info.AppendLine($"鼠标捕获元素: {_capturedElement?.GetType().Name ?? "无"}");
            info.AppendLine($"焦点元素: {_focusManager.FocusedElement?.GetType().Name ?? "无"}");
            info.AppendLine($"跟踪元素数量: {_elementStates.Count}");
            info.AppendLine();

            info.AppendLine(_inputAdapter.GenerateDiagnosticReport());

            return info.ToString();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _elementStates.Clear();
            _inputAdapter?.Dispose();
            _focusManager?.Dispose();

            Debug.WriteLine("布局输入管理器已释放");
        }
    }

    /// <summary>
    /// 输入元素状态
    /// </summary>
    public class InputElementState
    {
        /// <summary>
        /// 是否鼠标悬停
        /// </summary>
        public bool IsMouseOver { get; set; }

        /// <summary>
        /// 是否鼠标按下
        /// </summary>
        public bool IsMouseDown { get; set; }

        /// <summary>
        /// 最后鼠标位置
        /// </summary>
        public Point LastMousePosition { get; set; }

        /// <summary>
        /// 鼠标按下位置
        /// </summary>
        public Point MouseDownPosition { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;

        public override string ToString() =>
            $"MouseOver: {IsMouseOver}, MouseDown: {IsMouseDown}, LastPos: {LastMousePosition}";
    }

    /// <summary>
    /// 焦点管理器
    /// </summary>
    public class FocusManager : IDisposable
    {
        private UIElement? _focusedElement;
        private UIElement? _rootElement;

        /// <summary>
        /// 当前焦点元素
        /// </summary>
        public UIElement? FocusedElement => _focusedElement;

        /// <summary>
        /// 根元素
        /// </summary>
        public UIElement? RootElement => _rootElement;

        /// <summary>
        /// 设置根元素
        /// </summary>
        /// <param name="rootElement">根元素</param>
        public void SetRootElement(UIElement rootElement)
        {
            _rootElement = rootElement;
        }

        /// <summary>
        /// 设置焦点
        /// </summary>
        /// <param name="element">要获得焦点的元素</param>
        /// <returns>是否成功设置焦点</returns>
        public bool SetFocus(UIElement? element)
        {
            if (_focusedElement == element)
                return true;

            // 移除旧焦点
            if (_focusedElement != null)
            {
                _focusedElement.SetFocused(false);
            }

            // 设置新焦点
            _focusedElement = element;
            if (_focusedElement != null && _focusedElement.Focusable)
            {
                _focusedElement.SetFocused(true);
                Debug.WriteLine($"焦点已设置: {_focusedElement.GetType().Name}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 移动焦点到下一个元素
        /// </summary>
        /// <returns>是否成功移动焦点</returns>
        public bool MoveFocusNext()
        {
            if (_rootElement == null)
                return false;

            var focusableElements = GetFocusableElements(_rootElement).ToList();
            if (focusableElements.Count == 0)
                return false;

            int currentIndex = _focusedElement != null ? focusableElements.IndexOf(_focusedElement) : -1;
            int nextIndex = (currentIndex + 1) % focusableElements.Count;

            return SetFocus(focusableElements[nextIndex]);
        }

        /// <summary>
        /// 移动焦点到上一个元素
        /// </summary>
        /// <returns>是否成功移动焦点</returns>
        public bool MoveFocusPrevious()
        {
            if (_rootElement == null)
                return false;

            var focusableElements = GetFocusableElements(_rootElement).ToList();
            if (focusableElements.Count == 0)
                return false;

            int currentIndex = _focusedElement != null ? focusableElements.IndexOf(_focusedElement) : 0;
            int previousIndex = currentIndex > 0 ? currentIndex - 1 : focusableElements.Count - 1;

            return SetFocus(focusableElements[previousIndex]);
        }

        /// <summary>
        /// 获取可获得焦点的元素
        /// </summary>
        /// <param name="element">起始元素</param>
        /// <returns>可获得焦点的元素</returns>
        private IEnumerable<UIElement> GetFocusableElements(UIElement element)
        {
            if (element.Focusable && element.IsEnabled && element.Visibility == Visibility.Visible)
            {
                yield return element;
            }

            foreach (var child in element.GetVisualChildren())
            {
                foreach (var focusableChild in GetFocusableElements(child))
                {
                    yield return focusableChild;
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _focusedElement = null;
            _rootElement = null;
        }
    }
}
