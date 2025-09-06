using System;
using System.Collections.Generic;
using AetherUI.Input.Core;

namespace AetherUI.Input.Focus
{
    /// <summary>
    /// 焦点类型
    /// </summary>
    public enum FocusType
    {
        /// <summary>
        /// 逻辑焦点
        /// </summary>
        Logical,

        /// <summary>
        /// 键盘焦点
        /// </summary>
        Keyboard
    }

    /// <summary>
    /// 焦点导航方向
    /// </summary>
    public enum FocusNavigationDirection
    {
        /// <summary>
        /// 下一个
        /// </summary>
        Next,

        /// <summary>
        /// 上一个
        /// </summary>
        Previous,

        /// <summary>
        /// 第一个
        /// </summary>
        First,

        /// <summary>
        /// 最后一个
        /// </summary>
        Last,

        /// <summary>
        /// 向上
        /// </summary>
        Up,

        /// <summary>
        /// 向下
        /// </summary>
        Down,

        /// <summary>
        /// 向左
        /// </summary>
        Left,

        /// <summary>
        /// 向右
        /// </summary>
        Right
    }

    /// <summary>
    /// 可获得焦点的元素接口
    /// </summary>
    public interface IFocusable
    {
        /// <summary>
        /// 是否可获得焦点
        /// </summary>
        bool Focusable { get; }

        /// <summary>
        /// 是否可通过Tab键获得焦点
        /// </summary>
        bool IsTabStop { get; }

        /// <summary>
        /// Tab索引
        /// </summary>
        int TabIndex { get; }

        /// <summary>
        /// 是否有逻辑焦点
        /// </summary>
        bool IsLogicallyFocused { get; }

        /// <summary>
        /// 是否有键盘焦点
        /// </summary>
        bool IsKeyboardFocused { get; }

        /// <summary>
        /// 焦点域
        /// </summary>
        IFocusScope? FocusScope { get; }

        /// <summary>
        /// 获得焦点事件
        /// </summary>
        event EventHandler<FocusChangedEventArgs>? GotFocus;

        /// <summary>
        /// 失去焦点事件
        /// </summary>
        event EventHandler<FocusChangedEventArgs>? LostFocus;

        /// <summary>
        /// 尝试获得焦点
        /// </summary>
        /// <param name="focusType">焦点类型</param>
        /// <returns>是否成功获得焦点</returns>
        bool Focus(FocusType focusType = FocusType.Keyboard);

        /// <summary>
        /// 失去焦点
        /// </summary>
        /// <param name="focusType">焦点类型</param>
        void Unfocus(FocusType focusType = FocusType.Keyboard);
    }

    /// <summary>
    /// 焦点域接口
    /// </summary>
    public interface IFocusScope
    {
        /// <summary>
        /// 焦点域名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 是否为根焦点域
        /// </summary>
        bool IsRoot { get; }

        /// <summary>
        /// 父焦点域
        /// </summary>
        IFocusScope? Parent { get; }

        /// <summary>
        /// 子焦点域集合
        /// </summary>
        IEnumerable<IFocusScope> Children { get; }

        /// <summary>
        /// 当前逻辑焦点元素
        /// </summary>
        IFocusable? LogicalFocus { get; }

        /// <summary>
        /// 当前键盘焦点元素
        /// </summary>
        IFocusable? KeyboardFocus { get; }

        /// <summary>
        /// 焦点域内的可焦点元素
        /// </summary>
        IEnumerable<IFocusable> FocusableElements { get; }

        /// <summary>
        /// 设置焦点
        /// </summary>
        /// <param name="element">要获得焦点的元素</param>
        /// <param name="focusType">焦点类型</param>
        /// <returns>是否成功设置焦点</returns>
        bool SetFocus(IFocusable element, FocusType focusType);

        /// <summary>
        /// 清除焦点
        /// </summary>
        /// <param name="focusType">焦点类型</param>
        void ClearFocus(FocusType focusType);

