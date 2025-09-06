using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using AetherUI.Core;

namespace AetherUI.Rendering
{
    /// <summary>
    /// 几何渲染器，负责渲染基础几何图形
    /// </summary>
    public class GeometryRenderer : IDisposable
    {
        private readonly ShaderManager _shaderManager;
        private int _vao;
        private int _vbo;
        private int _ebo;
        private readonly List<float> _vertices;
        private readonly List<uint> _indices;
        private bool _disposed = false;

        #region 构造函数

        /// <summary>
        /// 初始化几何渲染器
        /// </summary>
        /// <param name="shaderManager">着色器管理器</param>
        public GeometryRenderer(ShaderManager shaderManager)
        {
            _shaderManager = shaderManager ?? throw new ArgumentNullException(nameof(shaderManager));
            _vertices = new List<float>();
            _indices = new List<uint>();

            InitializeBuffers();
}

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化缓冲区
        /// </summary>
        private void InitializeBuffers()
        {
            // 创建VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            // 创建VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            // 创建EBO
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);

            // 设置顶点属性
            // 位置属性 (location = 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // 颜色属性 (location = 1)
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
}

        #endregion

        #region 渲染方法

        /// <summary>
        /// 开始批量渲染
        /// </summary>
        public void BeginBatch()
        {
            _vertices.Clear();
            _indices.Clear();
        }

        /// <summary>
        /// 结束批量渲染并提交到GPU
        /// </summary>
        /// <param name="mvpMatrix">MVP矩阵</param>
        public void EndBatch(Matrix4 mvpMatrix)
        {
            if (_vertices.Count == 0)
                return;

            // 上传顶点数据
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * sizeof(float), _vertices.ToArray(), BufferUsageHint.DynamicDraw);
            RenderContext.CheckGLErrorStatic("Geometry:BufferData VBO");

