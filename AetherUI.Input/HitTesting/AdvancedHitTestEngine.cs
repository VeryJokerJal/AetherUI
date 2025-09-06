using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using AetherUI.Input.Core;

namespace AetherUI.Input.HitTesting
{
    /// <summary>
    /// 高级命中测试引擎，支持复杂的可视树场景
    /// </summary>
    public class AdvancedHitTestEngine : IHitTestEngine
    {
        private readonly HitTestOptions _options;
        private readonly SpatialIndex _spatialIndex;
        private readonly TransformCache _transformCache;
        private readonly BoundsCache _boundsCache;

        /// <summary>
        /// 初始化高级命中测试引擎
        /// </summary>
        /// <param name="options">命中测试选项</param>
        public AdvancedHitTestEngine(HitTestOptions? options = null)
        {
            _options = options ?? HitTestOptions.Default;
            _spatialIndex = new SpatialIndex();
            _transformCache = new TransformCache();
            _boundsCache = new BoundsCache();
        }

        /// <summary>
        /// 执行命中测试
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="point">测试点（全局坐标）</param>
        /// <returns>命中测试结果</returns>
        public HitTestResult HitTest(IHitTestable? root, Point point)
        {
            if (root == null)
                return HitTestResult.Empty(point);

            var context = new AdvancedHitTestContext(point, _options);
            
            // 更新空间索引
            UpdateSpatialIndex(root);

            // 使用空间索引快速筛选候选元素
            var candidates = _spatialIndex.Query(point);
            
            // 在候选元素中进行精确命中测试
            var hitElement = HitTestCandidates(candidates, context);

            if (hitElement != null)
            {
                var localPoint = TransformPointToElement(point, hitElement);
                var hitPath = BuildHitPath(root, hitElement);
                return new HitTestResult(hitElement, point, localPoint, hitPath);
            }

            return HitTestResult.Empty(point);
        }

        /// <summary>
        /// 执行命中测试，返回所有命中的元素
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="point">测试点（全局坐标）</param>
        /// <returns>所有命中的元素（从最上层到最下层）</returns>
        public IEnumerable<HitTestResult> HitTestAll(IHitTestable? root, Point point)
        {
            if (root == null)
                yield break;

            var context = new AdvancedHitTestContext(point, _options);
            
            // 更新空间索引
            UpdateSpatialIndex(root);

            // 使用空间索引快速筛选候选元素
            var candidates = _spatialIndex.Query(point);
            
            // 在候选元素中进行精确命中测试
            var hitElements = HitTestAllCandidates(candidates, context);

            // 按Z顺序排序（从高到低）
            foreach (var element in hitElements.OrderByDescending(e => e.ZIndex))
            {
                var localPoint = TransformPointToElement(point, element);
                var hitPath = BuildHitPath(root, element);
                yield return new HitTestResult(element, point, localPoint, hitPath);
            }
        }

        /// <summary>
        /// 执行区域命中测试
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="rect">测试区域（全局坐标）</param>
        /// <returns>命中的元素集合</returns>
        public IEnumerable<HitTestResult> HitTestRegion(IHitTestable? root, Rect rect)
        {
            if (root == null)
                yield break;

            // 更新空间索引
            UpdateSpatialIndex(root);

            // 使用空间索引查询区域内的元素
            var candidates = _spatialIndex.QueryRegion(rect);

            foreach (var element in candidates)
            {
                if (IsElementInRegion(element, rect))
                {
                    var center = rect.Center;
                    var localPoint = TransformPointToElement(center, element);
                    var hitPath = BuildHitPath(root, element);
                    yield return new HitTestResult(element, center, localPoint, hitPath);
                }
            }
        }

        /// <summary>
        /// 更新空间索引
        /// </summary>
        private void UpdateSpatialIndex(IHitTestable root)
        {
            _spatialIndex.Clear();
            BuildSpatialIndex(root, Matrix4x4.Identity);
        }

        /// <summary>
        /// 构建空间索引
        /// </summary>
        private void BuildSpatialIndex(IHitTestable element, Matrix4x4 parentTransform)
        {
            if (!_options.Filter.ShouldHitTest(element))
                return;

            // 计算元素的全局变换
            var elementTransform = Matrix4x4.Multiply(parentTransform, element.Transform);
            _transformCache.Set(element, elementTransform);

            // 计算元素的全局边界
            var globalBounds = TransformRect(element.RenderBounds, elementTransform);
            _boundsCache.Set(element, globalBounds);

            // 添加到空间索引
            _spatialIndex.Insert(element, globalBounds);

            // 递归处理子元素
            if (_options.Filter.ShouldHitTestChildren(element))
            {
                foreach (var child in element.Children)
                {
                    BuildSpatialIndex(child, elementTransform);
                }
            }
        }

