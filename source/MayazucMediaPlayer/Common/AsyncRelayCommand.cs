using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Common
{
    public class AsyncRelayCommand : CommandBase
    {
        private readonly Func<object, bool> _canExecuteMethod;
        private readonly Func<object, Task> _executeMethod;

        public AsyncRelayCommand(Func<object, Task> executeMethod)
            : this(executeMethod, null)
        {
        }

        public AsyncRelayCommand(Func<Task> executeMethod)
            : this(new Func<object, Task>((obj) => executeMethod()), null)
        {
        }

        public AsyncRelayCommand(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
            : base()
        {
            _executeMethod = executeMethod;
            if (canExecuteMethod != null)
            {
                _canExecuteMethod = canExecuteMethod;
            }
        }


        public override bool CanExecute(object parameter)
        {
            return ((_canExecuteMethod == null) || _canExecuteMethod(parameter));
        }

        public override void Execute(object parameter)
        {
            try
            {

                if (CanExecuteInternal(parameter))
                {
                    _executeMethod?.Invoke(parameter).ContinueWith((e) => { if (e.Exception != null) { throw e.Exception; } });
                }
            }
            finally
            {
            }
        }


        private bool CanExecuteInternal(object param)
        {
            if (_canExecuteMethod != null)
            {
                return CanExecute(param);
            }
            else
            {
                if (param is ButtonBase)
                {
                    return (param as ButtonBase).IsEnabled;
                }
                return true;

            }
        }

        public void RaiseCanExecuteChanged()
        {
            base.OnCanExecuteChanged(EventArgs.Empty);
        }


        public override async Task ExecuteAsync(object param)
        {
            if (CanExecuteInternal(param))
            {
                await _executeMethod?.Invoke(param);
            }
        }
    }
}
