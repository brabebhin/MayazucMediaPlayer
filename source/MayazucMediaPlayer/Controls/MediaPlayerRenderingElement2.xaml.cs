using CommunityToolkit.WinUI;
using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Settings;
using MayazucNativeFramework;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System.Threading;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class MediaPlayerRenderingElement2 : UserControl
    {
        readonly InputSystemCursor hiddenCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        ThreadPoolTimer? _fullscreenCursorTimer;

        SubtitleRenderer NativeSubtitleRenderer
        {
            get;
            set;
        }

        FrameServerRenderer renderer;

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
                        _mediaPlayer.VideoFrameAvailable -= VideoFameAvailanle;
                        _mediaPlayer.IsVideoFrameServerEnabled = false;
                    }
                    _mediaPlayer = value;
                    if (_mediaPlayer != null)
                    {
                        _mediaPlayer.VideoFrameAvailable += VideoFameAvailanle;
                        _mediaPlayer.IsVideoFrameServerEnabled = true;
                    }
                }
            }
        }

        public MediaPlayerRenderingElement2()
        {
            this.InitializeComponent();
            hiddenCursor.Dispose();
            this.SizeChanged += MediaPlayerRenderingElement_SizeChanged;
            AppState.Current.MediaServiceConnector.VideoEffectsConfiguration.ConfigurationChanged += EffectConfiguration_ConfigurationChanged;
            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened += PlayerInstance_OnMediaOpened;
            AppState.Current.MediaServiceConnector.CurrentPlaybackSession.PlaybackStateChanged += CurrentPlaybackSession_PlaybackStateChanged;
            this.ManipulationStarted += MediaPlayerRenderingElement2_ManipulationStarted;
            this.PointerMoved += MediaPlayerRenderingElement2_PointerMoved;
            renderer = new FrameServerRenderer(VideoSwapChain);
            NativeSubtitleRenderer = new SubtitleRenderer(SubtitleSwapChain);
        }

        private void MediaPlayerRenderingElement2_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ShowMouseShowTransportControls();
        }

        private void MediaPlayerRenderingElement2_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            ShowMouseShowTransportControls();
        }

        private void CurrentPlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            DispatcherQueue.TryEnqueue((() =>
            {
                if (sender.PlaybackState == MediaPlaybackState.Playing)
                {
                    StartMouseTransportControlHideTimer();
                }
                else
                {
                    ShowMouseShowTransportControls();
                }
            }));
        }

        private void HideMouseHideTransportControls(ThreadPoolTimer timer)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ProtectedCursor = hiddenCursor;
                this.MediaTransportControls.Visibility = Visibility.Collapsed;
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
            this.MediaTransportControls.Visibility = Visibility.Visible;
            if (AppState.Current.MediaServiceConnector.IsPlaying())
            {
                StartMouseTransportControlHideTimer();
            }
        }

        private async void PlayerInstance_OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            if (args.Reason == MediaOpenedEventReason.MediaPlayerObjectRequested) return;

            if (args.EventData.PreviousItem != null)
            {
                foreach (var tmd in args.EventData.PreviousItem.TimedMetadataTracks)
                {
                    tmd.CueEntered -= UpdateSubtitles;
                    tmd.CueExited -= UpdateSubtitles;
                }

                args.EventData.PreviousItem.TimedMetadataTracksChanged -= PlaybackItem_TimedMetadataTracksChanged;

            }
            foreach (var tmd in args.EventData.PlaybackItem.TimedMetadataTracks)
            {
                tmd.CueEntered += UpdateSubtitles;
                tmd.CueExited += UpdateSubtitles;
            }

            args.EventData.PlaybackItem.TimedMetadataTracksChanged += PlaybackItem_TimedMetadataTracksChanged;

            await DispatcherQueue.EnqueueAsync(() =>
            {
                if (args.EventData.PlaybackItem != null)
                {
                    if (!args.EventData.PlaybackItem.IsVideo())
                    {
                        PosterImageImage.Visibility = Visibility.Visible;
                        VideoSwapChain.Visibility = Visibility.Collapsed;
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
                    SubtitleSwapChain.Width = thisActualWidth;

                    SubtitleSwapChain.Visibility = Visibility.Visible;
                    SubtitleSwapChain.Opacity = 1;

                    NativeSubtitleRenderer.RenderSubtitlesToFrame(AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem, (float)thisActualWidth, (float)thisActualHeight, 96f, Windows.Graphics.DirectX.DirectXPixelFormat.R8G8B8A8UIntNormalized);
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

        private async void UpdateSubtitles(Windows.Media.Core.TimedMetadataTrack sender, Windows.Media.Core.MediaCueEventArgs args)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                DrawSubtitles(new Size(this.ActualWidth, this.ActualHeight));
            });
        }

        private async void EffectConfiguration_ConfigurationChanged(object? sender, string e)
        {
            await DispatcherQueue.EnqueueAsync(new Action(() =>
            {
                RedrawPaused(WrappedMediaPlayer, new Size(this.ActualWidth, this.ActualHeight));
            }));
        }

        private void MediaPlayerRenderingElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                RedrawPaused(WrappedMediaPlayer, e.NewSize);

                DrawSubtitles(e.NewSize);
            });
        }

        private async void VideoFameAvailanle(MediaPlayer sender, object args)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                DrawVideoFrame(sender, new Size(this.ActualWidth, this.ActualHeight));
            });
        }

        private void RedrawPaused(MediaPlayer sender, Size? requestedSize)
        {
            if (WrappedMediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                if (AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem.IsVideo())
                    DrawVideoFrame(sender, requestedSize);
        }



        private void DrawVideoFrame(MediaPlayer sender, Size? requestedSize)
        {
            try
            {
                if (this.ActualWidth == 0 || this.ActualHeight == 0) return;

                var thisActualHeight = requestedSize.HasValue ? requestedSize.Value.Height : this.ActualHeight;
                var thisActualWidth = requestedSize.HasValue ? requestedSize.Value.Width : this.ActualWidth;

                PosterImageImage.Visibility = Visibility.Collapsed;
                VideoSwapChain.Visibility = Visibility.Visible;

                VideoSwapChain.Opacity = 1;

                var currentPlaybackSession = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
                var ar = (float)currentPlaybackSession.NaturalVideoWidth / currentPlaybackSession.NaturalVideoHeight;

                var width = 0f;
                var height = 0f;

                if (currentPlaybackSession.NaturalVideoHeight != currentPlaybackSession.NaturalVideoWidth)
                {
                    height = (float)thisActualHeight;
                    width = height * ar;
                }
                else if (currentPlaybackSession.NaturalVideoHeight == currentPlaybackSession.NaturalVideoWidth)
                {
                    height = width = (float)thisActualWidth * ar;
                }
                //else
                //{
                //    width = (float)this.ActualWidth;
                //    height = (float)this.ActualWidth / ar;
                //}

                VideoSwapChain.Width = width;
                VideoSwapChain.Height = height;

                renderer.RenderMediaPlayerFrame(sender, (float)width, (float)height, 96f, Windows.Graphics.DirectX.DirectXPixelFormat.R8G8B8A8UIntNormalized, AppState.Current.MediaServiceConnector.VideoEffectsConfiguration);
            }
            catch
            {
            }
        }

        public async Task SaveVideoFrameAsync()
        {
            if (AppState.Current.MediaServiceConnector.HasActivePlaybackSession())
            {
                try
                {
                    var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
                    var outputFolder = await LocalFolders.GetSavedVideoFramesFolder();
                    var currentMediaData = AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackData;

                    var sourceFile = new FileInfo(currentMediaData.MediaPath);
                    if (sourceFile != null && sourceFile.Exists)
                    {
                        var name = $"{Path.GetFileNameWithoutExtension(sourceFile.Name)}-{session.Position.TotalSeconds}.png";
                        var folder = await LocalCache.LocalFolders.GetSavedVideoFramesFolder();
                        var file = await folder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
                        using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            //await renderer.RenderMediaPlayerFrameToStreamAsync(_mediaPlayer, AppState.Current.MediaServiceConnector.VideoEffectsConfiguration, stream);
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
            await this.MediaTransportControls.FullScreenAutoSwitch();
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
            switch (SettingsWrapper.PlaybackTapGestureMode)
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
