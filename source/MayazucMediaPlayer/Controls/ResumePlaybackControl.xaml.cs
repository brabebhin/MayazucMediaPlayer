using CommunityToolkit.WinUI;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class ResumePlaybackControl : UserControl
    {
        public bool IsDismissed { get; private set; }
        bool firstLoaded = false;
        readonly PlaybackSequenceService playbackService;

        public ResumePlaybackControl()
        {
            InitializeComponent();
            Loaded += ResumePlaybackControl_Loaded;
            playbackService = AppState.Current.Services.GetService<PlaybackSequenceService>();
            playbackService.NowPlayingBackStore.CollectionChanged += NowPlayingBackStore_CollectionChanged;
            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened += Result_OnMediaOpenedExternal;
        }

        public void FreeMemory()
        {
            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened -= Result_OnMediaOpenedExternal;
            if (playbackService != null)
                playbackService.NowPlayingBackStore.CollectionChanged -= NowPlayingBackStore_CollectionChanged;
        }

        private async void ResumePlaybackControl_Loaded(object? sender, RoutedEventArgs e)
        {
            if (!firstLoaded)
            {
                await LoadFromStoredData(playbackService);
                firstLoaded = true;
            }
        }

        private async void NowPlayingBackStore_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                Dismiss();
            });
        }

        private async void Result_OnMediaOpenedExternal(object? sender, MediaOpenedEventArgs e)
        {
            await SignalMediaOpened();
        }

        private async Task SignalMediaOpened()
        {
            await DispatcherQueue.EnqueueAsync(() => { Dismiss(); });
        }

        private async Task LoadFromStoredData(PlaybackSequenceService model)
        {
            var currentMds = await model.CurrentMediaMetadata();
            if (currentMds.IsFailed)
            {
                Visibility = Visibility.Collapsed;
            }
            else
            {
                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    var resumePosition = SettingsWrapper.PlayerResumePosition;
                    var thumbnail = await currentMds.Value.MediaData.GetThumbnailAtPositionAsync(TimeSpan.FromTicks(resumePosition));
                    ResumeThumbnail.Source = thumbnail.MediaThumbnailData;

                    ResumeFile.Text = currentMds.Value.MediaData.Title;
                    Position.Text = TimeSpan.FromTicks(resumePosition).ToString("hh':'mm':'ss");
                });
            }
        }

        public void SetVisibility(Visibility visibility)
        {
            if (IsDismissed) Visibility = Visibility.Collapsed;
            else Visibility = visibility;
        }

        public void Dismiss()
        {
            Visibility = Visibility.Collapsed;
            IsDismissed = true;
        }

        private void Dissmiss_dialog(object? sender, TappedRoutedEventArgs e)
        {
            Dismiss();
            e.Handled = true;
        }

        private async void Resume_click(object? sender, TappedRoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SendPlayPause();

            Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private async void StartOver_click(object? sender, TappedRoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SkipToIndex(SettingsWrapper.PlaybackIndex);

            Visibility = Visibility.Collapsed;
            e.Handled = true;
        }
    }
}
