using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
