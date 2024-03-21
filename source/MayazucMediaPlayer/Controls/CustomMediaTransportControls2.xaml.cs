using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.UI.Controls;
using FluentResults;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.Playback;
using Windows.System.Threading;
using Windows.UI.ViewManagement;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class CustomMediaTransportControls2 : UserControl
    {
        DispatcherQueueTimer StateUpdateTimer;
        SymbolIcon PlayIcon = new SymbolIcon(Symbol.Play);
        SymbolIcon PauseIcon = new SymbolIcon(Symbol.Pause);
        bool userInteractsWithSeekbar = false;
        private AsyncLock mediaOpenedLock = new AsyncLock();
        private bool progressSliderManipulating;

        public CustomMediaTransportControls2()
        {
            InitializeComponent();

            FullScreenButton.Icon = FullScreenIcon();

            StateUpdateTimer = DispatcherQueue.CreateTimer();
            StateUpdateTimer.Interval = TimeSpan.FromSeconds(0.5);
            StateUpdateTimer.Tick += StateUpdateTimer_Tick;

            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened += Current_MediaOpened;
            AppState.Current.MediaServiceConnector.CurrentPlaybackSession.PlaybackStateChanged += CurrentPlaybackSession_PlaybackStateChanged;
            StateUpdateTimer.Start();

            MediaProgressBar.AddHandler(Slider.PointerReleasedEvent, new PointerEventHandler(ProgressBarSeek), true);
            MediaProgressBar.AddHandler(Slider.PointerPressedEvent, new PointerEventHandler(ProgressBarManipulation), true);
        }

        private async void CurrentPlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                await UpdateState();
            });
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


        private async void Current_MediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            using (await mediaOpenedLock.LockAsync())
            {
                if (args.Reason == MediaOpenedEventReason.MediaPlaybackListItemChanged)
                {
                    var mds = args.EventData.ExtraData.MediaPlayerItemSource;

                    await DispatcherQueue.EnqueueAsync(async () =>
                    {
                        await mtcSubtitlesControl.LoadMediaPlaybackItem(args.EventData.PlaybackItem);
                        await mtcVideoTracks.LoadVideoTracksAsync(args.EventData.PlaybackItem);
                        await mtcAudioTracks.LoadAudioTracksAsync(args.EventData.PlaybackItem);
                    });
                }
            }
        }

        private async void StateUpdateTimer_Tick(DispatcherQueueTimer sender, object args)
        {
            await UpdateState();
        }

        private async Task UpdateState()
        {
            if (AppState.Current.MediaServiceConnector.IsPlaying())
            {
                PlayPauseButton.Icon = PauseIcon;

            }
            else PlayPauseButton.Icon = PlayIcon;
            var playbackSession = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;

            MediaPositionTextBlock.Text = playbackSession.Position.ToString("hh':'mm':'ss");
            MediaDurationTextBlock.Text = playbackSession.NaturalDuration.ToString("hh':'mm':'ss");

            if (!userInteractsWithSeekbar)
            {
                MediaProgressBar.Value = AppState.Current.MediaServiceConnector.GetNormalizedPosition();
            }
        }

        private async void GoToPrevious_click(object sender, RoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SkipPrevious();
        }

        private async void SkipBack_click(object sender, RoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SkipSecondsBack(Constants.JumpBackSeconds);
        }

        private async void PlayPause_click(object sender, RoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SendPlayPause();
        }

        private async void SkipForward_click(object sender, RoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SkipSecondsForth(Constants.JumpAheadSeconds);
        }

        private async void SkipNext_click(object sender, RoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SkipNext();
        }

        private async void GoFullScreen_click(object sender, RoutedEventArgs e)
        {
            //new FontIcon { FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets") , Glyph = "\uE73F" };
            bool shouldFullScreen = !MainWindowingService.Instance.IsInFullScreenMode();
            await MainWindowingService.Instance.RequestFullScreenMode(shouldFullScreen);

            FullScreenButton.Icon = (shouldFullScreen ? ExitFullScreenIcon() : FullScreenIcon());
        }

        private FontIcon FullScreenIcon()
        {
            return new FontIcon { FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE740" };
        }

        private FontIcon ExitFullScreenIcon()
        {
            return new FontIcon { FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE73F" };
        }
    }
}

