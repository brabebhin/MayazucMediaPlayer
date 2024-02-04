using MayazucMediaPlayer.Dialogs;
using Windows.Foundation;
using Windows.UI.Popups;
using WinRT;

namespace MayazucMediaPlayer.Runtime
{
    internal class MessageDialogService : IMessageDialogService
    {
        private static readonly object _lock = new object();
        private static IMessageDialogService? _messageDialogService;

        public static IMessageDialogService Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_messageDialogService == null)
                    {
                        _messageDialogService = new MessageDialogService();
                    }
                    return _messageDialogService;
                }
            }
        }

        public IAsyncOperation<IUICommand> ShowMessageDialogAsync(string message, string title, params IUICommand[]? commands)
        {
            MessageDialog dialog = new MessageDialog(message, title);

            if (commands != null)
            {
                foreach (var cmd in commands)
                {
                    dialog.Commands.Add(cmd);
                }
            }

            var initializeWithWindowWrapper = dialog.As<IInitializeWithWindow>();
            initializeWithWindowWrapper.Initialize(App.GetActiveWindow());
            return dialog.ShowAsync();
        }

        public IAsyncOperation<IUICommand> ShowMessageDialogAsync(string message)
        {
            return ShowMessageDialogAsync(message, string.Empty);
        }
    }
}
