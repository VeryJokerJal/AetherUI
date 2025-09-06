using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AetherUI.Core;
using AetherUI.Input.Core;
using AetherUI.Input.HitTesting;

namespace AetherUI.Integration.Input
{
    /// <summary>
    /// 布局元素适配器 - 将UIElement适配为IHitTestable
    /// </summary>
    public class LayoutElementAdapter : IHitTestable, IDisposable
    {
        private readonly UIElement _element;
        private readonly LayoutInputAdapter _inputAdapter;
        private readonly List<LayoutElementAdapter> _childAdapters = new();
        private bool _isDisposed;
        private Rect _cachedBounds;
        private bool _boundsValid;

        /// <summary>
        /// 关联的UI元素
        /// </summary>
        public UIElement Element => _element;

        /// <summary>
        /// 输入适配器
        /// </summary>
        public LayoutInputAdapter InputAdapter => _inputAdapter;

        /// <summary>
        /// 初始化布局元素适配器
        /// </summary>
        /// <param name="element">UI元素</param>
        /// <param name="inputAdapter">输入适配器</param>
        public LayoutElementAdapter(UIElement element, LayoutInputAdapter inputAdapter)
        {
            _element = element ?? throw new ArgumentNullException(nameof(element));
            _inputAdapter = inputAdapter ?? throw new ArgumentNullException(nameof(inputAdapter));

            // 初始化子适配器
            UpdateChildAdapters();

            Debug.WriteLine($"布局元素适配器已创建: {_element.GetType().Name}");
        }

        #region IHitTestable 实现

        /// <summary>
        /// 边界矩形
        /// </summary>
        public Rect Bounds
        {
            get
            {
                if (!_boundsValid)
                {
                    UpdateBounds();
                }
                return _cachedBounds;
            }
        }

        /// <summary>
        /// 裁剪边界
        /// </summary>
        public Rect? ClipBounds => null; // 布局系统暂不支持裁剪

        /// <summary>
        /// Z索引
        /// </summary>
        public int ZIndex => 0; // 布局系统暂不支持Z索引

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible => _element.Visibility == Visibility.Visible;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled => _element.IsEnabled;

        /// <summary>
        /// 变换矩阵
        /// </summary>
        public AetherUI.Input.Core.Transform Transform => AetherUI.Input.Core.Transform.Identity; // 布局系统暂不支持变换

        /// <summary>
        /// 子元素
        /// </summary>
        public IEnumerable<IHitTestable> Children => _childAdapters.Cast<IHitTestable>();

        /// <summary>
        /// 命中测试
        /// </summary>
        /// <param name="point">测试点</param>
        /// <returns>是否命中</returns>
        public bool HitTest(AetherUI.Input.Core.Point point)
        {
            if (!IsVisible || !IsEnabled)
                return false;

            // 转换输入系统的Point到布局系统的Point
            var layoutPoint = new Point(point.X, point.Y);
            return Bounds.Contains(layoutPoint);
        }

        /// <summary>
        /// 详细命中测试
        /// </summary>
        /// <param name="point">测试点</param>
        /// <param name="context">命中测试上下文</param>
        /// <returns>命中测试结果</returns>
        public HitTestResult HitTestDetailed(AetherUI.Input.Core.Point point, HitTestContext context)
        {
            if (!HitTest(point))
            {
                return HitTestResult.Miss;
            }

            // 创建命中结果
            var layoutPoint = new Point(point.X, point.Y);
            return new HitTestResult
            {
                IsHit = true,
                HitElement = this,
                HitPoint = point,
                Distance = 0, // 布局系统中距离为0
                HitPath = new List<IHitTestable> { this }
            };
        }

        #endregion

        /// <summary>
        /// 布局更新时调用
        /// </summary>
        public void OnLayoutUpdated()
        {
            // 标记边界无效，需要重新计算
            _boundsValid = false;

            // 更新子适配器
            UpdateChildAdapters();

            Debug.WriteLine($"布局元素适配器布局已更新: {_element.GetType().Name}");
        }

        /// <summary>
        /// 更新边界
        /// </summary>
        private void UpdateBounds()
        {
            // 从UI元素获取渲染边界
            _cachedBounds = _element.RenderBounds;
            _boundsValid = true;
        }

