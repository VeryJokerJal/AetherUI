using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AetherUI.Input.HitTesting;

namespace AetherUI.Input.Routing
{
    /// <summary>
    /// 事件路由缓存
    /// </summary>
    public class EventRoutingCache
    {
        private readonly ConcurrentDictionary<RouteCacheKey, CachedRoute> _cache = new();
        private readonly object _cleanupLock = new();
        private DateTime _lastCleanup = DateTime.UtcNow;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(10);

        /// <summary>
        /// 最大缓存大小
        /// </summary>
        public int MaxCacheSize { get; set; } = 1000;

        /// <summary>
        /// 缓存数量
        /// </summary>
        public int Count => _cache.Count;

        /// <summary>
        /// 尝试获取缓存的路由
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="route">路由</param>
        /// <returns>是否找到缓存</returns>
        public bool TryGetRoute(RouteCacheKey key, out EventRoute? route)
        {
            route = null;

            if (_cache.TryGetValue(key, out CachedRoute? cached))
            {
                // 检查是否过期
                if (DateTime.UtcNow - cached.CacheTime < _cacheExpiry)
                {
                    cached.AccessCount++;
                    cached.LastAccess = DateTime.UtcNow;
                    route = cached.Route;
                    return true;
                }
                else
                {
                    // 过期，移除
                    _cache.TryRemove(key, out _);
                }
            }

            // 定期清理
            TryCleanup();

            return false;
        }

        /// <summary>
        /// 缓存路由
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="route">路由</param>
        public void CacheRoute(RouteCacheKey key, EventRoute route)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            // 检查缓存大小限制
            if (_cache.Count >= MaxCacheSize)
            {
                EvictOldestEntries();
            }

            var cached = new CachedRoute(route, DateTime.UtcNow);
            _cache.AddOrUpdate(key, cached, (k, existing) => cached);
        }

