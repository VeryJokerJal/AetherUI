using System;
using System.Collections.Generic;
using System.Numerics;
using AetherUI.Input.Core;

namespace AetherUI.Input.HitTesting
{
    /// <summary>
    /// 变换缓存
    /// </summary>
    public class TransformCache
    {
        private readonly Dictionary<IHitTestable, CachedTransform> _cache = new();
        private uint _currentVersion = 1;

        /// <summary>
        /// 缓存数量
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// 设置元素变换
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="transform">变换矩阵</param>
        public void Set(IHitTestable element, Matrix4x4 transform)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            _cache[element] = new CachedTransform(transform, _currentVersion);
        }

        /// <summary>
        /// 获取元素变换
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>变换矩阵</returns>
        public Matrix4x4 Get(IHitTestable element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (_cache.TryGetValue(element, out CachedTransform cached))
            {
                return cached.Transform;
            }

            // 如果缓存中没有，计算并缓存
            var transform = element.GetTransformToRoot();
            Set(element, transform);
            return transform;
        }

        /// <summary>
        /// 检查缓存是否有效
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>是否有效</returns>
        public bool IsValid(IHitTestable element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return _cache.TryGetValue(element, out CachedTransform cached) && 
                   cached.Version == _currentVersion;
        }

        /// <summary>
        /// 使缓存失效
        /// </summary>
        /// <param name="element">元素</param>
        public void Invalidate(IHitTestable element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            _cache.Remove(element);
        }

        /// <summary>
        /// 使所有缓存失效
        /// </summary>
        public void InvalidateAll()
        {
            _currentVersion++;
            if (_currentVersion == 0) // 防止溢出
            {
                _currentVersion = 1;
                _cache.Clear();
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
            _currentVersion = 1;
        }

        /// <summary>
        /// 清理过期缓存
        /// </summary>
        public void Cleanup()
        {
            var keysToRemove = new List<IHitTestable>();

            foreach (var kvp in _cache)
            {
                if (kvp.Value.Version != _currentVersion)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }
    }

    /// <summary>
    /// 边界缓存
    /// </summary>
    public class BoundsCache
    {
        private readonly Dictionary<IHitTestable, CachedBounds> _cache = new();
        private uint _currentVersion = 1;

        /// <summary>
        /// 缓存数量
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// 设置元素边界
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="bounds">边界</param>
        public void Set(IHitTestable element, Rect bounds)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            _cache[element] = new CachedBounds(bounds, _currentVersion);
        }

        /// <summary>
        /// 获取元素边界
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>边界</returns>
        public Rect Get(IHitTestable element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (_cache.TryGetValue(element, out CachedBounds cached))
            {
                return cached.Bounds;
            }

            // 如果缓存中没有，返回元素的渲染边界
            var bounds = element.RenderBounds;
            Set(element, bounds);
            return bounds;
        }

        /// <summary>
        /// 检查缓存是否有效
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>是否有效</returns>
        public bool IsValid(IHitTestable element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return _cache.TryGetValue(element, out CachedBounds cached) && 
                   cached.Version == _currentVersion;
        }

        /// <summary>
        /// 使缓存失效
        /// </summary>
        /// <param name="element">元素</param>
        public void Invalidate(IHitTestable element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            _cache.Remove(element);
        }

        /// <summary>
        /// 使所有缓存失效
        /// </summary>
        public void InvalidateAll()
        {
            _currentVersion++;
            if (_currentVersion == 0) // 防止溢出
            {
                _currentVersion = 1;
                _cache.Clear();
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
            _currentVersion = 1;
        }

        /// <summary>
        /// 清理过期缓存
        /// </summary>
        public void Cleanup()
        {
            var keysToRemove = new List<IHitTestable>();

            foreach (var kvp in _cache)
            {
                if (kvp.Value.Version != _currentVersion)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }
    }

    /// <summary>
    /// 缓存的变换
    /// </summary>
    internal readonly struct CachedTransform
    {
        /// <summary>
        /// 变换矩阵
        /// </summary>
        public Matrix4x4 Transform { get; }

        /// <summary>
        /// 缓存版本
        /// </summary>
        public uint Version { get; }

        /// <summary>
        /// 初始化缓存的变换
        /// </summary>
        /// <param name="transform">变换矩阵</param>
        /// <param name="version">缓存版本</param>
        public CachedTransform(Matrix4x4 transform, uint version)
        {
            Transform = transform;
            Version = version;
        }
    }

    /// <summary>
    /// 缓存的边界
    /// </summary>
    internal readonly struct CachedBounds
    {
        /// <summary>
        /// 边界
        /// </summary>
        public Rect Bounds { get; }

        /// <summary>
        /// 缓存版本
        /// </summary>
        public uint Version { get; }

        /// <summary>
        /// 初始化缓存的边界
        /// </summary>
        /// <param name="bounds">边界</param>
        /// <param name="version">缓存版本</param>
        public CachedBounds(Rect bounds, uint version)
        {
            Bounds = bounds;
            Version = version;
        }
    }

    /// <summary>
    /// 缓存管理器
    /// </summary>
    public class CacheManager
    {
        private readonly TransformCache _transformCache;
        private readonly BoundsCache _boundsCache;

        /// <summary>
        /// 变换缓存
        /// </summary>
        public TransformCache TransformCache => _transformCache;

        /// <summary>
        /// 边界缓存
        /// </summary>
        public BoundsCache BoundsCache => _boundsCache;

        /// <summary>
        /// 初始化缓存管理器
        /// </summary>
        public CacheManager()
        {
            _transformCache = new TransformCache();
            _boundsCache = new BoundsCache();
        }

        /// <summary>
        /// 使元素缓存失效
        /// </summary>
        /// <param name="element">元素</param>
        public void InvalidateElement(IHitTestable element)
        {
            _transformCache.Invalidate(element);
            _boundsCache.Invalidate(element);
        }

        /// <summary>
        /// 使所有缓存失效
        /// </summary>
        public void InvalidateAll()
        {
            _transformCache.InvalidateAll();
            _boundsCache.InvalidateAll();
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void ClearAll()
        {
            _transformCache.Clear();
            _boundsCache.Clear();
        }

        /// <summary>
        /// 清理过期缓存
        /// </summary>
        public void Cleanup()
        {
            _transformCache.Cleanup();
            _boundsCache.Cleanup();
        }

        /// <summary>
        /// 获取缓存统计
        /// </summary>
        /// <returns>缓存统计</returns>
        public (int transformCount, int boundsCount) GetStats()
        {
            return (_transformCache.Count, _boundsCache.Count);
        }
    }
}
