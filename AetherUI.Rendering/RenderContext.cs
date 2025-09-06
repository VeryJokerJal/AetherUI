using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using AetherUI.Core;

namespace AetherUI.Rendering
{
    /// <summary>
    /// 渲染上下文，管理OpenGL状态和渲染操作
    /// </summary>
    public class RenderContext : IDisposable
    {
        private bool _disposed = false;
        private Matrix4 _viewMatrix;
        private Matrix4 _projectionMatrix;
        private Matrix4 _modelMatrix;
        private Vector4 _clearColor;

        #region 属性

        /// <summary>
        /// 视图矩阵
        /// </summary>
        public Matrix4 ViewMatrix
        {
            get => _viewMatrix;
            set
            {
                _viewMatrix = value;
                UpdateMVPMatrix();
            }
        }

        /// <summary>
        /// 投影矩阵
        /// </summary>
        public Matrix4 ProjectionMatrix
        {
            get => _projectionMatrix;
            set
            {
                _projectionMatrix = value;
                UpdateMVPMatrix();
            }
        }

        /// <summary>
        /// 模型矩阵
        /// </summary>
        public Matrix4 ModelMatrix
        {
            get => _modelMatrix;
            set
            {
                _modelMatrix = value;
                UpdateMVPMatrix();
            }
        }

        /// <summary>
        /// MVP矩阵（模型-视图-投影）
        /// </summary>
        public Matrix4 MVPMatrix { get; private set; }

        /// <summary>
        /// 清除颜色
        /// </summary>
        public Vector4 ClearColor
        {
            get => _clearColor;
            set
            {
                _clearColor = value;
                GL.ClearColor(value.X, value.Y, value.Z, value.W);
            }
        }

        /// <summary>
        /// 视口尺寸
        /// </summary>
        public Size ViewportSize { get; private set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化渲染上下文
        /// </summary>
        public RenderContext()
        {
            Initialize();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化OpenGL状态
        /// </summary>
        private void Initialize()
        {
            Debug.WriteLine("Initializing RenderContext...");

            // 设置默认矩阵
            _viewMatrix = Matrix4.Identity;
            _projectionMatrix = Matrix4.Identity;
            _modelMatrix = Matrix4.Identity;
            UpdateMVPMatrix();

            // 设置默认清除颜色（浅灰色）
            ClearColor = new Vector4(0.95f, 0.95f, 0.95f, 1.0f);

            // 启用混合
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // 启用深度测试
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            // 设置面剔除
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            // 设置线宽
            GL.LineWidth(1.0f);

            Debug.WriteLine("RenderContext initialized successfully");
        }

        #endregion

        #region 视口管理

        /// <summary>
        /// 设置视口尺寸
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void SetViewport(int width, int height)
        {
            ViewportSize = new Size(width, height);
            GL.Viewport(0, 0, width, height);

            // 更新投影矩阵为正交投影（适合2D UI）
            ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1000, 1000);

            Debug.WriteLine($"Viewport set to: {width}x{height}");
        }

        /// <summary>
        /// 设置视口尺寸
        /// </summary>
        /// <param name="size">尺寸</param>
        public void SetViewport(Size size)
        {
            SetViewport((int)size.Width, (int)size.Height);
        }

        #endregion

        #region 渲染操作

        /// <summary>
        /// 开始渲染帧
        /// </summary>
        public void BeginFrame()
        {
            // 清除颜色和深度缓冲
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // 重置模型矩阵
            ModelMatrix = Matrix4.Identity;
        }

        /// <summary>
        /// 结束渲染帧
        /// </summary>
        public void EndFrame()
        {
            // 刷新OpenGL命令
            GL.Flush();
        }

        /// <summary>
        /// 推入变换矩阵
        /// </summary>
        /// <param name="transform">变换矩阵</param>
        public void PushTransform(Matrix4 transform)
        {
            ModelMatrix = ModelMatrix * transform;
        }

        /// <summary>
        /// 推入平移变换
        /// </summary>
        /// <param name="x">X偏移</param>
        /// <param name="y">Y偏移</param>
        /// <param name="z">Z偏移</param>
        public void PushTranslation(float x, float y, float z = 0)
        {
            PushTransform(Matrix4.CreateTranslation(x, y, z));
        }

        /// <summary>
        /// 推入缩放变换
        /// </summary>
        /// <param name="scaleX">X缩放</param>
        /// <param name="scaleY">Y缩放</param>
        /// <param name="scaleZ">Z缩放</param>
        public void PushScale(float scaleX, float scaleY, float scaleZ = 1.0f)
        {
            PushTransform(Matrix4.CreateScale(scaleX, scaleY, scaleZ));
        }

        /// <summary>
        /// 推入旋转变换
        /// </summary>
        /// <param name="angle">旋转角度（弧度）</param>
        /// <param name="axis">旋转轴</param>
        public void PushRotation(float angle, Vector3 axis)
        {
            PushTransform(Matrix4.CreateFromAxisAngle(axis, angle));
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 更新MVP矩阵
        /// </summary>
        private void UpdateMVPMatrix()
        {
            MVPMatrix = _modelMatrix * _viewMatrix * _projectionMatrix;
        }

        /// <summary>
        /// 检查OpenGL错误
        /// </summary>
        public void CheckGLError(string operation = "")
        {
            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                string message = $"OpenGL Error: {error}";
                if (!string.IsNullOrEmpty(operation))
                {
                    message += $" during {operation}";
                }
                Debug.WriteLine(message);
                throw new InvalidOperationException(message);
            }
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
                    Debug.WriteLine("RenderContext disposed");
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
