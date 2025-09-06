using System;
using System.Collections.Generic;
using System.Diagnostics;
using AetherUI.Core;

namespace AetherUI.Rendering
{
    /// <summary>
    /// 窗口大小变化事件参数
    /// </summary>
    public class WindowResizeEventArgs : EventArgs
    {
        /// <summary>
        /// 旧尺寸
        /// </summary>
        public Size OldSize { get; }

        /// <summary>
        /// 新尺寸
        /// </summary>
        public Size NewSize { get; }

        /// <summary>
        /// 尺寸变化量
        /// </summary>
        public Size Delta => new Size(NewSize.Width - OldSize.Width, NewSize.Height - OldSize.Height);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="oldSize">旧尺寸</param>
        /// <param name="newSize">新尺寸</param>
        public WindowResizeEventArgs(Size oldSize, Size newSize)
        {
            OldSize = oldSize;
            NewSize = newSize;
        }
    }

    /// <summary>
    /// 窗口大小变化监听器接口
    /// </summary>
    public interface IWindowResizeListener
    {
        /// <summary>
        /// 处理窗口大小变化
        /// </summary>
        /// <param name="args">事件参数</param>
        void OnWindowResize(WindowResizeEventArgs args);
    }

    /// <summary>
    /// 窗口大小变化管理器
    /// </summary>
    public class WindowResizeManager
    {
        private readonly List<IWindowResizeListener> _listeners = new();
        private readonly List<Action<WindowResizeEventArgs>> _callbacks = new();
        private Size _currentSize;
        private bool _isResizing = false;
        private DateTime _lastResizeTime = DateTime.Now;
        private const int RESIZE_DEBOUNCE_MS = 50; // 防抖延迟

        #region 事件

        /// <summary>
        /// 窗口大小变化事件
        /// </summary>
        public event EventHandler<WindowResizeEventArgs>? WindowResized;

        /// <summary>
        /// 窗口大小变化开始事件
        /// </summary>
        public event EventHandler<WindowResizeEventArgs>? WindowResizeStarted;

        /// <summary>
        /// 窗口大小变化结束事件
        /// </summary>
        public event EventHandler<WindowResizeEventArgs>? WindowResizeCompleted;

        #endregion

        #region 属性

        /// <summary>
        /// 当前窗口尺寸
        /// </summary>
        public Size CurrentSize => _currentSize;

        /// <summary>
        /// 是否正在调整大小
        /// </summary>
        public bool IsResizing => _isResizing;

        /// <summary>
        /// 注册的监听器数量
        /// </summary>
        public int ListenerCount => _listeners.Count + _callbacks.Count;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化窗口大小变化管理器
        /// </summary>
        /// <param name="initialSize">初始尺寸</param>
        public WindowResizeManager(Size initialSize)
        {
            _currentSize = initialSize;
}

        #endregion

        #region 监听器管理

        /// <summary>
        /// 添加窗口大小变化监听器
        /// </summary>
        /// <param name="listener">监听器</param>
        public void AddListener(IWindowResizeListener listener)
        {
            if (listener != null && !_listeners.Contains(listener))
            {
                _listeners.Add(listener);
}
        }

        /// <summary>
        /// 移除窗口大小变化监听器
        /// </summary>
        /// <param name="listener">监听器</param>
        public void RemoveListener(IWindowResizeListener listener)
        {
            if (listener != null && _listeners.Remove(listener))
            {
}
        }

        /// <summary>
        /// 添加窗口大小变化回调
        /// </summary>
        /// <param name="callback">回调函数</param>
        public void AddCallback(Action<WindowResizeEventArgs> callback)
        {
            if (callback != null && !_callbacks.Contains(callback))
            {
                _callbacks.Add(callback);
}
        }

        /// <summary>
        /// 移除窗口大小变化回调
        /// </summary>
        /// <param name="callback">回调函数</param>
        public void RemoveCallback(Action<WindowResizeEventArgs> callback)
        {
            if (callback != null && _callbacks.Remove(callback))
            {
}
        }