        /// <summary>
        /// 查找下一个可焦点元素
        /// </summary>
        /// <param name="current">当前元素</param>
        /// <param name="direction">导航方向</param>
        /// <returns>下一个可焦点元素</returns>
        IFocusable? FindNextFocusable(IFocusable? current, FocusNavigationDirection direction);
    }

    /// <summary>
    /// 焦点管理器接口
    /// </summary>
    public interface IFocusManager
    {
        /// <summary>
        /// 焦点变化事件
        /// </summary>
        event EventHandler<FocusChangedEventArgs>? FocusChanged;

        /// <summary>
        /// 当前逻辑焦点元素
        /// </summary>
        IFocusable? LogicalFocus { get; }

        /// <summary>
        /// 当前键盘焦点元素
        /// </summary>
        IFocusable? KeyboardFocus { get; }

        /// <summary>
        /// 根焦点域
        /// </summary>
        IFocusScope RootScope { get; }

        /// <summary>
        /// 当前活动焦点域
        /// </summary>
        IFocusScope? ActiveScope { get; }

        /// <summary>
        /// 设置焦点
        /// </summary>
        /// <param name="element">要获得焦点的元素</param>
        /// <param name="focusType">焦点类型</param>
        /// <returns>是否成功设置焦点</returns>
        bool SetFocus(IFocusable? element, FocusType focusType = FocusType.Keyboard);

        /// <summary>
        /// 清除焦点
        /// </summary>
        /// <param name="focusType">焦点类型</param>
        void ClearFocus(FocusType focusType = FocusType.Keyboard);

        /// <summary>
        /// 移动焦点
        /// </summary>
        /// <param name="direction">导航方向</param>
        /// <param name="scope">焦点域（null表示当前域）</param>
        /// <returns>是否成功移动焦点</returns>
        bool MoveFocus(FocusNavigationDirection direction, IFocusScope? scope = null);

        /// <summary>
        /// 注册焦点域
        /// </summary>
        /// <param name="scope">焦点域</param>
        void RegisterScope(IFocusScope scope);

        /// <summary>
        /// 注销焦点域
        /// </summary>
        /// <param name="scope">焦点域</param>
        void UnregisterScope(IFocusScope scope);

        /// <summary>
        /// 查找焦点域
        /// </summary>
        /// <param name="name">焦点域名称</param>
        /// <returns>焦点域</returns>
        IFocusScope? FindScope(string name);

        /// <summary>
        /// 获取元素的焦点域
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>焦点域</returns>
        IFocusScope? GetElementScope(object element);

        /// <summary>
        /// 检查元素是否在焦点链中
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="focusType">焦点类型</param>
        /// <returns>是否在焦点链中</returns>
        bool IsInFocusChain(object element, FocusType focusType);
    }

    /// <summary>
    /// 焦点变化事件参数
    /// </summary>
    public class FocusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 焦点类型
        /// </summary>
        public FocusType FocusType { get; }

        /// <summary>
        /// 旧的焦点元素
        /// </summary>
        public IFocusable? OldFocus { get; }

        /// <summary>
        /// 新的焦点元素
        /// </summary>
        public IFocusable? NewFocus { get; }

        /// <summary>
        /// 焦点域
        /// </summary>
        public IFocusScope? Scope { get; }

        /// <summary>
        /// 焦点变化原因
        /// </summary>
        public FocusChangeReason Reason { get; }

        /// <summary>
        /// 初始化焦点变化事件参数
        /// </summary>
        /// <param name="focusType">焦点类型</param>
        /// <param name="oldFocus">旧的焦点元素</param>
        /// <param name="newFocus">新的焦点元素</param>
        /// <param name="scope">焦点域</param>
        /// <param name="reason">焦点变化原因</param>
        public FocusChangedEventArgs(
            FocusType focusType,
            IFocusable? oldFocus,
            IFocusable? newFocus,
            IFocusScope? scope,
            FocusChangeReason reason)
        {
            FocusType = focusType;
            OldFocus = oldFocus;
            NewFocus = newFocus;
            Scope = scope;
            Reason = reason;
        }

