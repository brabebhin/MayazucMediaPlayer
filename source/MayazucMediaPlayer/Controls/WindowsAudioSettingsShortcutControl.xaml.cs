using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml.Controls;
using System;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class WindowsAudioSettingsShortcutControl : BaseUserControl, IContentSettingsItem
    {
        public WindowsAudioSettingsShortcutControl()
        {
            InitializeComponent();
        }

        public void RecheckValue()
        {
        }

        private async void kjh(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync((sender as HyperlinkButton).NavigateUri);
        }
    }
}
