using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using AetherUI.Core;

namespace AetherUI.Rendering
{
    /// <summary>
    /// 字体渲染器，负责文本的OpenGL渲染
    /// </summary>
    public class FontRenderer : IDisposable
    {
        private readonly Dictionary<string, int> _textureCache = new();
        private readonly Dictionary<string, TextMetrics> _metricsCache = new();
        private readonly ShaderManager _shaderManager;
        private int _textShaderProgram;
        private int _vao, _vbo;
        private bool _disposed = false;

        #region 构造函数

        /// <summary>
        /// 初始化字体渲染器
        /// </summary>
        /// <param name="shaderManager">着色器管理器</param>
        public FontRenderer(ShaderManager shaderManager)
        {
            _shaderManager = shaderManager ?? throw new ArgumentNullException(nameof(shaderManager));

            InitializeTextShader();
            InitializeBuffers();
}

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化文本着色器
        /// </summary>
        private void InitializeTextShader()
        {
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

                uniform sampler2D uTexture;
                uniform vec4 uColor;

                out vec4 FragColor;

                void main()
                {
                    vec4 texColor = texture(uTexture, TexCoord);
                    // Keep texture RGB for color fonts/emoji, only modulate alpha
                    FragColor = vec4(texColor.rgb, texColor.a * uColor.a);
                }";

            _textShaderProgram = _shaderManager.CreateShaderProgram("text", vertexShader, fragmentShader);
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

            // 位置属性
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // 纹理坐标属性
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
}

        #endregion

        #region 文本渲染方法

        /// <summary>
        /// 渲染文本
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="fontInfo">字体信息</param>
        /// <param name="position">位置</param>
        /// <param name="mvpMatrix">MVP矩阵</param>
        public void RenderText(string text, FontInfo fontInfo, Vector2 position, Matrix4 mvpMatrix)
        {
            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                // 获取或创建文本纹理
                string cacheKey = GetCacheKey(text, fontInfo);
                if (!_textureCache.TryGetValue(cacheKey, out int textureId))
                {
                    textureId = CreateTextTexture(text, fontInfo);
                    _textureCache[cacheKey] = textureId;
                }

                // 获取文本尺寸
                TextMetrics metrics = GetTextMetrics(text, fontInfo);

                // 渲染文本纹理
                RenderTextTexture(textureId, position, metrics.ToSize(), fontInfo.Color, mvpMatrix);
}
            catch (Exception ex)
            {
}
        }

