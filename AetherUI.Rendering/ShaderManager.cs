using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace AetherUI.Rendering
{
    /// <summary>
    /// 着色器管理器，负责着色器的编译、链接和管理
    /// </summary>
    public class ShaderManager : IDisposable
    {
        private readonly Dictionary<string, int> _shaderPrograms;
        private readonly Dictionary<string, int> _uniformLocations;
        private int _currentProgram = -1;
        private bool _disposed = false;

        #region 构造函数

        /// <summary>
        /// 初始化着色器管理器
        /// </summary>
        public ShaderManager()
        {
            _shaderPrograms = new Dictionary<string, int>();
            _uniformLocations = new Dictionary<string, int>();

            Debug.WriteLine("ShaderManager initialized");
        }

        #endregion

        #region 着色器编译

        /// <summary>
        /// 创建着色器程序
        /// </summary>
        /// <param name="name">着色器程序名称</param>
        /// <param name="vertexSource">顶点着色器源码</param>
        /// <param name="fragmentSource">片段着色器源码</param>
        /// <returns>着色器程序ID</returns>
        public int CreateShaderProgram(string name, string vertexSource, string fragmentSource)
        {
            Debug.WriteLine($"Creating shader program: {name}");

            // 编译顶点着色器
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
            if (vertexShader == 0)
                throw new InvalidOperationException($"Failed to compile vertex shader for {name}");

            // 编译片段着色器
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);
            if (fragmentShader == 0)
            {
                GL.DeleteShader(vertexShader);
                string fragmentLog = GL.GetShaderInfoLog(fragmentShader);
                throw new InvalidOperationException($"Failed to compile fragment shader for {name}: {fragmentLog}");
            }

            // 创建着色器程序
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            // 检查链接状态
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                GL.DeleteProgram(program);
                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
                throw new InvalidOperationException($"Failed to link shader program {name}: {infoLog}");
            }

            // 清理着色器对象
            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // 存储程序
            _shaderPrograms[name] = program;

            Debug.WriteLine($"Shader program {name} created successfully (ID: {program})");
            return program;
        }

        /// <summary>
        /// 编译着色器
        /// </summary>
        /// <param name="type">着色器类型</param>
        /// <param name="source">着色器源码</param>
        /// <returns>着色器ID</returns>
        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            // 检查编译状态
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileStatus);
            if (compileStatus == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                GL.DeleteShader(shader);
                Debug.WriteLine($"Shader compilation failed ({type}): {infoLog}");
                return 0;
            }

            return shader;
        }

        #endregion

        #region 着色器使用

        /// <summary>
        /// 使用着色器程序
        /// </summary>
        /// <param name="name">着色器程序名称</param>
        public void UseShader(string name)
        {
            if (!_shaderPrograms.TryGetValue(name, out int program))
                throw new ArgumentException($"Shader program not found: {name}");

            if (_currentProgram != program)
            {
                GL.UseProgram(program);
                _currentProgram = program;
            }
        }

        /// <summary>
        /// 获取着色器程序ID
        /// </summary>
        /// <param name="name">着色器程序名称</param>
        /// <returns>程序ID</returns>
        public int GetShaderProgram(string name)
        {
            return _shaderPrograms.TryGetValue(name, out int program) ? program : 0;
        }

        #endregion

        #region Uniform变量

        /// <summary>
        /// 获取Uniform变量位置
        /// </summary>
        /// <param name="shaderName">着色器名称</param>
        /// <param name="uniformName">Uniform变量名称</param>
        /// <returns>变量位置</returns>
        public int GetUniformLocation(string shaderName, string uniformName)
        {
            string key = $"{shaderName}.{uniformName}";
            
            if (_uniformLocations.TryGetValue(key, out int location))
                return location;

            if (!_shaderPrograms.TryGetValue(shaderName, out int program))
                return -1;

            location = GL.GetUniformLocation(program, uniformName);
            _uniformLocations[key] = location;

            return location;
        }

        /// <summary>
        /// 设置Matrix4 Uniform变量
        /// </summary>
        /// <param name="shaderName">着色器名称</param>
        /// <param name="uniformName">变量名称</param>
        /// <param name="matrix">矩阵值</param>
        public void SetUniformMatrix4(string shaderName, string uniformName, Matrix4 matrix)
        {
            int location = GetUniformLocation(shaderName, uniformName);
            if (location >= 0)
            {
                GL.UniformMatrix4(location, false, ref matrix);
            }
        }

        /// <summary>
        /// 设置Vector4 Uniform变量
        /// </summary>
        /// <param name="shaderName">着色器名称</param>
        /// <param name="uniformName">变量名称</param>
        /// <param name="vector">向量值</param>
        public void SetUniformVector4(string shaderName, string uniformName, Vector4 vector)
        {
            int location = GetUniformLocation(shaderName, uniformName);
            if (location >= 0)
            {
                GL.Uniform4(location, vector);
            }
        }

        /// <summary>
        /// 设置Vector3 Uniform变量
        /// </summary>
        /// <param name="shaderName">着色器名称</param>
        /// <param name="uniformName">变量名称</param>
        /// <param name="vector">向量值</param>
        public void SetUniformVector3(string shaderName, string uniformName, Vector3 vector)
        {
            int location = GetUniformLocation(shaderName, uniformName);
            if (location >= 0)
            {
                GL.Uniform3(location, vector);
            }
        }

        /// <summary>
        /// 设置Vector2 Uniform变量
        /// </summary>
        /// <param name="shaderName">着色器名称</param>
        /// <param name="uniformName">变量名称</param>
        /// <param name="vector">向量值</param>
        public void SetUniformVector2(string shaderName, string uniformName, Vector2 vector)
        {
            int location = GetUniformLocation(shaderName, uniformName);
            if (location >= 0)
            {
                GL.Uniform2(location, vector);
            }
        }

        /// <summary>
        /// 设置float Uniform变量
        /// </summary>
        /// <param name="shaderName">着色器名称</param>
        /// <param name="uniformName">变量名称</param>
        /// <param name="value">浮点值</param>
        public void SetUniformFloat(string shaderName, string uniformName, float value)
        {
            int location = GetUniformLocation(shaderName, uniformName);
            if (location >= 0)
            {
                GL.Uniform1(location, value);
            }
        }

        /// <summary>
        /// 设置int Uniform变量
        /// </summary>
        /// <param name="shaderName">着色器名称</param>
        /// <param name="uniformName">变量名称</param>
        /// <param name="value">整数值</param>
        public void SetUniformInt(string shaderName, string uniformName, int value)
        {
            int location = GetUniformLocation(shaderName, uniformName);
            if (location >= 0)
            {
                GL.Uniform1(location, value);
            }
        }

        #endregion

        #region 默认着色器

        /// <summary>
        /// 创建默认着色器
        /// </summary>
        public void CreateDefaultShaders()
        {
            Debug.WriteLine("Creating default shaders...");

            // 基础颜色着色器
            string basicVertexShader = @"
#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec4 aColor;

uniform mat4 uMVP;

out vec4 vColor;

void main()
{
    gl_Position = uMVP * vec4(aPosition, 1.0);
    vColor = aColor;
}";

            string basicFragmentShader = @"
#version 330 core

in vec4 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vColor;
}";

            CreateShaderProgram("basic", basicVertexShader, basicFragmentShader);

            Debug.WriteLine("Default shaders created");
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
                    Debug.WriteLine("Disposing ShaderManager...");

                    // 删除所有着色器程序
                    foreach (var program in _shaderPrograms.Values)
                    {
                        GL.DeleteProgram(program);
                    }

                    _shaderPrograms.Clear();
                    _uniformLocations.Clear();

                    Debug.WriteLine("ShaderManager disposed");
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
