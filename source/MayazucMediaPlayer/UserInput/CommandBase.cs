using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MayazucMediaPlayer.UserInput
{
    /// <summary>
    /// TODO:needs reimplementation
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        public CommandBase()
        {

        }

        public event EventHandler? CanExecuteChanged;

        public abstract Task ExecuteAsync(object param);

        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);

        #region Protected Methods

        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }



        #endregion Protected Methods
    }
}
