using CommunityToolkit.WinUI;
using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.System.Threading;
using Windows.UI.Core;

namespace MayazucMediaPlayer.NowPlayingViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>    
    public sealed partial class NowPlayingHome : BasePage
    {
        public override string Title => "Now Playing";

        public MediaPlayerRenderingElement2 MediaPlayerElementInstance
        {
            get => mediaPlayerElementInstance;
        }

        public NowPlayingHome() : base()
        {
            InitializeComponent();
            InitMediaPlayerElement();
        }

        private void InitMediaPlayerElement()
        {
            MediaPlayerElementInstance.WrappedMediaPlayer = AppState.Current.MediaServiceConnector.CurrentPlayer;

            MediaPlayerElementInstance.AreTransportControlsEnabled = true;
        }

        private async Task SetPosterSourceFromStoredData()
        {
            var playbackModel = AppState.Current.Services.GetService<PlaybackSequenceService>();
            var currentMds = await playbackModel.CurrentMediaMetadata();
            if (currentMds.IsSuccess)
            {
                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    var resumePosition = SettingsWrapper.PlayerResumePosition;
                    var thumbnail = await currentMds.Value.MediaData.GetThumbnailAtPositionAsync(TimeSpan.FromTicks(resumePosition));
                    MediaPlayerElementInstance.PosterSource = thumbnail.MediaThumbnailData;
                });
            }
            else
            {
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    MediaPlayerElementInstance.PosterSource = new BitmapImage(new Uri(FontIconPaths.PlaceholderAlbumArt));
                });
            }
        }

        private void unInitMediaPlayerElement()
        {
            MediaPlayerElementInstance.WrappedMediaPlayer = null;
        }

        protected override void FreeMemory()
        {
            unInitMediaPlayerElement();

            base.FreeMemory();
        }

        protected override Task OnInitializeStateAsync(object? parameter)
        {
            SetMediaPlayerElementPosterSource();
            return Task.CompletedTask;
        }

        private void NowPlayingCOntroller_ExternalNavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            NotifyExternalNavigationRequest(sender, e);
        }

        public async override Task OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                if (args.Reason == MediaOpenedEventReason.MediaPlaybackListItemChanged)
                {
                    MediaOpenedCode(sender);
                }
            });
        }

        private void MediaOpenedCode(MediaPlayer sender)
        {
            if (sender == null)
            {
                return;
            }
            SetMediaPlayerElementPosterSource();
        }

        private async void SetMediaPlayerElementPosterSource()
        {
            var playerInstance = AppState.Current.MediaServiceConnector.PlayerInstance;
            var currentPlayer = AppState.Current.MediaServiceConnector.CurrentPlayer;
            var currentPlaybackItem = playerInstance.CurrentPlaybackItem;

            // active playback item, is playing video -> don't show the poster source
            if (currentPlaybackItem != null && currentPlaybackItem.IsVideo())
            {
                MusicPlaybackBackgroundGrid.Visibility = Visibility.Collapsed;
                mediaPlayerElementInstance.PosterSource = null;
                return;
            }

            //active playback item, not video, thumbnail in display adapter -> show the exact thumbnail as in adapter
            if (currentPlaybackItem != null && !currentPlaybackItem.IsVideo() && currentPlayer.SystemMediaTransportControls.DisplayUpdater.Thumbnail != null)
            {
                try
                {

                    BitmapImage img = new BitmapImage();
                    img.SetSource(await currentPlayer.SystemMediaTransportControls.DisplayUpdater.Thumbnail.OpenReadAsync());
                    img.ImageFailed += Img_ImageFailed;
                    MediaPlayerElementInstance.PosterSource = img;

                    MusicPlaybackBackgroundGrid.Visibility = Visibility.Visible;
                }
                catch { }
                return;
            }

            //all other cases, load from MDS forcefully.
            await SetPosterSourceFromStoredData();
        }

        private void Img_ImageFailed(object? sender, ExceptionRoutedEventArgs e)
        {

        }
    }
}
