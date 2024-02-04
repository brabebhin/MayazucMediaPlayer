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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class MediaPlayerRenderingElement : UserControl
    {
        private SoftwareBitmap frameServerImageSource;
        private CanvasImageSource canvasImageSource;
        readonly AsyncLock sizeChangedLock = new AsyncLock();
        private CanvasDevice canvasDevice;
        private ImageSource transparentPosterSource;
        private EffectProcessor EffectRenderer = new EffectProcessor();

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
                    }
                }
            }
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
            transparentPosterSource = new BitmapImage(new Uri("ms-appx:///assets/transparent.png"));
            this.SizeChanged += MediaPlayerRenderingElement_SizeChanged;
            EffectRenderer.EffectConfiguration = AppState.Current.MediaServiceConnector.VideoEffectsConfiguration;
            EffectRenderer.EffectConfiguration.ConfigurationChanged += EffectConfiguration_ConfigurationChanged;
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
                canvasDevice = CanvasDevice.GetSharedDevice();

                FrameServerImage.Width = this.ActualWidth;
                FrameServerImage.Height = this.ActualHeight;

                if (FrameServerImage.Width == 0 || FrameServerImage.Height == 0) return;

                FrameServerImage.Visibility = Visibility.Visible;
                FrameServerImage.Opacity = 1;

                if (frameServerImageSource == null || (frameServerImageSource.PixelWidth != FrameServerImage.Width) || (frameServerImageSource.PixelHeight != FrameServerImage.Height))
                {
                    frameServerImageSource?.Dispose();
                    frameServerImageSource = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)FrameServerImage.Width, (int)FrameServerImage.Height, BitmapAlphaMode.Ignore);
                }

                if (canvasImageSource == null || (canvasImageSource.Size.Width != FrameServerImage.Width) || (canvasImageSource.Size.Height != FrameServerImage.Height))
                {
                    canvasImageSource = new CanvasImageSource(canvasDevice, (int)FrameServerImage.Width, (int)FrameServerImage.Height, 96);
                }

                using (canvasDevice.Lock())
                {
                    using (CanvasBitmap inputBitmap = CanvasBitmap.CreateFromSoftwareBitmap(canvasDevice, frameServerImageSource))
                    using (CanvasDrawingSession ds = canvasImageSource.CreateDrawingSession(Microsoft.UI.Colors.Transparent))
                    {
                        sender.CopyFrameToVideoSurface(inputBitmap);
                        ds.DrawImage(EffectRenderer.ProcessFrame(inputBitmap));
                        ds.Flush();

                        FrameServerImage.Source = canvasImageSource;
                    }
                }
            }
            catch
            {
                frameServerImageSource?.Dispose();
                frameServerImageSource = null;
                canvasImageSource = null;
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
