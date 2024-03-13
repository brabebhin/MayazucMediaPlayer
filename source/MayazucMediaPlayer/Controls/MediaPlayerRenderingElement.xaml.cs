using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Playback;
using CommunityToolkit.WinUI;
using Microsoft.Graphics.Canvas.Brushes;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using CommunityToolkit.WinUI.UI;
using MayazucMediaPlayer.MediaPlayback;
using MayazucNativeFramework;
using MayazucMediaPlayer.LocalCache;
using Windows.Storage;
using MayazucMediaPlayer.Tests;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class MediaPlayerRenderingElement : UserControl
    {
        bool useNativeSubtitleRenderer = true;

        FrameServerRenderer renderer = new FrameServerRenderer();
        Win2DSubtitleRenderer subsRenderer = new Win2DSubtitleRenderer();

        private EffectProcessor EffectRenderer = new EffectProcessor();
        SubtitleRenderer nativeSubRenderer = new SubtitleRenderer();
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

            if (useNativeSubtitleRenderer)
                nativeSubRenderer.RenderSubtitlesToFrame(AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem, SubtitleImage);
            else subsRenderer.RenderSubtitlesToImage(SubtitleImage, AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem, DispatcherQueue);
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
            if (args.EventData.PreviousItem != null)
            {
                foreach (var tmd in args.EventData.PreviousItem.TimedMetadataTracks)
                {
                    tmd.CueEntered -= Tmd_CueEntered;
                    tmd.CueExited -= Tmd_CueEntered;
                }
            }
            foreach (var tmd in args.EventData.PlaybackItem.TimedMetadataTracks)
            {
                tmd.CueEntered += Tmd_CueEntered;
                tmd.CueExited += Tmd_CueEntered;
            }
        }

        private async void Tmd_CueEntered(Windows.Media.Core.TimedMetadataTrack sender, Windows.Media.Core.MediaCueEventArgs args)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                DrawSubtitles();

            });
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

                renderer.RenderMediaPlayerFrame(sender, FrameServerImage, AppState.Current.MediaServiceConnector.VideoEffectsConfiguration);
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
                            await renderer.RenderMediaPlayerFrameToStreamAsync(_mediaPlayer, AppState.Current.MediaServiceConnector.VideoEffectsConfiguration, stream);
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
