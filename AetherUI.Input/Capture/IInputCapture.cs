using System;
using System.Collections.Generic;
using AetherUI.Input.Core;
using AetherUI.Input.Events;

namespace AetherUI.Input.Capture
{
    /// <summary>
    /// 捕获类型
    /// </summary>
    public enum CaptureType
    {
        /// <summary>
        /// 无捕获
        /// </summary>
        None,

        /// <summary>
        /// 元素捕获（显式捕获）
        /// </summary>
        Element,

        /// <summary>
        /// 子树捕获（捕获元素及其子元素）
        /// </summary>
        SubTree
    }

    /// <summary>
    /// 捕获信息
    /// </summary>
    public class CaptureInfo
    {
        /// <summary>
        /// 捕获的元素
        /// </summary>
        public object CapturedElement { get; }

        /// <summary>
        /// 指针ID
        /// </summary>
        public PointerId PointerId { get; }

        /// <summary>
        /// 捕获类型
        /// </summary>
        public CaptureType CaptureType { get; }

        /// <summary>
        /// 捕获时间
        /// </summary>
        public DateTime CaptureTime { get; }

        /// <summary>
        /// 是否自动释放
        /// </summary>
        public bool AutoRelease { get; }

        /// <summary>
        /// 初始化捕获信息
        /// </summary>
        /// <param name="capturedElement">捕获的元素</param>
        /// <param name="pointerId">指针ID</param>
        /// <param name="captureType">捕获类型</param>
        /// <param name="autoRelease">是否自动释放</param>
        public CaptureInfo(object capturedElement, PointerId pointerId, CaptureType captureType, bool autoRelease = true)
        {
            CapturedElement = capturedElement ?? throw new ArgumentNullException(nameof(capturedElement));
            PointerId = pointerId;
            CaptureType = captureType;
            CaptureTime = DateTime.UtcNow;
            AutoRelease = autoRelease;
        }

        public override string ToString() =>
            $"Capture: {CapturedElement} ({PointerId}, {CaptureType})";
    }

    /// <summary>
    /// 输入捕获管理器接口
    /// </summary>
    public interface IInputCaptureManager
    {
        /// <summary>
        /// 捕获变化事件
        /// </summary>
        event EventHandler<CaptureChangedEventArgs>? CaptureChanged;

        /// <summary>
        /// 捕获指针到元素
        /// </summary>
        /// <param name="element">要捕获的元素</param>
        /// <param name="pointerId">指针ID</param>
        /// <param name="captureType">捕获类型</param>
        /// <param name="autoRelease">是否自动释放</param>
        /// <returns>是否成功捕获</returns>
        bool CapturePointer(object element, PointerId pointerId, CaptureType captureType = CaptureType.Element, bool autoRelease = true);

        /// <summary>
        /// 释放指针捕获
        /// </summary>
        /// <param name="pointerId">指针ID</param>
        /// <returns>是否成功释放</returns>
        bool ReleasePointerCapture(PointerId pointerId);

        /// <summary>
        /// 释放元素的所有捕获
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>释放的捕获数量</returns>
        int ReleaseElementCaptures(object element);

        /// <summary>
        /// 获取指针的捕获信息
        /// </summary>
        /// <param name="pointerId">指针ID</param>
        /// <returns>捕获信息，如果未捕获则返回null</returns>
        CaptureInfo? GetCapture(PointerId pointerId);

        /// <summary>
        /// 获取元素的所有捕获
        /// </summary>
        /// <param name="element">元素</param>
        /// <returns>捕获信息集合</returns>
        IEnumerable<CaptureInfo> GetElementCaptures(object element);

        /// <summary>
        /// 检查指针是否被捕获
        /// </summary>
        /// <param name="pointerId">指针ID</param>
        /// <returns>是否被捕获</returns>
        bool IsPointerCaptured(PointerId pointerId);

        /// <summary>
        /// 检查元素是否捕获了指针
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="pointerId">指针ID</param>
        /// <returns>是否捕获了指针</returns>
        bool HasPointerCapture(object element, PointerId pointerId);

        /// <summary>
        /// 获取所有活动的捕获
        /// </summary>
        /// <returns>所有捕获信息</returns>
        IEnumerable<CaptureInfo> GetAllCaptures();

        /// <summary>
        /// 清除所有捕获
        /// </summary>
        void ClearAllCaptures();

        /// <summary>
        /// 处理指针事件的捕获逻辑
        /// </summary>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>捕获的目标元素，如果没有捕获则返回null</returns>
        object? ProcessPointerCapture(PointerEvent pointerEvent);
    }

