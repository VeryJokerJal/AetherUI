using System;
using System.Windows.Input;
using System.Diagnostics;

namespace AetherUI.Core
{
    /// <summary>
    /// 实现ICommand接口的通用命令类
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// 命令可执行状态更改事件
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// 初始化RelayCommand实例
        /// </summary>
        /// <param name="execute">命令执行的操作</param>
        /// <param name="canExecute">判断命令是否可执行的函数</param>
        /// <exception cref="ArgumentNullException">execute为null时抛出</exception>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>如果命令可以执行则返回true</returns>
        public bool CanExecute(object? parameter)
        {
            bool canExecute = _canExecute?.Invoke() ?? true;
            Debug.WriteLine($"Command CanExecute: {canExecute}");
            return canExecute;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object? parameter)
        {
            Debug.WriteLine("Command Execute called");
            if (CanExecute(parameter))
            {
                _execute();
            }
        }

        /// <summary>
        /// 触发CanExecuteChanged事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            Debug.WriteLine("Command RaiseCanExecuteChanged called");
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 带参数的通用命令类
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        /// <summary>
        /// 命令可执行状态更改事件
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// 初始化RelayCommand实例
        /// </summary>
        /// <param name="execute">命令执行的操作</param>
        /// <param name="canExecute">判断命令是否可执行的函数</param>
        /// <exception cref="ArgumentNullException">execute为null时抛出</exception>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断命令是否可以执行
        /// </summary>
        /// <param name="parameter">命令参数</param>
        /// <returns>如果命令可以执行则返回true</returns>
        public bool CanExecute(object? parameter)
        {
            T? typedParameter = parameter is T param ? param : default;
            bool canExecute = _canExecute?.Invoke(typedParameter) ?? true;
            Debug.WriteLine($"Command<{typeof(T).Name}> CanExecute: {canExecute}");
            return canExecute;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">命令参数</param>
        public void Execute(object? parameter)
        {
            Debug.WriteLine($"Command<{typeof(T).Name}> Execute called");
            if (CanExecute(parameter))
            {
                T? typedParameter = parameter is T param ? param : default;
                _execute(typedParameter);
            }
        }

        /// <summary>
        /// 触发CanExecuteChanged事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            Debug.WriteLine($"Command<{typeof(T).Name}> RaiseCanExecuteChanged called");
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
