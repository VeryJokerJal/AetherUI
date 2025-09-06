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
        private Rendering.Input.PointerInputManager? _pointerInput;
        private MouseState _prevMouseState;

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
                    _pointerInput?.SetRoot(_rootElement);
                    RootElementChanged?.Invoke(this, value);
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
            RenderContext = new RenderContext();
            RenderContext.SetViewport(ClientSize.X, ClientSize.Y);

            // 初始化UI渲染器
            _uiRenderer = new UIRenderer(RenderContext);

            // 初始化背景效果渲染器
            unsafe
            {
                BackgroundRenderer = new BackgroundEffectRenderer(_uiRenderer.ShaderManager, new IntPtr(WindowPtr));
            }

            // 初始化指针输入
            _pointerInput = new Rendering.Input.PointerInputManager(_rootElement);
            _prevMouseState = MouseState;

            // 初始化窗口大小变化管理器
            ResizeManager = new WindowResizeManager(new Size(ClientSize.X, ClientSize.Y));
            ResizeManager.WindowResized += OnWindowResizedInternal;

            // 设置窗口可见
            IsVisible = true;
Debug.WriteLine($"OpenGL Renderer: {GL.GetString(StringName.Renderer)}");
        }

        /// <summary>
        /// 窗口卸载时调用
        /// </summary>
        protected override void OnUnload()
        {
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
}

        /// <summary>
        /// 窗口大小改变时调用
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            RenderContext?.SetViewport(e.Width, e.Height);

            // 通知窗口大小变化管理器
            ResizeManager?.NotifyResize(new Size(e.Width, e.Height));

            // 清理渲染缓存
            ClearRenderCaches();

            // 标记需要重新布局
            _needsLayout = true;

            // 强制立即重绘以避免黑色区域
            ForceRedraw();
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

                // 确保视口尺寸与当前窗口尺寸一致
                if (RenderContext.ViewportSize.Width != ClientSize.X || RenderContext.ViewportSize.Height != ClientSize.Y)
                {
                    RenderContext.SetViewport(ClientSize.X, ClientSize.Y);
                    _needsLayout = true;
}

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

            // 轮询鼠标
            MouseState mouse = MouseState;
            if (_pointerInput != null)
            {
                var pos = mouse.Position;
                AetherUI.Core.Point p = new AetherUI.Core.Point(pos.X, pos.Y);

                if (pos != _prevMouseState.Position)
                {
                    _pointerInput.OnMouseMove(p);
                }
                if (mouse.IsButtonDown(MouseButton.Left) && !_prevMouseState.IsButtonDown(MouseButton.Left))
                {
                    _pointerInput.OnMouseDown(p);
                }
                if (!mouse.IsButtonDown(MouseButton.Left) && _prevMouseState.IsButtonDown(MouseButton.Left))
                {
                    _pointerInput.OnMouseUp(p);
                }
            }
            _prevMouseState = mouse;

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

            // 确保可用尺寸有效
            if (availableSize.Width <= 0 || availableSize.Height <= 0)
            {
return;
            }
try
            {
                // 测量根元素
                _rootElement.Measure(availableSize);
                Rect finalRect = new(0, 0, availableSize.Width, availableSize.Height);
                _rootElement.Arrange(finalRect);
}
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        /// 清理渲染缓存（在窗口大小改变时调用）
        /// </summary>
        private void ClearRenderCaches()
        {
            try
            {
                _uiRenderer?.ClearCaches();

                // 强制垃圾回收以清理可能的纹理缓存
                GC.Collect();
                GC.WaitForPendingFinalizers();
}
            catch (Exception ex)
            {
}
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

        /// <summary>
        /// 强制重绘窗口
        /// </summary>
        public void ForceRedraw()
        {
            try
            {
                // 确保在主线程上执行
                if (RenderContext != null)
                {
                    // 立即执行一次渲染循环
                    if (_needsLayout && _rootElement != null)
                    {
                        PerformLayout();
                        _needsLayout = false;
                    }

                    // 开始渲染帧
                    RenderContext.BeginFrame();

                    // 渲染背景效果
                    if (BackgroundRenderer != null)
                    {
                        Vector2 resolution = new((float)RenderContext.ViewportSize.Width, (float)RenderContext.ViewportSize.Height);
                        BackgroundRenderer.RenderBackground(RenderContext.MVPMatrix, resolution, _time);
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
}
            }
            catch (Exception ex)
            {
}
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
}

        #endregion
    }
}