        /// <summary>
        /// 清除所有监听器和回调
        /// </summary>
        public void ClearAll()
        {
            _listeners.Clear();
            _callbacks.Clear();
}

        #endregion

        #region 大小变化处理

        /// <summary>
        /// 通知窗口大小变化
        /// </summary>
        /// <param name="newSize">新尺寸</param>
        public void NotifyResize(Size newSize)
        {
            if (newSize.Width <= 0 || newSize.Height <= 0)
            {
return;
            }

            Size oldSize = _currentSize;
            
            // 检查是否真的发生了变化
            if (Math.Abs(newSize.Width - oldSize.Width) < 0.1 && 
                Math.Abs(newSize.Height - oldSize.Height) < 0.1)
            {
                return; // 没有实质性变化
            }

            _currentSize = newSize;
            _lastResizeTime = DateTime.Now;

            var args = new WindowResizeEventArgs(oldSize, newSize);

            // 如果不在调整大小状态，标记开始
            if (!_isResizing)
            {
                _isResizing = true;
                OnResizeStarted(args);
            }

            // 通知大小变化
            OnResize(args);
}

        /// <summary>
        /// 更新调整大小状态（应该在每帧调用）
        /// </summary>
        public void Update()
        {
            // 检查是否应该结束调整大小状态
            if (_isResizing && (DateTime.Now - _lastResizeTime).TotalMilliseconds > RESIZE_DEBOUNCE_MS)
            {
                _isResizing = false;
                OnResizeCompleted(new WindowResizeEventArgs(_currentSize, _currentSize));
            }
        }

        /// <summary>
        /// 强制完成调整大小
        /// </summary>
        public void ForceCompleteResize()
        {
            if (_isResizing)
            {
                _isResizing = false;
                OnResizeCompleted(new WindowResizeEventArgs(_currentSize, _currentSize));
            }
        }

        #endregion

        #region 事件触发

        /// <summary>
        /// 触发窗口大小变化事件
        /// </summary>
        /// <param name="args">事件参数</param>
        protected virtual void OnResize(WindowResizeEventArgs args)
        {
            try
            {
                // 触发事件
                WindowResized?.Invoke(this, args);

                // 通知监听器
                foreach (var listener in _listeners)
                {
                    try
                    {
                        listener.OnWindowResize(args);
                    }
                    catch (Exception ex)
                    {
}
                }

                // 调用回调
                foreach (var callback in _callbacks)
                {
                    try
                    {
                        callback(args);
                    }
                    catch (Exception ex)
                    {
}
                }
            }
            catch (Exception ex)
            {
}
        }

        /// <summary>
        /// 触发窗口大小变化开始事件
        /// </summary>
        /// <param name="args">事件参数</param>
        protected virtual void OnResizeStarted(WindowResizeEventArgs args)
        {
            try
            {
                WindowResizeStarted?.Invoke(this, args);
}
            catch (Exception ex)
            {
}
        }

        /// <summary>
        /// 触发窗口大小变化结束事件
        /// </summary>
        /// <param name="args">事件参数</param>
        protected virtual void OnResizeCompleted(WindowResizeEventArgs args)
        {
            try
            {
                WindowResizeCompleted?.Invoke(this, args);
}
            catch (Exception ex)
            {
}
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取窗口宽高比
        /// </summary>
        /// <returns>宽高比</returns>
        public double GetAspectRatio()
        {
            return _currentSize.Height > 0 ? _currentSize.Width / _currentSize.Height : 1.0;
        }

        /// <summary>
        /// 检查是否为有效尺寸
        /// </summary>
        /// <param name="size">尺寸</param>
        /// <returns>是否有效</returns>
        public static bool IsValidSize(Size size)
        {
            return size.Width > 0 && size.Height > 0 && 
                   size.Width < 10000 && size.Height < 10000; // 合理的最大值
        }

        /// <summary>
        /// 获取状态信息
        /// </summary>
        /// <returns>状态字符串</returns>
        public string GetStatusInfo()
        {
            return $"Size: {_currentSize}, Resizing: {_isResizing}, Listeners: {ListenerCount}";
        }

        #endregion
    }
}
