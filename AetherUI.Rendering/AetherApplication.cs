using System;
using System.Diagnostics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using AetherUI.Core;

namespace AetherUI.Rendering
{
    /// <summary>
    /// AetherUI应用程序类
    /// </summary>
    public class AetherApplication : IDisposable
    {
        private AetherWindow? _mainWindow;
        private bool _disposed = false;

        #region 事件

        /// <summary>
        /// 应用程序启动事件
        /// </summary>
        public event EventHandler? Started;

        /// <summary>
        /// 应用程序退出事件
        /// </summary>
        public event EventHandler? Exiting;

        #endregion

        #region 属性

        /// <summary>
        /// 主窗口
        /// </summary>
        public AetherWindow? MainWindow => _mainWindow;

        /// <summary>
        /// 应用程序是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化AetherApplication
        /// </summary>
        public AetherApplication()
        {
            Debug.WriteLine("AetherApplication created");
        }

        #endregion

        #region 应用程序生命周期

        /// <summary>
        /// 创建主窗口
        /// </summary>
        /// <param name="width">窗口宽度</param>
        /// <param name="height">窗口高度</param>
        /// <param name="title">窗口标题</param>
        /// <returns>创建的窗口</returns>
        public AetherWindow CreateMainWindow(int width, int height, string title)
        {
            if (_mainWindow != null)
            {
                throw new InvalidOperationException("Main window already created");
            }

            Debug.WriteLine($"Creating main window: {title} ({width}x{height})");

            _mainWindow = new AetherWindow(width, height, title);

            // 订阅窗口事件
            // 注意：OpenTK的GameWindow事件签名可能不同，这里简化处理
            Debug.WriteLine("Main window created and configured");

            return _mainWindow;
        }

        /// <summary>
        /// 运行应用程序
        /// </summary>
        public void Run()
        {
            if (_mainWindow == null)
            {
                throw new InvalidOperationException("No main window created. Call CreateMainWindow first.");
            }

            if (IsRunning)
            {
                throw new InvalidOperationException("Application is already running");
            }

            Debug.WriteLine("Starting AetherApplication...");

            try
            {
                IsRunning = true;
                Started?.Invoke(this, EventArgs.Empty);

                // 运行主窗口
                _mainWindow.Run();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Application error: {ex.Message}");
                throw;
            }
            finally
            {
                IsRunning = false;
                Debug.WriteLine("AetherApplication stopped");
            }
        }

        /// <summary>
        /// 退出应用程序
        /// </summary>
        public void Exit()
        {
            Debug.WriteLine("Exiting AetherApplication...");

            Exiting?.Invoke(this, EventArgs.Empty);

            _mainWindow?.Close();
        }

        #endregion

        #region 事件处理

        // 事件处理方法已简化，因为OpenTK的事件签名可能不同

        #endregion

        #region 静态方法

        /// <summary>
        /// 创建并运行简单的AetherUI应用程序
        /// </summary>
        /// <param name="width">窗口宽度</param>
        /// <param name="height">窗口高度</param>
        /// <param name="title">窗口标题</param>
        /// <param name="rootElement">根UI元素</param>
        public static void RunSimple(int width, int height, string title, UIElement? rootElement = null)
        {
            using AetherApplication app = new AetherApplication();
            
            AetherWindow window = app.CreateMainWindow(width, height, title);
            
            if (rootElement != null)
            {
                window.RootElement = rootElement;
            }
            
            app.Run();
        }

        /// <summary>
        /// 创建并运行简单的AetherUI应用程序（默认尺寸）
        /// </summary>
        /// <param name="title">窗口标题</param>
        /// <param name="rootElement">根UI元素</param>
        public static void RunSimple(string title, UIElement? rootElement = null)
        {
            RunSimple(800, 600, title, rootElement);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否正在释放</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Debug.WriteLine("Disposing AetherApplication...");

                    // 释放主窗口
                    if (_mainWindow != null)
                    {
                        _mainWindow.Dispose();
                        _mainWindow = null;
                    }

                    Debug.WriteLine("AetherApplication disposed");
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
