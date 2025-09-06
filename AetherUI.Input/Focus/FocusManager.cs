using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AetherUI.Input.Focus
{
    /// <summary>
    /// 焦点管理器实现
    /// </summary>
    public class FocusManager : IFocusManager
    {
        private readonly Dictionary<string, IFocusScope> _scopes = new();
        private readonly IFocusNavigationStrategy _navigationStrategy;
        private IFocusable? _logicalFocus;
        private IFocusable? _keyboardFocus;
        private IFocusScope? _activeScope;

        /// <summary>
        /// 焦点变化事件
        /// </summary>
        public event EventHandler<FocusChangedEventArgs>? FocusChanged;

        /// <summary>
        /// 当前逻辑焦点元素
        /// </summary>
        public IFocusable? LogicalFocus => _logicalFocus;

        /// <summary>
        /// 当前键盘焦点元素
        /// </summary>
        public IFocusable? KeyboardFocus => _keyboardFocus;

        /// <summary>
        /// 根焦点域
        /// </summary>
        public IFocusScope RootScope { get; }

        /// <summary>
        /// 当前活动焦点域
        /// </summary>
        public IFocusScope? ActiveScope => _activeScope;

        /// <summary>
        /// 初始化焦点管理器
        /// </summary>
        /// <param name="navigationStrategy">焦点导航策略</param>
        public FocusManager(IFocusNavigationStrategy? navigationStrategy = null)
        {
            _navigationStrategy = navigationStrategy ?? TabNavigationStrategy.Instance;
            RootScope = new FocusScope("Root", true);
            _activeScope = RootScope;
            RegisterScope(RootScope);
        }

        /// <summary>
        /// 设置焦点
        /// </summary>
        /// <param name="element">要获得焦点的元素</param>
        /// <param name="focusType">焦点类型</param>
        /// <returns>是否成功设置焦点</returns>
        public bool SetFocus(IFocusable? element, FocusType focusType = FocusType.Keyboard)
        {
            try
            {
                var oldFocus = focusType == FocusType.Keyboard ? _keyboardFocus : _logicalFocus;

                // 检查是否可以获得焦点
                if (element != null && !element.Focusable)
                {
                    Debug.WriteLine($"元素 {element} 不可获得焦点");
                    return false;
                }

                // 移除旧焦点
                if (oldFocus != null)
                {
                    RemoveFocus(oldFocus, focusType);
                }

                // 设置新焦点
                if (element != null)
                {
                    SetFocusInternal(element, focusType);
                }

                // 更新焦点变量
                if (focusType == FocusType.Keyboard)
                {
                    _keyboardFocus = element;
                }
                else
                {
                    _logicalFocus = element;
                }

                // 触发焦点变化事件
                var scope = element?.FocusScope ?? _activeScope;
                var eventArgs = new FocusChangedEventArgs(
                    focusType,
                    oldFocus,
                    element,
                    scope,
                    FocusChangeReason.Programmatic);

                FocusChanged?.Invoke(this, eventArgs);

                Debug.WriteLine($"焦点已设置: {focusType} -> {element}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"设置焦点失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清除焦点
        /// </summary>
        /// <param name="focusType">焦点类型</param>
        public void ClearFocus(FocusType focusType = FocusType.Keyboard)
        {
            SetFocus(null, focusType);
        }

        /// <summary>
        /// 移动焦点
        /// </summary>
        /// <param name="direction">导航方向</param>
        /// <param name="scope">焦点域（null表示当前域）</param>
        /// <returns>是否成功移动焦点</returns>
        public bool MoveFocus(FocusNavigationDirection direction, IFocusScope? scope = null)
        {
            try
            {
                scope ??= _activeScope ?? RootScope;
                var currentFocus = _keyboardFocus;

                var nextFocus = _navigationStrategy.FindNext(scope, currentFocus, direction);
                if (nextFocus != null)
                {
                    return SetFocus(nextFocus, FocusType.Keyboard);
                }

                Debug.WriteLine($"未找到下一个可焦点元素: {direction}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"移动焦点失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 注册焦点域
        /// </summary>
        /// <param name="scope">焦点域</param>
        public void RegisterScope(IFocusScope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            _scopes[scope.Name] = scope;
            Debug.WriteLine($"焦点域已注册: {scope.Name}");
        }

        /// <summary>
        /// 注销焦点域
        /// </summary>
        /// <param name="scope">焦点域</param>
        public void UnregisterScope(IFocusScope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            if (_scopes.Remove(scope.Name))
            {
                // 如果是当前活动域，切换到根域
                if (_activeScope == scope)
                {
                    _activeScope = RootScope;
                }

                Debug.WriteLine($"焦点域已注销: {scope.Name}");
            }
        }

        /// <summary>
        /// 查找焦点域
        /// </summary>
        /// <param name="name">焦点域名称</param>
        /// <returns>焦点域</returns>
        public IFocusScope? FindScope(string name)
        {
            _scopes.TryGetValue(name, out IFocusScope? scope);
            return scope;
        }

        /// <summary>
        /// 获取元素的焦点域
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>焦点域</returns>
        public IFocusScope? GetElementScope(object element)
        {
            if (element is IFocusable focusable)
            {
                return focusable.FocusScope ?? RootScope;
            }

            return RootScope;
        }

        /// <summary>
        /// 检查元素是否在焦点链中
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="focusType">焦点类型</param>
        /// <returns>是否在焦点链中</returns>
        public bool IsInFocusChain(object element, FocusType focusType)
        {
            var currentFocus = focusType == FocusType.Keyboard ? _keyboardFocus : _logicalFocus;
            return currentFocus == element;
        }

        /// <summary>
        /// 内部设置焦点
        /// </summary>
        private void SetFocusInternal(IFocusable element, FocusType focusType)
        {
            // 更新焦点域
            var scope = element.FocusScope ?? RootScope;
            if (scope != _activeScope)
            {
                _activeScope = scope;
            }

            // 在焦点域中设置焦点
            scope.SetFocus(element, focusType);

            // 触发元素的获得焦点事件
            var eventArgs = new FocusChangedEventArgs(
                focusType,
                null,
                element,
                scope,
                FocusChangeReason.Programmatic);

            element.GotFocus?.Invoke(element, eventArgs);
        }

        /// <summary>
        /// 移除焦点
        /// </summary>
        private void RemoveFocus(IFocusable element, FocusType focusType)
        {
            // 在焦点域中清除焦点
            var scope = element.FocusScope ?? RootScope;
            scope.ClearFocus(focusType);

            // 触发元素的失去焦点事件
            var eventArgs = new FocusChangedEventArgs(
                focusType,
                element,
                null,
                scope,
                FocusChangeReason.Programmatic);

            element.LostFocus?.Invoke(element, eventArgs);
        }
    }

    /// <summary>
    /// 焦点域实现
    /// </summary>
    public class FocusScope : IFocusScope
    {
        private readonly List<IFocusScope> _children = new();
        private readonly List<IFocusable> _focusableElements = new();
        private IFocusable? _logicalFocus;
        private IFocusable? _keyboardFocus;

        /// <summary>
        /// 焦点域名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 是否为根焦点域
        /// </summary>
        public bool IsRoot { get; }

        /// <summary>
        /// 父焦点域
        /// </summary>
        public IFocusScope? Parent { get; private set; }

        /// <summary>
        /// 子焦点域集合
        /// </summary>
        public IEnumerable<IFocusScope> Children => _children;

        /// <summary>
        /// 当前逻辑焦点元素
        /// </summary>
        public IFocusable? LogicalFocus => _logicalFocus;

        /// <summary>
        /// 当前键盘焦点元素
        /// </summary>
        public IFocusable? KeyboardFocus => _keyboardFocus;

        /// <summary>
        /// 焦点域内的可焦点元素
        /// </summary>
        public IEnumerable<IFocusable> FocusableElements => _focusableElements;

        /// <summary>
        /// 初始化焦点域
        /// </summary>
        /// <param name="name">焦点域名称</param>
        /// <param name="isRoot">是否为根焦点域</param>
        public FocusScope(string name, bool isRoot = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsRoot = isRoot;
        }

        /// <summary>
        /// 设置焦点
        /// </summary>
        /// <param name="element">要获得焦点的元素</param>
        /// <param name="focusType">焦点类型</param>
        /// <returns>是否成功设置焦点</returns>
        public bool SetFocus(IFocusable element, FocusType focusType)
        {
            if (focusType == FocusType.Keyboard)
            {
                _keyboardFocus = element;
            }
            else
            {
                _logicalFocus = element;
            }

            return true;
        }

        /// <summary>
        /// 清除焦点
        /// </summary>
        /// <param name="focusType">焦点类型</param>
        public void ClearFocus(FocusType focusType)
        {
            if (focusType == FocusType.Keyboard)
            {
                _keyboardFocus = null;
            }
            else
            {
                _logicalFocus = null;
            }
        }

        /// <summary>
        /// 查找下一个可焦点元素
        /// </summary>
        /// <param name="current">当前元素</param>
        /// <param name="direction">导航方向</param>
        /// <returns>下一个可焦点元素</returns>
        public IFocusable? FindNextFocusable(IFocusable? current, FocusNavigationDirection direction)
        {
            return TabNavigationStrategy.Instance.FindNext(this, current, direction);
        }

        /// <summary>
        /// 添加可焦点元素
        /// </summary>
        /// <param name="element">可焦点元素</param>
        public void AddFocusableElement(IFocusable element)
        {
            if (!_focusableElements.Contains(element))
            {
                _focusableElements.Add(element);
            }
        }

        /// <summary>
        /// 移除可焦点元素
        /// </summary>
        /// <param name="element">可焦点元素</param>
        public void RemoveFocusableElement(IFocusable element)
        {
            _focusableElements.Remove(element);

            // 如果移除的是当前焦点元素，清除焦点
            if (_logicalFocus == element)
            {
                _logicalFocus = null;
            }
            if (_keyboardFocus == element)
            {
                _keyboardFocus = null;
            }
        }

        public override string ToString() => Name;
    }
}
