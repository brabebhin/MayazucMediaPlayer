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
        public event EventHandler<FullscreenRequestEventArgs>? OnFullScreenRequest;

        private const double NowPlayingSplitViewsPercentage = 0.5;
        CustomMediaTransportControls? UIMediaTransportControls = null;
        public override string Title => "Now Playing";
        readonly InputSystemCursor disposableCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);

        public MediaPlayerElement MediaPlayerElementInstance
        {
            get => mediaPlayerElementInstance.MediaPlayerElementInstance;
        }

        readonly AutoResetEvent _fullscreenTimerLock = new AutoResetEvent(true);
        ThreadPoolTimer? _fullscreenCursorTimer;
        readonly AsyncLock fullScreenLock = new AsyncLock();
        bool fullScreenBusy = false;
        public NowPlayingHomeViewModel NowPlayingHomeViewModelInstance { get; private set; }


        long fullscreenCallbackRegistrationToken;

        bool _isCompactMode = false;
        public bool IsCompactMode
        {
            get => _isCompactMode;
            set
            {
                _isCompactMode = value;
                IsDoubleTapEnabled = !_isCompactMode;
                //MediaPlayerElementInstance.SetCompactMode(_isCompactMode);
                SetSizeModeBackground(_isCompactMode);
            }
        }

        public NowPlayingHome() : base()
        {
            InitializeComponent();
            disposableCursor.Dispose();

            UIMediaTransportControls = CreateTransportControls(ServiceProvider.GetService<PlaybackSequenceService>());
            MediaPlayerElementInstance.TransportControls = UIMediaTransportControls;

            NowPlayingHomeViewModelInstance = ServiceProvider.GetService<NowPlayingHomeViewModel>();

            NavigationCacheMode = NavigationCacheMode.Required;
            PointerMoved += NowPlayingHome_PointerMoved;
            NowPlayingHomeViewModelInstance.PlaybackServiceInstance.NowPlayingBackStore.CollectionChanged += NowPlayingBackStore_CollectionChanged;
            SizeChanged += This_SizeChanged;
        }

        private void NowPlayingHome_PointerMoved(object? sender, PointerRoutedEventArgs e)
        {
            try
            {
                _fullscreenTimerLock.WaitOne();
                if (ProtectedCursor == disposableCursor && UIMediaTransportControls.PlayerElementIsFullWindow)
                {
                    ShowCursorEnableHideTimer();
                }
                else if (ProtectedCursor == disposableCursor)
                {
                    ShowCursorNoTimer();
                }

                FadeInTransportControls();
            }
            finally
            {
                _fullscreenTimerLock.Set();
            }
        }


        private void NowPlayingBackStore_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (NowPlayingHomeViewModelInstance.PlaybackServiceInstance.NowPlayingBackStore.Count == 0)
                FadeOutTransportControls();
        }

        private void SetSizeModeBackground(bool isCompact)
        {
            Background = new SolidColorBrush(isCompact ? Colors.Transparent : Colors.Transparent);
        }

        private void This_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            CheckCompactState(e.NewSize.Height);
        }

        private void CheckCompactState(double height)
        {
            IsCompactMode = height < MainWindow.CompactNowPlayingHight + 1;
        }

        private void ShowCursorNoTimer()
        {
            _fullscreenCursorTimer?.Cancel();

            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
            System.Diagnostics.Debug.WriteLine("****SHOW CURSOR****");
        }

        int GetNowPlayingCount()
        {
            if (NowPlayingHomeViewModelInstance != null)
                return NowPlayingHomeViewModelInstance.PlaybackServiceInstance.NowPlayingBackStore.Count;
            return 0;
        }

        public void FadeInTransportControls()
        {
            if (UIMediaTransportControls != null && GetNowPlayingCount() > 0)
                VisualStateManager.GoToState(UIMediaTransportControls, "ControlPanelFadeIn2", false);
        }

        public void FadeOutTransportControls()
        {
            if (UIMediaTransportControls != null)
                if (IsCompactMode && GetNowPlayingCount() > 0 || UIMediaTransportControls.IsPointerOverControlAreas) FadeInTransportControls();
                else if (!UIMediaTransportControls.IsPointerOverControlAreas)
                    VisualStateManager.GoToState(UIMediaTransportControls, "ControlPanelFadeOut2", false);
        }

        bool mediaPlayerInit = false;
        private Task InitMediaPlayerElement()
        {
            if (!mediaPlayerInit)
            {
                MediaPlayerElementInstance.SetMediaPlayer(AppState.Current.MediaServiceConnector.CurrentPlayer);
                //MediaPlayerElementInstance.EffectsConfiguration = (await AppState.Current.BackgroundMediaServiceCurrent.PlayerInstance).VideoEffectsConfiguration;

                fullscreenCallbackRegistrationToken = MediaPlayerElementInstance.RegisterPropertyChangedCallback(MediaPlayerElement.IsFullWindowProperty, FullscreenChangedCallback);
                MediaPlayerElementInstance.AreTransportControlsEnabled = true;
                MediaPlayerElementInstance.KeyDown += MediaPlayerElementInstance_KeyDown;
                MediaPlayerElementInstance.TransportControls.IsNextTrackButtonVisible = true;
                MediaPlayerElementInstance.TransportControls.IsPreviousTrackButtonVisible = true;
                MediaPlayerElementInstance.TransportControls.IsVolumeButtonVisible = false;
                MediaPlayerElementInstance.TransportControls.IsSeekBarVisible = false;
                MediaPlayerElementInstance.TransportControls.IsZoomButtonVisible = false;
                MediaPlayerElementInstance.TransportControls.IsPlaybackRateEnabled = true;
                MediaPlayerElementInstance.TransportControls.IsPlaybackRateButtonVisible = true;

                UIMediaTransportControls = (MediaPlayerElementInstance.TransportControls as CustomMediaTransportControls);
                UIMediaTransportControls?.SetMediaPlayer(AppState.Current.MediaServiceConnector.CurrentPlayer);
                UIMediaTransportControls.PlayerElementIsFullWindowChanged += _transportControls_PlayerElementIsFullWindowChanged;
                AppState.Current.MediaServiceConnector.MediaPlayerElementFullScreenModeChanged += BackgroundAudioService_MediaPlayerElementFullScreenModeChanged;
                UIMediaTransportControls.PlayerElement = MediaPlayerElementInstance;
                mediaPlayerInit = true;
                FadeInTransportControls();
                CheckCompactState(ActualHeight);
            }

            return Task.CompletedTask;
        }

        private void _transportControls_PlayerElementIsFullWindowChanged(object? sender, bool e)
        {
            FullscreenChangedCallback(MediaPlayerElementInstance, MediaPlayerElement.IsFullWindowProperty);
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
            MediaPlayerElementInstance.UnregisterPropertyChangedCallback(MediaPlayerElement.IsFullWindowProperty, fullscreenCallbackRegistrationToken);
            MediaPlayerElementInstance.KeyDown -= MediaPlayerElementInstance_KeyDown;
            AppState.Current.MediaServiceConnector.MediaPlayerElementFullScreenModeChanged -= BackgroundAudioService_MediaPlayerElementFullScreenModeChanged;

            MediaPlayerElementInstance.SetMediaPlayer(null);
            UIMediaTransportControls?.Dispose();
            mediaPlayerInit = false;
        }

        private CustomMediaTransportControls CreateTransportControls(PlaybackSequenceService service)
        {
            CustomMediaTransportControls transportControls = new CustomMediaTransportControls(service)
            {
                IsPlaybackRateButtonVisible = true,
                IsPlaybackRateEnabled = true,
                IsVolumeButtonVisible = true,
                IsVolumeEnabled = true,
                IsCompact = false
            };
            transportControls.TemplateApplied += TransportControls_TemplateApplied;
            transportControls.ExternalNavigationRequest += NowPlayingCOntroller_ExternalNavigationRequest;
            return transportControls;
        }

        private async void TransportControls_TemplateApplied(object? sender, EventArgs e)
        {
            (sender as CustomMediaTransportControls).SubtitlesAgent = (IOpenSubtitlesAgent)ServiceProvider.GetService(typeof(IOpenSubtitlesAgent));
        }

        private void MediaPlayerElementInstance_KeyDown(object? sender, KeyRoutedEventArgs e)
        {
            if (!APIContractUtilities.UniversalContract5 && e.Key == Windows.System.VirtualKey.Escape)
            {
                if (AppState.Current.MediaServiceConnector.IsRenderingFullScreen)
                {
                    AppState.Current.MediaServiceConnector.IsRenderingFullScreen = false;
                }
            }
        }

        private void FullscreenChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            try
            {
                _fullscreenTimerLock.WaitOne();
                if (UIMediaTransportControls.PlayerElementIsFullWindow)
                {
                    MediaPlayerElementInstance.TransportControls.IsNextTrackButtonVisible = true;
                    MediaPlayerElementInstance.TransportControls.IsPreviousTrackButtonVisible = true;
                    MediaPlayerElementInstance.TransportControls.IsVolumeButtonVisible = true;

                    ScreenSessionManager.Current.RequestKeepScreenOn();
                    _fullscreenCursorTimer = ThreadPoolTimer.CreatePeriodicTimer(HideMouseCursor, TimeSpan.FromSeconds(6));
                }
                else
                {
                    MediaPlayerElementInstance.TransportControls.IsNextTrackButtonVisible = true;
                    MediaPlayerElementInstance.TransportControls.IsPreviousTrackButtonVisible = true;
                    MediaPlayerElementInstance.TransportControls.IsVolumeButtonVisible = false;


                    ScreenSessionManager.Current.RequestScreenOff();
                    _fullscreenCursorTimer?.Cancel();

                    ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
                }
                AppState.Current.MediaServiceConnector.NotifyViewMode(true, MediaPlayerElementInstance);

            }
            finally
            {
                _fullscreenTimerLock.Set();
            }

            AppState.Current.MediaServiceConnector.IsRenderingFullScreen = UIMediaTransportControls.PlayerElementIsFullWindow;
        }

        private async void HideMouseCursor(ThreadPoolTimer timer)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                try
                {
                    _fullscreenTimerLock.WaitOne();
                    if (UIMediaTransportControls.PlayerElementIsFullWindow && AppState.Current.MediaServiceConnector.IsPlaying() && !UIMediaTransportControls.IsPointerOverControlAreas)
                    {
                        ProtectedCursor = disposableCursor;
                        System.Diagnostics.Debug.WriteLine("****HIDE CURSOR****");
                    }
                    FadeOutTransportControls();
                }
                finally
                {
                    _fullscreenTimerLock.Set();
                }
            });
        }


        protected override void FreeMemory()
        {
            unInitMediaPlayerElement();

            MediaPlayerElementInstance.UnregisterPropertyChangedCallback(MediaPlayerElement.IsFullWindowProperty, fullscreenCallbackRegistrationToken);
            UIMediaTransportControls.ExternalNavigationRequest -= NowPlayingCOntroller_ExternalNavigationRequest;


            Window.Current.CoreWindow.SizeChanged -= NowPlayingHome_SizeChanged;
            //MediaPlayerElementInstance.TransportControls = null;
            MediaPlayerElementInstance.SetMediaPlayer(null);
            UIMediaTransportControls.SetMediaPlayer(null);

            NowPlayingHomeViewModelInstance.Dispose();
            UIMediaTransportControls.Dispose();

            NowPlayingHomeViewModelInstance.PlaybackServiceInstance.NowPlayingBackStore.CollectionChanged -= NowPlayingBackStore_CollectionChanged;

            AppState.Current.MediaServiceConnector.MediaPlayerElementFullScreenModeChanged -= BackgroundAudioService_MediaPlayerElementFullScreenModeChanged;
            base.FreeMemory();
        }

        private void NowPlayingHome_SizeChanged(CoreWindow sender, Windows.UI.Core.WindowSizeChangedEventArgs args)
        {
        }

        protected override async Task OnInitializeStateAsync(object? parameter)
        {
            await InitMediaPlayerElement();

            DataContext = NowPlayingHomeViewModelInstance;

            CheckCompactState(ActualHeight);
            SetMediaPlayerElementPosterSource(ServiceProvider.GetService<IBackgroundPlayer>().CurrentPlayer);
            if (GetNowPlayingCount() == 0) FadeOutTransportControls();
        }

        private void PreventDoubleTap(object? sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void NowPlayingCOntroller_ExternalNavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            if (UIMediaTransportControls.PlayerElementIsFullWindow)
            {
                //MediaPlayerElementInstance.IsFullWindow = false;
            }
            NotifyExternalNavigationRequest(sender, e);
        }

        public override async Task OnMediaStateChanged()
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                _fullscreenTimerLock.WaitOne();

                try
                {
                    if (AppState.Current.MediaServiceConnector.IsPlaying())
                    {
                        ShowCursorEnableHideTimer();
                        FadeOutTransportControls();
                    }
                    else
                    {
                        ShowCursorNoTimer();
                        FadeInTransportControls();
                    }
                }
                catch { }
                finally
                {
                    _fullscreenTimerLock.Set();
                }
            });
        }

        private void ShowCursorEnableHideTimer()
        {
            _fullscreenCursorTimer?.Cancel();
            //_transportControls.SetUIVisibility(Visibility.Visible);
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
            _fullscreenCursorTimer = ThreadPoolTimer.CreatePeriodicTimer(HideMouseCursor, TimeSpan.FromSeconds(6));
            System.Diagnostics.Debug.WriteLine("****STARTING HIDE CURSOR****");

        }

        private async void BackgroundAudioService_MediaPlayerElementFullScreenModeChanged(object? sender, bool e)
        {
            if (fullScreenBusy)
            {
                return;
            }

            using (await fullScreenLock.LockAsync())
            {
                fullScreenBusy = true;
                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    if (UIMediaTransportControls.PlayerElementIsFullWindow)
                    {
                        if (MainWindowingService.Instance.IsInCompactOverlayMode())
                        {
                            await MainWindowingService.Instance.RequestCompactOverlayMode(false);
                        }
                    }

                    if (e != UIMediaTransportControls.PlayerElementIsFullWindow)
                    {
                        //MediaPlayerElementInstance.IsFullWindow = e;
                    }
                    fullScreenBusy = false;

                });
            }
        }

        public async override Task OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                if (args.Reason == MediaOpenedEventReason.MediaPlaybackListItemChanged)
                {
                    //if (args.EventData != null)
                    //MediaPlayerElementInstance.SignalMediaOpened(args.EventData.PlaybackItem.VideoTracks.Any());
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
            SetMediaPlayerElementPosterSource(sender);
        }

        private async void SetMediaPlayerElementPosterSource(MediaPlayer sender)
        {
            var playerInstance = AppState.Current.MediaServiceConnector.PlayerInstance;
            var currentPlayer = AppState.Current.MediaServiceConnector.CurrentPlayer;
            var currentPlaybackItem = playerInstance.CurrentPlaybackItem;

            // active playback item, is playing video -> don't show the poster source
            if (currentPlaybackItem != null && currentPlaybackItem.IsVideo())
            {
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

        public override async Task<bool> GoBack()
        {
            return await UIMediaTransportControls.GoBack();
        }
    }
}
