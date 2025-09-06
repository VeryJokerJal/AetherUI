using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Diagnostics;

namespace AetherUI.Events
{
    /// <summary>
    /// 命令绑定类，将命令与事件处理器关联
    /// </summary>
    public class CommandBinding
    {
        /// <summary>
        /// 绑定的命令
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        /// 执行事件处理器
        /// </summary>
        public ExecutedRoutedEventHandler? Executed { get; set; }

        /// <summary>
        /// 可执行查询事件处理器
        /// </summary>
        public CanExecuteRoutedEventHandler? CanExecute { get; set; }

        /// <summary>
        /// 预览执行事件处理器
        /// </summary>
        public ExecutedRoutedEventHandler? PreviewExecuted { get; set; }

        /// <summary>
        /// 预览可执行查询事件处理器
        /// </summary>
        public CanExecuteRoutedEventHandler? PreviewCanExecute { get; set; }

        /// <summary>
        /// 初始化命令绑定
        /// </summary>
        /// <param name="command">要绑定的命令</param>
        public CommandBinding(ICommand command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <summary>
        /// 初始化命令绑定
        /// </summary>
        /// <param name="command">要绑定的命令</param>
        /// <param name="executed">执行事件处理器</param>
        public CommandBinding(ICommand command, ExecutedRoutedEventHandler executed)
            : this(command)
        {
            Executed = executed;
        }

        /// <summary>
        /// 初始化命令绑定
        /// </summary>
        /// <param name="command">要绑定的命令</param>
        /// <param name="executed">执行事件处理器</param>
        /// <param name="canExecute">可执行查询事件处理器</param>
        public CommandBinding(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
            : this(command, executed)
        {
            CanExecute = canExecute;
        }
    }

    /// <summary>
    /// 执行路由事件参数
    /// </summary>
    public class ExecutedRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 命令
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        /// 命令参数
        /// </summary>
        public object? Parameter { get; }

        /// <summary>
        /// 初始化执行路由事件参数
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="parameter">命令参数</param>
        public ExecutedRoutedEventArgs(ICommand command, object? parameter)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Parameter = parameter;
        }
    }

    /// <summary>
    /// 可执行查询路由事件参数
    /// </summary>
    public class CanExecuteRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 命令
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        /// 命令参数
        /// </summary>
        public object? Parameter { get; }

        /// <summary>
        /// 是否可以执行
        /// </summary>
        public bool CanExecute { get; set; }

        /// <summary>
        /// 是否继续路由
        /// </summary>
        public bool ContinueRouting { get; set; }

        /// <summary>
        /// 初始化可执行查询路由事件参数
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="parameter">命令参数</param>
        public CanExecuteRoutedEventArgs(ICommand command, object? parameter)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Parameter = parameter;
            CanExecute = false;
            ContinueRouting = false;
        }
    }

    /// <summary>
    /// 执行路由事件处理器委托
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">执行路由事件参数</param>
    public delegate void ExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs e);

    /// <summary>
    /// 可执行查询路由事件处理器委托
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">可执行查询路由事件参数</param>
    public delegate void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e);

    /// <summary>
    /// 命令管理器，处理命令的路由和执行
    /// </summary>
    public static class CommandManager
    {
        private static readonly List<CommandBinding> _globalCommandBindings = new List<CommandBinding>();

        /// <summary>
        /// 添加全局命令绑定
        /// </summary>
        /// <param name="commandBinding">命令绑定</param>
        public static void RegisterClassCommandBinding(CommandBinding commandBinding)
        {
            if (commandBinding == null)
                throw new ArgumentNullException(nameof(commandBinding));

            _globalCommandBindings.Add(commandBinding);
}

        /// <summary>
        /// 移除全局命令绑定
        /// </summary>
        /// <param name="commandBinding">命令绑定</param>
        public static void UnregisterClassCommandBinding(CommandBinding commandBinding)
        {
            if (commandBinding == null)
                throw new ArgumentNullException(nameof(commandBinding));

            _globalCommandBindings.Remove(commandBinding);
}

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="parameter">命令参数</param>
        /// <param name="target">目标元素</param>
        /// <returns>如果命令被执行则返回true</returns>
        public static bool ExecuteCommand(ICommand command, object? parameter, object target)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (!CanExecuteCommand(command, parameter, target))
            {
return false;
            }

            // 查找命令绑定并执行
            CommandBinding? binding = FindCommandBinding(command, target);
            if (binding?.Executed != null)
            {
                ExecutedRoutedEventArgs args = new ExecutedRoutedEventArgs(command, parameter);
                binding.Executed(target, args);
return true;
            }

            // 如果没有找到绑定，直接执行命令
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
return true;
            }

            return false;
        }

        /// <summary>
        /// 查询命令是否可以执行
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="parameter">命令参数</param>
        /// <param name="target">目标元素</param>
        /// <returns>如果命令可以执行则返回true</returns>
        public static bool CanExecuteCommand(ICommand command, object? parameter, object target)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            // 查找命令绑定并查询
            CommandBinding? binding = FindCommandBinding(command, target);
            if (binding?.CanExecute != null)
            {
                CanExecuteRoutedEventArgs args = new CanExecuteRoutedEventArgs(command, parameter);
                binding.CanExecute(target, args);
return args.CanExecute;
            }

            // 如果没有找到绑定，使用命令自身的CanExecute
            bool canExecute = command.CanExecute(parameter);
return canExecute;
        }

        /// <summary>
        /// 查找命令绑定
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="target">目标元素</param>
        /// <returns>找到的命令绑定，如果没有则返回null</returns>
        private static CommandBinding? FindCommandBinding(ICommand command, object target)
        {
            // 首先在全局绑定中查找
            foreach (CommandBinding binding in _globalCommandBindings)
            {
                if (binding.Command == command)
                {
                    return binding;
                }
            }

            // 这里可以扩展为在目标元素的本地绑定中查找
            // 暂时只支持全局绑定

            return null;
        }
    }
}
