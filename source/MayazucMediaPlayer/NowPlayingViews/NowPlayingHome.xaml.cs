using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;
using Windows.Media.Playback;

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
        }

        private async Task SetPosterSourceFromStoredData()
        {
            var playbackModel = AppState.Current.Services.GetService<PlaybackSequenceService>();
            var currentMds = await playbackModel.CurrentMediaMetadata();
            if (currentMds.IsSuccess)
            {
                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    var resumePosition = SettingsService.Instance.PlayerResumePosition;
                    var thumbnail = await currentMds.Value.MediaData.GetThumbnailAtPositionAsync(TimeSpan.FromMilliseconds(resumePosition));
                    MediaPlayerElementInstance.PosterSource = thumbnail.MediaThumbnailData;
                });
            }
            else
            {
                await SetDefaultPosterSource();
            }
        }

        private async Task SetDefaultPosterSource()
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                MediaPlayerElementInstance.PosterSource = new BitmapImage(new Uri(FontIconPaths.PlaceholderAlbumArt));
            });
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

        protected async override Task OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                if (args.Reason == MediaOpenedEventReason.MediaPlaybackListItemChanged)
                {
                    HandleMediaOpened(sender);
                }
            });
        }

        private void HandleMediaOpened(MediaPlayer sender)
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
                    img.ImageFailed += Img_ImageFailed;
                    img.SetSource(await currentPlayer.SystemMediaTransportControls.DisplayUpdater.Thumbnail.OpenReadAsync());
                    MediaPlayerElementInstance.PosterSource = img;

                    MusicPlaybackBackgroundGrid.Visibility = Visibility.Visible;
                }
                catch { }
                return;
            }

            //all other cases, load from MDS forcefully.
            await SetPosterSourceFromStoredData();
        }

        private async void Img_ImageFailed(object? sender, ExceptionRoutedEventArgs e)
        {
            await SetDefaultPosterSource();
        }
    }
}
