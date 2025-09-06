using System;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Layout
{
    /// <summary>
    /// 停靠方向枚举
    /// </summary>
    public enum Dock
    {
        /// <summary>
        /// 停靠到左边
        /// </summary>
        Left,

        /// <summary>
        /// 停靠到顶部
        /// </summary>
        Top,

        /// <summary>
        /// 停靠到右边
        /// </summary>
        Right,

        /// <summary>
        /// 停靠到底部
        /// </summary>
        Bottom
    }

    /// <summary>
    /// 停靠面板，子元素可以停靠到面板的四个边
    /// </summary>
    public class DockPanel : Panel
    {
        #region 依赖属性

        /// <summary>
        /// 最后一个子元素填充剩余空间依赖属性
        /// </summary>
        public static readonly DependencyProperty LastChildFillProperty = DependencyProperty.Register(
            nameof(LastChildFill), typeof(bool), typeof(DockPanel),
            new PropertyMetadata(true, OnLastChildFillChanged));

        /// <summary>
        /// 停靠方向附加属性
        /// </summary>
        public static readonly DependencyProperty DockProperty = DependencyProperty.Register(
            "Dock", typeof(Dock), typeof(DockPanel),
            new PropertyMetadata(Dock.Left, OnDockChanged));

        #endregion

        #region 属性

        /// <summary>
        /// 最后一个子元素是否填充剩余空间
        /// </summary>
        public bool LastChildFill
        {
            get => (bool)(GetValue(LastChildFillProperty) ?? true);
            set => SetValue(LastChildFillProperty, value);
        }

        #endregion

        #region 附加属性访问器

        /// <summary>
        /// 获取元素的停靠方向
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>停靠方向</returns>
        public static Dock GetDock(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            return (Dock)(element.GetValue(DockProperty) ?? Dock.Left);
        }

        /// <summary>
        /// 设置元素的停靠方向
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="value">停靠方向</param>
        public static void SetDock(UIElement element, Dock value)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            element.SetValue(DockProperty, value);
        }

        #endregion

        #region 布局重写

        /// <summary>
        /// 测量子元素
        /// </summary>
        /// <param name="availableSize">可用尺寸</param>
        /// <returns>期望尺寸</returns>
        protected override Size MeasureChildren(Size availableSize)
        {
            Debug.WriteLine($"DockPanel measuring {Children.Count} children, Available size: {availableSize}");

            double usedWidth = 0;
            double usedHeight = 0;
            double maxWidth = 0;
            double maxHeight = 0;

            Size remainingSize = availableSize;

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                Dock dock = GetDock(child);
                bool isLastChild = (i == Children.Count - 1);

                Size childAvailableSize;

                if (isLastChild && LastChildFill)
                {
                    // 最后一个子元素填充剩余空间
                    childAvailableSize = remainingSize;
                }
                else
                {
                    // 根据停靠方向确定可用尺寸
                    switch (dock)
                    {
                        case Dock.Left:
                        case Dock.Right:
                            childAvailableSize = new Size(double.PositiveInfinity, remainingSize.Height);
                            break;
                        case Dock.Top:
                        case Dock.Bottom:
                            childAvailableSize = new Size(remainingSize.Width, double.PositiveInfinity);
                            break;
                        default:
                            childAvailableSize = remainingSize;
                            break;
                    }
                }

                child.Measure(childAvailableSize);
                Size childDesiredSize = child.DesiredSize;

                Debug.WriteLine($"Child {i} dock {dock} desired size: {childDesiredSize}");

                if (isLastChild && LastChildFill)
                {
                    // 最后一个子元素填充剩余空间，不影响DockPanel的尺寸计算
                    maxWidth = Math.Max(maxWidth, usedWidth + childDesiredSize.Width);
                    maxHeight = Math.Max(maxHeight, usedHeight + childDesiredSize.Height);
                }
                else
                {
                    // 根据停靠方向更新已使用空间和剩余空间
                    switch (dock)
                    {
                        case Dock.Left:
                        case Dock.Right:
                            usedWidth += childDesiredSize.Width;
                            remainingSize = new Size(
                                Math.Max(0, remainingSize.Width - childDesiredSize.Width),
                                remainingSize.Height);
                            maxHeight = Math.Max(maxHeight, childDesiredSize.Height);
                            break;
                        case Dock.Top:
                        case Dock.Bottom:
                            usedHeight += childDesiredSize.Height;
                            remainingSize = new Size(
                                remainingSize.Width,
                                Math.Max(0, remainingSize.Height - childDesiredSize.Height));
                            maxWidth = Math.Max(maxWidth, childDesiredSize.Width);
                            break;
                    }
                }
            }

            Size desiredSize = new Size(
                Math.Max(maxWidth, usedWidth),
                Math.Max(maxHeight, usedHeight));

            Debug.WriteLine($"DockPanel desired size: {desiredSize}");
            return desiredSize;
        }

        /// <summary>
        /// 排列子元素
        /// </summary>
        /// <param name="finalSize">最终尺寸</param>
        /// <returns>实际尺寸</returns>
        protected override Size ArrangeChildren(Size finalSize)
        {
            Debug.WriteLine($"DockPanel arranging {Children.Count} children, Final size: {finalSize}");

            double left = 0;
            double top = 0;
            double right = finalSize.Width;
            double bottom = finalSize.Height;

            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                if (child.Visibility == Visibility.Collapsed)
                    continue;

                Dock dock = GetDock(child);
                bool isLastChild = (i == Children.Count - 1);

                Rect childRect;

                if (isLastChild && LastChildFill)
                {
                    // 最后一个子元素填充剩余空间
                    childRect = new Rect(left, top, right - left, bottom - top);
                }
                else
                {
                    Size childDesiredSize = child.DesiredSize;

                    switch (dock)
                    {
                        case Dock.Left:
                            childRect = new Rect(left, top, childDesiredSize.Width, bottom - top);
                            left += childDesiredSize.Width;
                            break;
                        case Dock.Right:
                            childRect = new Rect(right - childDesiredSize.Width, top, childDesiredSize.Width, bottom - top);
                            right -= childDesiredSize.Width;
                            break;
                        case Dock.Top:
                            childRect = new Rect(left, top, right - left, childDesiredSize.Height);
                            top += childDesiredSize.Height;
                            break;
                        case Dock.Bottom:
                            childRect = new Rect(left, bottom - childDesiredSize.Height, right - left, childDesiredSize.Height);
                            bottom -= childDesiredSize.Height;
                            break;
                        default:
                            childRect = new Rect(left, top, right - left, bottom - top);
                            break;
                    }
                }

                child.Arrange(childRect);
                Debug.WriteLine($"Child {i} dock {dock} arranged to: {childRect}");
            }

            return finalSize;
        }

        #endregion

        #region 属性更改回调

        private static void OnLastChildFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DockPanel dockPanel)
            {
                dockPanel.InvalidateMeasure();
                Debug.WriteLine($"DockPanel LastChildFill changed to: {e.NewValue}");
            }
        }

        private static void OnDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 当停靠方向更改时，需要重新布局父DockPanel
            if (d is UIElement element)
            {
                Debug.WriteLine($"DockPanel Dock property changed to: {e.NewValue}");
            }
        }

        #endregion
    }
}
