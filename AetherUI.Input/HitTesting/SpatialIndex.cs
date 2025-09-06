using System;
using System.Collections.Generic;
using System.Linq;
using AetherUI.Input.Core;

namespace AetherUI.Input.HitTesting
{
    /// <summary>
    /// 空间索引，用于快速查找空间中的元素
    /// </summary>
    public class SpatialIndex
    {
        private readonly QuadTree _quadTree;
        private readonly Dictionary<IHitTestable, Rect> _elementBounds = new();

        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => _elementBounds.Count;

        /// <summary>
        /// 初始化空间索引
        /// </summary>
        /// <param name="bounds">索引边界</param>
        /// <param name="maxDepth">最大深度</param>
        /// <param name="maxElementsPerNode">每个节点最大元素数</param>
        public SpatialIndex(Rect? bounds = null, int maxDepth = 8, int maxElementsPerNode = 10)
        {
            var indexBounds = bounds ?? new Rect(-10000, -10000, 20000, 20000);
            _quadTree = new QuadTree(indexBounds, maxDepth, maxElementsPerNode);
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="bounds">元素边界</param>
        public void Insert(IHitTestable element, Rect bounds)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            // 移除旧的条目（如果存在）
            if (_elementBounds.TryGetValue(element, out Rect oldBounds))
            {
                _quadTree.Remove(element, oldBounds);
            }

            // 插入新条目
            _elementBounds[element] = bounds;
            _quadTree.Insert(element, bounds);
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="element">元素</param>
        public bool Remove(IHitTestable element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (_elementBounds.TryGetValue(element, out Rect bounds))
            {
                _elementBounds.Remove(element);
                return _quadTree.Remove(element, bounds);
            }

            return false;
        }

        /// <summary>
        /// 查询点附近的元素
        /// </summary>
        /// <param name="point">查询点</param>
        /// <returns>附近的元素</returns>
        public IEnumerable<IHitTestable> Query(Point point)
        {
            return _quadTree.Query(point);
        }

        /// <summary>
        /// 查询区域内的元素
        /// </summary>
        /// <param name="region">查询区域</param>
        /// <returns>区域内的元素</returns>
        public IEnumerable<IHitTestable> QueryRegion(Rect region)
        {
            return _quadTree.QueryRegion(region);
        }

        /// <summary>
        /// 清除所有元素
        /// </summary>
        public void Clear()
        {
            _elementBounds.Clear();
            _quadTree.Clear();
        }

        /// <summary>
        /// 获取元素边界
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>元素边界</returns>
        public Rect? GetElementBounds(IHitTestable element)
        {
            return _elementBounds.TryGetValue(element, out Rect bounds) ? bounds : null;
        }
    }

    /// <summary>
    /// 四叉树实现
    /// </summary>
    internal class QuadTree
    {
        private readonly Rect _bounds;
        private readonly int _maxDepth;
        private readonly int _maxElementsPerNode;
        private readonly int _depth;
        private readonly List<QuadTreeElement> _elements = new();
        private QuadTree[]? _children;

        /// <summary>
        /// 初始化四叉树节点
        /// </summary>
        /// <param name="bounds">节点边界</param>
        /// <param name="maxDepth">最大深度</param>
        /// <param name="maxElementsPerNode">每个节点最大元素数</param>
        /// <param name="depth">当前深度</param>
        public QuadTree(Rect bounds, int maxDepth, int maxElementsPerNode, int depth = 0)
        {
            _bounds = bounds;
            _maxDepth = maxDepth;
            _maxElementsPerNode = maxElementsPerNode;
            _depth = depth;
        }

        /// <summary>
        /// 插入元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="bounds">元素边界</param>
        public void Insert(IHitTestable element, Rect bounds)
        {
            if (!_bounds.IntersectsWith(bounds))
                return;

            if (_children == null)
            {
                _elements.Add(new QuadTreeElement(element, bounds));

                // 检查是否需要分割
                if (_elements.Count > _maxElementsPerNode && _depth < _maxDepth)
                {
                    Split();
                }
            }
            else
            {
                // 插入到子节点
                foreach (var child in _children)
                {
                    child.Insert(element, bounds);
                }
            }
        }

