using System;
using System.Diagnostics;
using System.Reflection;
using AetherUI.Core;
using AetherUI.Events;
using AetherUI.Layout;

namespace AetherUI.Rendering.Input
{
    /// <summary>
    /// 指针输入管理器：处理鼠标移动/按下/抬起，派发到UI树
    /// </summary>
    public class PointerInputManager
    {
        private UIElement? _root;
        private UIElement? _capturedForClick;
        private AetherUI.Core.Point _lastMousePoint;
        private ScrollBar? _draggingScrollBar;
        private bool _isDraggingThumb;

        public PointerInputManager(UIElement? root)
        {
            _root = root;
        }

        public void SetRoot(UIElement? root)
        {
            _root = root;
        }

        public void OnMouseMove(AetherUI.Core.Point point)
        {
            _lastMousePoint = point;

            // 处理滚动条拖拽
            if (_isDraggingThumb && _draggingScrollBar != null)
            {
                HandleScrollBarDrag(point);
            }

            // 预留悬停/进入/离开等
        }

        public void OnMouseDown(AetherUI.Core.Point point)
        {
            _lastMousePoint = point;
            HitTestResult result = HitTestService.HitTest(_root, point);
            _capturedForClick = result.HitElement;

            // 检查是否点击了滚动条
            if (HandleScrollBarClick(point))
            {
                return; // 滚动条处理了点击，不继续处理其他元素
            }

            if (_capturedForClick != null)
            {
                // 触发路由 MouseDown
                MouseButtonEventArgs args = new MouseButtonEventArgs(MouseButton.Left, MouseButtonState.Pressed, new AetherUI.Events.Point(point.X, point.Y), 0, 1)
                {
                    RoutedEvent = InputEvents.MouseDownEvent
                };
                EventManager.RaiseEvent(_capturedForClick, args);
            }
        }

        public void OnMouseUp(AetherUI.Core.Point point)
        {
            _lastMousePoint = point;

            // 结束滚动条拖拽
            if (_isDraggingThumb)
            {
                _isDraggingThumb = false;
                if (_draggingScrollBar != null)
                {
                    _draggingScrollBar.IsDragging = false;
                    _draggingScrollBar = null;
                }
                return;
            }

            HitTestResult result = HitTestService.HitTest(_root, point);
            UIElement? releasedOn = result.HitElement;

            if (releasedOn != null)
            {
                // 路由 MouseUp
                MouseButtonEventArgs args = new MouseButtonEventArgs(MouseButton.Left, MouseButtonState.Released, new AetherUI.Events.Point(point.X, point.Y), 0, 1)
                {
                    RoutedEvent = InputEvents.MouseUpEvent
                };
                EventManager.RaiseEvent(releasedOn, args);
            }

            // Click 判定：按下与抬起命中同一元素，且为 Button 时触发 Click
            if (_capturedForClick != null && releasedOn == _capturedForClick)
            {
                if (_capturedForClick is AetherUI.Layout.Button btn)
                {
                    MethodInfo? method = typeof(AetherUI.Layout.Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (method != null)
                    {
                        method.Invoke(btn, null);
                    }
                }
            }

            _capturedForClick = null;
        }

        /// <summary>
        /// 处理鼠标滚轮事件
        /// </summary>
        /// <param name="point">鼠标位置</param>
        /// <param name="delta">滚轮增量</param>
        public void OnMouseWheel(AetherUI.Core.Point point, double delta)
        {
            System.Diagnostics.Debug.WriteLine($"PointerInputManager: OnMouseWheel at {point}, delta={delta}");

            // 查找包含鼠标位置的 ScrollViewer
            ScrollViewer? scrollViewer = FindScrollViewerAt(point);
            if (scrollViewer != null)
            {
                System.Diagnostics.Debug.WriteLine($"PointerInputManager: Found ScrollViewer, calling ScrollByWheel");
                scrollViewer.ScrollByWheel(delta);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"PointerInputManager: No ScrollViewer found at {point}");
            }
        }

