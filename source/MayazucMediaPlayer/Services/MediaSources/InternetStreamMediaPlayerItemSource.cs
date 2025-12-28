using FFmpegInteropX;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.NowPlayingViews;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MayazucMediaPlayer.Services.MediaSources
{
    public class InternetStreamMediaPlayerItemSource : IMediaPlayerItemSource
    {
        public string Title { get; private set; }

        public Guid ID { get; private set; }

        public string StreamingAddress { get; private set; }

        private EmbeddedMetadata Metadata { get; set; }

        public bool Persistent => true;

        public event EventHandler<EmbeddedMetadata> MetadataChanged;

        public async Task<FFmpegMediaSource> GetFFmpegMediaSourceAsync(ulong windowId)
        {
            try
            {
                var config = FFmpegInteropXExtensions.GetFFmpegUserConfigs();
                var ffmpegSource = await FFmpegMediaSource.CreateFromUriAsync(StreamingAddress, config, windowId);

                return ffmpegSource;
            }
            catch { }

            return null;
        }

        public Task<EmbeddedMetadata> GetMetadataAsync()
        {
            return Task.FromResult(Metadata);
        }

        public bool IsAvailable()
        {
            return true;
        }

        public Task<RandomAccessStreamReference> GetThumbnailStreamAsync()
        {
            return Task.FromResult(RandomAccessStreamReference.CreateFromUri(new Uri(FontIconPaths.PlaceholderAlbumArt, UriKind.Absolute)));
        }

        public Task<FrameGrabber> GetFrameGrabberAsync()
        {
            return Task.FromResult<FrameGrabber>(null);
        }

        public string GetTitleAndArtist()
        {
            return $"{Title} at {StreamingAddress}";
        }

        public Task<MediaThumbnailPreviewData> GetThumbnailAtPositionAsync(TimeSpan timeSpan)
        {
            return Task.FromResult(new MediaThumbnailPreviewData(timeSpan, new BitmapImage(new Uri(AssetsPaths.PlaceholderAlbumArt))));
        }

        public Task<SubtitleRequest> PrepareSubtitles()
        {
            return Task.FromResult(new SubtitleRequest(false, string.Empty, string.Empty));
        }

        public InternetStreamMediaPlayerItemSource(string title, string streamingAddress, EmbeddedMetadata metadata)
        {
            Title = title;
            StreamingAddress = streamingAddress;
            Metadata = metadata;
        }

        public InternetStreamMediaPlayerItemSource(string title, string streamingAddress)
        {
            Title = title;
            StreamingAddress = streamingAddress;
            Metadata = EmbeddedMetadataResolver.GetDefaultMetadataForFile(streamingAddress);
        }

        public int ExpectedPlaybackIndex
        {
            get;
            set;
        }

        public string MediaPath => StreamingAddress;

        public bool HasExternalSource => true;

        public bool SupportsSubtitles => false;

        public bool SupportsChapters => false;

        public override string ToString()
        {
            return Title;
        }
    }
}
