using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AetherUI.Input.Core;

namespace AetherUI.Input.HitTesting
{
    /// <summary>
    /// 可视树遍历器
    /// </summary>
    public class VisualTreeWalker
    {
        /// <summary>
        /// 遍历策略
        /// </summary>
        public enum TraversalStrategy
        {
            /// <summary>
            /// 深度优先
            /// </summary>
            DepthFirst,

            /// <summary>
            /// 广度优先
            /// </summary>
            BreadthFirst,

            /// <summary>
            /// Z顺序（从前到后）
            /// </summary>
            ZOrder,

            /// <summary>
            /// 反向Z顺序（从后到前）
            /// </summary>
            ReverseZOrder
        }

        /// <summary>
        /// 遍历可视树
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="strategy">遍历策略</param>
        /// <param name="filter">过滤器</param>
        /// <returns>遍历的元素序列</returns>
        public static IEnumerable<IHitTestable> Traverse(
            IHitTestable root, 
            TraversalStrategy strategy = TraversalStrategy.DepthFirst,
            Func<IHitTestable, bool>? filter = null)
        {
            if (root == null)
                yield break;

            filter ??= _ => true;

            switch (strategy)
            {
                case TraversalStrategy.DepthFirst:
                    foreach (var element in TraverseDepthFirst(root, filter))
                        yield return element;
                    break;

                case TraversalStrategy.BreadthFirst:
                    foreach (var element in TraverseBreadthFirst(root, filter))
                        yield return element;
                    break;

                case TraversalStrategy.ZOrder:
                    foreach (var element in TraverseZOrder(root, filter, false))
                        yield return element;
                    break;

                case TraversalStrategy.ReverseZOrder:
                    foreach (var element in TraverseZOrder(root, filter, true))
                        yield return element;
                    break;
            }
        }

