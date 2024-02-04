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
using Windows.UI.ViewManagement;

namespace MayazucMediaPlayer.Controls
{
    public sealed class CustomMediaTransportControls : MediaTransportControls, INotifyPropertyChanged, IDisposable
    {
        public event EventHandler<NavigationRequestEventArgs> ExternalNavigationRequest;

        InAppNotification notificationRoot;
        readonly AsyncLock frameSaveLock = new AsyncLock();
        bool firstMediaOpenedEventFired = false;

        Button FullWindowButton;
        readonly AsyncLock durationLock = new AsyncLock();

        MTCSubtitlesSelectionControl mtcSubtitlesControl;
        VideoTrackSelectionDialog mtcVideoTracks;

        MediaPlaybackItem m_CurrentPlaybackItem = null;

        ButtonBase SaveVideoFramesButton;
        ToggleButton MiniPlayerToggle;

        Border PlaybackAreaOverlay;
        Button SecondaryPlayToReceiverDisconnectButton;

        readonly DispatcherQueueTimer timer;
        bool progressSliderManipulating = false;
        private bool disposedValue;
        MediaPlayer _player = null;
        Slider progressSlider = null;
        TextBlock progressText = null;
        TextBlock totalDuration = null;
        TextBlock durationLeftText = null;
        Grid ControlPanel_ControlPanelVisibilityStates_Border2, ControlPanel_ControlPanelVisibilityStates_Border;
        AppBarButton LeaveCompactModeButton;
        readonly AsyncLock mediaOpenedLock = new AsyncLock();

        readonly List<FrameworkElement> doubleTappedDisabledChildren = new List<FrameworkElement>();

        public event PropertyChangedEventHandler PropertyChanged;
        public MediaPlayerElement PlayerElement
        {
            get;
            set;
        }

        public string TimestampEstimation
        {
            get;
            set;
        } = "43";


        public event EventHandler TemplateApplied;

        public IOpenSubtitlesAgent SubtitlesAgent
        {
            get => mtcSubtitlesControl.SubtitlesAgent;
            internal set => mtcSubtitlesControl.SubtitlesAgent = value;
        }

        public PlaybackSequenceService PlaybackServiceModel
        {
            get;
            private set;
        }

        public string CurrentTrackTitle
        {
            get => (string)GetValue(CurrentTrackTitleProperty);
            set => SetValue(CurrentTrackTitleProperty, value);
        }

        public static DependencyProperty CurrentTrackTitleProperty = DependencyProperty.Register(nameof(CurrentTrackTitle), typeof(string), typeof(CustomMediaTransportControls), new PropertyMetadata("Nothing is currently playing"));

        public CommandBase SaveVideoFrameCommand
        {
            get;
            private set;
        }

        public bool IsPointerOverControlAreas
        {
            get;
            private set;
        }

        public bool PlayerElementIsFullWindow { get; private set; }
        public event EventHandler<bool> PlayerElementIsFullWindowChanged;

        public CustomMediaTransportControls(PlaybackSequenceService playbackService)
        {
            DefaultStyleKey = typeof(CustomMediaTransportControls);
            PlaybackServiceModel = playbackService;

            timer = DispatcherQueue.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.IsRepeating = true;
            timer.Tick += Timer_Tick;

            DataContext = this;

            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened += Current_MediaOpened;
            AppState.Current.MediaServiceConnector.SubtitleDelayChanged += BackgroundMediaService_SubtitleDelayChanged;
            SaveVideoFrameCommand = new AsyncRelayCommand(async (o) => { await SaveVideoFrameButtonClickInternal(); }, null);

            DoubleTapped += CustomMediaTransportControls_DoubleTapped;
            AppState.Current.KeyboardInputManager.AcceleratorInvoked += KeyboardInputManager_AcceleratorInvoked;
        }


