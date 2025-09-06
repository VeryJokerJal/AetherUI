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
        /// 亚克力效果（半透明模糊）
        /// </summary>
        Acrylic,

        /// <summary>
        /// 云母效果（自然半透明材质）
        /// </summary>
        Mica,

        /// <summary>
        /// 渐变效果
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
    /// </summary>
    public class BackgroundEffectRenderer : IDisposable
    {
        private readonly ShaderManager _shaderManager;
        private int _backgroundShaderProgram;
        private int _vao, _vbo;
        private BackgroundEffectConfig _config;
        private bool _disposed = false;

        #region 构造函数

        /// <summary>
        /// 初始化背景效果渲染器
        /// </summary>
        /// <param name="shaderManager">着色器管理器</param>
        public BackgroundEffectRenderer(ShaderManager shaderManager)
        {
            _shaderManager = shaderManager ?? throw new ArgumentNullException(nameof(shaderManager));
            _config = new BackgroundEffectConfig();
            
            InitializeShaders();
            InitializeBuffers();
            
            Debug.WriteLine("BackgroundEffectRenderer initialized");
        }

        #endregion

        #region 属性

        /// <summary>
        /// 背景效果配置
        /// </summary>
        public BackgroundEffectConfig Config
        {
            get => _config;
            set => _config = value ?? new BackgroundEffectConfig();
        }

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化着色器
        /// </summary>
        private void InitializeShaders()
        {
            string vertexShader = @"
                #version 330 core
                layout (location = 0) in vec2 aPosition;
                layout (location = 1) in vec2 aTexCoord;
                
                uniform mat4 uMVP;
                
                out vec2 TexCoord;
                out vec2 ScreenPos;
                
                void main()
                {
                    gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
                    TexCoord = aTexCoord;
                    ScreenPos = aPosition;
                }";

            string fragmentShader = @"
#version 330 core
in vec2 TexCoord;

uniform int uEffectType;
uniform float uOpacity;
uniform vec4 uTintColor;
uniform vec4 uGradientStart;
uniform vec4 uGradientEnd;
uniform float uTime;

out vec4 FragColor;

void main()
{
    vec2 uv = TexCoord;
    vec4 finalColor = uTintColor;

    if (uEffectType == 1) { // Acrylic
        // 简单的亚克力效果
        float noise = fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453);
        finalColor.rgb += (noise - 0.5) * 0.02;

        vec2 center = vec2(0.5, 0.5);
        float dist = distance(uv, center);
        float radialGradient = 1.0 - smoothstep(0.0, 0.8, dist);
        finalColor.rgb += radialGradient * 0.05;

    } else if (uEffectType == 2) { // Mica
        // 简单的云母效果
        float noise = fract(sin(dot(uv * 15.0, vec2(12.9898, 78.233))) * 43758.5453);
        finalColor.rgb += (noise - 0.5) * 0.1;

        float colorShift = sin(uv.x * 3.14159 + uTime * 0.5) * 0.02;
        finalColor.rgb += vec3(colorShift, -colorShift * 0.5, colorShift * 0.3);

    } else if (uEffectType == 3) { // Gradient
        // 渐变效果
        float gradientFactor = uv.y;
        finalColor = mix(uGradientStart, uGradientEnd, gradientFactor);

        float horizontalVariation = sin(uv.x * 3.14159) * 0.05;
        finalColor.rgb += horizontalVariation;
    }

    finalColor.a = uOpacity;
    FragColor = finalColor;
}";

            _backgroundShaderProgram = _shaderManager.CreateShaderProgram("background", vertexShader, fragmentShader);
            Debug.WriteLine($"Background shader program created: {_backgroundShaderProgram}");
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
            
            Debug.WriteLine("Background effect buffers initialized");
        }

        #endregion

        #region 渲染方法

        /// <summary>
        /// 渲染背景效果
        /// </summary>
        /// <param name="mvpMatrix">MVP矩阵</param>
        /// <param name="resolution">分辨率</param>
        /// <param name="time">时间</param>
        public void RenderBackground(Matrix4 mvpMatrix, Vector2 resolution, float time)
        {
            if (_config.Type == BackgroundEffectType.None)
                return;

            GL.UseProgram(_backgroundShaderProgram);
            GL.BindVertexArray(_vao);

            // 设置uniform变量
            SetShaderUniforms(mvpMatrix, resolution, time);

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
        /// 设置着色器uniform变量
        /// </summary>
        /// <param name="mvpMatrix">MVP矩阵</param>
        /// <param name="resolution">分辨率</param>
        /// <param name="time">时间</param>
        private void SetShaderUniforms(Matrix4 mvpMatrix, Vector2 resolution, float time)
        {
            // MVP矩阵
            int mvpLocation = GL.GetUniformLocation(_backgroundShaderProgram, "uMVP");
            GL.UniformMatrix4(mvpLocation, false, ref mvpMatrix);

            // 效果类型
            int effectTypeLocation = GL.GetUniformLocation(_backgroundShaderProgram, "uEffectType");
            GL.Uniform1(effectTypeLocation, (int)_config.Type);

            // 透明度
            int opacityLocation = GL.GetUniformLocation(_backgroundShaderProgram, "uOpacity");
            GL.Uniform1(opacityLocation, _config.Opacity);

            // 主色调
            int tintLocation = GL.GetUniformLocation(_backgroundShaderProgram, "uTintColor");
            GL.Uniform4(tintLocation, _config.TintColor);

            // 渐变颜色
            int gradientStartLocation = GL.GetUniformLocation(_backgroundShaderProgram, "uGradientStart");
            GL.Uniform4(gradientStartLocation, _config.GradientStart);

            int gradientEndLocation = GL.GetUniformLocation(_backgroundShaderProgram, "uGradientEnd");
            GL.Uniform4(gradientEndLocation, _config.GradientEnd);

            // 时间
            int timeLocation = GL.GetUniformLocation(_backgroundShaderProgram, "uTime");
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
                    Debug.WriteLine("Disposing BackgroundEffectRenderer...");

                    // 清理OpenGL资源
                    if (_vao != 0) GL.DeleteVertexArray(_vao);
                    if (_vbo != 0) GL.DeleteBuffer(_vbo);
                    if (_backgroundShaderProgram != 0) GL.DeleteProgram(_backgroundShaderProgram);

                    Debug.WriteLine("BackgroundEffectRenderer disposed");
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
