using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Tests;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class MediaPlayerRenderingElement : BaseUserControl
    {
        Win2DSubtitleRenderer subsRenderer = new Win2DSubtitleRenderer();

        private VideoEffectProcessor EffectRenderer = new VideoEffectProcessor();
        MediaPlayer _mediaPlayer;

        ConcurrentQueue<Size> swapChainResizeQueue = new ConcurrentQueue<Size>();

        public MediaPlayer WrappedMediaPlayer
        {
            get => _mediaPlayer;
            set
            {
                if (_mediaPlayer != value)
                {
                    if (_mediaPlayer != null)
                    {
                        _mediaPlayer.VideoFrameAvailable -= _mediaPlayer_VideoFrameAvailable;
                        _mediaPlayer.IsVideoFrameServerEnabled = false;
                    }
                    _mediaPlayer = value;
                    if (_mediaPlayer != null)
                    {
                        _mediaPlayer.VideoFrameAvailable += _mediaPlayer_VideoFrameAvailable;
                        _mediaPlayer.IsVideoFrameServerEnabled = true;
                        _mediaPlayer.SubtitleFrameChanged += _mediaPlayer_SubtitleFrameChanged;
                    }
                }
            }
        }

        private async void _mediaPlayer_SubtitleFrameChanged(MediaPlayer sender, object args)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                DrawSubtitles();
            });
        }

        private void DrawSubtitles()
        {
            SubtitleImage.Width = this.ActualWidth;
            SubtitleImage.Height = this.ActualHeight;

            SubtitleImage.Visibility = Visibility.Visible;
            SubtitleImage.Opacity = 1;

        }

        public ImageSource PosterSource
        {
            get
            {
                return mediaPlayerElementInstance.PosterSource;
            }
            set
            {
                mediaPlayerElementInstance.PosterSource = value;
            }
        }

        public MediaPlayerRenderingElement()
        {
            this.InitializeComponent();
            mediaPlayerElementInstance.RegisterPropertyChangedCallback(MediaPlayerElement.MediaPlayerProperty, new DependencyPropertyChangedCallback(OnMediaPlayerChanged));
            this.SizeChanged += MediaPlayerRenderingElement_SizeChanged;
            AppState.Current.MediaServiceConnector.VideoEffectsConfiguration.ConfigurationChanged += EffectConfiguration_ConfigurationChanged;
            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened += PlayerInstance_OnMediaOpened;
        }

        private void PlayerInstance_OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            if (args.Reason == MediaOpenedEventReason.MediaPlayerObjectRequested) return;


        }

        private async void EffectConfiguration_ConfigurationChanged(object? sender, string e)
        {
            await DispatcherQueue.EnqueueAsync(new Action(() =>
            {
                RedrawPaused(WrappedMediaPlayer);
            }));
        }

        private async void MediaPlayerRenderingElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawPaused(WrappedMediaPlayer);
            swapChainResizeQueue.Enqueue(e.NewSize);
        }

        private void OnMediaPlayerChanged(DependencyObject sender, DependencyProperty dp)
        {
            WrappedMediaPlayer = (sender as MediaPlayerElement).MediaPlayer;
        }

        private async void _mediaPlayer_VideoFrameAvailable(MediaPlayer sender, object args)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                DrawFrame(sender);
            });
        }

        private void RedrawPaused(MediaPlayer sender)
        {
            if (WrappedMediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                if (AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem.IsVideo())
                    DrawFrame(sender);
        }

        private void DrawFrame(MediaPlayer sender)
        {
            try
            {
                FrameServerImage.Width = this.ActualWidth;
                FrameServerImage.Height = this.ActualHeight;


                if (FrameServerImage.Width == 0 || FrameServerImage.Height == 0) return;

                FrameServerImage.Visibility = Visibility.Visible;
                FrameServerImage.Opacity = 1;

                //renderer.RenderMediaPlayerFrame(sender, FrameServerImage, AppState.Current.MediaServiceConnector.VideoEffectsConfiguration);
                //DrawSubtitles();

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
                    var outputFolder = await KnownLocations.GetSavedVideoFramesFolder();
                    var currentMediaData = AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackData;

                    var sourceFile = new FileInfo(currentMediaData.MediaPath);
                    if (sourceFile != null && sourceFile.Exists)
                    {
                        var name = $"{Path.GetFileNameWithoutExtension(sourceFile.Name)}-{session.Position.TotalSeconds}.png";
                        var folder = await LocalCache.KnownLocations.GetSavedVideoFramesFolder();
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

        public MediaPlayerElement MediaPlayerElementInstance
        {
            get
            {
                return mediaPlayerElementInstance;
            }
        }
    }
}