        #region 滚动条交互处理

        /// <summary>
        /// 处理滚动条点击
        /// </summary>
        /// <param name="point">点击位置</param>
        /// <returns>是否处理了点击</returns>
        private bool HandleScrollBarClick(AetherUI.Core.Point point)
        {
            ScrollBar? scrollBar = FindScrollBarAt(point);
            if (scrollBar == null)
            {
                System.Diagnostics.Debug.WriteLine($"PointerInputManager: No ScrollBar found at {point}");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"PointerInputManager: Found ScrollBar at {point}, checking click regions");
            System.Diagnostics.Debug.WriteLine($"  ThumbRect: {scrollBar.ThumbRect}");
            System.Diagnostics.Debug.WriteLine($"  UpButtonRect: {scrollBar.UpButtonRect}");
            System.Diagnostics.Debug.WriteLine($"  DownButtonRect: {scrollBar.DownButtonRect}");
            System.Diagnostics.Debug.WriteLine($"  TrackRect: {scrollBar.TrackRect}");

            // 检查点击位置
            if (scrollBar.ThumbRect.Contains(point))
            {
                // 开始拖拽滑块
                System.Diagnostics.Debug.WriteLine($"PointerInputManager: Starting thumb drag");
                _isDraggingThumb = true;
                _draggingScrollBar = scrollBar;
                scrollBar.IsDragging = true;
                scrollBar.DragStartPoint = point;
                scrollBar.DragStartValue = scrollBar.Value;
                return true;
            }
            else if (scrollBar.UpButtonRect.Contains(point))
            {
                // 点击上/左按钮
                double oldValue = scrollBar.Value;
                double newValue = scrollBar.Value - scrollBar.SmallChange;
                scrollBar.Value = Math.Max(scrollBar.Minimum, newValue);
                System.Diagnostics.Debug.WriteLine($"PointerInputManager: Up button clicked, value: {oldValue} -> {scrollBar.Value}");
                return true;
            }
            else if (scrollBar.DownButtonRect.Contains(point))
            {
                // 点击下/右按钮
                double oldValue = scrollBar.Value;
                double newValue = scrollBar.Value + scrollBar.SmallChange;
                scrollBar.Value = Math.Min(scrollBar.Maximum, newValue);
                System.Diagnostics.Debug.WriteLine($"PointerInputManager: Down button clicked, value: {oldValue} -> {scrollBar.Value}");
                return true;
            }
            else if (scrollBar.TrackRect.Contains(point))
            {
                // 点击轨道，跳转到对应位置
                System.Diagnostics.Debug.WriteLine($"PointerInputManager: Track clicked");
                HandleTrackClick(scrollBar, point);
                return true;
            }

            System.Diagnostics.Debug.WriteLine($"PointerInputManager: Click not in any ScrollBar region");
            return false;
        }

        /// <summary>
        /// 处理轨道点击
        /// </summary>
        /// <param name="scrollBar">滚动条</param>
        /// <param name="point">点击位置</param>
        private void HandleTrackClick(ScrollBar scrollBar, AetherUI.Core.Point point)
        {
            double range = scrollBar.Maximum - scrollBar.Minimum;
            if (range <= 0) return;

            if (scrollBar.Orientation == Orientation.Vertical)
            {
                double relativeY = point.Y - scrollBar.TrackRect.Y;
                double ratio = relativeY / scrollBar.TrackRect.Height;
                scrollBar.Value = scrollBar.Minimum + range * ratio;
            }
            else
            {
                double relativeX = point.X - scrollBar.TrackRect.X;
                double ratio = relativeX / scrollBar.TrackRect.Width;
                scrollBar.Value = scrollBar.Minimum + range * ratio;
            }
        }

