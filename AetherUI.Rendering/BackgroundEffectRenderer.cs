using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace AetherUI.Rendering
{
    /// <summary>
    /// 背景效果类型
    /// </summary>
    public enum BackgroundEffectType
    {
        /// <summary>
        /// 无效果
        /// </summary>
        None,

        /// <summary>
        /// 亚克力效果（Windows原生半透明模糊）
        /// </summary>
        Acrylic,

        /// <summary>
        /// 云母效果（Windows 11原生材质）
        /// </summary>
        Mica,

        /// <summary>
        /// 云母Alt效果（Windows 11原生材质变体）
        /// </summary>
        MicaAlt,

        /// <summary>
        /// 渐变效果（OpenGL自定义渲染）
        /// </summary>
        Gradient
    }

    /// <summary>
    /// 背景效果配置
    /// </summary>
    public class BackgroundEffectConfig
    {
        /// <summary>
        /// 效果类型
        /// </summary>
        public BackgroundEffectType Type { get; set; } = BackgroundEffectType.Acrylic;

        /// <summary>
        /// 透明度 (0.0 - 1.0)
        /// </summary>
        public float Opacity { get; set; } = 0.8f;

        /// <summary>
        /// 模糊强度 (0.0 - 1.0)
        /// </summary>
        public float BlurStrength { get; set; } = 0.5f;

        /// <summary>
        /// 主色调
        /// </summary>
        public Vector4 TintColor { get; set; } = new Vector4(0.95f, 0.95f, 0.95f, 1.0f);

        /// <summary>
        /// 噪声强度（用于云母效果）
        /// </summary>
        public float NoiseStrength { get; set; } = 0.1f;

        /// <summary>
        /// 渐变起始颜色
        /// </summary>
        public Vector4 GradientStart { get; set; } = new Vector4(0.9f, 0.9f, 1.0f, 1.0f);

        /// <summary>
        /// 渐变结束颜色
        /// </summary>
        public Vector4 GradientEnd { get; set; } = new Vector4(0.95f, 0.95f, 0.95f, 1.0f);
    }

    /// <summary>
    /// 背景效果渲染器，实现现代化窗口背景效果
    /// 使用Windows原生API实现亚克力和云母效果，OpenGL实现渐变效果
    /// </summary>
    public class BackgroundEffectRenderer : IDisposable
    {
        private readonly ShaderManager? _shaderManager;
        private int _gradientShaderProgram;
        private int _vao, _vbo;
        private BackgroundEffectConfig _config;
        private IntPtr _windowHandle;
        private bool _disposed = false;
        private bool _windowEffectApplied = false;

        #region 构造函数

        /// <summary>
        /// 初始化背景效果渲染器
        /// </summary>
        /// <param name="shaderManager">着色器管理器（可选，仅用于渐变效果）</param>
        /// <param name="windowHandle">窗口句柄</param>
        public BackgroundEffectRenderer(ShaderManager? shaderManager, IntPtr windowHandle)
        {
            _shaderManager = shaderManager;
            _windowHandle = windowHandle;
            _config = new BackgroundEffectConfig();

            // 仅在需要渐变效果时初始化OpenGL资源
            if (_shaderManager != null)
            {
                try
                {
                    InitializeGradientShader();
                    InitializeBuffers();
                }
                catch (Exception ex)
                {
                }
            }
}

        #endregion

        #region 属性

        /// <summary>
        /// 背景效果配置
        /// </summary>
        public BackgroundEffectConfig Config
        {
            get => _config;
            set
            {
                _config = value ?? new BackgroundEffectConfig();
                ApplyBackgroundEffect();
            }
        }

        /// <summary>
        /// 窗口句柄
        /// </summary>
        public IntPtr WindowHandle
        {
            get => _windowHandle;
            set
            {
                _windowHandle = value;
                ApplyBackgroundEffect();
            }
        }

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化渐变着色器（仅用于渐变效果）
        /// </summary>
        private void InitializeGradientShader()
        {
            if (_shaderManager == null)
                return;

            string vertexShader = @"
                #version 330 core
                layout (location = 0) in vec2 aPosition;
                layout (location = 1) in vec2 aTexCoord;

                uniform mat4 uMVP;

                out vec2 TexCoord;

                void main()
                {
                    gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
                    TexCoord = aTexCoord;
                }";

            string fragmentShader = @"
#version 330 core
in vec2 TexCoord;

uniform float uOpacity;
uniform vec4 uGradientStart;
uniform vec4 uGradientEnd;
uniform float uTime;

out vec4 FragColor;

void main()
{
    vec2 uv = TexCoord;

    // Gradient factor along vertical axis
    float gradientFactor = uv.y;
    vec4 finalColor = mix(uGradientStart, uGradientEnd, gradientFactor);

    // Subtle horizontal variation
    float horizontalVariation = sin(uv.x * 3.14159 + uTime * 0.1) * 0.05;
    finalColor.rgb += horizontalVariation;

    finalColor.a *= uOpacity;
    FragColor = finalColor;
}";

            _gradientShaderProgram = _shaderManager.CreateShaderProgram("gradient", vertexShader, fragmentShader);
}

        /// <summary>
        /// 初始化缓冲区
        /// </summary>
        private void InitializeBuffers()
        {
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            // 全屏四边形顶点数据
            float[] vertices = {
                // 位置        // 纹理坐标
                -1.0f, -1.0f,  0.0f, 0.0f, // 左下
                 1.0f, -1.0f,  1.0f, 0.0f, // 右下
                 1.0f,  1.0f,  1.0f, 1.0f, // 右上
                -1.0f,  1.0f,  0.0f, 1.0f  // 左上
            };

            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // 位置属性
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // 纹理坐标属性
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
}

        #endregion

        #region 背景效果应用

        /// <summary>
        /// 应用背景效果
        /// </summary>
        private void ApplyBackgroundEffect()
        {
            if (_windowHandle == IntPtr.Zero)
            {
return;
            }

            try
            {
                bool success = false;

                switch (_config.Type)
                {
                    case BackgroundEffectType.None:
                        success = WindowsCompositionApi.SetWindowBackgroundEffect(_windowHandle, WindowCompositionType.Disabled);
                        break;

                    case BackgroundEffectType.Acrylic:
                        // 将配置参数映射到Windows API
                        uint tintColor = ColorToUint(_config.TintColor);
                        success = WindowsCompositionApi.SetWindowBackgroundEffect(_windowHandle, WindowCompositionType.Acrylic, _config.Opacity, tintColor);
                        break;

                    case BackgroundEffectType.Mica:
                        success = WindowsCompositionApi.SetWindowBackgroundEffect(_windowHandle, WindowCompositionType.Mica);
                        break;

                    case BackgroundEffectType.MicaAlt:
                        success = WindowsCompositionApi.SetWindowBackgroundEffect(_windowHandle, WindowCompositionType.MicaAlt);
                        break;

                    case BackgroundEffectType.Gradient:
                        // 渐变效果不需要Windows API，在渲染时处理
                        success = true;
                        break;
                }

                _windowEffectApplied = success;

                if (success)
                {
}
                else
                {
}
            }
            catch (Exception ex)
            {
_windowEffectApplied = false;
            }
        }

        /// <summary>
        /// 将Vector4颜色转换为uint格式
        /// </summary>
        /// <param name="color">颜色向量</param>
        /// <returns>uint颜色值</returns>
        private static uint ColorToUint(Vector4 color)
        {
            byte r = (byte)(color.X * 255);
            byte g = (byte)(color.Y * 255);
            byte b = (byte)(color.Z * 255);
            return (uint)(r | (g << 8) | (b << 16));
        }

        #endregion

        #region 渲染方法

        /// <summary>
        /// 渲染背景效果（仅用于渐变效果，其他效果由Windows API处理）
        /// </summary>
        /// <param name="mvpMatrix">MVP矩阵</param>
        /// <param name="resolution">分辨率</param>
        /// <param name="time">时间</param>
        public void RenderBackground(Matrix4 mvpMatrix, Vector2 resolution, float time)
        {
            // 只有渐变效果需要OpenGL渲染，其他效果由Windows API处理
            if (_config.Type != BackgroundEffectType.Gradient || _shaderManager == null)
                return;

            GL.UseProgram(_gradientShaderProgram);
            GL.BindVertexArray(_vao);

            // 设置uniform变量
            SetGradientShaderUniforms(mvpMatrix, time);

            // 禁用深度写入，启用混合
            GL.DepthMask(false);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // 绘制全屏四边形
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            // 恢复状态
            GL.DepthMask(true);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        /// <summary>
        /// 设置渐变着色器uniform变量
        /// </summary>
        /// <param name="mvpMatrix">MVP矩阵</param>
        /// <param name="time">时间</param>
        private void SetGradientShaderUniforms(Matrix4 mvpMatrix, float time)
        {
            // MVP矩阵
            int mvpLocation = GL.GetUniformLocation(_gradientShaderProgram, "uMVP");
            GL.UniformMatrix4(mvpLocation, false, ref mvpMatrix);

            // 透明度
            int opacityLocation = GL.GetUniformLocation(_gradientShaderProgram, "uOpacity");
            GL.Uniform1(opacityLocation, _config.Opacity);

            // 渐变颜色
            int gradientStartLocation = GL.GetUniformLocation(_gradientShaderProgram, "uGradientStart");
            GL.Uniform4(gradientStartLocation, _config.GradientStart);

            int gradientEndLocation = GL.GetUniformLocation(_gradientShaderProgram, "uGradientEnd");
            GL.Uniform4(gradientEndLocation, _config.GradientEnd);

            // 时间
            int timeLocation = GL.GetUniformLocation(_gradientShaderProgram, "uTime");
            GL.Uniform1(timeLocation, time);
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
                    if (_windowHandle != IntPtr.Zero && _windowEffectApplied)
                    {
                        WindowsCompositionApi.SetWindowBackgroundEffect(_windowHandle, WindowCompositionType.Disabled);
                    }

                    // 清理OpenGL资源
                    if (_vao != 0) GL.DeleteVertexArray(_vao);
                    if (_vbo != 0) GL.DeleteBuffer(_vbo);
                    if (_gradientShaderProgram != 0) GL.DeleteProgram(_gradientShaderProgram);
}

                _disposed = true;
            }
        }

        #endregion
    }
}