        /// <summary>
        /// 使元素相关的缓存失效
        /// </summary>
        /// <param name="element">元素</param>
        public void InvalidateElement(object element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var keysToRemove = new List<RouteCacheKey>();

            foreach (var kvp in _cache)
            {
                if (kvp.Key.ContainsElement(element))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// 获取缓存统计
        /// </summary>
        /// <returns>缓存统计</returns>
        public RoutingCacheStats GetStats()
        {
            var totalAccess = _cache.Values.Sum(c => c.AccessCount);
            var avgAccess = _cache.Count > 0 ? (double)totalAccess / _cache.Count : 0;

            return new RoutingCacheStats
            {
                TotalEntries = _cache.Count,
                TotalAccess = totalAccess,
                AverageAccess = avgAccess,
                MaxCacheSize = MaxCacheSize,
                CacheHitRate = CalculateHitRate()
            };
        }

        /// <summary>
        /// 尝试清理过期缓存
        /// </summary>
        private void TryCleanup()
        {
            if (DateTime.UtcNow - _lastCleanup < _cleanupInterval)
                return;

            lock (_cleanupLock)
            {
                if (DateTime.UtcNow - _lastCleanup < _cleanupInterval)
                    return;

                CleanupExpiredEntries();
                _lastCleanup = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// 清理过期条目
        /// </summary>
        private void CleanupExpiredEntries()
        {
            var now = DateTime.UtcNow;
            var keysToRemove = new List<RouteCacheKey>();

            foreach (var kvp in _cache)
            {
                if (now - kvp.Value.CacheTime >= _cacheExpiry)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// 驱逐最旧的条目
        /// </summary>
        private void EvictOldestEntries()
        {
            var entriesToRemove = _cache.Count - MaxCacheSize + 100; // 一次性移除多个，减少频繁清理
            if (entriesToRemove <= 0) return;

            var oldestEntries = _cache
                .OrderBy(kvp => kvp.Value.LastAccess)
                .Take(entriesToRemove)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in oldestEntries)
            {
                _cache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// 计算缓存命中率
        /// </summary>
        private double CalculateHitRate()
        {
            // 这里需要额外的统计信息来计算准确的命中率
            // 简化实现，返回基于访问次数的估算值
            if (_cache.Count == 0) return 0;

            var totalAccess = _cache.Values.Sum(c => c.AccessCount);
            var uniqueAccess = _cache.Count;

            return uniqueAccess > 0 ? (double)totalAccess / (totalAccess + uniqueAccess) : 0;
        }
    }

    /// <summary>
    /// 路由缓存键
    /// </summary>
    public readonly struct RouteCacheKey : IEquatable<RouteCacheKey>
    {
        /// <summary>
        /// 目标元素
        /// </summary>
        public object Target { get; }

        /// <summary>
        /// 命中路径哈希
        /// </summary>
        public int HitPathHash { get; }

        /// <summary>
        /// 路由事件
        /// </summary>
        public RoutedEventDefinition RoutedEvent { get; }

        /// <summary>
        /// 初始化路由缓存键
        /// </summary>
        /// <param name="target">目标元素</param>
        /// <param name="hitPath">命中路径</param>
        /// <param name="routedEvent">路由事件</param>
        public RouteCacheKey(object target, IReadOnlyList<IHitTestable>? hitPath, RoutedEventDefinition routedEvent)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            RoutedEvent = routedEvent ?? throw new ArgumentNullException(nameof(routedEvent));
            HitPathHash = CalculateHitPathHash(hitPath);
        }

        /// <summary>
        /// 检查是否包含指定元素
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>是否包含</returns>
        public bool ContainsElement(object element)
        {
            return Target == element;
            // 注意：这里简化了实现，实际应该检查整个路径
        }

        /// <summary>
        /// 计算命中路径哈希
        /// </summary>
        private static int CalculateHitPathHash(IReadOnlyList<IHitTestable>? hitPath)
        {
            if (hitPath == null || hitPath.Count == 0)
                return 0;

            var hash = new HashCode();
            foreach (var element in hitPath)
            {
                hash.Add(element.GetHashCode());
            }
            return hash.ToHashCode();
        }

        public bool Equals(RouteCacheKey other)
        {
            return Target.Equals(other.Target) &&
                   HitPathHash == other.HitPathHash &&
                   RoutedEvent.Equals(other.RoutedEvent);
        }

        public override bool Equals(object? obj)
        {
            return obj is RouteCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Target, HitPathHash, RoutedEvent);
        }

        public static bool operator ==(RouteCacheKey left, RouteCacheKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RouteCacheKey left, RouteCacheKey right)
        {
            return !left.Equals(right);
        }

        public override string ToString() =>
            $"Target: {Target}, HitPathHash: {HitPathHash}, Event: {RoutedEvent.Name}";
    }

    /// <summary>
    /// 缓存的路由
    /// </summary>
    internal class CachedRoute
    {
        /// <summary>
        /// 路由
        /// </summary>
        public EventRoute Route { get; }

        /// <summary>
        /// 缓存时间
        /// </summary>
        public DateTime CacheTime { get; }

        /// <summary>
        /// 最后访问时间
        /// </summary>
        public DateTime LastAccess { get; set; }

        /// <summary>
        /// 访问次数
        /// </summary>
        public int AccessCount { get; set; }

        /// <summary>
        /// 初始化缓存的路由
        /// </summary>
        /// <param name="route">路由</param>
        /// <param name="cacheTime">缓存时间</param>
        public CachedRoute(EventRoute route, DateTime cacheTime)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            CacheTime = cacheTime;
            LastAccess = cacheTime;
            AccessCount = 1;
        }
    }

    /// <summary>
    /// 路由缓存统计
    /// </summary>
    public class RoutingCacheStats
    {
        /// <summary>
        /// 总条目数
        /// </summary>
        public int TotalEntries { get; set; }

        /// <summary>
        /// 总访问次数
        /// </summary>
        public long TotalAccess { get; set; }

        /// <summary>
        /// 平均访问次数
        /// </summary>
        public double AverageAccess { get; set; }

        /// <summary>
        /// 最大缓存大小
        /// </summary>
        public int MaxCacheSize { get; set; }

        /// <summary>
        /// 缓存命中率
        /// </summary>
        public double CacheHitRate { get; set; }

        public override string ToString() =>
            $"Entries: {TotalEntries}/{MaxCacheSize}, Access: {TotalAccess} (avg: {AverageAccess:F1}), HitRate: {CacheHitRate:P2}";
    }
}
