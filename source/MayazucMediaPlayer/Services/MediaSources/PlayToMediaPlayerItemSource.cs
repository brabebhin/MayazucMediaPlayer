using FFmpegInteropX;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.MediaPlayback.PlayTo;
using MayazucMediaPlayer.NowPlayingViews;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.PlayTo;
using Windows.Storage.Streams;

namespace MayazucMediaPlayer.Services.MediaSources
{
    public class PlayToMediaPlayerItemSource : IMediaPlayerItemSource
    {
        public string Title { get; private set; }

        public Guid ID { get; private set; }

        public SourceChangeRequestedEventArgs PlayToSourceEvent { get; private set; }

        private EmbeddedMetadataResult Metadata { get; set; }

        public event EventHandler<EmbeddedMetadataResult> MetadataChanged;

        public async Task<FFmpegMediaSource> GetFFmpegMediaSourceAsync()
        {
            try
            {
                return await ItemBuilder.GetFFmpegInteropMssAsync(PlayToSourceEvent, false);
            }
            catch { }

            return null;
        }

        public IFFmpegInteropMediaSourceProvider<SourceChangeRequestedEventArgs> ItemBuilder { get; private set; }

        public bool Persistent => false;

        public int ExpectedPlaybackIndex { get; set; }

        public string MediaPath => Title;

        public bool HasExternalSource => true;

        public bool SupportsSubtitles => false;

        public bool SupportsChapters => false;

        public Task<EmbeddedMetadataResult> GetMetadataAsync()
        {
            return Task.FromResult(Metadata);
        }

        public bool IsAvailable()
        {
            return true;
        }

        public async Task<RandomAccessStreamReference> GetThumbnailStreamAsync()
        {
            using var stream = await PlayToSourceEvent.Thumbnail.OpenReadAsync();
            using var netStream = stream.AsStreamForRead();
            var imras = await netStream.AsInMemoryRandomAccessStream();
            return RandomAccessStreamReference.CreateFromStream(imras);
        }

        public Task<FrameGrabber> GetFrameGrabberAsync()
        {
            return Task.FromResult<FrameGrabber>(null);
        }

        public string GetTitleAndArtist()
        {
            return Title;
        }

        public Task<MediaThumbnailPreviewData> GetThumbnailAtPositionAsync(TimeSpan timeSpan)
        {
            return Task.FromResult(new MediaThumbnailPreviewData(timeSpan, new BitmapImage(new Uri(AssetsPaths.PlaceholderAlbumArt))));
        }

        public Task<SubtitleRequest> PrepareSubtitles()
        {
            return Task.FromResult(new SubtitleRequest(false, "", ""));
        }

        public PlayToMediaPlayerItemSource(SourceChangeRequestedEventArgs playToSourceEvent)
        {
            PlayToSourceEvent = playToSourceEvent;
            ItemBuilder = new PlayToMediaPlaybackItemBuilder();
            Title = $"cast://{PlayToSourceEvent.Title}";
            Metadata = new EmbeddedMetadataResult(PlayToSourceEvent.Album, PlayToSourceEvent.Author, PlayToSourceEvent.Genre, PlayToSourceEvent.Title);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
