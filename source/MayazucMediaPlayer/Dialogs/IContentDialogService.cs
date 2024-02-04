using MayazucMediaPlayer.Controls;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Dialogs
{
    public interface IContentDialogService
    {
        Task<ContentDialogServiceResult> ShowDialogAsync(FrameworkElement content);
    }
}