    /// <summary>
    /// 捕获变化事件参数
    /// </summary>
    public class CaptureChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 指针ID
        /// </summary>
        public PointerId PointerId { get; }

        /// <summary>
        /// 旧的捕获元素
        /// </summary>
        public object? OldCapturedElement { get; }

        /// <summary>
        /// 新的捕获元素
        /// </summary>
        public object? NewCapturedElement { get; }

        /// <summary>
        /// 捕获类型
        /// </summary>
        public CaptureType CaptureType { get; }

        /// <summary>
        /// 初始化捕获变化事件参数
        /// </summary>
        /// <param name="pointerId">指针ID</param>
        /// <param name="oldCapturedElement">旧的捕获元素</param>
        /// <param name="newCapturedElement">新的捕获元素</param>
        /// <param name="captureType">捕获类型</param>
        public CaptureChangedEventArgs(PointerId pointerId, object? oldCapturedElement, object? newCapturedElement, CaptureType captureType)
        {
            PointerId = pointerId;
            OldCapturedElement = oldCapturedElement;
            NewCapturedElement = newCapturedElement;
            CaptureType = captureType;
        }

        public override string ToString() =>
            $"CaptureChanged: {PointerId} from {OldCapturedElement} to {NewCapturedElement}";
    }

    /// <summary>
    /// 捕获策略接口
    /// </summary>
    public interface ICaptureStrategy
    {
        /// <summary>
        /// 检查是否应该自动捕获
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>是否应该自动捕获</returns>
        bool ShouldAutoCapture(object element, PointerEvent pointerEvent);

        /// <summary>
        /// 检查是否应该自动释放捕获
        /// </summary>
        /// <param name="captureInfo">捕获信息</param>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>是否应该自动释放</returns>
        bool ShouldAutoRelease(CaptureInfo captureInfo, PointerEvent pointerEvent);

        /// <summary>
        /// 获取捕获类型
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>捕获类型</returns>
        CaptureType GetCaptureType(object element, PointerEvent pointerEvent);
    }

    /// <summary>
    /// 默认捕获策略
    /// </summary>
    public class DefaultCaptureStrategy : ICaptureStrategy
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static DefaultCaptureStrategy Instance { get; } = new();

        private DefaultCaptureStrategy() { }

        /// <summary>
        /// 检查是否应该自动捕获
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>是否应该自动捕获</returns>
        public bool ShouldAutoCapture(object element, PointerEvent pointerEvent)
        {
            // 鼠标按下时自动捕获
            return pointerEvent.EventType == PointerEventType.Pressed && 
                   pointerEvent.ChangedButton != PointerButton.None;
        }

        /// <summary>
        /// 检查是否应该自动释放捕获
        /// </summary>
        /// <param name="captureInfo">捕获信息</param>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>是否应该自动释放</returns>
        public bool ShouldAutoRelease(CaptureInfo captureInfo, PointerEvent pointerEvent)
        {
            // 鼠标释放时自动释放捕获
            return captureInfo.AutoRelease && 
                   pointerEvent.EventType == PointerEventType.Released &&
                   pointerEvent.Button == PointerButton.None; // 所有按钮都释放了
        }

        /// <summary>
        /// 获取捕获类型
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="pointerEvent">指针事件</param>
        /// <returns>捕获类型</returns>
        public CaptureType GetCaptureType(object element, PointerEvent pointerEvent)
        {
            return CaptureType.Element;
        }
    }

    /// <summary>
    /// 捕获范围
    /// </summary>
    public enum CaptureScope
    {
        /// <summary>
        /// 应用程序范围
        /// </summary>
        Application,

        /// <summary>
        /// 窗口范围
        /// </summary>
        Window,

        /// <summary>
        /// 元素范围
        /// </summary>
        Element
    }

    /// <summary>
    /// 捕获选项
    /// </summary>
    public class CaptureOptions
    {
        /// <summary>
        /// 捕获范围
        /// </summary>
        public CaptureScope Scope { get; set; } = CaptureScope.Application;

        /// <summary>
        /// 是否允许嵌套捕获
        /// </summary>
        public bool AllowNested { get; set; } = false;

        /// <summary>
        /// 捕获超时时间（毫秒）
        /// </summary>
        public int TimeoutMs { get; set; } = 30000; // 30秒

        /// <summary>
        /// 捕获策略
        /// </summary>
        public ICaptureStrategy Strategy { get; set; } = DefaultCaptureStrategy.Instance;

        /// <summary>
        /// 默认选项
        /// </summary>
        public static CaptureOptions Default { get; } = new();
    }
}