        private async void KeyboardInputManager_AcceleratorInvoked(object? sender, HotKeyId e)
        {
            switch (e)
            {
                case HotKeyId.PlayPause:
                    await AppState.Current.MediaServiceConnector.SendPlayPause();

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
            }
        }

        private void CustomMediaTransportControls_DoubleTapped(object? sender, DoubleTappedRoutedEventArgs e)
        {
            FullWindowButton_Click(FullWindowButton, e);
        }

        private void CoreApplicationTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            ControlPanel_ControlPanelVisibilityStates_Border2.Margin = new Thickness(sender.SystemOverlayLeftInset, sender.Height, sender.SystemOverlayRightInset, 0);
        }

        public async Task LeaveCompactOverlayMode(CoreApplicationViewTitleBar coreTitleBar)
        {
            if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default))
            {
                coreTitleBar.LayoutMetricsChanged -= CoreApplicationTitleBar_LayoutMetricsChanged;
                coreTitleBar.ExtendViewIntoTitleBar = false;
                LeaveCompactModeButton.Visibility = Visibility.Collapsed;
                MiniPlayerToggle.IsChecked = false;
                ControlPanel_ControlPanelVisibilityStates_Border2.Margin = new Thickness(0);
            }
        }

        private void BackgroundMediaService_SubtitleDelayChanged(object? sender, TimeSpan e)
        {
            ShowNotification($"Subtitles {e.TotalSeconds} s");
        }

        protected override async void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PlaybackAreaOverlay = GetTemplateChild("PlaybackAreaOverlay") as Border;
            PlaybackAreaOverlay.Tapped += PlaybackAreaOverlay_Tapped;

            FullWindowButton = GetTemplateChildEx("FullWindowButton2") as Button;
            if (APIContractUtilities.UniversalContract5)
            {
                FullWindowButton.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = Windows.System.VirtualKey.Escape, IsEnabled = true });
            }
            FullWindowButton.Click += FullWindowButton_Click;
            var NextTrackButton = GetTemplateChildEx("NextTrackButton2") as Button;
            NextTrackButton.Tapped += NextTrackButton_Tapped;
            var PlayPauseButton = GetTemplateChildEx("PlayPauseButton2") as Button;
            PlayPauseButton.Tapped += PlayPauseButton_Tapped;

            var PreviousTrackButton = GetTemplateChildEx("PreviousTrackButton2") as Button;
            PreviousTrackButton.Tapped += PreviousTrackButton_Tapped;
            progressSlider = GetTemplateChildEx("ProgressSlider2") as Slider;
            progressSlider.AddHandler(Slider.PointerReleasedEvent, new PointerEventHandler(ProgressBarSeek), true);
            progressSlider.AddHandler(Slider.PointerPressedEvent, new PointerEventHandler(ProgressBarManipulation), true);

            progressSlider.Visibility = Visibility.Visible;
            progressSlider.IsEnabled = false;

            progressText = GetTemplateChildEx("Custom_TimeElapsedElement") as TextBlock;
            totalDuration = GetTemplateChildEx("Custom_TimeDurationElement") as TextBlock;
            durationLeftText = GetTemplateChildEx("Custom_TimeRemainingElement") as TextBlock;

            var SkipBackward2Button = GetTemplateChildEx("SkipBackward2Button") as Button;
            SkipBackward2Button.Click += SkipBackward2Button_Click;

            var SkipForward2Button = GetTemplateChildEx("SkipForward2Button") as Button;
            SkipForward2Button.Click += SkipForward2Button_Click;

            GetTemplateChildEx("RepeatOptions");
            GetTemplateChildEx("ShuffleOptions");

            GetTemplateChildEx("cbAudioTrack");
            GetTemplateChildEx("cbClosedCapitons");

            SaveVideoFramesButton = GetTemplateChildEx("SaveVideoFramesButton") as ButtonBase;
            SaveVideoFramesButton.Click += SaveVideoFramesButton_Click;

            MiniPlayerToggle = GetTemplateChildEx("MiniPlayerToggle") as ToggleButton;
            MiniPlayerToggle.Visibility = Visibility.Visible;

            MiniPlayerToggle.Checked += MiniPlayerToggle_Toggled;
            MiniPlayerToggle.Unchecked += MiniPlayerToggle_Toggled;


            notificationRoot = GetTemplateChildEx("notificationRoot") as InAppNotification;

            mtcSubtitlesControl = GetTemplateChildEx("mtcSubtitlesControl") as MTCSubtitlesSelectionControl;
            mtcVideoTracks = GetTemplateChildEx("mtcVideoTracks") as VideoTrackSelectionDialog;

            GetTemplateChildEx("VideoTrackSelectionButton");

            progressSlider.PointerMoved += ProgressSlider_PointerMoved;
            progressSlider.PointerExited += ProgressSlider_PointerExited;
            progressSlider.ValueChanged += ProgressSlider_ValueChanged;

            SecondaryPlayToReceiverDisconnectButton = GetTemplateChildEx("SecondaryPlayToReceieverDisconnectButton") as Button;
            SecondaryPlayToReceiverDisconnectButton.Click += SecondaryPlayToReceieverDisconnectButton_Click;

            GetTemplateChild("TimelineGrid");

            progressSlider.Tag = false;

            LeaveCompactModeButton = GetTemplateChildEx("LeaveCompactModeButton") as AppBarButton;
            LeaveCompactModeButton.Click += LeaveCompactModeButton_Click;

            TemplateApplied?.Invoke(this, new EventArgs());
            ControlPanel_ControlPanelVisibilityStates_Border2 = GetTemplateChildEx("ControlPanel_ControlPanelVisibilityStates_Border2") as Grid;
            ControlPanel_ControlPanelVisibilityStates_Border = GetTemplateChildEx("ControlPanel_ControlPanelVisibilityStates_Border") as Grid;

            ControlPanel_ControlPanelVisibilityStates_Border2.PointerEntered += Pointer_Entered_Over_Controls_Region;
            ControlPanel_ControlPanelVisibilityStates_Border.PointerEntered += Pointer_Entered_Over_Controls_Region;
            ControlPanel_ControlPanelVisibilityStates_Border.PointerExited += Pointer_Exited_Over_Controls_Region;
            ControlPanel_ControlPanelVisibilityStates_Border2.PointerExited += Pointer_Exited_Over_Controls_Region;


            timer.Start();
        }

        private void Pointer_Exited_Over_Controls_Region(object sender, PointerRoutedEventArgs e)
        {
            IsPointerOverControlAreas = false;
        }

        private void Pointer_Entered_Over_Controls_Region(object sender, PointerRoutedEventArgs e)
        {
            IsPointerOverControlAreas = true;
        }

        private async void FullWindowButton_Click(object? sender, RoutedEventArgs e)
        {
            PlayerElementIsFullWindow = !PlayerElementIsFullWindow;
            await MainWindowingService.Instance.RequestFullScreenMode(PlayerElementIsFullWindow);
            PlayerElementIsFullWindowChanged?.Invoke(this, PlayerElementIsFullWindow);
        }

        private void LeaveCompactModeButton_Click(object? sender, RoutedEventArgs e)
        {
            AppState.Current.MediaServiceConnector.NotifyViewMode(true, PlayerElement);
        }

        private async void NextTrackButton_Tapped(object? sender, TappedRoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SkipNext();
        }

        private async void PreviousTrackButton_Tapped(object? sender, TappedRoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SkipPrevious();
        }

        private void ProgressSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (progressSliderManipulating)
            {
                GetProgressPreviewTooltip().IsOpen = false;
                TimestampEstimation = TimeSpan.FromSeconds(progressSlider.Value).ToString("hh':'mm':'ss");
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimestampEstimation)));
            }
        }

        private void ProgressSlider_PointerExited(object? sender, PointerRoutedEventArgs e)
        {
            GetProgressPreviewTooltip().IsOpen = false;
        }

        private async void ProgressSlider_PointerMoved(object? sender, PointerRoutedEventArgs e)
        {
            var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
            var point = e.GetCurrentPoint(progressSlider);
            var width = progressSlider.ActualWidth;
            /*withw ------ durationp
             pointW-----tooltipSeconds
             */
            var tooltipSeconds = point.Position.X * session.NaturalDuration.TotalSeconds / width;
            TimestampEstimation = TimeSpan.FromSeconds(tooltipSeconds).ToString("hh':'mm':'ss");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimestampEstimation)));
            if (!progressSliderManipulating)
            {
                ToolTip tooltip = GetProgressPreviewTooltip();
                tooltip.IsOpen = true;
            }
        }

        private ToolTip GetProgressPreviewTooltip()
        {
            return ToolTipService.GetToolTip(progressSlider) as ToolTip;
        }

        private void ProgressBarManipulation(object? sender, PointerRoutedEventArgs e)
        {
            GetProgressPreviewTooltip().IsOpen = false;
            progressSliderManipulating = true;
        }

        async void ProgressBarSeek(object? sender, RoutedEventArgs args)
        {
            var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
            if (session != null)
            {
                var time = MediaPlaybackSeekbarUtil.GetDenormalizedValue(progressSlider.Value, session.NaturalDuration);
                await (AppState.Current.MediaServiceConnector.PlayerInstance).Seek(time, true);
                GetProgressPreviewTooltip().IsOpen = false;
                progressSliderManipulating = false;
            }
        }

        private async void SecondaryPlayToReceieverDisconnectButton_Click(object? sender, RoutedEventArgs e)
        {
            await (AppState.Current.MediaServiceConnector.PlayerInstance).ForceDisconnectDlnaAsync();
            if (PlaybackServiceModel.NowPlayingBackStore.Count == 0)
                SetSecondaryPlayToReceieverDisconnectButtonVisibility(null);
        }

        private DependencyObject GetTemplateChildEx(string name)
        {
            var obj = GetTemplateChild(name);
            if (obj != null && obj is FrameworkElement)
            {
                var fe = obj as FrameworkElement;
                fe.DoubleTapped += PreventDoubleTapped;
                fe.Tapped += PreventSingleTapped;
                doubleTappedDisabledChildren.Add(fe);
            }

            return obj;
        }

        private void PreventSingleTapped(object? sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void UnsubscribeEventHandlers()
        {
            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened -= Current_MediaOpened;
            AppState.Current.MediaServiceConnector.SubtitleDelayChanged -= BackgroundMediaService_SubtitleDelayChanged;
            timer.Stop();
            timer.Tick -= Timer_Tick;
            _player = null;
        }

        private void PreventDoubleTapped(object? sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private async Task CheckState(MediaPlayer sender)
        {
            if (sender != null && sender.PlaybackSession != null)
            {
                switch (sender.PlaybackSession.PlaybackState)
                {
                    case MediaPlaybackState.Playing:

                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            VisualStateManager.GoToState(this, "PauseState", true);
                        });
                        break;
                    default:

                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            VisualStateManager.GoToState(this, "PlayState", true);
                        });

                        break;
                }
            }

            var mds = (AppState.Current.MediaServiceConnector.PlayerInstance).CurrentPlaybackData;


            await DispatcherQueue.EnqueueAsync(() =>
            {
                SetPlaybackTitle(mds);
            });
        }

        private async void Current_MediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            firstMediaOpenedEventFired = true;
            using (await mediaOpenedLock.LockAsync())
            {
                if (args.Reason == MediaOpenedEventReason.MediaPlaybackListItemChanged)
                {
                    m_CurrentPlaybackItem = args.EventData.PlaybackItem;
                    var mds = args.EventData.ExtraData.MediaPlayerItemSource;

                    await DispatcherQueue.EnqueueAsync(async () =>
                    {
                        progressSlider.IsEnabled = true;

                        mtcSubtitlesControl.LoadMediaPlaybackItem(args.EventData.PlaybackItem);
                        await mtcVideoTracks.LoadVideoTracksAsync(args.EventData.PlaybackItem);

                        if (m_CurrentPlaybackItem.IsVideo())
                            VisualStateManager.GoToState(this, "CastDisabledForVideo", false);
                        else VisualStateManager.GoToState(this, "CastEnabledForAudio", false);

                        try
                        {
                            SetSecondaryPlayToReceieverDisconnectButtonVisibility(Result.Ok(new MediaPlayerItemSourceUIWrapper(args.EventData.ExtraData.MediaPlayerItemSource, DispatcherQueue)));
                        }
                        catch { }
                    });

                }
            }
        }

        private void SetSecondaryPlayToReceieverDisconnectButtonVisibility(Result<MediaPlayerItemSourceUIWrapper> CurrentDataStorage)
        {
            if (CurrentDataStorage.IsSuccess && CurrentDataStorage.Value.MediaData.HasExternalSource)
            {
                SecondaryPlayToReceiverDisconnectButton.Visibility = Visibility.Visible;
            }
            else
            {
                SecondaryPlayToReceiverDisconnectButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void Timer_Tick(object? sender, object e)
        {
            using (await durationLock.LockAsync())
            {
                SetTimestamps(!progressSliderManipulating);
                await CheckState(AppState.Current.MediaServiceConnector.CurrentPlayer);
            }
        }

        private void SetTimestamps(bool setSliderValue)
        {
            try
            {
                var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
                if (session != null)
                {
                    if (progressText != null)
                        progressText.Text = session.Position.ToString("hh':'mm':'ss");
                    if (totalDuration != null)
                        totalDuration.Text = session.NaturalDuration.ToString("hh':'mm':'ss");
                    if (durationLeftText != null)
                        durationLeftText.Text = (session.NaturalDuration - session.Position).ToString("hh':'mm':'ss");
                    if (progressSlider != null)
                    {
                        if (setSliderValue)
                            progressSlider.Value = MediaPlaybackSeekbarUtil.GetNormalizedValue(session.Position, session.NaturalDuration);
                    }
                }
            }
            catch { }
        }

        private void SetPlaybackTitle(IMediaPlayerItemSource mds)
        {
            if (mds != null)
            {
                CurrentTrackTitle = mds.Title;
            }
            else
            {
                CurrentTrackTitle = string.Empty;
            }
        }

        private async void SaveVideoFramesButton_Click(object? sender, RoutedEventArgs e)
        {
            await SaveVideoFrameButtonClickInternal();
        }

        private async Task SaveVideoFrameButtonClickInternal()
        {
            if (_player.PlaybackSession != null && _player.PlaybackSession.PlaybackState != MediaPlaybackState.None)
            {
                var oldState = (AppState.Current.MediaServiceConnector.CurrentPlaybackSession).PlaybackState;

                (AppState.Current.MediaServiceConnector.CurrentPlayer).Pause();
                var position = _player.PlaybackSession.Position;

                await SaveVideoFrameInternalCommand(position);

                if (oldState == MediaPlaybackState.Playing)
                {
                    (AppState.Current.MediaServiceConnector.CurrentPlayer).Play();
                }
            }
        }

        private async Task SaveVideoFrameInternalCommand(TimeSpan position)
        {
            using (await frameSaveLock.LockAsync())
            {
                var currentDataStatus = await PlaybackServiceModel.CurrentMediaMetadata();
                if (currentDataStatus.IsFailed) return;
                var CurrentData = currentDataStatus.Value.MediaData;
                throw new NotImplementedException();
                //if (m_CurrentPlaybackItem.IsVideo())
                //{
                //    if (AppState.Current.MediaServiceConnector.HasActivePlaybackSession())
                //    {
                //        var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
                //        if (session.PlaybackState == MediaPlaybackState.Paused || session.PlaybackState == MediaPlaybackState.Playing)
                //        {
                //            var sourceFile = await CurrentData.GetFileAsync();
                //            if (sourceFile != null)
                //            {
                //                var name = $"{sourceFile.Name}-{session.Position.TotalSeconds}.png";

                //                await VideoThumbnailPreviewData.SaveVideoFrameAsync(position, await LocalCache.LocalFolders.GetSavedVideoFramesFolder(), sourceFile, name);

                //            }

                //            ShowNotification($"Frame saved: {DateTime.Now.ToString("hh:mm:ss")}");
                //        }
                //    }
                //}
            }
        }

        private async void PlaybackAreaOverlay_Tapped(object? sender, TappedRoutedEventArgs e)
        {
            bool execute = false;
            switch (SettingsWrapper.PlaybackTapGestureMode)
            {
                case PlaybackTapGestureMode.Always:
                    execute = true;
                    break;

                case PlaybackTapGestureMode.FullScreenOnly:
                    execute = AppState.Current.MediaServiceConnector.IsRenderingFullScreen;

                    break;
                case PlaybackTapGestureMode.NormalViewOnly:

                    execute = !AppState.Current.MediaServiceConnector.IsRenderingFullScreen;

                    break;
            }
            if (execute)
            {
                await (AppState.Current.MediaServiceConnector.PlayerInstance).PlayPauseAsync(false);
            }
        }

        private async void MiniPlayerToggle_Toggled(object? sender, RoutedEventArgs e)
        {
            if (MiniPlayerToggle.IsChecked.HasValue)
            {
                if (MiniPlayerToggle.IsChecked.Value)
                {
                }
                await MainWindowingService.Instance.RequestCompactOverlayMode(MiniPlayerToggle.IsChecked.Value);
            }
        }

        private async void SkipForward2Button_Click(object? sender, RoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SkipSecondsForth(Constants.JumpAheadSeconds);
        }

        private async void SkipBackward2Button_Click(object? sender, RoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SkipSecondsBack(Constants.JumpBackSeconds);
        }

        private async void PlayPauseButton_Tapped(object? sender, TappedRoutedEventArgs e)
        {
            await AppState.Current.MediaServiceConnector.SendPlayPause();
        }

        public void SetMediaPlayer(MediaPlayer player)
        {
            _player = player;
        }

        public async Task OpenSubtitleFile()
        {
            try
            {
                await mtcSubtitlesControl.LoadLocalSubtitleInternal(m_CurrentPlaybackItem);
            }
            catch { }
        }

        public async Task FindSubtitleOnline()
        {
            await mtcSubtitlesControl.LookForSubtitleOnline();
        }

        private async void ShowNotification(string content, int duration = 2500)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                notificationRoot.Content = new PopupRequestData(content);
                notificationRoot.Show(duration);
            });
        }


        private void ExternalNavigationRequested(object? sender, NavigationRequestEventArgs e)
        {
            ExternalNavigationRequest?.Invoke(sender, e);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                timer.Stop();
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                var buttons = this.FindVisualChildren<Button>();
                foreach (var b in buttons)
                {
                    b.DoubleTapped -= PreventDoubleTapped;
                }

                AppState.Current.KeyboardInputManager.AcceleratorInvoked -= KeyboardInputManager_AcceleratorInvoked;

                UnsubscribeEventHandlers();

                foreach (var v in doubleTappedDisabledChildren)
                {
                    v.DoubleTapped -= PreventDoubleTapped;
                    v.Tapped -= PreventSingleTapped;
                }

                doubleTappedDisabledChildren.Clear();

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~CustomMediaTransportControls()
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

        internal Task<bool> GoBack()
        {
            return Task.FromResult(false);
        }
    }
}
