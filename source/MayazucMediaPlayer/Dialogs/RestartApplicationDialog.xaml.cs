using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel.Core;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class RestartApplicationDialog : BaseDialog
    {
        private string RestartArguments { get; set; }

        public RestartApplicationDialog(string restartReason)
        {
            InitializeComponent();
            RestartArguments = restartReason;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await CoreApplication.RequestRestartAsync(RestartArguments);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
