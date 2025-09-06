using System;

namespace AetherUI.Core
{
    /// <summary>
    /// 定义命令接口
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 当命令的可执行状态发生变化时发生
        /// </summary>
        event EventHandler? CanExecuteChanged;

        /// <summary>
        /// 定义确定此命令是否可以在其当前状态下执行的方法
        /// </summary>
        /// <param name="parameter">此命令使用的数据。如果此命令不需要传递数据，则该对象可以设置为null</param>
        /// <returns>如果可以执行此命令，则为true；否则为false</returns>
        bool CanExecute(object? parameter);

        /// <summary>
        /// 定义在调用此命令时要调用的方法
        /// </summary>
        /// <param name="parameter">此命令使用的数据。如果此命令不需要传递数据，则该对象可以设置为null</param>
        void Execute(object? parameter);
    }
}
