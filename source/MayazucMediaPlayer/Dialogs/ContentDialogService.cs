using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Dialogs
{
    public sealed class ContentDialogService
    {
        ContentDialog currentDialog;

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new ContentDialogService();
            }
        }

        public static ContentDialogService Instance { get; private set; }

        private ContentDialogService() { }

        public async Task ShowAsync(ContentDialog next)
        {
            currentDialog?.Hide();
            next.XamlRoot = App.CurrentInstance.CurrentXamlRoot();
            currentDialog = next;

            await currentDialog.ShowAsync();
        }
    }
}
