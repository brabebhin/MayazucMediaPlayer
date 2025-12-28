using CommunityToolkit.WinUI;
using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.NowPlayingViews;
using MayazucMediaPlayer.Rendering;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.UserInput;
using MayazucMediaPlayer.VideoEffects;
using MayazucNativeFramework;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Display;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System.Threading;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class MediaPlayerRenderingElement2 : BaseUserControl
    {
        private record VideoSwapChainResizeRequest(Size TargetSize, double VideoStreamAspectRatio)
        {
            public Size TargetSize { get; private set; } = TargetSize;
            public double VideoStreamAspectRatio { get; private set; } = VideoStreamAspectRatio;
        }
        volatile bool disposed = false;
        readonly InputSystemCursor hiddenCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        ThreadPoolTimer? _fullscreenCursorTimer;
        volatile bool isPointerOverTransportControls = false;
        bool useMfSubsRenderer = false;
        ConcurrentQueue<VideoSwapChainResizeRequest> pendingResizesQueue = new ConcurrentQueue<VideoSwapChainResizeRequest>();

        private readonly Task VideoRenderingTask;
        private readonly ManualResetEventSlim PlayPauseSignal = new ManualResetEventSlim(false);

        ManagedVideoEffectProcessorConfiguration effectProcessorConfiguration
        {
            get
            {
                return AppState.Current.MediaServiceConnector.VideoEffectsConfiguration;
            }
        }

        MayazucNativeFramework.SubtitleRenderer NativeSubtitleRenderer
        {
            get;
            set;
        }

        MediaFoundationSubtitleRenderer MFSubtitleRenderer
        {
            get;
            set;
        }

        VideoRenderer videoRenderer;

        MediaPlayer _mediaPlayer;

        public MediaPlayer WrappedMediaPlayer
        {
            get => _mediaPlayer;
            set
            {
                if (_mediaPlayer != value)
                {
                    if (_mediaPlayer != null)
                    {
                        _mediaPlayer.SubtitleFrameChanged -= _mediaPlayer_SubtitleFrameChanged;
                        _mediaPlayer.VideoFrameAvailable -= VideoFameAvailable;
                        _mediaPlayer.IsVideoFrameServerEnabled = false;
                    }
                    _mediaPlayer = value;
                    if (_mediaPlayer != null)
                    {
                        _mediaPlayer.SubtitleFrameChanged += _mediaPlayer_SubtitleFrameChanged;
                        _mediaPlayer.VideoFrameAvailable += VideoFameAvailable;
                        _mediaPlayer.IsVideoFrameServerEnabled = true;
                    }
                }
            }
        }

        private volatile bool windowSupportsHdr = false;
        private DisplayInformation displayInformation;

        public MediaPlayerRenderingElement2()
        {
            this.InitializeComponent();
            hiddenCursor.Dispose();
            this.SizeChanged += MediaPlayerRenderingElement_SizeChanged;
            AppState.Current.MediaServiceConnector.VideoEffectsConfiguration.ConfigurationChanged += EffectConfiguration_ConfigurationChanged;
            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened += PlayerInstance_OnMediaOpened;
            AppState.Current.MediaServiceConnector.PlayerInstance.OnStateChanged += CurrentPlaybackSession_PlaybackStateChanged;
            this.ManipulationStarted += MediaPlayerRenderingElement2_ManipulationStarted;
            this.PointerMoved += MediaPlayerRenderingElement2_PointerMoved;
            videoRenderer = new VideoRenderer(VideoSwapChain);
            allocSubtitleRenderer();
            _ = MediaEffectsFrame.NavigateAsync(typeof(MediaEffectsPage));
            AppState.Current.KeyboardInputManager.AcceleratorInvoked += KeyboardInputManager_AcceleratorInvoked;
            MediaTransportControlsInstance.PointerEntered += MediaTransportControlsInstance_PointerEntered;
            MediaTransportControlsInstance.PointerExited += MediaTransportControlsInstance_PointerExited;

            displayInformation = DisplayInformation.CreateForWindowId(MainWindowingService.Instance.Id);
            displayInformation.AdvancedColorInfoChanged += DisplayInformation_AdvancedColorInfoChanged;
            CheckHdr(displayInformation);

            _ = nowPlayingList.InitializeStateAsync(null);
            VideoRenderingTask = Task.Factory.StartNew(VideoRenderingLoop, TaskCreationOptions.LongRunning);

        }

        private async Task VideoRenderingLoop()
        {
            while (!disposed)
            {
                try
                {
                    PlayPauseSignal.Wait();
                    if (!disposed)
                        await DrawVideoFrame(_mediaPlayer);
                }
                catch
                {
                    //something may have happened with the video rendering loop, carry on
                }
            }
        }

        void allocSubtitleRenderer()
        {
            if (useMfSubsRenderer)
            {
                MFSubtitleRenderer = new MediaFoundationSubtitleRenderer(SubtitleSwapChain);
            }
            else
            {
                //NativeSubtitleRenderer = new SubtitleRenderer(SubtitleSwapChain);
            }
        }

        protected override void OnDispose(bool disposing)
        {
            disposed = true;
            base.OnDispose(disposing);
            displayInformation.AdvancedColorInfoChanged -= DisplayInformation_AdvancedColorInfoChanged;
            displayInformation.Dispose();
            WrappedMediaPlayer = null;
        }

        private async void DisplayInformation_AdvancedColorInfoChanged(DisplayInformation sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                CheckHdr(sender);
            });
        }

        private void CheckHdr(DisplayInformation sender)
        {
            windowSupportsHdr = sender.GetAdvancedColorInfo().CurrentAdvancedColorKind == DisplayAdvancedColorKind.HighDynamicRange;
        }

        private async void MediaTransportControlsInstance_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                isPointerOverTransportControls = false;
            });
        }

        private async void MediaTransportControlsInstance_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                isPointerOverTransportControls = true;
            });
        }

        private async void KeyboardInputManager_AcceleratorInvoked(object? sender, HotKeyId e)
        {
            if (e == HotKeyId.SaveVideoFrame)
            {
                await SaveVideoFrameAsync();
            }
        }

        private void MediaPlayerRenderingElement2_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ShowMouseShowTransportControls();
        }

        private void MediaPlayerRenderingElement2_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            ShowMouseShowTransportControls();
        }

        private void PlayPauseSignalSetIfVideo(MediaPlaybackItem item)
        {
            if (item.IsVideo())
            {
                PlayPauseSignal.Set();
            }
        }

        private void CurrentPlaybackSession_PlaybackStateChanged(MediaPlayer sender, MediaPlaybackState state)
        {
            DispatcherQueue?.TryEnqueue((() =>
            {
                if (state == MediaPlaybackState.Playing)
                {
                    StartMouseTransportControlHideTimer();
                    PlayPauseSignalSetIfVideo(AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem);
                }
                else
                {
                    ShowMouseShowTransportControls();
                    PlayPauseSignal.Reset();
                }
            }));
        }

        private void HideMouseHideTransportControls(ThreadPoolTimer timer)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (AppState.Current.MediaServiceConnector.IsPlaying()
                && !isPointerOverTransportControls
                && !MediaTransportControlsInstance.UserInteracting()
                && !MediaEffectsToggleButton.IsChecked.TrueOrDefault()
                && !NowPlayingToggleButton.IsChecked.TrueOrDefault()
                )
                {
                    ProtectedCursor = hiddenCursor;
                    this.MediaTransportControlsInstance.Visibility = Visibility.Collapsed;
                }
                else
                {
                    StartMouseTransportControlHideTimer();
                }
            });
        }

        private void StartMouseTransportControlHideTimer()
        {
            _fullscreenCursorTimer?.Cancel();
            _fullscreenCursorTimer = ThreadPoolTimer.CreateTimer(HideMouseHideTransportControls, TimeSpan.FromSeconds(5));
        }

        private void ShowMouseShowTransportControls()
        {
            this.ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
            this.MediaTransportControlsInstance.Visibility = Visibility.Visible;
            if (AppState.Current.MediaServiceConnector.IsPlaying())
            {
                StartMouseTransportControlHideTimer();
            }
        }

        private void PlayerInstance_OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            if (args.Reason == MediaOpenedEventReason.MediaPlayerObjectRequested) return;

            if (!useMfSubsRenderer)
            {
                if (args.Data.PreviousItem != null)
                {
                    foreach (var tmd in args.Data.PreviousItem.TimedMetadataTracks)
                    {
                        tmd.CueEntered -= UpdateSubtitles;
                        tmd.CueExited -= UpdateSubtitles;
                    }

                    args.Data.PreviousItem.TimedMetadataTracksChanged -= PlaybackItem_TimedMetadataTracksChanged;

                }
                foreach (var tmd in args.Data.PlaybackItem.TimedMetadataTracks)
                {
                    tmd.CueEntered += UpdateSubtitles;
                    tmd.CueExited += UpdateSubtitles;
                }

                args.Data.PlaybackItem.TimedMetadataTracksChanged += PlaybackItem_TimedMetadataTracksChanged;
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                if (args.Data.PlaybackItem != null)
                {
                    if (!args.Data.PlaybackItem.IsVideo())
                    {

                        PosterImageImage.Visibility = Visibility.Visible;
                        VideoSwapChain.Visibility = Visibility.Collapsed;
                        PlayPauseSignal.Reset();
                    }
                    else
                    {
                        pendingResizesQueue.Enqueue(new VideoSwapChainResizeRequest(new Size(this.ActualWidth, this.ActualHeight), args.Data.ExtraData.FFmpegMediaSource.CurrentVideoStream.DisplayAspectRatio));

                        PlayPauseSignalSetIfVideo(args.Data.PlaybackItem);
                        PosterImageImage.Visibility = Visibility.Collapsed;
                        VideoSwapChain.Visibility = Visibility.Visible;
                    }
                }
            });
        }

        private void PlaybackItem_TimedMetadataTracksChanged(MediaPlaybackItem sender, IVectorChangedEventArgs args)
        {
            switch (args.CollectionChange)
            {
                case CollectionChange.ItemInserted:
                    sender.TimedMetadataTracks[(int)args.Index].CueEntered += UpdateSubtitles;
                    sender.TimedMetadataTracks[(int)args.Index].CueExited += UpdateSubtitles;
                    break;
                case CollectionChange.ItemRemoved:
                    sender.TimedMetadataTracks[(int)args.Index].CueEntered -= UpdateSubtitles;
                    sender.TimedMetadataTracks[(int)args.Index].CueExited -= UpdateSubtitles;
                    break;
            }
        }

        private void DrawSubtitles(Size? requestedSize)
        {
            if (AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem.IsVideo())
            {
                try
                {
                    if (this.ActualWidth == 0 || this.ActualHeight == 0) return;

                    var thisActualWidth = requestedSize.HasValue ? requestedSize.Value.Width : VideoSwapChain.ActualWidth;
                    var thisActualHeight = (requestedSize.HasValue ? requestedSize.Value.Height : VideoSwapChain.Height) - TransportControlsRow.ActualHeight;
                    SubtitleSwapChain.Height = thisActualHeight < 0 ? 0 : thisActualHeight;
                    SubtitleSwapChain.Width = thisActualWidth < 0 ? 0 : thisActualWidth;

                    SubtitleSwapChain.Visibility = Visibility.Visible;
                    SubtitleSwapChain.Opacity = 1;

                    if (useMfSubsRenderer)
                    {
                        MFSubtitleRenderer.RenderSubtitlesToFrame(_mediaPlayer, (uint)thisActualWidth, (uint)thisActualHeight, 96u, Windows.Graphics.DirectX.DirectXPixelFormat.R8G8B8A8UIntNormalized);
                    }
                    else
                    {
                        NativeSubtitleRenderer.RenderSubtitlesToFrame(AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem, (uint)thisActualWidth, (uint)thisActualHeight, 96u, Windows.Graphics.DirectX.DirectXPixelFormat.R8G8B8A8UIntNormalized);
                    }
                }
                catch
                {

                }
            }
        }

        public ImageSource PosterSource
        {
            get => PosterImageImage.Source;
            set => PosterImageImage.Source = value;
        }
        public bool IsFullWindow { get; internal set; }

        private void UpdateSubtitles(Windows.Media.Core.TimedMetadataTrack sender, Windows.Media.Core.MediaCueEventArgs args)
        {
            try
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    //DrawSubtitles(new Size(this.ActualWidth, this.ActualHeight));
                });
            }
            catch { }
        }

        private void EffectConfiguration_ConfigurationChanged(object? sender, string e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                _ = RedrawPaused(WrappedMediaPlayer);
            });
        }

        private void MediaPlayerRenderingElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                NowPlayingListSplitView.OpenPaneLength = e.NewSize.Width / 2;
                MediaEffectsSplitView.OpenPaneLength = e.NewSize.Width * 0.67;

                var ar = AppState.Current.MediaServiceConnector.PlayerInstance.FfmpegInteropInstance.GetCurrentVideoAspectRatio();
                if (!ar.HasValue)
                {
                    ar = 1;
                }
                pendingResizesQueue.Enqueue(new(e.NewSize, ar.Value));
                //RedrawPaused(WrappedMediaPlayer);

                //DrawSubtitles(e.NewSize);
            });
        }

        private void _mediaPlayer_SubtitleFrameChanged(MediaPlayer sender, object args)
        {
            //DispatcherQueue.TryEnqueue(() =>
            //{
            //    DrawSubtitles(new Size(this.ActualWidth, this.ActualHeight));
            //});
        }

        private void VideoFameAvailable(MediaPlayer sender, object args)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                //_ = DrawVideoFrame(sender);
            });
        }

        private async Task RedrawPaused(MediaPlayer sender)
        {
            if (WrappedMediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                if (AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem.IsVideo())
                    await DrawVideoFrame(sender);
        }



        private async Task DrawVideoFrame(MediaPlayer sender)
        {
            try
            {
                if (!pendingResizesQueue.IsEmpty)
                {
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        try
                        {
                            var videoRenderSize = default(VideoSwapChainResizeRequest);
                            while (!pendingResizesQueue.IsEmpty)
                            {
                                pendingResizesQueue.TryDequeue(out videoRenderSize);
                            }
                            if (videoRenderSize != default(VideoSwapChainResizeRequest))
                                SetVideoSwapChainRenderSize(videoRenderSize);
                        }
                        catch
                        {

                        }
                    });
                }
                videoRenderer.RenderMediaPlayerFrame(sender, effectProcessorConfiguration);
            }
            catch
            {
            }
        }

        private void SetVideoSwapChainRenderSize(VideoSwapChainResizeRequest requestedSizeData)
        {
            var newSize = requestedSizeData.TargetSize;
            var requestedSize = new Size(newSize.Width, newSize.Height);

            if (requestedSize.Width == 0 || requestedSize.Height == 0) return;

            var thisActualHeight = requestedSize.Height;
            var thisActualWidth = requestedSize.Width;

            var currentPlaybackSession = WrappedMediaPlayer.PlaybackSession;
            var ar = (float)requestedSizeData.VideoStreamAspectRatio;

            float width = 0f;
            float height = 0f;
            if (currentPlaybackSession.NaturalVideoHeight != currentPlaybackSession.NaturalVideoWidth)
            {
                height = (float)thisActualHeight;
                width = height * ar;
            }
            else if (currentPlaybackSession.NaturalVideoHeight == currentPlaybackSession.NaturalVideoWidth)
            {
                height = width = (float)thisActualWidth * ar;
            }

            VideoSwapChain.Width = width;
            VideoSwapChain.Height = height;

            videoRenderer.ResizeSwapChain((uint)width, (uint)height, 96, GetPixelFormat());
        }

        private DirectXPixelFormat GetPixelFormat()
        {
            return windowSupportsHdr ? DirectXPixelFormat.R16G16B16A16Float : Windows.Graphics.DirectX.DirectXPixelFormat.R8G8B8A8UIntNormalized;
        }

        private async Task SaveVideoFrameAsync()
        {
            if (AppState.Current.MediaServiceConnector.HasActivePlaybackSession() && AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem.IsVideo())
            {
                try
                {
                    var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
                    var outputFolder = await LocalFolders.GetSavedVideoFramesFolder();
                    var currentMediaData = AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackData;

                    var sourceFile = new FileInfo(currentMediaData.MediaPath);
                    if (sourceFile != null && sourceFile.Exists)
                    {
                        var name = $"{Path.GetFileNameWithoutExtension(sourceFile.Name)}-{session.Position.FileFormatTimespan()}.png";
                        var folder = await LocalFolders.GetSavedVideoFramesFolder();
                        var file = await folder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
                        using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            //await videoRenderer.RenderMediaPlayerFrameToStreamAsync(_mediaPlayer, AppState.Current.MediaServiceConnector.VideoEffectsConfiguration, stream);
                        }

                        PopupHelper.ShowInfoMessage($"Frame saved: {DateTime.Now.ToString("hh:mm:ss")}");
                    }
                }
                catch
                {

                }
            }
        }

        private async void GoFullScreen_doubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await this.MediaTransportControlsInstance.FullScreenAutoSwitch();
        }

        private void PreventDoubleTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void PreventDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private async void HandlePlaybackAreaOverlayCommands(object sender, TappedRoutedEventArgs e)
        {
            bool canHandle = false;
            switch (SettingsService.Instance.PlaybackTapGestureMode)
            {
                case PlaybackTapGestureMode.Always:
                    canHandle = true;
                    break;
                case PlaybackTapGestureMode.NormalViewOnly:
                    canHandle = !MainWindowingService.Instance.IsInFullScreenMode();
                    break;
                case PlaybackTapGestureMode.FullScreenOnly:
                    canHandle = MainWindowingService.Instance.IsInFullScreenMode();
                    break;
            }

            if (canHandle)
                await AppState.Current.MediaServiceConnector.PlayPauseAutoSwitch();
        }
    }
}
