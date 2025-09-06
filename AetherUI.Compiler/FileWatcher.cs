using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AetherUI.Compiler
{
    /// <summary>
    /// 文件监控器，用于监控文件变化并触发热重载
    /// </summary>
    public class FileWatcher : IDisposable
    {
        private readonly Dictionary<string, FileSystemWatcher> _watchers;
        private readonly Dictionary<string, DateTime> _lastChangeTime;
        private readonly Timer _debounceTimer;
        private readonly object _lock = new object();
        private bool _disposed = false;

        #region 事件

        /// <summary>
        /// 文件变化事件
        /// </summary>
        public event EventHandler<FileChangedEventArgs>? FileChanged;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化文件监控器
        /// </summary>
        public FileWatcher()
        {
            _watchers = new Dictionary<string, FileSystemWatcher>();
            _lastChangeTime = new Dictionary<string, DateTime>();
            
            // 创建防抖动计时器
            _debounceTimer = new Timer(OnDebounceTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);

            Debug.WriteLine("FileWatcher initialized");
        }

        #endregion

        #region 监控管理

        /// <summary>
        /// 添加监控路径
        /// </summary>
        /// <param name="path">要监控的路径</param>
        /// <param name="filter">文件过滤器</param>
        public void AddWatch(string path, string filter = "*.*")
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");

            lock (_lock)
            {
                if (_watchers.ContainsKey(path))
                {
                    Debug.WriteLine($"Path already being watched: {path}");
                    return;
                }

                Debug.WriteLine($"Adding watch for path: {path}, filter: {filter}");

                FileSystemWatcher watcher = new FileSystemWatcher(path, filter)
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
                };

                watcher.Changed += OnFileSystemEvent;
                watcher.Created += OnFileSystemEvent;
                watcher.Deleted += OnFileSystemEvent;
                watcher.Renamed += OnFileSystemEvent;

                watcher.EnableRaisingEvents = true;
                _watchers[path] = watcher;

                Debug.WriteLine($"Watch added successfully for: {path}");
            }
        }

        /// <summary>
        /// 移除监控路径
        /// </summary>
        /// <param name="path">要移除的路径</param>
        public void RemoveWatch(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            lock (_lock)
            {
                if (_watchers.TryGetValue(path, out FileSystemWatcher? watcher))
                {
                    Debug.WriteLine($"Removing watch for path: {path}");

                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                    _watchers.Remove(path);

                    Debug.WriteLine($"Watch removed for: {path}");
                }
            }
        }

        /// <summary>
        /// 清除所有监控
        /// </summary>
        public void ClearWatches()
        {
            lock (_lock)
            {
                Debug.WriteLine("Clearing all watches");

                foreach (var watcher in _watchers.Values)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }

                _watchers.Clear();
                _lastChangeTime.Clear();

                Debug.WriteLine("All watches cleared");
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 文件系统事件处理
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
        {
            try
            {
                // 忽略临时文件和隐藏文件
                if (IsIgnoredFile(e.FullPath))
                    return;

                Debug.WriteLine($"File system event: {e.ChangeType} - {e.FullPath}");

                lock (_lock)
                {
                    // 防抖动处理
                    DateTime now = DateTime.Now;
                    if (_lastChangeTime.TryGetValue(e.FullPath, out DateTime lastTime))
                    {
                        if ((now - lastTime).TotalMilliseconds < 500) // 500ms防抖动
                        {
                            Debug.WriteLine($"Debouncing file change: {e.FullPath}");
                            return;
                        }
                    }

                    _lastChangeTime[e.FullPath] = now;

                    // 延迟触发事件，避免文件正在写入时读取
                    _debounceTimer.Change(200, Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling file system event: {ex.Message}");
            }
        }

        /// <summary>
        /// 防抖动计时器回调
        /// </summary>
        /// <param name="state">状态对象</param>
        private void OnDebounceTimerElapsed(object? state)
        {
            try
            {
                List<string> changedFiles = new List<string>();

                lock (_lock)
                {
                    DateTime cutoff = DateTime.Now.AddMilliseconds(-300);
                    foreach (var kvp in _lastChangeTime)
                    {
                        if (kvp.Value >= cutoff && File.Exists(kvp.Key))
                        {
                            changedFiles.Add(kvp.Key);
                        }
                    }
                }

                // 触发文件变化事件
                foreach (string filePath in changedFiles)
                {
                    Debug.WriteLine($"Triggering file changed event: {filePath}");
                    
                    FileChanged?.Invoke(this, new FileChangedEventArgs
                    {
                        FilePath = filePath,
                        ChangeType = GetFileChangeType(filePath),
                        Timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in debounce timer: {ex.Message}");
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 判断是否为忽略的文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否忽略</returns>
        private bool IsIgnoredFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            // 忽略临时文件
            if (fileName.StartsWith("~") || fileName.StartsWith("."))
                return true;

            // 忽略特定扩展名
            string[] ignoredExtensions = { ".tmp", ".temp", ".bak", ".swp", ".log" };
            if (Array.Exists(ignoredExtensions, ext => ext == extension))
                return true;

            // 忽略编译输出目录
            if (filePath.Contains("\\bin\\") || filePath.Contains("\\obj\\"))
                return true;

            return false;
        }

        /// <summary>
        /// 获取文件变化类型
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>变化类型</returns>
        private FileChangeType GetFileChangeType(string filePath)
        {
            if (!File.Exists(filePath))
                return FileChangeType.Deleted;

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".xaml" => FileChangeType.XamlChanged,
                ".json" => FileChangeType.JsonChanged,
                ".cs" => FileChangeType.CodeChanged,
                _ => FileChangeType.Other
            };
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
                    Debug.WriteLine("Disposing FileWatcher...");

                    _debounceTimer?.Dispose();
                    ClearWatches();

                    Debug.WriteLine("FileWatcher disposed");
                }

                _disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// 文件变化事件参数
    /// </summary>
    public class FileChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 变化类型
        /// </summary>
        public FileChangeType ChangeType { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// 文件变化类型
    /// </summary>
    public enum FileChangeType
    {
        /// <summary>
        /// XAML文件变化
        /// </summary>
        XamlChanged,

        /// <summary>
        /// JSON文件变化
        /// </summary>
        JsonChanged,

        /// <summary>
        /// 代码文件变化
        /// </summary>
        CodeChanged,

        /// <summary>
        /// 文件删除
        /// </summary>
        Deleted,

        /// <summary>
        /// 其他类型
        /// </summary>
        Other
    }
}