        /// <summary>
        /// 处理滚动条拖拽
        /// </summary>
        /// <param name="point">当前鼠标位置</param>
        private void HandleScrollBarDrag(AetherUI.Core.Point point)
        {
            if (_draggingScrollBar == null) return;

            double range = _draggingScrollBar.Maximum - _draggingScrollBar.Minimum;
            if (range <= 0) return;

            if (_draggingScrollBar.Orientation == Orientation.Vertical)
            {
                double deltaY = point.Y - _draggingScrollBar.DragStartPoint.Y;
                double availableHeight = _draggingScrollBar.TrackRect.Height - _draggingScrollBar.ThumbRect.Height;
                if (availableHeight > 0)
                {
                    double ratio = deltaY / availableHeight;
                    double newValue = _draggingScrollBar.DragStartValue + range * ratio;
                    _draggingScrollBar.Value = Math.Max(_draggingScrollBar.Minimum,
                        Math.Min(_draggingScrollBar.Maximum, newValue));
                }
            }
            else
            {
                double deltaX = point.X - _draggingScrollBar.DragStartPoint.X;
                double availableWidth = _draggingScrollBar.TrackRect.Width - _draggingScrollBar.ThumbRect.Width;
                if (availableWidth > 0)
                {
                    double ratio = deltaX / availableWidth;
                    double newValue = _draggingScrollBar.DragStartValue + range * ratio;
                    _draggingScrollBar.Value = Math.Max(_draggingScrollBar.Minimum,
                        Math.Min(_draggingScrollBar.Maximum, newValue));
                }
            }
        }

        /// <summary>
        /// 查找指定位置的滚动条
        /// </summary>
        /// <param name="point">位置</param>
        /// <returns>滚动条或null</returns>
        private ScrollBar? FindScrollBarAt(AetherUI.Core.Point point)
        {
            return FindScrollBarInElement(_root, point);
        }

        /// <summary>
        /// 在元素树中查找滚动条
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="point">位置</param>
        /// <returns>滚动条或null</returns>
        private ScrollBar? FindScrollBarInElement(UIElement? element, AetherUI.Core.Point point)
        {
            if (element == null || !element.LayoutRect.Contains(point)) return null;

            if (element is ScrollBar scrollBar)
            {
                return scrollBar;
            }
            else if (element is ScrollViewer scrollViewer)
            {
                // 检查垂直滚动条
                if (scrollViewer.VerticalScrollBar.Visibility == Visibility.Visible &&
                    scrollViewer.VerticalScrollBar.LayoutRect.Contains(point))
                {
                    return scrollViewer.VerticalScrollBar;
                }

                // 检查水平滚动条
                if (scrollViewer.HorizontalScrollBar.Visibility == Visibility.Visible &&
                    scrollViewer.HorizontalScrollBar.LayoutRect.Contains(point))
                {
                    return scrollViewer.HorizontalScrollBar;
                }
            }

            // 递归查找子元素
            if (element is Panel panel)
            {
                foreach (UIElement child in panel.Children)
                {
                    ScrollBar? result = FindScrollBarInElement(child, point);
                    if (result != null) return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 查找指定位置的滚动视图
        /// </summary>
        /// <param name="point">位置</param>
        /// <returns>滚动视图或null</returns>
        private ScrollViewer? FindScrollViewerAt(AetherUI.Core.Point point)
        {
            return FindScrollViewerInElement(_root, point);
        }

        /// <summary>
        /// 在元素树中查找滚动视图
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="point">位置</param>
        /// <returns>滚动视图或null</returns>
        private ScrollViewer? FindScrollViewerInElement(UIElement? element, AetherUI.Core.Point point)
        {
            if (element == null || !element.LayoutRect.Contains(point)) return null;

            if (element is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            // 递归查找子元素
            if (element is Panel panel)
            {
                foreach (UIElement child in panel.Children)
                {
                    ScrollViewer? result = FindScrollViewerInElement(child, point);
                    if (result != null) return result;
                }
            }

            return null;
        }

        #endregion
    }
}

