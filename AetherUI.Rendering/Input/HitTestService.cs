using System;
using System.Collections.Generic;
using AetherUI.Core;

namespace AetherUI.Rendering.Input
{
    /// <summary>
    /// 命中测试结果
    /// </summary>
    public class HitTestResult
    {
        public UIElement? HitElement { get; set; }
        public AetherUI.Core.Point HitPoint { get; set; }
    }

    /// <summary>
    /// 简单命中测试服务：基于可视树与 LayoutRect，自上而下到自下而上命中
    /// </summary>
    public static class HitTestService
    {
        /// <summary>
        /// 在给定根元素下，对窗口坐标位置执行命中测试
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="point">窗口坐标</param>
        /// <returns>命中结果</returns>
        public static HitTestResult HitTest(UIElement? root, AetherUI.Core.Point point)
        {
            if (root == null)
            {
                return new HitTestResult { HitElement = null, HitPoint = point };
            }

            try
            {
                UIElement? hit = HitTestRecursive(root, point, new AetherUI.Core.Point(0, 0));
                return new HitTestResult { HitElement = hit, HitPoint = point };
            }
            catch (Exception)
            {
                return new HitTestResult { HitElement = null, HitPoint = point };
            }
        }

        /// <summary>
        /// 递归命中：考虑子元素的 LayoutRect 偏移（父坐标累加）
        /// </summary>
        private static UIElement? HitTestRecursive(UIElement element, AetherUI.Core.Point windowPoint, AetherUI.Core.Point accumulatedOffset)
        {
            // 当前元素在窗口坐标中的矩形
            AetherUI.Core.Rect localRect = element.LayoutRect;
            AetherUI.Core.Rect rectInWindow = new(
                accumulatedOffset.X + localRect.X,
                accumulatedOffset.Y + localRect.Y,
                localRect.Width,
                localRect.Height);
            if (!rectInWindow.Contains(windowPoint))
            {
                return null;
            }
            List<UIElement> children = [.. element.GetVisualChildren()];
            children.Reverse();

            foreach (UIElement child in children)
            {
                UIElement? hitChild = HitTestRecursive(child, windowPoint, new AetherUI.Core.Point(rectInWindow.X, rectInWindow.Y));
                if (hitChild != null)
                {
                    return hitChild;
                }
            }

            // 没有子命中，则当前元素命中
            return element;
        }
    }
}

