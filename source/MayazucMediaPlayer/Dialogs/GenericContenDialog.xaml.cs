using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class GenericContenDialog : ContentDialog
    {
        public GenericContenDialog(FrameworkElement content)
        {
            InitializeComponent();
            ContentPresenterRoot.Children.Add(content);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