            // 上传索引数据
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count * sizeof(uint), _indices.ToArray(), BufferUsageHint.DynamicDraw);
            RenderContext.CheckGLErrorStatic("Geometry:BufferData EBO");

            // 使用着色器
            _shaderManager.UseShader("basic");
            RenderContext.CheckGLErrorStatic("Geometry:UseShader");

            // 绑定VAO（Core Profile 必需）
            GL.BindVertexArray(_vao);
            RenderContext.CheckGLErrorStatic("Geometry:BindVAO");

            // 设uniform必须在使用了program之后
            _shaderManager.SetUniformMatrix4("basic", "uMVP", mvpMatrix);
            RenderContext.CheckGLErrorStatic("Geometry:SetUniform");

            // 渲染
            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
            RenderContext.CheckGLErrorStatic("Geometry:DrawElements");
            GL.BindVertexArray(0);
}

        /// <summary>
        /// 渲染矩形
        /// </summary>
        /// <param name="rect">矩形区域</param>
        /// <param name="color">颜色</param>
        public void DrawRect(Rect rect, Vector4 color)
        {
uint baseIndex = (uint)(_vertices.Count / 7);

            // 添加四个顶点 (x, y, z, r, g, b, a)
            _vertices.AddRange(new float[]
            {
                // 左下角
                (float)rect.X, (float)(rect.Y + rect.Height), 0.0f, color.X, color.Y, color.Z, color.W,
                // 右下角
                (float)(rect.X + rect.Width), (float)(rect.Y + rect.Height), 0.0f, color.X, color.Y, color.Z, color.W,
                // 右上角
                (float)(rect.X + rect.Width), (float)rect.Y, 0.0f, color.X, color.Y, color.Z, color.W,
                // 左上角
                (float)rect.X, (float)rect.Y, 0.0f, color.X, color.Y, color.Z, color.W
            });

            // 添加两个三角形的索引
            _indices.AddRange(new uint[]
            {
                baseIndex, baseIndex + 1, baseIndex + 2,  // 第一个三角形
                baseIndex, baseIndex + 2, baseIndex + 3   // 第二个三角形
            });
        }

        /// <summary>
        /// 渲染矩形边框
        /// </summary>
        /// <param name="rect">矩形区域</param>
        /// <param name="color">颜色</param>
        /// <param name="thickness">边框厚度</param>
        public void DrawRectBorder(Rect rect, Vector4 color, double thickness)
        {
            DrawRect(new Rect(rect.X, rect.Y, rect.Width, thickness), color);
            // 底边
            DrawRect(new Rect(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            // 左边
            DrawRect(new Rect(rect.X, rect.Y, thickness, rect.Height), color);
            // 右边
            DrawRect(new Rect(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }

        /// <summary>
        /// 渲染圆角矩形
        /// </summary>
        /// <param name="rect">矩形区域</param>
        /// <param name="color">颜色</param>
        /// <param name="cornerRadius">圆角半径</param>
        public void DrawRoundedRect(Rect rect, Vector4 color, double cornerRadius)
        {
if (cornerRadius <= 0)
            {
DrawRect(rect, color);
                return;
            }

            // 简化实现：使用多个小矩形近似圆角
            double radius = Math.Min(cornerRadius, Math.Min(rect.Width, rect.Height) / 2);
            
            // 中心矩形
            DrawRect(new Rect(rect.X + radius, rect.Y, rect.Width - 2 * radius, rect.Height), color);
            DrawRect(new Rect(rect.X, rect.Y + radius, radius, rect.Height - 2 * radius), color);
            DrawRect(new Rect(rect.X + rect.Width - radius, rect.Y + radius, radius, rect.Height - 2 * radius), color);

            // 四个角的近似（使用小矩形）
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                double angle = i * Math.PI / (2 * segments);
                double x = radius * (1 - Math.Cos(angle));
                double y = radius * (1 - Math.Sin(angle));
                double size = radius / segments;

                // 左上角
                DrawRect(new Rect(rect.X + x, rect.Y + y, size, size), color);
                // 右上角
                DrawRect(new Rect(rect.X + rect.Width - radius + radius * Math.Cos(angle), rect.Y + y, size, size), color);
                // 左下角
                DrawRect(new Rect(rect.X + x, rect.Y + rect.Height - radius + radius * Math.Sin(angle), size, size), color);
                // 右下角
                DrawRect(new Rect(rect.X + rect.Width - radius + radius * Math.Cos(angle), rect.Y + rect.Height - radius + radius * Math.Sin(angle), size, size), color);
            }
        }

        /// <summary>
        /// 渲染线条
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="color">颜色</param>
        /// <param name="thickness">线条厚度</param>
        public void DrawLine(Point start, Point end, Vector4 color, double thickness)
        {
            // 计算线条方向和垂直方向
            Vector2 direction = new Vector2((float)(end.X - start.X), (float)(end.Y - start.Y));
            direction = Vector2.Normalize(direction);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X) * (float)(thickness / 2);

            // 计算四个顶点
            Vector2 p1 = new Vector2((float)start.X, (float)start.Y) - perpendicular;
            Vector2 p2 = new Vector2((float)start.X, (float)start.Y) + perpendicular;
            Vector2 p3 = new Vector2((float)end.X, (float)end.Y) + perpendicular;
            Vector2 p4 = new Vector2((float)end.X, (float)end.Y) - perpendicular;

            uint baseIndex = (uint)(_vertices.Count / 7);

            // 添加四个顶点
            _vertices.AddRange(new float[]
            {
                p1.X, p1.Y, 0.0f, color.X, color.Y, color.Z, color.W,
                p2.X, p2.Y, 0.0f, color.X, color.Y, color.Z, color.W,
                p3.X, p3.Y, 0.0f, color.X, color.Y, color.Z, color.W,
                p4.X, p4.Y, 0.0f, color.X, color.Y, color.Z, color.W
            });

            // 添加两个三角形的索引
            _indices.AddRange(new uint[]
            {
                baseIndex, baseIndex + 1, baseIndex + 2,
                baseIndex, baseIndex + 2, baseIndex + 3
            });
        }

        /// <summary>
        /// 渲染椭圆
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radiusX">X轴半径</param>
        /// <param name="radiusY">Y轴半径</param>
        /// <param name="color">颜色</param>
        /// <param name="segments">分段数</param>
        public void DrawEllipse(Point center, double radiusX, double radiusY, Vector4 color, int segments = 32)
        {
            uint baseIndex = (uint)(_vertices.Count / 7);

            // 添加中心点
            _vertices.AddRange(new float[]
            {
                (float)center.X, (float)center.Y, 0.0f, color.X, color.Y, color.Z, color.W
            });

            // 添加圆周上的点
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2.0 * Math.PI * i / segments;
                float x = (float)(center.X + radiusX * Math.Cos(angle));
                float y = (float)(center.Y + radiusY * Math.Sin(angle));

                _vertices.AddRange(new float[]
                {
                    x, y, 0.0f, color.X, color.Y, color.Z, color.W
                });
            }

            // 添加三角形索引
            for (int i = 0; i < segments; i++)
            {
                _indices.AddRange(new uint[]
                {
                    baseIndex, baseIndex + (uint)i + 1, baseIndex + (uint)i + 2
                });
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
                    if (_vao != 0)
                    {
                        GL.DeleteVertexArray(_vao);
                        _vao = 0;
                    }

                    if (_vbo != 0)
                    {
                        GL.DeleteBuffer(_vbo);
                        _vbo = 0;
                    }

                    if (_ebo != 0)
                    {
                        GL.DeleteBuffer(_ebo);
                        _ebo = 0;
                    }
}

                _disposed = true;
            }
        }

        #endregion
    }
}
