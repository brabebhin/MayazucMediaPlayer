using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Media.Playback;
using Microsoft.UI.Xaml.Shapes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class MediaProgressBarSlider : UserControl
    {
        private bool progressSliderManipulating = false;

        public MediaProgressBarSlider()
        {
            this.InitializeComponent();

            MediaProgressBar.AddHandler(Slider.PointerReleasedEvent, new PointerEventHandler(ProgressBarSeek), true);
            MediaProgressBar.AddHandler(Slider.PointerPressedEvent, new PointerEventHandler(ProgressBarManipulation), true);
        }

        public bool IsUserManipulating()
        {
            return progressSliderManipulating;
        }

        private void ProgressBarManipulation(object sender, PointerRoutedEventArgs e)
        {
            progressSliderManipulating = true;
        }

        private async void ProgressBarSeek(object sender, PointerRoutedEventArgs e)
        {
            var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
            if (session != null)
            {
                var time = MediaPlaybackSeekbarUtil.GetDenormalizedValue(MediaProgressBar.Value, session.NaturalDuration);
                await (AppState.Current.MediaServiceConnector.PlayerInstance).Seek(time, true);
                progressSliderManipulating = false;
            }
        }

        public void UpdateState(MediaPlaybackSession playbackSession)
        {
            MediaPositionTextBlock.Text = playbackSession.Position.FormatTimespan();
            MediaDurationTextBlock.Text = playbackSession.NaturalDuration.FormatTimespan();

            if (!progressSliderManipulating)
            {
                MediaProgressBar.Value = AppState.Current.MediaServiceConnector.GetNormalizedPosition();
            }
        }
    }
}
