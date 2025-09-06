using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AetherUI.Core;
using AetherUI.Xaml;

namespace AetherUI.Compiler
{
    /// <summary>
    /// 热重载管理器，负责文件监控和动态重新编译
    /// </summary>
    public class HotReloadManager : IDisposable
    {
        private readonly FileWatcher _fileWatcher;
        private readonly AetherCompiler _compiler;
        private readonly Dictionary<string, object> _loadedElements;
        private readonly Dictionary<string, DateTime> _lastCompileTime;
        private bool _disposed = false;

        #region 事件

        /// <summary>
        /// 热重载完成事件
        /// </summary>
        public event EventHandler<HotReloadEventArgs>? HotReloadCompleted;

        /// <summary>
        /// 热重载错误事件
        /// </summary>
        public event EventHandler<HotReloadErrorEventArgs>? HotReloadError;

        #endregion

        #region 属性

        /// <summary>
        /// 是否启用热重载
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 监控的根目录
        /// </summary>
        public string? RootDirectory { get; private set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化热重载管理器
        /// </summary>
        public HotReloadManager()
        {
            _fileWatcher = new FileWatcher();
            _compiler = new AetherCompiler();
            _loadedElements = new Dictionary<string, object>();
            _lastCompileTime = new Dictionary<string, DateTime>();

            _fileWatcher.FileChanged += OnFileChanged;
}

        #endregion

        #region 监控管理

        /// <summary>
        /// 开始监控指定目录
        /// </summary>
        /// <param name="rootDirectory">根目录</param>
        public void StartWatching(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory))
                throw new ArgumentException("Root directory cannot be null or empty", nameof(rootDirectory));

            if (!Directory.Exists(rootDirectory))
                throw new DirectoryNotFoundException($"Directory not found: {rootDirectory}");

            RootDirectory = rootDirectory;
            _fileWatcher.AddWatch(rootDirectory, "*.xaml");
            
            // 监控JSON文件
            _fileWatcher.AddWatch(rootDirectory, "*.json");
}

        /// <summary>
        /// 停止监控
        /// </summary>
        public void StopWatching()
        {
_fileWatcher.ClearWatches();
            RootDirectory = null;
}

        /// <summary>
        /// 注册已加载的元素
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="element">UI元素</param>
        public void RegisterElement(string filePath, object element)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (element == null)
                throw new ArgumentNullException(nameof(element));

            string normalizedPath = Path.GetFullPath(filePath);
            _loadedElements[normalizedPath] = element;
            _lastCompileTime[normalizedPath] = DateTime.Now;
}

        /// <summary>
        /// 取消注册元素
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void UnregisterElement(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            string normalizedPath = Path.GetFullPath(filePath);
            _loadedElements.Remove(normalizedPath);
            _lastCompileTime.Remove(normalizedPath);
}

        #endregion

        #region 热重载处理

        /// <summary>
        /// 文件变化事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private async void OnFileChanged(object? sender, FileChangedEventArgs e)
        {
            if (!IsEnabled)
                return;

            try
            {
                if (!ShouldRecompile(e.FilePath))
                {
return;
                }

                // 异步处理热重载
                await Task.Run(() => ProcessHotReload(e.FilePath, e.ChangeType));
            }
            catch (Exception ex)
            {
HotReloadError?.Invoke(this, new HotReloadErrorEventArgs
                {
                    FilePath = e.FilePath,
                    Error = ex,
                    Timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 处理热重载
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="changeType">变化类型</param>
        private void ProcessHotReload(string filePath, FileChangeType changeType)
        {
try
            {
                object? newElement = null;
                CompilationResult? compilationResult = null;

                switch (changeType)
                {
                    case FileChangeType.XamlChanged:
                        newElement = ReloadXamlFile(filePath, out compilationResult);
                        break;

                    case FileChangeType.JsonChanged:
                        newElement = ReloadJsonFile(filePath, out compilationResult);
                        break;

                    case FileChangeType.Deleted:
                        HandleFileDeleted(filePath);
                        return;

                    default:
return;
                }

                if (newElement != null)
                {
                    // 更新注册的元素
                    _loadedElements[filePath] = newElement;
                    _lastCompileTime[filePath] = DateTime.Now;
                    HotReloadCompleted?.Invoke(this, new HotReloadEventArgs
                    {
                        FilePath = filePath,
                        NewElement = newElement,
                        CompilationResult = compilationResult,
                        Timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
HotReloadError?.Invoke(this, new HotReloadErrorEventArgs
                {
                    FilePath = filePath,
                    Error = ex,
                    Timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 重新加载XAML文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="compilationResult">编译结果</param>
        /// <returns>新的UI元素</returns>
        private object? ReloadXamlFile(string filePath, out CompilationResult? compilationResult)
        {
try
            {
                string xamlContent = File.ReadAllText(filePath);
                string className = Path.GetFileNameWithoutExtension(filePath);

                // 编译XAML
                compilationResult = _compiler.CompileXaml(xamlContent, className);
                
                if (!compilationResult.Success)
                {
return null;
                }

                // 解析XAML
                object element = XamlLoader.Load(xamlContent);
return element;
            }
            catch (Exception ex)
            {
compilationResult = CompilationResult.Error($"XAML reload failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 重新加载JSON文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="compilationResult">编译结果</param>
        /// <returns>新的UI元素</returns>
        private object? ReloadJsonFile(string filePath, out CompilationResult? compilationResult)
        {
try
            {
                string jsonContent = File.ReadAllText(filePath);
                string className = Path.GetFileNameWithoutExtension(filePath);

                // 编译JSON
                compilationResult = _compiler.CompileJson(jsonContent, className);
                
                if (!compilationResult.Success)
                {
return null;
                }

                // 解析JSON
                object element = JsonLoader.Load(jsonContent);
return element;
            }
            catch (Exception ex)
            {
compilationResult = CompilationResult.Error($"JSON reload failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 处理文件删除
        /// </summary>
        /// <param name="filePath">文件路径</param>
        private void HandleFileDeleted(string filePath)
        {
UnregisterElement(filePath);

            HotReloadCompleted?.Invoke(this, new HotReloadEventArgs
            {
                FilePath = filePath,
                NewElement = null,
                CompilationResult = null,
                Timestamp = DateTime.Now
            });
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 判断是否需要重新编译
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否需要重新编译</returns>
        private bool ShouldRecompile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            // 检查文件是否已注册
            if (!_loadedElements.ContainsKey(filePath))
                return false;

            // 检查文件修改时间
            if (_lastCompileTime.TryGetValue(filePath, out DateTime lastCompile))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.LastWriteTime > lastCompile;
            }

            return true;
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
StopWatching();
                    _fileWatcher?.Dispose();

                    _loadedElements.Clear();
                    _lastCompileTime.Clear();
}

                _disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// 热重载事件参数
    /// </summary>
    public class HotReloadEventArgs : EventArgs
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 新的UI元素
        /// </summary>
        public object? NewElement { get; set; }

        /// <summary>
        /// 编译结果
        /// </summary>
        public CompilationResult? CompilationResult { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 热重载错误事件参数
    /// </summary>
    public class HotReloadErrorEventArgs : EventArgs
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 错误信息
        /// </summary>
        public Exception Error { get; set; } = new Exception();

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
