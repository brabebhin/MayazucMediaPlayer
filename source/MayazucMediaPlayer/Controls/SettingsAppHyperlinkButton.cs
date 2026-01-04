using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;

namespace MayazucMediaPlayer.Controls
{
    internal partial class SettingsAppHyperlinkButton : HyperlinkButton
    {
        public SettingsAppHyperlinkButton()
        {
            this.Tapped += OnTapped;
        }

        private async void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync((sender as HyperlinkButton).NavigateUri);
        }
    }
}
