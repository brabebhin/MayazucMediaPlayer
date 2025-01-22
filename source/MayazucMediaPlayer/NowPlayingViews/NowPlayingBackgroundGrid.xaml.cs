// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.WinUI;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.NowPlayingViews
{
    public sealed partial class NowPlayingBackgroundGrid : BaseUserControl
    {
        readonly DispatcherQueueTimer timer = null;

        public NowPlayingBackgroundGrid()
        {
            InitializeComponent();
            SizeChanged += NowPlayingBackgroundGrid_SizeChanged;
            _ = LoadBackgroundImages();
            timer = DispatcherQueue.CreateTimer();
            timer.Interval = TimeSpan.FromMinutes(5);
            timer.Tick += Timer_Tick;
            timer.IsRepeating = true;
            timer.Start();
        }

        private void Timer_Tick(DispatcherQueueTimer sender, object args)
        {
            _ = LoadBackgroundImages();
        }

        private async Task LoadBackgroundImages()
        {
            var imagesLoadTask = Task.Run<List<string>>(async () =>
            {
                var albumArt = await LocalFolders.GetAlbumArtFolder();
                var files = albumArt.GetFiles();
                files.Randomize();
                return files.Select(x => x.FullName).ToList();
            });

            var images = TopLeft.FindVisualChildrenDeep<Image>();
            images = images.Union(BottomRight.FindVisualChildrenDeep<Image>());
            images = images.Union(TopRight.FindVisualChildrenDeep<Image>());
            images = images.Union(BottomLeft.FindVisualChildrenDeep<Image>());
            images = images.ToList();//we need the count.

            var imageSources = await imagesLoadTask;

            int i = 0;
            foreach (var img in images)
            {
                if (imageSources.Count > 0)
                {
                    if (i >= imageSources.Count) i = 0;
                    await DispatcherQueue.EnqueueAsync(() =>
                    {
                        img.Source = new BitmapImage(new Uri(imageSources[i]));
                    });
                    i++;
                }
            }
        }

        private void NowPlayingBackgroundGrid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            //LayoutRoot.DesiredWidth = e.NewSize.Width / 2;
            //LayoutRoot.ItemHeight = e.NewSize.Height / 2;
        }
    }
}
