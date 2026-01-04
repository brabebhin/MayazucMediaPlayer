using FFmpegInteropX;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.NowPlayingViews;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Media.PlayTo;
using Windows.Storage.Streams;

namespace MayazucMediaPlayer.Services.MediaSources
{
    /// <summary>
    /// Acts as a source for a single MediaPlaybackItem or FFmpegMediaSource
    /// 
    /// May be based on a file, an URI on a server or an incoming DLNA/PlayTo stream
    /// </summary>
    public interface IMediaPlayerItemSource
    {
        Task<FFmpegMediaSource> GetFFmpegMediaSourceAsync(ulong windowId);

        Task<EmbeddedMetadata> GetMetadataAsync();

        string Title { get; }

        Guid ID { get; }

        event EventHandler<EmbeddedMetadata> MetadataChanged;

        /// <summary>
        /// True if this source is persistent (normally files or internet streams). False for DLNA sources
        /// </summary>
        bool Persistent { get; }

        int ExpectedPlaybackIndex
        {
            get;
            set;
        }
        string MediaPath { get; }
        bool HasExternalSource { get; }

        bool IsAvailable();
        Task<RandomAccessStreamReference> GetThumbnailStreamAsync();
        Task<FrameGrabber> GetFrameGrabberAsync();
        Task<MediaThumbnailPreviewData> GetThumbnailAtPositionAsync(TimeSpan timeSpan);
        bool SupportsSubtitles { get; }
        bool SupportsChapters { get; }

        Task<SubtitleRequest> PrepareSubtitles();
    }

    public static class IMediaPlayerItemSourceFactory
    {
        public static IMediaPlayerItemSource Get(string path)
        {
            if (Uri.TryCreate(path, UriKind.Absolute, out var streamUri))
            {
                if (SupportedFileFormats.IsSupportedStreamingProtocol(streamUri.Scheme))
                {
                    return Get(streamUri);
                }
            }
            return new FileMediaPlayerItemSource(new PickedFileItem(new FileInfo(path)));
        }

        public static IMediaPlayerItemSource Get(Uri uri)
        {
            return new InternetStreamMediaPlayerItemSource(title: uri.Host, streamingAddress: uri.OriginalString, metadata: new EmbeddedMetadata(title: uri.Host, performer: string.Empty));
        }

        public static IMediaPlayerItemSource Get(PickedFileItem item)
        {
            return new FileMediaPlayerItemSource(item);
        }

        public static IMediaPlayerItemSource Get(SourceChangeRequestedEventArgs source)
        {
            return new PlayToMediaPlayerItemSource(source);
        }

        public static IMediaPlayerItemSource Get(FileInfo file)
        {
            return Get(new PickedFileItem(file));
        }
    }

    public class SystemMediaTransportControlsDisplayAdapterUIAdapter
    {
        readonly SystemMediaTransportControlsDisplayUpdater DisplayAdapter;
        readonly IMediaPlayerItemSource MediaPlayerItemSource;
        readonly MediaPlaybackItem MediaPlaybackItem;

        public SystemMediaTransportControlsDisplayAdapterUIAdapter(SystemMediaTransportControlsDisplayUpdater displayAdapter, IMediaPlayerItemSource mediaPlayerItemSource, MediaPlaybackItem mediaPlaybackItem)
        {
            DisplayAdapter = displayAdapter;
            MediaPlayerItemSource = mediaPlayerItemSource;
            MediaPlaybackItem = mediaPlaybackItem;
            mediaPlayerItemSource.MetadataChanged += MediaPlayerItemSource_MetadataChanged;
        }

        private void MediaPlayerItemSource_MetadataChanged(object? sender, EmbeddedMetadata e)
        {


        }
    }
}
