using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace AetherUI.Input.HitTesting
{
    /// <summary>
    /// 命中测试引擎实现
    /// </summary>
    public class HitTestEngine : IHitTestEngine
    {
        private readonly HitTestOptions _options;
        private readonly Dictionary<IHitTestable, Rect> _boundsCache = new();
        private readonly Dictionary<IHitTestable, Matrix4x4> _transformCache = new();

        /// <summary>
        /// 初始化命中测试引擎
        /// </summary>
        /// <param name="options">命中测试选项</param>
        public HitTestEngine(HitTestOptions? options = null)
        {
            _options = options ?? HitTestOptions.Default;
        }

        /// <summary>
        /// 执行命中测试
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="point">测试点（全局坐标）</param>
        /// <returns>命中测试结果</returns>
        public HitTestResult HitTest(IHitTestable? root, Core.Point point)
        {
            if (root == null)
                return HitTestResult.Empty(point);

            var context = new HitTestContext(point, _options);
            var hitElement = HitTestRecursive(root, context);

            if (hitElement != null)
            {
                var localPoint = TransformPoint(point, hitElement.GetTransformFromRoot());
                return new HitTestResult(hitElement, point, localPoint, context.HitPath.ToArray());
            }

            return HitTestResult.Empty(point);
        }

        /// <summary>
        /// 执行命中测试，返回所有命中的元素
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="point">测试点（全局坐标）</param>
        /// <returns>所有命中的元素（从最上层到最下层）</returns>
        public IEnumerable<HitTestResult> HitTestAll(IHitTestable? root, Core.Point point)
        {
            if (root == null)
                yield break;

            var context = new HitTestContext(point, _options);
            var results = new List<HitTestResult>();

            HitTestAllRecursive(root, context, results);

            // 按Z顺序排序（从高到低）
            foreach (var result in results.OrderByDescending(r => r.HitElement?.ZIndex ?? 0))
            {
                yield return result;
            }
        }

        /// <summary>
        /// 执行区域命中测试
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="rect">测试区域（全局坐标）</param>
        /// <returns>命中的元素集合</returns>
        public IEnumerable<HitTestResult> HitTestRegion(IHitTestable? root, Core.Rect rect)
        {
            if (root == null)
                yield break;

            var results = new List<HitTestResult>();

            // 简单实现：测试矩形的四个角和中心点
            var testPoints = new[]
            {
                new Core.Point(rect.Left, rect.Top),
                new Core.Point(rect.Right, rect.Top),
                new Core.Point(rect.Left, rect.Bottom),
                new Core.Point(rect.Right, rect.Bottom),
                new Core.Point(rect.Center.X, rect.Center.Y)
            };

            var hitElements = new HashSet<IHitTestable>();

            foreach (var point in testPoints)
            {
                foreach (var result in HitTestAll(root, point))
                {
                    if (result.HitElement != null && hitElements.Add(result.HitElement))
                    {
                        results.Add(result);
                    }
                }
            }

            foreach (var result in results)
            {
                yield return result;
            }
        }

        /// <summary>
        /// 递归命中测试
        /// </summary>
        private IHitTestable? HitTestRecursive(IHitTestable element, HitTestContext context)
        {
            // 检查深度限制
            if (context.Depth >= context.Options.MaxDepth)
                return null;

            // 检查是否应该测试此元素
            if (!context.Options.Filter.ShouldHitTest(element))
                return null;

            // 检查边界
            if (!IsPointInBounds(element, context.GlobalPoint))
                return null;

            context.PushElement(element);

            try
            {
                // 更新变换矩阵
                var elementTransform = GetCachedTransform(element);
                var previousTransform = context.Transform;
                context.Transform = Matrix4x4.Multiply(context.Transform, elementTransform);

                // 更新裁剪区域
                var previousClip = context.ClipBounds;
                if (element.ClipBounds.HasValue)
                {
                    var clipBounds = TransformRect(element.ClipBounds.Value, elementTransform);
                    context.ClipBounds = previousClip.HasValue 
                        ? IntersectRects(previousClip.Value, clipBounds)
                        : clipBounds;
                }

                // 检查是否在裁剪区域内
                if (context.ClipBounds.HasValue && !context.ClipBounds.Value.Contains(context.GlobalPoint))
                {
                    return null;
                }

                // 测试子元素（按Z顺序从高到低）
                if (context.Options.Filter.ShouldHitTestChildren(element))
                {
                    var children = element.Children
                        .Where(child => context.Options.Filter.ShouldHitTest(child))
                        .OrderByDescending(child => child.ZIndex);

                    foreach (var child in children)
                    {
                        var hitChild = HitTestRecursive(child, context);
                        if (hitChild != null)
                        {
                            return hitChild;
                        }
                    }
                }

                // 测试当前元素
                if (TestElementHit(element, context))
                {
                    return element;
                }

                return null;
            }
            finally
            {
                context.PopElement();
            }
        }

        /// <summary>
        /// 递归命中测试所有元素
        /// </summary>
        private void HitTestAllRecursive(IHitTestable element, HitTestContext context, List<HitTestResult> results)
        {
            // 检查深度限制
            if (context.Depth >= context.Options.MaxDepth)
                return;

            // 检查是否应该测试此元素
            if (!context.Options.Filter.ShouldHitTest(element))
                return;

            // 检查边界
            if (!IsPointInBounds(element, context.GlobalPoint))
                return;

            context.PushElement(element);

            try
            {
                // 更新变换矩阵
                var elementTransform = GetCachedTransform(element);
                var previousTransform = context.Transform;
                context.Transform = Matrix4x4.Multiply(context.Transform, elementTransform);

                // 测试当前元素
                if (TestElementHit(element, context))
                {
                    var localPoint = TransformPoint(context.GlobalPoint, element.GetTransformFromRoot());
                    var result = new HitTestResult(element, context.GlobalPoint, localPoint, context.HitPath.ToArray());
                    results.Add(result);
                }

                // 测试子元素
                if (context.Options.Filter.ShouldHitTestChildren(element))
                {
                    foreach (var child in element.Children)
                    {
                        HitTestAllRecursive(child, context, results);
                    }
                }
            }
            finally
            {
                context.PopElement();
            }
        }

        /// <summary>
        /// 检查点是否在元素边界内
        /// </summary>
        private bool IsPointInBounds(IHitTestable element, Core.Point point)
        {
            var bounds = GetCachedBounds(element);
            return bounds.Contains(point);
        }

        /// <summary>
        /// 测试元素命中
        /// </summary>
        private bool TestElementHit(IHitTestable element, HitTestContext context)
        {
            if (context.Options.UseBoundingBoxOnly)
            {
                return true; // 已经通过边界测试
            }

            // 转换到元素本地坐标
            var localPoint = context.GetLocalPoint();

            // 调用元素的命中测试方法
            return element.HitTest(localPoint);
        }

        /// <summary>
        /// 获取缓存的边界
        /// </summary>
        private Core.Rect GetCachedBounds(IHitTestable element)
        {
            if (!_boundsCache.TryGetValue(element, out Core.Rect bounds))
            {
                bounds = element.RenderBounds;
                _boundsCache[element] = bounds;
            }
            return bounds;
        }

        /// <summary>
        /// 获取缓存的变换矩阵
        /// </summary>
        private Matrix4x4 GetCachedTransform(IHitTestable element)
        {
            if (!_transformCache.TryGetValue(element, out Matrix4x4 transform))
            {
                transform = element.Transform;
                _transformCache[element] = transform;
            }
            return transform;
        }

        /// <summary>
        /// 变换点
        /// </summary>
        private Core.Point TransformPoint(Core.Point point, Matrix4x4 transform)
        {
            var vector = new Vector4((float)point.X, (float)point.Y, 0, 1);
            var transformed = Vector4.Transform(vector, transform);
            return new Core.Point(transformed.X, transformed.Y);
        }

        /// <summary>
        /// 变换矩形
        /// </summary>
        private Core.Rect TransformRect(Core.Rect rect, Matrix4x4 transform)
        {
            var topLeft = TransformPoint(new Core.Point(rect.Left, rect.Top), transform);
            var bottomRight = TransformPoint(new Core.Point(rect.Right, rect.Bottom), transform);

            return new Core.Rect(
                Math.Min(topLeft.X, bottomRight.X),
                Math.Min(topLeft.Y, bottomRight.Y),
                Math.Abs(bottomRight.X - topLeft.X),
                Math.Abs(bottomRight.Y - topLeft.Y));
        }

        /// <summary>
        /// 矩形相交
        /// </summary>
        private Core.Rect? IntersectRects(Core.Rect rect1, Core.Rect rect2)
        {
            var left = Math.Max(rect1.Left, rect2.Left);
            var top = Math.Max(rect1.Top, rect2.Top);
            var right = Math.Min(rect1.Right, rect2.Right);
            var bottom = Math.Min(rect1.Bottom, rect2.Bottom);

            if (left < right && top < bottom)
            {
                return new Core.Rect(left, top, right - left, bottom - top);
            }

            return null;
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            _boundsCache.Clear();
            _transformCache.Clear();
        }

        /// <summary>
        /// 获取缓存统计
        /// </summary>
        public (int boundsCount, int transformCount) GetCacheStats()
        {
            return (_boundsCache.Count, _transformCache.Count);
        }
    }
}
