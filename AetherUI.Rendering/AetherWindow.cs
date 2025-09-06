using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using AetherUI.Core;

namespace AetherUI.Rendering
{
    /// <summary>
    /// AetherUI窗口，基于OpenTK的游戏窗口
    /// </summary>
    public class AetherWindow : GameWindow
    {
        private RenderContext? _renderContext;
        private UIElement? _rootElement;
        private bool _needsLayout = true;

        #region 事件

        /// <summary>
        /// 根元素变更事件
        /// </summary>
        public event EventHandler<UIElement?>? RootElementChanged;

        #endregion

        #region 属性

        /// <summary>
        /// 渲染上下文
        /// </summary>
        public RenderContext? RenderContext => _renderContext;

        /// <summary>
        /// 根UI元素
        /// </summary>
        public UIElement? RootElement
        {
            get => _rootElement;
            set
            {
                if (_rootElement != value)
                {
                    _rootElement = value;
                    _needsLayout = true;
                    RootElementChanged?.Invoke(this, value);
                    Debug.WriteLine($"Root element changed to: {value?.GetType().Name ?? "null"}");
                }
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化AetherWindow
        /// </summary>
        /// <param name="gameWindowSettings">游戏窗口设置</param>
        /// <param name="nativeWindowSettings">本地窗口设置</param>
        public AetherWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Debug.WriteLine("AetherWindow created");
        }

        /// <summary>
        /// 使用默认设置初始化AetherWindow
        /// </summary>
        /// <param name="width">窗口宽度</param>
        /// <param name="height">窗口高度</param>
        /// <param name="title">窗口标题</param>
        public AetherWindow(int width, int height, string title)
            : this(GameWindowSettings.Default, new NativeWindowSettings()
            {
                ClientSize = new Vector2i(width, height),
                Title = title,
                WindowBorder = WindowBorder.Resizable,
                WindowState = WindowState.Normal,
                StartVisible = false,
                API = ContextAPI.OpenGL,
                APIVersion = new Version(4, 6),
                Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug,
                Profile = ContextProfile.Core,
                NumberOfSamples = 4 // 4x MSAA
            })
        {
        }

        #endregion

        #region OpenTK重写

        /// <summary>
        /// 窗口加载时调用
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();

            Debug.WriteLine("AetherWindow loading...");

            // 初始化渲染上下文
            _renderContext = new RenderContext();
            _renderContext.SetViewport(ClientSize.X, ClientSize.Y);

            // 设置窗口可见
            IsVisible = true;

            Debug.WriteLine($"AetherWindow loaded. OpenGL Version: {GL.GetString(StringName.Version)}");
            Debug.WriteLine($"OpenGL Renderer: {GL.GetString(StringName.Renderer)}");
        }

        /// <summary>
        /// 窗口卸载时调用
        /// </summary>
        protected override void OnUnload()
        {
            Debug.WriteLine("AetherWindow unloading...");

            // 释放渲染上下文
            _renderContext?.Dispose();
            _renderContext = null;

            base.OnUnload();

            Debug.WriteLine("AetherWindow unloaded");
        }

        /// <summary>
        /// 窗口大小改变时调用
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            Debug.WriteLine($"AetherWindow resized to: {e.Width}x{e.Height}");

            // 更新渲染上下文视口
            _renderContext?.SetViewport(e.Width, e.Height);

            // 标记需要重新布局
            _needsLayout = true;
        }

        /// <summary>
        /// 渲染帧时调用
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            if (_renderContext == null)
                return;

            try
            {
                // 开始渲染帧
                _renderContext.BeginFrame();

                // 执行布局（如果需要）
                if (_needsLayout && _rootElement != null)
                {
                    PerformLayout();
                    _needsLayout = false;
                }

                // 渲染UI元素
                if (_rootElement != null)
                {
                    RenderElement(_rootElement);
                }

                // 结束渲染帧
                _renderContext.EndFrame();

                // 交换缓冲区
                SwapBuffers();

                // 检查OpenGL错误
                _renderContext.CheckGLError("OnRenderFrame");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Render error: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新帧时调用
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // 检查退出条件
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        #endregion

        #region 布局和渲染

        /// <summary>
        /// 执行UI布局
        /// </summary>
        private void PerformLayout()
        {
            if (_rootElement == null || _renderContext == null)
                return;

            Size availableSize = _renderContext.ViewportSize;
            Debug.WriteLine($"Performing layout with available size: {availableSize}");

            // 测量根元素
            _rootElement.Measure(availableSize);
            Debug.WriteLine($"Root element desired size: {_rootElement.DesiredSize}");

            // 排列根元素
            Rect finalRect = new Rect(0, 0, availableSize.Width, availableSize.Height);
            _rootElement.Arrange(finalRect);
            Debug.WriteLine($"Root element arranged to: {finalRect}");
        }

        /// <summary>
        /// 渲染UI元素
        /// </summary>
        /// <param name="element">要渲染的元素</param>
        private void RenderElement(UIElement element)
        {
            if (_renderContext == null || element.Visibility == Visibility.Collapsed)
                return;

            // 这里是基础的渲染框架
            // 实际的渲染逻辑将在后续的渲染管道中实现

            // 渲染一个简单的矩形作为占位符
            RenderElementPlaceholder(element);
        }

        /// <summary>
        /// 渲染元素占位符（简单矩形）
        /// </summary>
        /// <param name="element">元素</param>
        private void RenderElementPlaceholder(UIElement element)
        {
            if (_renderContext == null)
                return;

            // 获取元素的渲染边界
            Rect bounds = new Rect(0, 0, element.RenderSize.Width, element.RenderSize.Height);

            // 设置颜色（根据元素类型）
            Vector4 color = element.GetType().Name switch
            {
                "Button" => new Vector4(0.3f, 0.6f, 1.0f, 0.8f), // 蓝色
                "TextBlock" => new Vector4(0.2f, 0.2f, 0.2f, 1.0f), // 深灰色
                "StackPanel" => new Vector4(1.0f, 0.8f, 0.6f, 0.3f), // 浅橙色
                "Grid" => new Vector4(0.8f, 1.0f, 0.8f, 0.3f), // 浅绿色
                "Canvas" => new Vector4(1.0f, 0.9f, 0.9f, 0.3f), // 浅粉色
                "DockPanel" => new Vector4(0.9f, 0.9f, 1.0f, 0.3f), // 浅蓝色
                _ => new Vector4(0.7f, 0.7f, 0.7f, 0.5f) // 默认灰色
            };

            // 渲染简单的有色矩形
            RenderColoredRect(bounds, color);
        }

        /// <summary>
        /// 渲染有色矩形
        /// </summary>
        /// <param name="rect">矩形区域</param>
        /// <param name="color">颜色</param>
        private void RenderColoredRect(Rect rect, Vector4 color)
        {
            // 这是一个简化的矩形渲染实现
            // 在实际应用中，应该使用现代OpenGL的着色器和顶点缓冲区
            // 这里为了简单起见，暂时跳过实际渲染

            Debug.WriteLine($"Rendering rect: {rect} with color: {color}");

            // TODO: 实现现代OpenGL渲染
            // 1. 创建顶点缓冲区
            // 2. 编写顶点和片段着色器
            // 3. 使用VAO/VBO进行渲染
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 使布局无效，强制重新布局
        /// </summary>
        public void InvalidateLayout()
        {
            _needsLayout = true;
        }

        #endregion
    }
}
