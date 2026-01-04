using FFmpegInteropX;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MayazucMediaPlayer.NowPlayingViews
{
    public partial class MediaThumbnailPreviewData : ObservableObject
    {
        string _title;

        public string Title
        {
            get => _title;
            private set
            {
                if (_title == value) return;

                _title = value;
                NotifyPropertyChanged(nameof(Title));
            }
        }

        public TimeSpan SeekPosition
        {
            get;
            private set;
        }

        BitmapImage _image;
        public BitmapImage MediaThumbnailData
        {
            get => _image;
            private set
            {
                if (_image == value) return;

                _image = value;
                NotifyPropertyChanged(nameof(MediaThumbnailData));
            }
        }

        public string ToolTip
        {
            get
            {
                return $"Seek to {SeekPosition.ToString()}";
            }
        }

        public MediaThumbnailPreviewData(TimeSpan position, string title)
        {
            SeekPosition = position;
            if (!string.IsNullOrWhiteSpace(title))
            {
                Title = title;
            }
            else
            {
                Title = position.ToString(@"mm\:ss");
            }
        }

        public MediaThumbnailPreviewData(TimeSpan position, BitmapImage frame)
        {
            SeekPosition = position;
            MediaThumbnailData = frame;
            Title = position.ToString(@"mm\:ss");
        }

        public async Task LoadPreviewAsync(FrameGrabber mss)
        {
            MediaThumbnailData = await GetImageAsync(mss, SeekPosition);
        }


        public static async Task<MediaThumbnailPreviewData> CreateFromFileAsync(FrameGrabber mss, TimeSpan position)
        {
            var img = await GetImageAsync(mss, position);
            return new MediaThumbnailPreviewData(position, img);
        }

        public static IReadOnlyCollection<MediaThumbnailPreviewData> ProcessMediaPlaybackItem(MediaPlaybackItemUIInformation item)
        {
            List<MediaThumbnailPreviewData> returnValue = new List<MediaThumbnailPreviewData>();
            var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
            if (item.Chapters.Any())
            {
                foreach (var chapter in item.Chapters)
                {
                    returnValue.Add(new MediaThumbnailPreviewData(chapter.StartTime, chapter.Title));
                }
            }
            else if (item.Duration.Ticks > 0)
            {
                var thumbnailDataFrequency = (double)(item.Duration.TotalMinutes / 40);
                var startMinutes = 0d;
                for (int i = 0; i < 40; i++)
                {
                    try
                    {
                        returnValue.Add(new MediaThumbnailPreviewData(TimeSpan.FromMinutes(startMinutes), string.Empty));
                        startMinutes += thumbnailDataFrequency;
                    }
                    catch { }
                }
            }

            return returnValue.AsReadOnly();
        }

        public static async Task<BitmapImage> GetImageAsync(FrameGrabber mss, TimeSpan position)
        {
            var videoThumbanilData = await mss.ExtractVideoFrameAsync(position, false);
            using (var ms = new InMemoryRandomAccessStream())
            {
                await videoThumbanilData.EncodeAsPngAsync(ms);
                ms.Seek(0);
                BitmapImage img = new BitmapImage();
                await img.SetSourceAsync(ms);

                var origHeight = videoThumbanilData.PixelHeight;
                var origWidth = videoThumbanilData.PixelWidth;
                var ratioX = 250 / (float)origWidth;
                var ratioY = 250 / (float)origHeight;
                var ratio = Math.Min(ratioX, ratioY);
                var newHeight = (int)(origHeight * ratio);
                var newWidth = (int)(origWidth * ratio);

                img.DecodePixelWidth = newWidth;
                img.DecodePixelHeight = newHeight;

                return img;
            }
        }

        public static async Task<IReadOnlyCollection<MediaThumbnailPreviewData>> ProcessFileAsync(StorageFile file)
        {
            using (var stream = await file.OpenReadAsync())
            {
                List<MediaThumbnailPreviewData> returnValue = new List<MediaThumbnailPreviewData>();
                var mss = await FrameGrabber.CreateFromStreamAsync(stream);
                var duration = mss.Duration;

                var thumbnailDataFrequency = (double)(duration.TotalMinutes / 40);
                var startMinutes = 0d;
                for (int i = 0; i < 40; i++)
                {
                    try
                    {
                        returnValue.Add(await CreateFromFileAsync(mss, TimeSpan.FromMinutes(startMinutes)));
                        startMinutes += thumbnailDataFrequency;
                    }
                    catch { }
                }

                return returnValue;

            }
        }

        public static async Task<StorageFile?> SaveVideoFrameAsync(TimeSpan position, StorageFolder folder, IRandomAccessStream sourceStream, string name)
        {
            var mss = await FrameGrabber.CreateFromStreamAsync(sourceStream);
            var file = await folder.CreateFileAsync(name + ".png", CreationCollisionOption.GenerateUniqueName);

            var videoThumbanilData = await mss.ExtractVideoFrameAsync(position, true, 0);


            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await videoThumbanilData.EncodeAsPngAsync(stream);
            }


            return file;
        }
    }
}