using MayazucMediaPlayer.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    /// <summary>
    /// Displays arbitrary, complex content (framework elements) in a manner similar to a ContentDialog
    /// This works around WinUI 3 limitations in regards to content dialogs, particularly Width behavior 
    /// 
    /// Should probably be replaced at some point in the future, if WinUI 3 comes up with a suitable alternative.
    /// </summary>
    public sealed partial class GenericContentControl : BaseUserControl, IContentDialogService
    {
        TaskCompletionSource<ContentDialogServiceResult>? currentAsyncDialogOperation = default(TaskCompletionSource<ContentDialogServiceResult>);
        public GenericContentControl()
        {
            InitializeComponent();
        }

        public Task<ContentDialogServiceResult> ShowDialogAsync(FrameworkElement content)
        {
            currentAsyncDialogOperation?.TrySetResult(new ContentDialogServiceResult(true));
            currentAsyncDialogOperation = new TaskCompletionSource<ContentDialogServiceResult>();
            ContentDialogPresenter.Children.Clear();
            ContentDialogPresenter.Children.Add(content);
            Visibility = Visibility.Visible;

            return currentAsyncDialogOperation.Task;
        }

        private void ContentDialogPresenter_tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void ContentDialogServiceRootGrid_tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            ContentDialogPresenter.Children.Clear();
            Visibility = Visibility.Collapsed;
            currentAsyncDialogOperation?.TrySetResult(new ContentDialogServiceResult(false));
        }
    }

    public class ContentDialogServiceResult
    {
        public bool Canceled { get; private set; }

        public ContentDialogServiceResult(bool canceled)
        {
            Canceled = canceled;
        }
    }
}
