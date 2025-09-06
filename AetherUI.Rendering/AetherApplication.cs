using System;
using AetherUI.Core;

namespace AetherUI.Rendering
{
    /// <summary>
    /// AetherUI应用程序类
    /// </summary>
    public class AetherApplication : IDisposable
    {
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
        public Window? MainWindow { get; private set; }

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
        public Window CreateMainWindow(int width, int height, string title)
        {
            if (MainWindow != null)
            {
                throw new InvalidOperationException("Main window already created");
            }
            MainWindow = new Window(width, height, title);

            // 订阅窗口事件
            // 注意：OpenTK的GameWindow事件签名可能不同，这里简化处理
            return MainWindow;
        }

        /// <summary>
        /// 运行应用程序
        /// </summary>
        public void Run()
        {
            if (MainWindow == null)
            {
                throw new InvalidOperationException("No main window created. Call CreateMainWindow first.");
            }

            if (IsRunning)
            {
                throw new InvalidOperationException("Application is already running");
            }
            try
            {
                IsRunning = true;
                Started?.Invoke(this, EventArgs.Empty);

                // 运行主窗口
                MainWindow.Run();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                IsRunning = false;
            }
        }

        /// <summary>
        /// 退出应用程序
        /// </summary>
        public void Exit()
        {
            Exiting?.Invoke(this, EventArgs.Empty);

            MainWindow?.Close();
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
            using AetherApplication app = new();

            Window window = app.CreateMainWindow(width, height, title);

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
                    if (MainWindow != null)
                    {
                        MainWindow.Dispose();
                        MainWindow = null;
                    }
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