        /// <summary>
        /// 移除元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="bounds">元素边界</param>
        /// <returns>是否成功移除</returns>
        public bool Remove(IHitTestable element, Rect bounds)
        {
            if (!_bounds.IntersectsWith(bounds))
                return false;

            bool removed = false;

            if (_children == null)
            {
                for (int i = _elements.Count - 1; i >= 0; i--)
                {
                    if (_elements[i].Element == element)
                    {
                        _elements.RemoveAt(i);
                        removed = true;
                    }
                }
            }
            else
            {
                foreach (var child in _children)
                {
                    if (child.Remove(element, bounds))
                    {
                        removed = true;
                    }
                }
            }

            return removed;
        }

        /// <summary>
        /// 查询点附近的元素
        /// </summary>
        /// <param name="point">查询点</param>
        /// <returns>附近的元素</returns>
        public IEnumerable<IHitTestable> Query(Point point)
        {
            if (!_bounds.Contains(point))
                yield break;

            if (_children == null)
            {
                foreach (var element in _elements)
                {
                    if (element.Bounds.Contains(point))
                    {
                        yield return element.Element;
                    }
                }
            }
            else
            {
                foreach (var child in _children)
                {
                    foreach (var element in child.Query(point))
                    {
                        yield return element;
                    }
                }
            }
        }

        /// <summary>
        /// 查询区域内的元素
        /// </summary>
        /// <param name="region">查询区域</param>
        /// <returns>区域内的元素</returns>
        public IEnumerable<IHitTestable> QueryRegion(Rect region)
        {
            if (!_bounds.IntersectsWith(region))
                yield break;

            if (_children == null)
            {
                foreach (var element in _elements)
                {
                    if (element.Bounds.IntersectsWith(region))
                    {
                        yield return element.Element;
                    }
                }
            }
            else
            {
                foreach (var child in _children)
                {
                    foreach (var element in child.QueryRegion(region))
                    {
                        yield return element;
                    }
                }
            }
        }

        /// <summary>
        /// 清除所有元素
        /// </summary>
        public void Clear()
        {
            _elements.Clear();
            _children = null;
        }

        /// <summary>
        /// 分割节点
        /// </summary>
        private void Split()
        {
            var halfWidth = _bounds.Width / 2;
            var halfHeight = _bounds.Height / 2;

            _children = new QuadTree[4];
            _children[0] = new QuadTree(new Rect(_bounds.X, _bounds.Y, halfWidth, halfHeight), _maxDepth, _maxElementsPerNode, _depth + 1);
            _children[1] = new QuadTree(new Rect(_bounds.X + halfWidth, _bounds.Y, halfWidth, halfHeight), _maxDepth, _maxElementsPerNode, _depth + 1);
            _children[2] = new QuadTree(new Rect(_bounds.X, _bounds.Y + halfHeight, halfWidth, halfHeight), _maxDepth, _maxElementsPerNode, _depth + 1);
            _children[3] = new QuadTree(new Rect(_bounds.X + halfWidth, _bounds.Y + halfHeight, halfWidth, halfHeight), _maxDepth, _maxElementsPerNode, _depth + 1);

            // 将现有元素重新分配到子节点
            foreach (var element in _elements)
            {
                foreach (var child in _children)
                {
                    child.Insert(element.Element, element.Bounds);
                }
            }

            _elements.Clear();
        }
    }

    /// <summary>
    /// 四叉树元素
    /// </summary>
    internal class QuadTreeElement
    {
        /// <summary>
        /// 元素
        /// </summary>
        public IHitTestable Element { get; }

        /// <summary>
        /// 元素边界
        /// </summary>
        public Rect Bounds { get; }

        /// <summary>
        /// 初始化四叉树元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="bounds">元素边界</param>
        public QuadTreeElement(IHitTestable element, Rect bounds)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
            Bounds = bounds;
        }
    }
}
