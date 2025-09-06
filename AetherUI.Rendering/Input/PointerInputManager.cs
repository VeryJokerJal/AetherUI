using System;
using System.Diagnostics;
using System.Reflection;
using AetherUI.Core;
using AetherUI.Events;

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
            // 预留悬停/进入/离开等
        }

        public void OnMouseDown(AetherUI.Core.Point point)
        {
            _lastMousePoint = point;
            Debug.WriteLine($"PointerInputManager.OnMouseDown at ({point.X}, {point.Y})");

            HitTestResult result = HitTestService.HitTest(_root, point);
            _capturedForClick = result.HitElement;

            Debug.WriteLine($"Hit test result: {_capturedForClick?.GetType().Name ?? "null"}");

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
            Debug.WriteLine($"PointerInputManager.OnMouseUp at ({point.X}, {point.Y})");

            HitTestResult result = HitTestService.HitTest(_root, point);
            UIElement? releasedOn = result.HitElement;

            Debug.WriteLine($"Released on: {releasedOn?.GetType().Name ?? "null"}");
            Debug.WriteLine($"Captured for click: {_capturedForClick?.GetType().Name ?? "null"}");

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
                Debug.WriteLine("Same element clicked - checking if it's a Button");
                if (_capturedForClick is AetherUI.Layout.Button btn)
                {
                    Debug.WriteLine($"Triggering Click on Button: {btn.Content}");
                    // 直接调用 OnClick 受保护，使用反射调用
                    MethodInfo? method = typeof(AetherUI.Layout.Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (method != null)
                    {
                        method.Invoke(btn, null);
                        Debug.WriteLine("Button.OnClick invoked successfully");
                    }
                    else
                    {
                        Debug.WriteLine("Failed to find OnClick method");
                    }
                }
                else
                {
                    Debug.WriteLine($"Clicked element is not a Button: {_capturedForClick.GetType().Name}");
                }
            }
            else
            {
                Debug.WriteLine("Click cancelled - different elements or null");
            }

            _capturedForClick = null;
        }
    }
}