        /// <summary>
        /// 更新子适配器
        /// </summary>
        private void UpdateChildAdapters()
        {
            // 清理旧的子适配器
            foreach (var childAdapter in _childAdapters)
            {
                childAdapter.Dispose();
            }
            _childAdapters.Clear();

            // 为每个可见子元素创建适配器
            foreach (var child in _element.GetVisualChildren())
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    var childAdapter = _inputAdapter.GetOrCreateElementAdapter(child);
                    _childAdapters.Add(childAdapter);
                }
            }
        }

        /// <summary>
        /// 获取元素的输入属性
        /// </summary>
        /// <returns>输入属性</returns>
        public LayoutElementInputProperties GetInputProperties()
        {
            return new LayoutElementInputProperties
            {
                IsHitTestVisible = _element.IsHitTestVisible,
                IsEnabled = _element.IsEnabled,
                IsFocusable = _element.Focusable,
                AcceptsInput = GetAcceptsInput(),
                SupportedGestures = GetSupportedGestures(),
                InputScope = GetInputScope()
            };
        }

        /// <summary>
        /// 检查元素是否接受输入
        /// </summary>
        /// <returns>是否接受输入</returns>
        private bool GetAcceptsInput()
        {
            // 根据元素类型判断是否接受输入
            return _element switch
            {
                // 这里可以根据具体的控件类型来判断
                // 例如：Button, TextBox, etc.
                _ => _element.IsHitTestVisible && _element.IsEnabled
            };
        }

        /// <summary>
        /// 获取支持的手势
        /// </summary>
        /// <returns>支持的手势列表</returns>
        private List<string> GetSupportedGestures()
        {
            var gestures = new List<string>();

            // 基本手势支持
            if (_element.IsHitTestVisible && _element.IsEnabled)
            {
                gestures.Add("Tap");
                gestures.Add("DoubleTap");
                gestures.Add("LongPress");
            }

            // 根据元素类型添加特定手势
            // 例如：ScrollViewer支持Pan和Pinch等

            return gestures;
        }

        /// <summary>
        /// 获取输入范围
        /// </summary>
        /// <returns>输入范围</returns>
        private string GetInputScope()
        {
            // 根据元素类型返回输入范围
            return _element switch
            {
                // TextBox => "Text",
                // PasswordBox => "Password",
                // NumericUpDown => "Number",
                _ => "Default"
            };
        }

        /// <summary>
        /// 处理输入事件
        /// </summary>
        /// <param name="inputEvent">输入事件</param>
        public void HandleInputEvent(AetherUI.Input.Events.InputEvent inputEvent)
        {
            try
            {
                // 将输入事件转发给布局输入适配器
                _inputAdapter.RaiseInputEvent(_element, inputEvent);

                // 根据事件类型进行特定处理
                switch (inputEvent)
                {
                    case AetherUI.Input.Events.PointerEvent pointerEvent:
                        HandlePointerEvent(pointerEvent);
                        break;

                    case AetherUI.Input.Events.KeyboardEvent keyboardEvent:
                        HandleKeyboardEvent(keyboardEvent);
                        break;

                    case AetherUI.Input.Events.TextInputEvent textInputEvent:
                        HandleTextInputEvent(textInputEvent);
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
        /// <param name="pointerEvent">指针事件</param>
        private void HandlePointerEvent(AetherUI.Input.Events.PointerEvent pointerEvent)
        {
            // 这里可以将指针事件转换为布局系统的事件
            // 例如：MouseDown, MouseUp, MouseMove等
            Debug.WriteLine($"处理指针事件: {pointerEvent.EventType} at {pointerEvent.Position}");
        }

        /// <summary>
        /// 处理键盘事件
        /// </summary>
        /// <param name="keyboardEvent">键盘事件</param>
        private void HandleKeyboardEvent(AetherUI.Input.Events.KeyboardEvent keyboardEvent)
        {
            // 这里可以将键盘事件转换为布局系统的事件
            // 例如：KeyDown, KeyUp等
            Debug.WriteLine($"处理键盘事件: {keyboardEvent.EventType} key {keyboardEvent.Key}");
        }

        /// <summary>
        /// 处理文本输入事件
        /// </summary>
        /// <param name="textInputEvent">文本输入事件</param>
        private void HandleTextInputEvent(AetherUI.Input.Events.TextInputEvent textInputEvent)
        {
            // 这里可以将文本输入事件转换为布局系统的事件
            Debug.WriteLine($"处理文本输入事件: {textInputEvent.EventType} text '{textInputEvent.Text}'");
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            // 清理子适配器
            foreach (var childAdapter in _childAdapters)
            {
                childAdapter.Dispose();
            }
            _childAdapters.Clear();

            Debug.WriteLine($"布局元素适配器已释放: {_element.GetType().Name}");
        }

        public override string ToString() => $"LayoutAdapter({_element.GetType().Name})";
    }

    /// <summary>
    /// 布局元素输入属性
    /// </summary>
    public class LayoutElementInputProperties
    {
        /// <summary>
        /// 是否可命中测试
        /// </summary>
        public bool IsHitTestVisible { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否可获得焦点
        /// </summary>
        public bool IsFocusable { get; set; }

        /// <summary>
        /// 是否接受输入
        /// </summary>
        public bool AcceptsInput { get; set; }

        /// <summary>
        /// 支持的手势
        /// </summary>
        public List<string> SupportedGestures { get; set; } = new();

        /// <summary>
        /// 输入范围
        /// </summary>
        public string InputScope { get; set; } = "Default";

        public override string ToString() =>
            $"HitTest: {IsHitTestVisible}, Enabled: {IsEnabled}, Focusable: {IsFocusable}, " +
            $"AcceptsInput: {AcceptsInput}, Gestures: {SupportedGestures.Count}";
    }
}