        /// <summary>
        /// 测量文本尺寸
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="fontInfo">字体信息</param>
        /// <returns>文本尺寸</returns>
        public TextMetrics GetTextMetrics(string text, FontInfo fontInfo)
        {
            if (string.IsNullOrEmpty(text))
                return new TextMetrics(0, 0, 0, fontInfo.Size);

            string cacheKey = GetCacheKey(text, fontInfo);
            if (_metricsCache.TryGetValue(cacheKey, out TextMetrics cachedMetrics))
                return cachedMetrics;

            try
            {
                using (var bitmap = new Bitmap(1, 1))
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    // 使用与渲染相同的高质量设置来测量Unicode文本
                    // 注意：透明背景+OpenGL纹理的Alpha通道不适合ClearType，使用AntiAliasGridFit避免彩边并获得锐利灰度抗锯齿
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    using (Font font = CreateFontForText(fontInfo, text))
                    {
                        // 使用StringFormat确保正确的Unicode字符测量
                        using (var stringFormat = new StringFormat(StringFormat.GenericTypographic))
                        {
                            stringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces |
                                                      StringFormatFlags.NoWrap |
                                                      StringFormatFlags.NoClip;

                            var size = graphics.MeasureString(text, font, PointF.Empty, stringFormat);
                            var fontMetrics = font.FontFamily.GetCellAscent(font.Style) / (float)font.FontFamily.GetEmHeight(font.Style);
                            var baseline = fontInfo.Size * fontMetrics;
                            var lineHeight = fontInfo.Size * 1.2; // 行高为字体大小的1.2倍
var metrics = new TextMetrics(size.Width, size.Height, baseline, lineHeight);
                            _metricsCache[cacheKey] = metrics;
                            return metrics;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                double estimatedWidth = text.Length * fontInfo.Size * 0.6;
                double estimatedHeight = fontInfo.Size * 1.2;
                return new TextMetrics(estimatedWidth, estimatedHeight, fontInfo.Size * 0.8, fontInfo.Size * 1.2);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建文本纹理
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="fontInfo">字体信息</param>
        /// <returns>纹理ID</returns>
        private int CreateTextTexture(string text, FontInfo fontInfo)
        {
            try
            {
                using (Font font = CreateFontForText(fontInfo, text))
                using (var brush = new SolidBrush(ParseColor(fontInfo.Color)))
                {
                    // 测量文本尺寸
                    System.Drawing.Size textSize;
                    using (var tempBitmap = new Bitmap(1, 1))
                    using (var tempGraphics = Graphics.FromImage(tempBitmap))
                    {
                        using (StringFormat fmt = new StringFormat(StringFormat.GenericTypographic))
                        {
                            fmt.FormatFlags = StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
                            var measuredSize = tempGraphics.MeasureString(text, font, PointF.Empty, fmt);
                            textSize = new System.Drawing.Size((int)Math.Ceiling(measuredSize.Width) + 4,
                                              (int)Math.Ceiling(measuredSize.Height) + 4);
                        }
                    }

                    // 创建位图并绘制文本
                    using (var bitmap = new Bitmap(textSize.Width, textSize.Height))
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.Clear(Color.Transparent);

                        // 优化Unicode字符渲染设置
                        // 注：透明背景上使用ClearType会产生彩色条纹边缘，因此使用 AntiAliasGridFit 获取灰度抗锯齿
                        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                            // 使用 GenericTypographic 防止额外字距并提升排版精度
                            using (var fmtDraw = new StringFormat(StringFormat.GenericTypographic))
                            {
                                fmtDraw.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
                                graphics.DrawString(text, font, brush, 2, 2, fmtDraw);
                            }

                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                        // 添加Unicode字符调试信息
                        return CreateOpenGLTexture(bitmap);
                    }
                }
            }
            catch (Exception ex)
            {
return 0;
            }
        }

        /// <summary>
        /// 创建字体对象
        /// </summary>
        /// <param name="fontInfo">字体信息</param>
        /// <returns>字体对象</returns>
        private Font CreateFont(FontInfo fontInfo)
        {
            var style = System.Drawing.FontStyle.Regular;

            if (fontInfo.Weight >= FontWeight.Bold)
                style |= System.Drawing.FontStyle.Bold;

            if (fontInfo.Style == AetherUI.Core.FontStyle.Italic)
                style |= System.Drawing.FontStyle.Italic;

            // Unicode字体回退列表（按优先级排序）
            string[] unicodeFontFallbacks = {
                fontInfo.Family,           // 用户指定的字体
                "Microsoft YaHei UI",      // Windows 10+ 推荐的Unicode字体
                "Microsoft YaHei",         // 经典的Unicode字体
                "Segoe UI",               // Windows系统字体
                "Arial Unicode MS",       // 广泛的Unicode支持
                "Noto Sans CJK SC",       // Google Noto字体（简体中文）
                "SimSun",                 // 宋体（中文回退）
                "Arial"                   // 最终回退
            };

            foreach (string fontFamily in unicodeFontFallbacks)
            {
                try
                {
                    var font = new Font(fontFamily, (float)fontInfo.Size, style);
return font;
                }
                catch (Exception ex)
                {
continue;
                }
            }

            // 如果所有字体都失败，使用系统默认字体
return new Font(FontFamily.GenericSansSerif, (float)fontInfo.Size, style);
        }


        /// <summary>
        /// 根据文本内容创建最合适的字体（自动处理表情符号/符号等回退）
        /// </summary>
        /// <param name="fontInfo">字体信息</param>
        /// <param name="text">文本内容（用于检测是否包含Emoji或扩展字符）</param>
        /// <returns>字体对象</returns>
        private Font CreateFontForText(FontInfo fontInfo, string text)
        {
            // 检测是否包含Emoji或扩展平面字符
            bool containsEmoji = false;
            if (!string.IsNullOrEmpty(text))
            {
                foreach (char c in text)
                {
                    int codePoint = c;
                    // 简单检测：代理对或常见Emoji范围（包括部分BMP内符号）
                    if (char.IsSurrogate(c)) { containsEmoji = true; break; }
                    if ((codePoint >= 0x2600 && codePoint <= 0x27BF) || // 杂项符号
                        (codePoint >= 0x1F300 && codePoint <= 0x1FAFF) || // Emoji扩展（需要代理对）
                        (codePoint >= 0xFE0F && codePoint <= 0xFE0F)) // VS16 变体选择器
                    { containsEmoji = true; break; }
                }
            }

            // Emoji优先回退字体（Color Font）
            if (containsEmoji)
            {
                string[] emojiFallbacks = {
                    "Segoe UI Emoji",
                    "Segoe UI Symbol",
                    "Noto Color Emoji",
                    "Twemoji Mozilla"
                };

                var style = System.Drawing.FontStyle.Regular;
                if (fontInfo.Weight >= FontWeight.Bold) style |= System.Drawing.FontStyle.Bold;
                if (fontInfo.Style == AetherUI.Core.FontStyle.Italic) style |= System.Drawing.FontStyle.Italic;

                foreach (string ff in emojiFallbacks)
                {
                    try
                    {
                        var f = new Font(ff, (float)fontInfo.Size, style);
return f;
                    }
                    catch { /* ignore */ }
                }
            }

            // 默认路径：使用现有的Unicode回退策略
            return CreateFont(fontInfo);
        }

        /// <summary>
        /// 解析颜色字符串
        /// </summary>
        /// <param name="colorString">颜色字符串</param>
        /// <returns>颜色对象</returns>
        private Color ParseColor(string colorString)
        {
            try
            {
                if (colorString.StartsWith("#"))
                {
                    return ColorTranslator.FromHtml(colorString);
                }
                else
                {
                    return Color.FromName(colorString);
                }
            }
            catch
            {
                return Color.Black;
            }
        }

        /// <summary>
        /// 创建OpenGL纹理
        /// </summary>
        /// <param name="bitmap">位图</param>
        /// <returns>纹理ID</returns>
        private int CreateOpenGLTexture(Bitmap bitmap)
        {
            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                         bitmap.Width, bitmap.Height, 0,
                         PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);

            bitmap.UnlockBits(bitmapData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return textureId;
        }

        /// <summary>
        /// 渲染文本纹理
        /// </summary>
        /// <param name="textureId">纹理ID</param>
        /// <param name="position">位置</param>
        /// <param name="size">尺寸</param>
        /// <param name="color">颜色</param>
        /// <param name="mvpMatrix">MVP矩阵</param>
        private void RenderTextTexture(int textureId, Vector2 position, AetherUI.Core.Size size, string color, Matrix4 mvpMatrix)
        {
            if (textureId == 0) return;

            GL.UseProgram(_textShaderProgram);
            GL.BindVertexArray(_vao);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            // 设置MVP矩阵
            int mvpLocation = GL.GetUniformLocation(_textShaderProgram, "uMVP");
            GL.UniformMatrix4(mvpLocation, false, ref mvpMatrix);

            // 设置颜色
            var colorVec = ParseColorToVector4(color);
            int colorLocation = GL.GetUniformLocation(_textShaderProgram, "uColor");
            GL.Uniform4(colorLocation, colorVec);

            // 设置纹理
            int textureLocation = GL.GetUniformLocation(_textShaderProgram, "uTexture");
            GL.Uniform1(textureLocation, 0);

            // 创建四边形顶点数据
            float[] vertices = {
                // 位置                                    // 纹理坐标
                (float)position.X,                        (float)(position.Y + size.Height), 0.0f, 1.0f, // 左下
                (float)(position.X + size.Width),         (float)(position.Y + size.Height), 1.0f, 1.0f, // 右下
                (float)(position.X + size.Width),         (float)position.Y,                  1.0f, 0.0f, // 右上
                (float)position.X,                        (float)position.Y,                  0.0f, 0.0f  // 左上
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            // 绘制四边形
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        /// <summary>
        /// 解析颜色为Vector4
        /// </summary>
        /// <param name="colorString">颜色字符串</param>
        /// <returns>Vector4颜色</returns>
        private Vector4 ParseColorToVector4(string colorString)
        {
            var color = ParseColor(colorString);
            return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
        }

        /// <summary>
        /// 获取缓存键
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="fontInfo">字体信息</param>
        /// <returns>缓存键</returns>
        private string GetCacheKey(string text, FontInfo fontInfo)
        {
            return $"{text}|{fontInfo}";
        }
        /// <summary>
        /// 清理字体渲染缓存
        /// </summary>
        public void ClearCaches()
        {
            try
            {
                foreach (int textureId in _textureCache.Values)
                {
                    if (textureId != 0)
                        GL.DeleteTexture(textureId);
                }
                _textureCache.Clear();

                // 清理度量缓存
                _metricsCache.Clear();
}
            catch (Exception ex)
            {
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
                    foreach (var textureId in _textureCache.Values)
                    {
                        if (textureId != 0)
                            GL.DeleteTexture(textureId);
                    }
                    _textureCache.Clear();
                    _metricsCache.Clear();

                    // 清理OpenGL资源
                    if (_vao != 0) GL.DeleteVertexArray(_vao);
                    if (_vbo != 0) GL.DeleteBuffer(_vbo);
                    if (_textShaderProgram != 0) GL.DeleteProgram(_textShaderProgram);
}

                _disposed = true;
            }
        }

        #endregion
    }
}