        /// <summary>
        /// 在候选元素中进行命中测试
        /// </summary>
        private IHitTestable? HitTestCandidates(IEnumerable<IHitTestable> candidates, AdvancedHitTestContext context)
        {
            // 按Z顺序排序（从高到低）
            var sortedCandidates = candidates
                .Where(c => _options.Filter.ShouldHitTest(c))
                .OrderByDescending(c => c.ZIndex);

            foreach (var candidate in sortedCandidates)
            {
                if (HitTestElement(candidate, context))
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// 在候选元素中进行全部命中测试
        /// </summary>
        private IEnumerable<IHitTestable> HitTestAllCandidates(IEnumerable<IHitTestable> candidates, AdvancedHitTestContext context)
        {
            var hitElements = new List<IHitTestable>();

            foreach (var candidate in candidates)
            {
                if (_options.Filter.ShouldHitTest(candidate) && HitTestElement(candidate, context))
                {
                    hitElements.Add(candidate);
                }
            }

            return hitElements;
        }

        /// <summary>
        /// 测试单个元素
        /// </summary>
        private bool HitTestElement(IHitTestable element, AdvancedHitTestContext context)
        {
            // 检查可见性
            if (!element.IsVisible || element.Opacity <= 0)
                return false;

            // 检查是否可命中测试
            if (!element.IsHitTestVisible)
                return false;

            // 检查边界
            var globalBounds = _boundsCache.Get(element);
            if (!globalBounds.Contains(context.GlobalPoint))
                return false;

            // 检查裁剪
            if (element.ClipBounds.HasValue)
            {
                var globalTransform = _transformCache.Get(element);
                var globalClipBounds = TransformRect(element.ClipBounds.Value, globalTransform);
                if (!globalClipBounds.Contains(context.GlobalPoint))
                    return false;
            }

            // 检查透明度阈值
            if (element.Opacity < 0.01f) // 几乎透明
                return false;

            // 转换到元素本地坐标进行精确测试
            var localPoint = TransformPointToElement(context.GlobalPoint, element);
            return element.HitTest(localPoint);
        }

        /// <summary>
        /// 将点转换到元素本地坐标
        /// </summary>
        private Point TransformPointToElement(Point globalPoint, IHitTestable element)
        {
            var globalTransform = _transformCache.Get(element);
            if (Matrix4x4.Invert(globalTransform, out Matrix4x4 inverseTransform))
            {
                var vector = new Vector4((float)globalPoint.X, (float)globalPoint.Y, 0, 1);
                var transformed = Vector4.Transform(vector, inverseTransform);
                return new Point(transformed.X, transformed.Y);
            }
            return globalPoint;
        }

        /// <summary>
        /// 变换矩形
        /// </summary>
        private Rect TransformRect(Rect rect, Matrix4x4 transform)
        {
            var corners = new[]
            {
                new Vector4((float)rect.Left, (float)rect.Top, 0, 1),
                new Vector4((float)rect.Right, (float)rect.Top, 0, 1),
                new Vector4((float)rect.Left, (float)rect.Bottom, 0, 1),
                new Vector4((float)rect.Right, (float)rect.Bottom, 0, 1)
            };

            var transformedCorners = corners.Select(c => Vector4.Transform(c, transform)).ToArray();

            var minX = transformedCorners.Min(c => c.X);
            var maxX = transformedCorners.Max(c => c.X);
            var minY = transformedCorners.Min(c => c.Y);
            var maxY = transformedCorners.Max(c => c.Y);

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// 构建命中路径
        /// </summary>
        private IReadOnlyList<IHitTestable> BuildHitPath(IHitTestable root, IHitTestable target)
        {
            var path = new List<IHitTestable>();
            var current = target;

            while (current != null)
            {
                path.Insert(0, current);
                if (current == root)
                    break;
                current = current.Parent;
            }

            return path;
        }

        /// <summary>
        /// 检查元素是否在区域内
        /// </summary>
        private bool IsElementInRegion(IHitTestable element, Rect region)
        {
            var elementBounds = _boundsCache.Get(element);
            return elementBounds.IntersectsWith(region);
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            _spatialIndex.Clear();
            _transformCache.Clear();
            _boundsCache.Clear();
        }

        /// <summary>
        /// 获取缓存统计
        /// </summary>
        public (int spatialCount, int transformCount, int boundsCount) GetCacheStats()
        {
            return (_spatialIndex.Count, _transformCache.Count, _boundsCache.Count);
        }
    }

    /// <summary>
    /// 高级命中测试上下文
    /// </summary>
    public class AdvancedHitTestContext : HitTestContext
    {
        /// <summary>
        /// 当前透明度
        /// </summary>
        public float Opacity { get; set; } = 1.0f;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 初始化高级命中测试上下文
        /// </summary>
        /// <param name="globalPoint">测试点</param>
        /// <param name="options">测试选项</param>
        public AdvancedHitTestContext(Point globalPoint, HitTestOptions? options = null)
            : base(globalPoint, options)
        {
        }
    }
}