        /// <summary>
        /// 深度优先遍历
        /// </summary>
        private static IEnumerable<IHitTestable> TraverseDepthFirst(IHitTestable element, Func<IHitTestable, bool> filter)
        {
            if (filter(element))
                yield return element;

            foreach (var child in element.Children)
            {
                foreach (var descendant in TraverseDepthFirst(child, filter))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// 广度优先遍历
        /// </summary>
        private static IEnumerable<IHitTestable> TraverseBreadthFirst(IHitTestable root, Func<IHitTestable, bool> filter)
        {
            var queue = new Queue<IHitTestable>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (filter(current))
                    yield return current;

                foreach (var child in current.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// Z顺序遍历
        /// </summary>
        private static IEnumerable<IHitTestable> TraverseZOrder(IHitTestable root, Func<IHitTestable, bool> filter, bool reverse)
        {
            var allElements = new List<IHitTestable>();
            CollectAllElements(root, allElements, filter);

            var sorted = reverse 
                ? allElements.OrderByDescending(e => e.ZIndex)
                : allElements.OrderBy(e => e.ZIndex);

            return sorted;
        }

        /// <summary>
        /// 收集所有元素
        /// </summary>
        private static void CollectAllElements(IHitTestable element, List<IHitTestable> collection, Func<IHitTestable, bool> filter)
        {
            if (filter(element))
                collection.Add(element);

            foreach (var child in element.Children)
            {
                CollectAllElements(child, collection, filter);
            }
        }

        /// <summary>
        /// 查找祖先元素
        /// </summary>
        /// <param name="element">起始元素</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>找到的祖先元素</returns>
        public static IHitTestable? FindAncestor(IHitTestable element, Func<IHitTestable, bool> predicate)
        {
            var current = element.Parent;
            while (current != null)
            {
                if (predicate(current))
                    return current;
                current = current.Parent;
            }
            return null;
        }

        /// <summary>
        /// 查找后代元素
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>找到的后代元素</returns>
        public static IHitTestable? FindDescendant(IHitTestable root, Func<IHitTestable, bool> predicate)
        {
            return TraverseDepthFirst(root, _ => true).FirstOrDefault(predicate);
        }

        /// <summary>
        /// 查找所有后代元素
        /// </summary>
        /// <param name="root">根元素</param>
        /// <param name="predicate">查找条件</param>
        /// <returns>找到的所有后代元素</returns>
        public static IEnumerable<IHitTestable> FindAllDescendants(IHitTestable root, Func<IHitTestable, bool> predicate)
        {
            return TraverseDepthFirst(root, predicate).Skip(1); // 跳过根元素
        }

        /// <summary>
        /// 获取元素路径（从根到元素）
        /// </summary>
        /// <param name="element">目标元素</param>
        /// <param name="root">根元素</param>
        /// <returns>元素路径</returns>
        public static IReadOnlyList<IHitTestable> GetPath(IHitTestable element, IHitTestable? root = null)
        {
            var path = new List<IHitTestable>();
            var current = element;

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
        /// 获取公共祖先
        /// </summary>
        /// <param name="element1">元素1</param>
        /// <param name="element2">元素2</param>
        /// <returns>公共祖先</returns>
        public static IHitTestable? GetCommonAncestor(IHitTestable element1, IHitTestable element2)
        {
            var path1 = GetPath(element1);
            var path2 = GetPath(element2);

            IHitTestable? commonAncestor = null;
            int minLength = Math.Min(path1.Count, path2.Count);

            for (int i = 0; i < minLength; i++)
            {
                if (path1[i] == path2[i])
                {
                    commonAncestor = path1[i];
                }
                else
                {
                    break;
                }
            }

            return commonAncestor;
        }

        /// <summary>
        /// 检查元素是否为另一个元素的祖先
        /// </summary>
        /// <param name="ancestor">可能的祖先</param>
        /// <param name="descendant">可能的后代</param>
        /// <returns>是否为祖先关系</returns>
        public static bool IsAncestorOf(IHitTestable ancestor, IHitTestable descendant)
        {
            var current = descendant.Parent;
            while (current != null)
            {
                if (current == ancestor)
                    return true;
                current = current.Parent;
            }
            return false;
        }

        /// <summary>
        /// 获取元素深度
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="root">根元素</param>
        /// <returns>深度</returns>
        public static int GetDepth(IHitTestable element, IHitTestable? root = null)
        {
            int depth = 0;
            var current = element;

            while (current != null && current != root)
            {
                depth++;
                current = current.Parent;
            }

            return depth;
        }

        /// <summary>
        /// 获取兄弟元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>兄弟元素</returns>
        public static IEnumerable<IHitTestable> GetSiblings(IHitTestable element)
        {
            var parent = element.Parent;
            if (parent == null)
                return Enumerable.Empty<IHitTestable>();

            return parent.Children.Where(child => child != element);
        }

        /// <summary>
        /// 获取下一个兄弟元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>下一个兄弟元素</returns>
        public static IHitTestable? GetNextSibling(IHitTestable element)
        {
            var parent = element.Parent;
            if (parent == null)
                return null;

            var siblings = parent.Children.ToList();
            var index = siblings.IndexOf(element);

            return index >= 0 && index < siblings.Count - 1 ? siblings[index + 1] : null;
        }

        /// <summary>
        /// 获取上一个兄弟元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>上一个兄弟元素</returns>
        public static IHitTestable? GetPreviousSibling(IHitTestable element)
        {
            var parent = element.Parent;
            if (parent == null)
                return null;

            var siblings = parent.Children.ToList();
            var index = siblings.IndexOf(element);

            return index > 0 ? siblings[index - 1] : null;
        }

        /// <summary>
        /// 计算可视树统计信息
        /// </summary>
        /// <param name="root">根元素</param>
        /// <returns>统计信息</returns>
        public static VisualTreeStats CalculateStats(IHitTestable root)
        {
            var stats = new VisualTreeStats();
            CalculateStatsRecursive(root, stats, 0);
            return stats;
        }

        /// <summary>
        /// 递归计算统计信息
        /// </summary>
        private static void CalculateStatsRecursive(IHitTestable element, VisualTreeStats stats, int depth)
        {
            stats.TotalElements++;
            stats.MaxDepth = Math.Max(stats.MaxDepth, depth);

            if (element.IsVisible)
                stats.VisibleElements++;

            if (element.IsHitTestVisible)
                stats.HitTestableElements++;

            var childCount = element.Children.Count();
            if (childCount == 0)
                stats.LeafElements++;

            stats.MaxChildrenPerElement = Math.Max(stats.MaxChildrenPerElement, childCount);

            foreach (var child in element.Children)
            {
                CalculateStatsRecursive(child, stats, depth + 1);
            }
        }
    }

    /// <summary>
    /// 可视树统计信息
    /// </summary>
    public class VisualTreeStats
    {
        /// <summary>
        /// 总元素数
        /// </summary>
        public int TotalElements { get; set; }

        /// <summary>
        /// 可见元素数
        /// </summary>
        public int VisibleElements { get; set; }

        /// <summary>
        /// 可命中测试元素数
        /// </summary>
        public int HitTestableElements { get; set; }

        /// <summary>
        /// 叶子元素数
        /// </summary>
        public int LeafElements { get; set; }

        /// <summary>
        /// 最大深度
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// 每个元素的最大子元素数
        /// </summary>
        public int MaxChildrenPerElement { get; set; }

        public override string ToString()
        {
            return $"Elements: {TotalElements}, Visible: {VisibleElements}, HitTestable: {HitTestableElements}, " +
                   $"Leaves: {LeafElements}, MaxDepth: {MaxDepth}, MaxChildren: {MaxChildrenPerElement}";
        }
    }
}
