using MayazucMediaPlayer.MediaPlayback;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static MayazucMediaPlayer.MediaPlayback.PlaybackSpeedService;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class MTCPlaybackSpeedControl : BaseUserControl
    {
        public MTCPlaybackSpeedControl()
        {
            this.InitializeComponent();
            this.Loaded += MTCPlaybackSpeedControl_Loaded;
            MediaPlaybackSpeedSlider.Maximum = SupportedPlaybackSpeeds.Count - 1;
            MediaPlaybackSpeedSlider.Minimum = 0;
            MediaPlaybackSpeedSlider.ValueChanged += OnMediaPlaybackSpeedValueChanged;
        }

        private async void MTCPlaybackSpeedControl_Loaded(object sender, RoutedEventArgs e)
        {
            MediaPlaybackSpeedSlider.Value = SupportedPlaybackSpeeds.IndexOf(await AppState.Current.MediaServiceConnector.PlayerInstance.GetPlaybackSpeed());
        }

        private void ResetAudioSpeed(object sender, RoutedEventArgs e)
        {
            MediaPlaybackSpeedSlider.Value = 2;
        }

        private async void OnMediaPlaybackSpeedValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var newValue = (int)MediaPlaybackSpeedSlider.Value;
            await AppState.Current.MediaServiceConnector.PlayerInstance.SetPlaybackSpeed(SupportedPlaybackSpeeds[newValue]);
        }
    }
}
