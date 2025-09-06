using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AetherUI.Input.Core;
using AetherUI.Input.Events;

namespace AetherUI.Input.Capture
{
    /// <summary>
    /// 输入捕获管理器实现
    /// </summary>
    public class InputCaptureManager : IInputCaptureManager
    {
        private readonly CaptureOptions _options;
        private readonly ConcurrentDictionary<PointerId, CaptureInfo> _captures = new();

        /// <summary>
        /// 捕获变化事件
        /// </summary>
        public event EventHandler<CaptureChangedEventArgs>? CaptureChanged;

        /// <summary>
        /// 初始化输入捕获管理器
        /// </summary>
        /// <param name="options">捕获选项</param>
        public InputCaptureManager(CaptureOptions? options = null)
        {
            _options = options ?? CaptureOptions.Default;
        }

        /// <summary>
        /// 捕获指针到元素
        /// </summary>
        /// <param name="element">要捕获的元素</param>
        /// <param name="pointerId">指针ID</param>
        /// <param name="captureType">捕获类型</param>
        /// <param name="autoRelease">是否自动释放</param>
        /// <returns>是否成功捕获</returns>
        public bool CapturePointer(object element, PointerId pointerId, CaptureType captureType = CaptureType.Element, bool autoRelease = true)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            try
            {
                // 检查是否允许嵌套捕获
                if (!_options.AllowNested && _captures.ContainsKey(pointerId))
                {
                    Debug.WriteLine($"指针 {pointerId} 已被捕获，且不允许嵌套捕获");
                    return false;
                }

                var oldCapture = _captures.TryGetValue(pointerId, out CaptureInfo? existing) ? existing : null;
                var newCapture = new CaptureInfo(element, pointerId, captureType, autoRelease);

                _captures[pointerId] = newCapture;

                // 触发捕获变化事件
                var eventArgs = new CaptureChangedEventArgs(
                    pointerId,
                    oldCapture?.CapturedElement,
                    element,
                    captureType);

                CaptureChanged?.Invoke(this, eventArgs);

                Debug.WriteLine($"指针 {pointerId} 已捕获到元素 {element}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"捕获指针失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 释放指针捕获
        /// </summary>
        /// <param name="pointerId">指针ID</param>
        /// <returns>是否成功释放</returns>
        public bool ReleasePointerCapture(PointerId pointerId)
        {
            if (_captures.TryRemove(pointerId, out CaptureInfo? capture))
            {
                // 触发捕获变化事件
                var eventArgs = new CaptureChangedEventArgs(
                    pointerId,
                    capture.CapturedElement,
                    null,
                    capture.CaptureType);

                CaptureChanged?.Invoke(this, eventArgs);

                Debug.WriteLine($"指针 {pointerId} 的捕获已释放");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 释放元素的所有捕获
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>释放的捕获数量</returns>
        public int ReleaseElementCaptures(object element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var releasedCount = 0;
            var capturesToRelease = _captures.Values
                .Where(c => c.CapturedElement == element)
                .ToList();

            foreach (var capture in capturesToRelease)
            {
                if (ReleasePointerCapture(capture.PointerId))
                {
                    releasedCount++;
                }
            }

            return releasedCount;
        }

        /// <summary>
        /// 获取指针的捕获信息
        /// </summary>
        /// <param name="pointerId">指针ID</param>
        /// <returns>捕获信息，如果未捕获则返回null</returns>
        public CaptureInfo? GetCapture(PointerId pointerId)
        {
            _captures.TryGetValue(pointerId, out CaptureInfo? capture);
            return capture;
        }

        /// <summary>
        /// 获取元素的所有捕获
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>捕获信息集合</returns>
        public IEnumerable<CaptureInfo> GetElementCaptures(object element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return _captures.Values.Where(c => c.CapturedElement == element);
        }

        /// <summary>
        /// 检查指针是否被捕获
        /// </summary>
        /// <param name="pointerId">指针ID</param>
        /// <returns>是否被捕获</returns>
        public bool IsPointerCaptured(PointerId pointerId)
        {
            return _captures.ContainsKey(pointerId);
        }

        /// <summary>
        /// 检查元素是否捕获了指针
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="pointerId">指针ID</param>
        /// <returns>是否捕获了指针</returns>
        public bool HasPointerCapture(object element, PointerId pointerId)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return _captures.TryGetValue(pointerId, out CaptureInfo? capture) && 
                   capture.CapturedElement == element;
        }

        /// <summary>
        /// 获取所有活动的捕获
        /// </summary>
        /// <returns>所有捕获信息</returns>
        public IEnumerable<CaptureInfo> GetAllCaptures()
        {
            return _captures.Values.ToArray();
        }

        /// <summary>
        /// 清除所有捕获
        /// </summary>
        public void ClearAllCaptures()
        {
            var allCaptures = _captures.Values.ToList();
            _captures.Clear();

            // 触发捕获变化事件
            foreach (var capture in allCaptures)
            {
                var eventArgs = new CaptureChangedEventArgs(
                    capture.PointerId,
                    capture.CapturedElement,
                    null,
                    capture.CaptureType);

                CaptureChanged?.Invoke(this, eventArgs);
            }

            Debug.WriteLine("所有捕获已清除");
        }

        /// <summary>
        /// 处理指针事件的捕获逻辑
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>捕获的目标元素，如果没有捕获则返回null</returns>
        public object? ProcessPointerCapture(PointerEvent pointerEvent)
        {
            var pointerId = pointerEvent.PointerId;

            // 检查是否有现有捕获
            if (_captures.TryGetValue(pointerId, out CaptureInfo? existingCapture))
            {
                // 检查是否应该自动释放捕获
                if (_options.Strategy.ShouldAutoRelease(existingCapture, pointerEvent))
                {
                    ReleasePointerCapture(pointerId);
                    return null;
                }

                return existingCapture.CapturedElement;
            }

            // 检查是否应该自动捕获
            if (pointerEvent.Source != null && _options.Strategy.ShouldAutoCapture(pointerEvent.Source, pointerEvent))
            {
                var captureType = _options.Strategy.GetCaptureType(pointerEvent.Source, pointerEvent);
                if (CapturePointer(pointerEvent.Source, pointerId, captureType))
                {
                    return pointerEvent.Source;
                }
            }

            return null;
        }

        /// <summary>
        /// 清理过期的捕获
        /// </summary>
        public void CleanupExpiredCaptures()
        {
            var now = DateTime.UtcNow;
            var expiredCaptures = _captures.Values
                .Where(c => (now - c.CaptureTime).TotalMilliseconds > _options.TimeoutMs)
                .ToList();

            foreach (var capture in expiredCaptures)
            {
                ReleasePointerCapture(capture.PointerId);
                Debug.WriteLine($"捕获 {capture.PointerId} 已过期并被释放");
            }
        }
    }
}
