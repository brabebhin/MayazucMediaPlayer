using CommunityToolkit.WinUI;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class CustomMediaTransportControls2 : BaseUserControl, IDisposable
    {
        DispatcherQueueTimer StateUpdateTimer;
        SymbolIcon PlayIcon = new SymbolIcon(Symbol.Play);
        SymbolIcon PauseIcon = new SymbolIcon(Symbol.Pause);
        private AsyncLock mediaOpenedLock = new AsyncLock();

        public bool UserInteracting()
        {
            return MediaTimelineControls.IsUserManipulating();
        }

        public CustomMediaTransportControls2()
        {
            InitializeComponent();

            FullScreenButton.Icon = FullScreenIcon();

            StateUpdateTimer = DispatcherQueue.CreateTimer();
            StateUpdateTimer.Interval = TimeSpan.FromSeconds(0.5);
            StateUpdateTimer.Tick += StateUpdateTimer_Tick;

            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened += Current_MediaOpened;
            AppState.Current.MediaServiceConnector.PlayerInstance.OnStateChanged += CurrentPlaybackSession_PlaybackStateChanged;
            StateUpdateTimer.Start();

            MainWindowingService.Instance.MediaPlayerElementFullScreenModeChanged += Instance_MediaPlayerElementFullScreenModeChanged;

            AppState.Current.KeyboardInputManager.AcceleratorInvoked += KeyboardInputManager_AcceleratorInvoked;
            DataContext = this;

            this.SizeChanged += CustomMediaTransportControls2_SizeChanged;

            VolumeControlBarInstance.SetMediaPlayer(AppState.Current.MediaServiceConnector.CurrentPlayer);

            Program.OnApplicationClosing += Program_OnApplicationClosing;
        }

        private void Program_OnApplicationClosing(object? sender, EventArgs e)
        {
            StateUpdateTimer.Stop();
            Program.OnApplicationClosing -= Program_OnApplicationClosing;
        }

        private void CustomMediaTransportControls2_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FullCommandBarButtonsVisibility = e.NewSize.Width > 400 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility FullCommandBarButtonsVisibility
        {
            get => (Visibility)GetValue(FullCommandBarButtonsVisibilityProperty);
            set => SetValue(FullCommandBarButtonsVisibilityProperty, value);
        }

        public static DependencyProperty FullCommandBarButtonsVisibilityProperty = DependencyProperty.Register(nameof(FullCommandBarButtonsVisibility), typeof(Visibility), typeof(CustomMediaTransportControls2), new PropertyMetadata(Visibility.Visible));

        private async void KeyboardInputManager_AcceleratorInvoked(object? sender, HotKeyId e)
        {
            switch (e)
            {
                case HotKeyId.PlayPause:
                    await AppState.Current.MediaServiceConnector.PlayPauseAutoSwitch();

                    break;
                case HotKeyId.SkipNext:
                    await AppState.Current.MediaServiceConnector.SkipNext();

                    break;
                case HotKeyId.SkipPrevious:
                    await AppState.Current.MediaServiceConnector.SkipPrevious();

                    break;
                case HotKeyId.JumpBack:
                    await AppState.Current.MediaServiceConnector.SkipSecondsBack(Constants.JumpBackSeconds);

                    break;
                case HotKeyId.JumpForward:
                    await AppState.Current.MediaServiceConnector.SkipSecondsForth(Constants.JumpAheadSeconds);

                    break;

                case HotKeyId.ExitFullScreen:
                    await FullScreenAutoSwitch();

                    break;
            }
        }

        private void Instance_MediaPlayerElementFullScreenModeChanged(object? sender, bool e)
        {
            SetFullScreenButtonIcon(e);
        }

        private async void CurrentPlaybackSession_PlaybackStateChanged(MediaPlayer sender, MediaPlaybackState args)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                await UpdateState();
            });
        }

        private async void Current_MediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            using (await mediaOpenedLock.LockAsync())
            {
                if (args.Reason == MediaOpenedEventReason.MediaPlaybackListItemChanged)
                {
                    var mds = args.Data.ExtraData.MediaPlayerItemSource;

                    await DispatcherQueue.EnqueueAsync(async () =>
                    {
                        await mtcSubtitlesControl.LoadMediaPlaybackItem(args.Data.PlaybackItem);
                        await mtcVideoTracks.LoadVideoTracksAsync(args.Data.PlaybackItem);
                        await mtcAudioTracks.LoadAudioTracksAsync(args.Data.PlaybackItem);
                        ChaptersControlInstance.LoadMediaPlaybackItem(args.Data.PlaybackItem);
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

            MediaTimelineControls.UpdateState(playbackSession);
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
            await AppState.Current.MediaServiceConnector.PlayPauseAutoSwitch();
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
            await FullScreenAutoSwitch();
        }

        public async Task FullScreenAutoSwitch()
        {
            bool shouldFullScreen = !MainWindowingService.Instance.IsInFullScreenMode();
            await MainWindowingService.Instance.RequestFullScreenMode(shouldFullScreen);
            SetFullScreenButtonIcon(shouldFullScreen);
        }

        private void SetFullScreenButtonIcon(bool shouldFullScreen)
        {
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

        public static DependencyProperty LeftContentProperty = DependencyProperty.Register(nameof(LeftContent), typeof(object), typeof(CustomMediaTransportControls2), new PropertyMetadata(null, LeftContentChanged));

        private static void LeftContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CustomMediaTransportControls2).LeftContentPresenter.Content = e.NewValue;
        }

        public static DependencyProperty RightContentProperty = DependencyProperty.Register(nameof(RightContent), typeof(object), typeof(CustomMediaTransportControls2), new PropertyMetadata(null, RightContentChanged));
        private bool disposedValue;

        private static void RightContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CustomMediaTransportControls2).RightContentPresenter.Content = e.NewValue;
        }

        public object LeftContent
        {
            get => GetValue(LeftContentProperty);
            set => SetValue(LeftContentProperty, value);
        }

        public object RightContent
        {
            get => GetValue(RightContentProperty);
            set => SetValue(RightContentProperty, value);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened -= Current_MediaOpened;
                AppState.Current.MediaServiceConnector.PlayerInstance.OnStateChanged -= CurrentPlaybackSession_PlaybackStateChanged;

                StateUpdateTimer.Stop();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        ~CustomMediaTransportControls2()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

