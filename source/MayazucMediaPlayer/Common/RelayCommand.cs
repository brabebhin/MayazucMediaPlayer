using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Common
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality 
    /// to other objects by invoking delegates. 
    /// The default return value for the CanExecute method is 'true'.
    /// <see cref="RaiseCanExecuteChanged"/> needs to be called whenever
    /// <see cref="CanExecute"/> is expected to return a different value.
    /// </summary>
    public class RelayCommand : CommandBase
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        private readonly Action<object> _executeParametrized;
        /// <summary>
        /// Raised when RaiseCanExecuteChanged is called.
        /// </summary>

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute)
         : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action execute, Func<bool> canExecute) : base()
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Action<object> executeParametrized, Func<bool> canExecute) : base()
        {
            if (executeParametrized == null)
                throw new ArgumentNullException("execute");
            _executeParametrized = executeParametrized;
            if (canExecute != null)
                _canExecute = canExecute;

        }


        private bool CanExecuteInternal(object param)
        {
            if (_canExecute != null)
                return CanExecute(param);
            else
            {
                if (param is ButtonBase)
                {
                    return (param as ButtonBase).IsEnabled;
                }
                return true;

            }
        }

        /// <summary>
        /// Determines whether this <see cref="RelayCommand"/> can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public override bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        /// <summary>
        /// Executes the <see cref="RelayCommand"/> on the current command target.
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. If the command does not require data to be passed, this object can be set to null.
        /// </param>
        public override void Execute(object parameter)
        {

            if (CanExecuteInternal(parameter))
            {
                _execute?.Invoke();
                _executeParametrized?.Invoke(parameter);
            }
        }

        public override Task ExecuteAsync(object param)
        {
            Execute(param);
            return Task.CompletedTask;
        }
    }
}