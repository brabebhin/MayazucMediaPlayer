using Windows.Foundation;
using Windows.UI.Popups;

namespace MayazucMediaPlayer.Dialogs
{
    public interface IMessageDialogService
    {
        public IAsyncOperation<IUICommand> ShowMessageDialogAsync(string message, string title, params IUICommand[]? commands);
        public IAsyncOperation<IUICommand> ShowMessageDialogAsync(string message);
    }
}
