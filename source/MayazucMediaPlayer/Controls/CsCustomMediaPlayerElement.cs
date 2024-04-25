using CommunityToolkit.WinUI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.Controls
{

    [Obsolete("this doesn't work on winui 3")]
    public class CsCustomMediaPlayerElement : MediaPlayerElement, IDisposable
    {
        public MediaPlayerPresenter PlayerPresenter
        {
            get;
            private set;
        }

        public Image FrameServerImage
        {
            get;
            private set;
        }

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


        private SoftwareBitmap frameServerDest;
        private CanvasImageSource canvasImageSource;
        readonly AsyncLock sizeChangedLock = new AsyncLock();
        private readonly long mediaPlayerchangedToken;
        readonly App app;
        private CanvasDevice canvasDevice;

        public CsCustomMediaPlayerElement()
        {
            DefaultStyleKey = typeof(CsCustomMediaPlayerElement);

            app = App.Current as App;
        }

        protected override void OnApplyTemplate()
        {
            PlayerPresenter = GetTemplateChild("MediaPlayerPresenter") as MediaPlayerPresenter;

            FrameServerImage = GetTemplateChild("PosterImage2") as Image;
            FrameServerImage.Unloaded += FrameServerImage_Unloaded;
            FrameServerImage.Visibility = Visibility.Visible;
            FrameServerImage.ImageFailed += FrameServerImage_ImageFailed;
            base.OnApplyTemplate();
        }

        private void FrameServerImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FrameServerImage_Unloaded(object sender, RoutedEventArgs e)
        {

        }



        private async void _mediaPlayer_VideoFrameAvailable(MediaPlayer sender, object args)
        {

            await DispatcherQueue.EnqueueAsync(async () =>
            {
                await DrawFrameAsync(sender);
            });
        }

        private async Task DrawFrameAsync(MediaPlayer sender)
        {
            try
            {
                canvasDevice = CanvasDevice.GetSharedDevice();

                if (sender.PlaybackSession.PlaybackState == MediaPlaybackState.Paused) return;
                FrameServerImage.Width = PlayerPresenter.ActualWidth;
                FrameServerImage.Height = PlayerPresenter.ActualHeight;
                if (!Window.Current.Visible || FrameServerImage.Width == 0 || FrameServerImage.Height == 0) return;
                FrameServerImage.Visibility = Visibility.Visible;
                FrameServerImage.Opacity = 1;
                Visibility = Visibility.Visible;
                PlayerPresenter.Opacity = 0;
                if (frameServerDest == null || (frameServerDest.PixelWidth != FrameServerImage.Width) || (frameServerDest.PixelHeight != FrameServerImage.Height))
                {
                    frameServerDest?.Dispose();
                    // FrameServerImage in this example is a XAML image control
                    frameServerDest = new SoftwareBitmap(BitmapPixelFormat.Bgra8, (int)FrameServerImage.Width, (int)FrameServerImage.Height, BitmapAlphaMode.Ignore);
                }
                if (canvasImageSource == null || (canvasImageSource.Size.Width != FrameServerImage.Width) || (canvasImageSource.Size.Height != FrameServerImage.Height))
                {
                    canvasImageSource = new CanvasImageSource(canvasDevice, (int)FrameServerImage.Width, (int)FrameServerImage.Height, 96);
                }

                using (canvasDevice.Lock())
                {

                    using (CanvasBitmap inputBitmap = CanvasBitmap.CreateFromSoftwareBitmap(canvasDevice, frameServerDest))
                    using (CanvasDrawingSession ds = canvasImageSource.CreateDrawingSession(Microsoft.UI.Colors.Black))
                    {
                        bool stupid = false;
                        sender.CopyFrameToVideoSurface(inputBitmap);
                        if (stupid)
                        {
                            sender.Pause();
                            var folder = await LocalCache.LocalFolders.GetSavedVideoFramesFolder();
                            var file = await folder.CreateFileAsync("testpula345345.png", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                            {
                                await inputBitmap.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                            }
                        }

                        ds.DrawImage(inputBitmap);
                        ds.Flush();

                        FrameServerImage.Source = canvasImageSource;
                        CanvasSolidColorBrush brush = new CanvasSolidColorBrush(canvasDevice, Microsoft.UI.Colors.Red);
                        ds.FillRectangle(new Windows.Foundation.Rect(300, 300, 100, 100), brush);
                        ds.Flush();
                    }
                }
            }
            catch
            {
                frameServerDest?.Dispose();
                frameServerDest = null;
                canvasImageSource = null;
            }
        }


        public void Dispose()
        {
        }
    }


}
