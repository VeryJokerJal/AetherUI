using System;
using System.Diagnostics;
using AetherUI.Core;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace AetherUI.Rendering
{
    /// <summary>
    /// AetherUI窗口，基于OpenTK的游戏窗口
    /// </summary>
    public class Window : GameWindow
    {
        private UIRenderer? _uiRenderer;
        private UIElement? _rootElement;
        private bool _needsLayout = true;
        private float _time = 0.0f;

        #region 事件

        /// <summary>
        /// 根元素变更事件
        /// </summary>
        public event EventHandler<UIElement?>? RootElementChanged;

        /// <summary>
        /// 窗口大小变化事件
        /// </summary>
        public event EventHandler<WindowResizeEventArgs>? WindowResized;

        #endregion

        #region 属性

        /// <summary>
        /// 渲染上下文
        /// </summary>
        public RenderContext? RenderContext { get; private set; }

        /// <summary>
        /// 背景效果渲染器
        /// </summary>
        public BackgroundEffectRenderer? BackgroundRenderer { get; private set; }

        /// <summary>
        /// 窗口大小变化管理器
        /// </summary>
        public WindowResizeManager? ResizeManager { get; private set; }

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
        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
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
        public Window(int width, int height, string title)
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
            RenderContext = new RenderContext();
            RenderContext.SetViewport(ClientSize.X, ClientSize.Y);

            // 初始化UI渲染器
            _uiRenderer = new UIRenderer(RenderContext);

            // 初始化背景效果渲染器
            unsafe
            {
                BackgroundRenderer = new BackgroundEffectRenderer(_uiRenderer.ShaderManager, new IntPtr(WindowPtr));
            }

            // 初始化窗口大小变化管理器
            ResizeManager = new WindowResizeManager(new Size(ClientSize.X, ClientSize.Y));
            ResizeManager.WindowResized += OnWindowResizedInternal;

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

            // 释放背景效果渲染器
            BackgroundRenderer?.Dispose();
            BackgroundRenderer = null;

            // 释放窗口大小变化管理器
            if (ResizeManager != null)
            {
                ResizeManager.WindowResized -= OnWindowResizedInternal;
                ResizeManager = null;
            }

            // 释放UI渲染器
            _uiRenderer?.Dispose();
            _uiRenderer = null;

            // 释放渲染上下文
            RenderContext?.Dispose();
            RenderContext = null;

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
            RenderContext?.SetViewport(e.Width, e.Height);

            // 通知窗口大小变化管理器
            ResizeManager?.NotifyResize(new Size(e.Width, e.Height));

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

            if (RenderContext == null)
            {
                return;
            }

            try
            {
                // 更新时间
                _time += (float)e.Time;

                // 开始渲染帧
                RenderContext.BeginFrame();

                // 渲染背景效果
                if (BackgroundRenderer != null && RenderContext != null)
                {
                    Vector2 resolution = new((float)RenderContext.ViewportSize.Width, (float)RenderContext.ViewportSize.Height);
                    BackgroundRenderer.RenderBackground(RenderContext.MVPMatrix, resolution, _time);
                }

                // 执行布局（如果需要）
                if (_needsLayout && _rootElement != null)
                {
                    PerformLayout();
                    _needsLayout = false;
                }

                // 渲染UI元素
                if (_rootElement != null && _uiRenderer != null)
                {
                    _uiRenderer.RenderElement(_rootElement);
                }

                // 结束渲染帧
                RenderContext.EndFrame();

                // 交换缓冲区
                SwapBuffers();

                // 检查OpenGL错误
                RenderContext.CheckGLError("OnRenderFrame");
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

            // 更新窗口大小变化管理器
            ResizeManager?.Update();

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
            if (_rootElement == null || RenderContext == null)
            {
                return;
            }

            Size availableSize = RenderContext.ViewportSize;
            Debug.WriteLine($"Performing layout with available size: {availableSize}");

            // 测量根元素
            _rootElement.Measure(availableSize);
            Debug.WriteLine($"Root element desired size: {_rootElement.DesiredSize}");

            // 排列根元素
            Rect finalRect = new(0, 0, availableSize.Width, availableSize.Height);
            _rootElement.Arrange(finalRect);
            Debug.WriteLine($"Root element arranged to: {finalRect}");
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

        /// <summary>
        /// 设置背景效果配置
        /// </summary>
        /// <param name="config">背景效果配置</param>
        public void SetBackgroundEffect(BackgroundEffectConfig config)
        {
            if (BackgroundRenderer != null)
            {
                BackgroundRenderer.Config = config;
                Debug.WriteLine($"Background effect set to: {config.Type}");
            }
        }

        /// <summary>
        /// 获取当前窗口尺寸
        /// </summary>
        /// <returns>窗口尺寸</returns>
        public Size GetWindowSize()
        {
            return ResizeManager?.CurrentSize ?? new Size(ClientSize.X, ClientSize.Y);
        }

        /// <summary>
        /// 添加窗口大小变化监听器
        /// </summary>
        /// <param name="listener">监听器</param>
        public void AddResizeListener(IWindowResizeListener listener)
        {
            ResizeManager?.AddListener(listener);
        }

        /// <summary>
        /// 移除窗口大小变化监听器
        /// </summary>
        /// <param name="listener">监听器</param>
        public void RemoveResizeListener(IWindowResizeListener listener)
        {
            ResizeManager?.RemoveListener(listener);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 内部窗口大小变化事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="args">事件参数</param>
        private void OnWindowResizedInternal(object? sender, WindowResizeEventArgs args)
        {
            // 触发公共事件
            WindowResized?.Invoke(this, args);

            Debug.WriteLine($"Window resize event: {args.OldSize} -> {args.NewSize}");
        }

        #endregion
    }
}
