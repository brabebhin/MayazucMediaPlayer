using FFmpegInteropX;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.NowPlayingViews;
using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MayazucMediaPlayer.Services.MediaSources
{
    /// <summary>
    /// A media player item source backed by a file
    /// </summary>
    public class FileMediaPlayerItemSource : IMediaPlayerItemSource
    {
        public string Title { get; private set; }

        public Guid ID { get; private set; }

        public event EventHandler<EmbeddedMetadataResult> MetadataChanged;

        public async Task<FFmpegMediaSource> GetFFmpegMediaSourceAsync()
        {
            PickedFile.File.Refresh();
            if (PickedFile.File.Exists)
            {
                try
                {
                    var stream = await PickedFile.File.OpenReadAsync();
                    var config = MediaHelperExtensions.GetFFmpegUserConfigs();
                    var ffmpegSource = await FFmpegMediaSource.CreateFromStreamAsync(stream, config);

                    return ffmpegSource;
                }
                catch { }
            }

            return null;
        }

        public Task<EmbeddedMetadataResult> GetMetadataAsync()
        {
            return MetadataTask;
        }

        public PickedFileItem PickedFile { get; private set; }

        private Task<EmbeddedMetadataResult> MetadataTask { get; set; }

        public bool Persistent => true;

        public FileMediaPlayerItemSource(PickedFileItem pickedItem)
        {
            ID = Guid.NewGuid();
            PickedFile = pickedItem;
            MetadataTask = Task.FromResult(pickedItem.Metadata);
            pickedItem.MetadataChanged += PickedItem_MetadataChanged;
            Title = pickedItem.File.Name;
        }

        private void PickedItem_MetadataChanged(object? sender, EmbeddedMetadataResult e)
        {
            MetadataTask = Task.FromResult(e);
            MetadataChanged?.Invoke(this, e);
        }

        public bool IsAvailable()
        {
            PickedFile.File.Refresh();
            return PickedFile.File.Exists;
        }

        public async Task<RandomAccessStreamReference> GetThumbnailStreamAsync()
        {
            var metadata = await GetMetadataAsync();

            if (metadata.HasSavedThumbnailFile())
            {
                var ImageFile = new FileInfo(metadata.SavedThumbnailFile);

                using var stream = ImageFile.OpenRead();
                return await Task.FromResult<RandomAccessStreamReference>(RandomAccessStreamReference.CreateFromStream(await stream.AsInMemoryRandomAccessStream()));
            }
            return await Task.FromResult<RandomAccessStreamReference>(RandomAccessStreamReference.CreateFromUri(new Uri(FontIconPaths.PlaceholderAlbumArt)));

        }

        public async Task<FrameGrabber> GetFrameGrabberAsync()
        {
            PickedFile.File.Refresh();
            if (PickedFile.File.Exists)
            {
                try
                {
                    var grabber = await FrameGrabber.CreateFromStreamAsync(await PickedFile.File.OpenReadAsync());
                    return grabber;
                }
                catch { }
            }
            return null;
        }

        public string GetTitleAndArtist()
        {
            return "";
        }

        public async Task<MediaThumbnailPreviewData> GetThumbnailAtPositionAsync(TimeSpan timeSpan)
        {
            try
            {
                if (SupportsChapters)
                {
                    var file = PickedFile.File;
                    if (file != null)
                    {
                        FrameGrabber mss = await GetFrameGrabberAsync();
                        if (mss != null)
                        {
                            var thumbData = await MediaThumbnailPreviewData.CreateFromFileAsync(mss, timeSpan);
                            return thumbData;
                        }
                    }
                }
                else
                {
                    var metadata = await GetMetadataAsync();

                    if (metadata.HasSavedThumbnailFile())
                    {
                        BitmapImage img = new BitmapImage(new Uri(metadata.SavedThumbnailFile));
                        return new MediaThumbnailPreviewData(timeSpan, img);
                    }
                }
            }
            catch (Exception) { }


            BitmapImage fallbackAlbumArt = new BitmapImage(new Uri(AssetsPaths.PlaceholderAlbumArt));
            return new MediaThumbnailPreviewData(timeSpan, fallbackAlbumArt);
        }

        public async Task<SubtitleRequest> PrepareSubtitles()
        {
            try
            {
                var hash = await OSDBExtensions.ComputeOSDBHash(PickedFile.File);
                return new SubtitleRequest(true, MediaPath, hash);
            }
            catch
            {
                return new SubtitleRequest(false, string.Empty, string.Empty);
            }
        }

        public int ExpectedPlaybackIndex
        {
            get;
            set;
        }

        public string MediaPath => PickedFile.File.FullName;

        public bool HasExternalSource => false;

        public bool SupportsSubtitles => true;

        public bool SupportsChapters { get => SupportedFileFormats.IsVideoFile(MediaPath); }

        public override string ToString()
        {
            return Title;
        }
    }
}
