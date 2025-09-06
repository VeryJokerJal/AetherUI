using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace AetherUI.Core
{
    /// <summary>
    /// 面板基类，所有布局容器的基类
    /// </summary>
    public abstract class Panel : FrameworkElement
    {
        private readonly UIElementCollection _children;

        /// <summary>
        /// 初始化面板
        /// </summary>
        protected Panel()
        {
            _children = new UIElementCollection(this);
        }

        #region 依赖属性

        /// <summary>
        /// 背景依赖属性
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background), typeof(object), typeof(Panel),
            new PropertyMetadata(null, OnBackgroundChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 子元素集合
        /// </summary>
        public UIElementCollection Children => _children;

        /// <summary>
        /// 用于命中测试和可视树遍历的子元素枚举
        /// </summary>
        public override System.Collections.Generic.IEnumerable<UIElement> GetVisualChildren()
        {
            foreach (UIElement child in _children)
            {
                yield return child;
            }
        }

        /// <summary>
        /// 背景
        /// </summary>
        public object? Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        #endregion

        #region 布局重写

        /// <summary>
        /// 测量所有子元素
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Debug.WriteLine($"Panel {GetType().Name} measuring {_children.Count} children");

            // 测量所有可见的子元素
            foreach (UIElement child in _children)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    child.Measure(availableSize);
                }
            }

            // 调用子类的测量逻辑
            return MeasureChildren(availableSize);
        }

        /// <summary>
        /// 排列所有子元素
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine($"Panel {GetType().Name} arranging {_children.Count} children");

            // 调用子类的排列逻辑
            return ArrangeChildren(finalSize);
        }

        /// <summary>
        /// 子类重写以提供自定义的子元素测量逻辑
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected virtual Size MeasureChildren(Size availableSize)
        {
            return Size.Empty;
        }

        /// <summary>
        /// 子类重写以提供自定义的子元素排列逻辑
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected virtual Size ArrangeChildren(Size finalSize)
        {
            // 默认实现：将所有子元素排列到整个区域
            Rect childRect = new Rect(0, 0, finalSize.Width, finalSize.Height);
            foreach (UIElement child in _children)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    child.Arrange(childRect);
                    child.Parent = this;
                }
            }
            return finalSize;
        }

        #endregion

        #region 子元素管理

        /// <summary>
        /// 当子元素被添加时调用
        /// </summary>
        /// <param name="child">被添加的子元素</param>
        internal virtual void OnChildAdded(UIElement child)
        {
            if (child != null)
            {
                child.Parent = this;
            }
            InvalidateMeasure();
            Debug.WriteLine($"Child {child.GetType().Name} added to {GetType().Name}");
        }

        /// <summary>
        /// 当子元素被移除时调用
        /// </summary>
        /// <param name="child">被移除的子元素</param>
        internal virtual void OnChildRemoved(UIElement child)
        {
            if (child != null && child.Parent == this)
            {
                child.Parent = null;
            }
            InvalidateMeasure();
            Debug.WriteLine($"Child {child.GetType().Name} removed from {GetType().Name}");
        }

        #endregion

        #region 属性更改回调

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Panel panel)
            {
                Debug.WriteLine($"Background changed for {panel.GetType().Name}: {e.NewValue}");
            }
        }

        #endregion
    }

    /// <summary>
    /// UI元素集合，用于管理面板的子元素
    /// </summary>
    public class UIElementCollection : Collection<UIElement>
    {
        private readonly Panel _owner;

        /// <summary>
        /// 初始化UI元素集合
        /// </summary>
        /// <param name="owner">拥有者面板</param>
        public UIElementCollection(Panel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="item">元素</param>
        protected override void InsertItem(int index, UIElement item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            base.InsertItem(index, item);
            _owner.OnChildAdded(item);
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="index">索引</param>
        protected override void RemoveItem(int index)
        {
            UIElement item = this[index];
            base.RemoveItem(index);
            _owner.OnChildRemoved(item);
        }

        /// <summary>
        /// 设置元素
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="item">新元素</param>
        protected override void SetItem(int index, UIElement item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            UIElement oldItem = this[index];
            base.SetItem(index, item);
            _owner.OnChildRemoved(oldItem);
            _owner.OnChildAdded(item);
        }

        /// <summary>
        /// 清除所有元素
        /// </summary>
        protected override void ClearItems()
        {
            List<UIElement> oldItems = new List<UIElement>(this);
            base.ClearItems();
            foreach (UIElement item in oldItems)
            {
                _owner.OnChildRemoved(item);
            }
        }
    }
}
