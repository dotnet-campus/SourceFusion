using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;

namespace dotnetCampus.SourceFusion.Sample.Fakes
{
    /// <summary>
    /// 这是一个纯测试用类，没有任何用途。
    /// </summary>
    public class ActionCommand<T> : ICommand
    {
        /// <summary>
        /// 创建 <see cref="ActionCommand{T}"/> 的新实例，当 <see cref="ICommand"/> 被执行时，将调用参数传入的动作。
        /// </summary>
        public ActionCommand(Action<T> action, Func<bool> canExecute = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 用于接受所提供的参数并执行的委托。
        /// 只可能是 Action{T} 或 Func{T, Task}。
        /// </summary>
        private readonly Action<T> _action;

        /// <summary>
        /// 此 <see cref="ActionCommand{T}"/> 中用于判定任务是否可以执行。
        /// </summary>
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// 使用指定的参数执行此命令。
        /// 框架中没有约定参数值是否允许为 null，这由参数定义时的泛型类型约定（C#8.0）或由命令的实现者约定。
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "t")]
        public void Execute(T t)
        {
            _action(t);
        }

#if GENERATED_CODE
        /// <summary>
        /// 尝试以异步的方式执行此同步命令。因为参数的获取可能是异步的（例如使用 <see cref="IAsyncParameterProvider{T}"/>），所以此方法也必须是异步的。
        /// 对于传入的参数会被解析成多个参数，传入的参数一定不允许为 null。
        /// </summary>
        /// <param name="parameter">接口中传入的原始参数。</param>
#else
        /// <summary>
        /// 尝试以异步的方式执行此同步命令。因为参数的获取可能是异步的（例如使用 <see cref="IAsyncParameterProvider{T}"/>），所以此方法也必须是异步的。
        /// 对于单个泛型参数的 <see cref="ActionCommand{T}"/> 而言，传入的参数由业务定义含义，所以不能保证 null 值的合理性。
        /// </summary>
        /// <param name="parameter">接口中传入的原始参数。</param>
#endif
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task ExecuteAsync(object parameter)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Execute((T)parameter);
        }

        void ICommand.Execute(object parameter)
        {
            ExecuteAsync(parameter).ConfigureAwait(false);
        }

        bool ICommand.CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        /// <inheritdoc />
        /// <summary>
        /// 当命令的可执行性改变时发生。
        /// </summary>
        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
