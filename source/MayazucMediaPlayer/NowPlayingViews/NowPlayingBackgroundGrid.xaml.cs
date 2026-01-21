using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.LocalCache;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.NowPlayingViews
{
    public sealed partial class NowPlayingBackgroundGrid : BaseUserControl
    {
        readonly DispatcherQueueTimer timer = null;

        List<Image> images = new List<Image>();

        public NowPlayingBackgroundGrid()
        {
            InitializeComponent();
            InitImages();
            SizeChanged += NowPlayingBackgroundGrid_SizeChanged;
            _ = LoadBackgroundImages();
            timer = DispatcherQueue.CreateTimer();
            timer.Interval = TimeSpan.FromMinutes(5);
            timer.Tick += Timer_Tick;
            timer.IsRepeating = true;
            timer.Start();
        }

        private void InitImages()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Image image = new Image();
                    Grid.SetRow(image, i);
                    Grid.SetColumn(image, j);
                    image.Stretch = Microsoft.UI.Xaml.Media.Stretch.Fill;
                    images.Add(image);
                    rootGrid.Children.Add(image);
                }
            }
        }


        private void Timer_Tick(DispatcherQueueTimer sender, object args)
        {
            _ = LoadBackgroundImages();
        }

        private async Task LoadBackgroundImages()
        {
            var imagesLoadTask = Task.Run<List<string>>(async () =>
            {
                var albumArt = await KnownLocations.GetAlbumArtFolder();
                var files = albumArt.GetFiles();
                files.Randomize();
                return files.Select(x => x.FullName).ToList();
            });



            var imageSources = await imagesLoadTask;

            if (imageSources.Count < images.Count) return;
            for (int i = 0; i < images.Count; i++)
            {
                images[i].Source = new BitmapImage(new Uri(imageSources[i]));
            }

        }

        private void NowPlayingBackgroundGrid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
        }
    }
}