        public override string ToString() =>
            $"FocusChanged: {FocusType} from {OldFocus} to {NewFocus} ({Reason})";
    }

    /// <summary>
    /// 焦点变化原因
    /// </summary>
    public enum FocusChangeReason
    {
        /// <summary>
        /// 程序设置
        /// </summary>
        Programmatic,

        /// <summary>
        /// 鼠标点击
        /// </summary>
        Mouse,

        /// <summary>
        /// 键盘导航
        /// </summary>
        Keyboard,

        /// <summary>
        /// Tab导航
        /// </summary>
        Tab,

        /// <summary>
        /// 方向键导航
        /// </summary>
        Directional,

        /// <summary>
        /// 元素移除
        /// </summary>
        ElementRemoved,

        /// <summary>
        /// 窗口激活
        /// </summary>
        WindowActivated,

        /// <summary>
        /// 窗口失活
        /// </summary>
        WindowDeactivated
    }

    /// <summary>
    /// 焦点导航策略接口
    /// </summary>
    public interface IFocusNavigationStrategy
    {
        /// <summary>
        /// 查找下一个可焦点元素
        /// </summary>
        /// <param name="scope">焦点域</param>
        /// <param name="current">当前元素</param>
        /// <param name="direction">导航方向</param>
        /// <returns>下一个可焦点元素</returns>
        IFocusable? FindNext(IFocusScope scope, IFocusable? current, FocusNavigationDirection direction);

        /// <summary>
        /// 检查元素是否可导航到
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="direction">导航方向</param>
        /// <returns>是否可导航到</returns>
        bool CanNavigateTo(IFocusable element, FocusNavigationDirection direction);
    }

    /// <summary>
    /// Tab导航策略
    /// </summary>
    public class TabNavigationStrategy : IFocusNavigationStrategy
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static TabNavigationStrategy Instance { get; } = new();

        private TabNavigationStrategy() { }

        /// <summary>
        /// 查找下一个可焦点元素
        /// </summary>
        /// <param name="scope">焦点域</param>
        /// <param name="current">当前元素</param>
        /// <param name="direction">导航方向</param>
        /// <returns>下一个可焦点元素</returns>
        public IFocusable? FindNext(IFocusScope scope, IFocusable? current, FocusNavigationDirection direction)
        {
            var focusableElements = scope.FocusableElements
                .Where(e => e.IsTabStop && CanNavigateTo(e, direction))
                .OrderBy(e => e.TabIndex)
                .ToList();

            if (focusableElements.Count == 0)
                return null;

            return direction switch
            {
                FocusNavigationDirection.Next => GetNextElement(focusableElements, current),
                FocusNavigationDirection.Previous => GetPreviousElement(focusableElements, current),
                FocusNavigationDirection.First => focusableElements.First(),
                FocusNavigationDirection.Last => focusableElements.Last(),
                _ => null
            };
        }

        /// <summary>
        /// 检查元素是否可导航到
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="direction">导航方向</param>
        /// <returns>是否可导航到</returns>
        public bool CanNavigateTo(IFocusable element, FocusNavigationDirection direction)
        {
            return element.Focusable && element.IsTabStop;
        }

        private IFocusable? GetNextElement(List<IFocusable> elements, IFocusable? current)
        {
            if (current == null)
                return elements.FirstOrDefault();

            int currentIndex = elements.IndexOf(current);
            if (currentIndex == -1)
                return elements.FirstOrDefault();

            return currentIndex < elements.Count - 1 ? elements[currentIndex + 1] : elements.FirstOrDefault();
        }

        private IFocusable? GetPreviousElement(List<IFocusable> elements, IFocusable? current)
        {
            if (current == null)
                return elements.LastOrDefault();

            int currentIndex = elements.IndexOf(current);
            if (currentIndex == -1)
                return elements.LastOrDefault();

            return currentIndex > 0 ? elements[currentIndex - 1] : elements.LastOrDefault();
        }
    }
}
