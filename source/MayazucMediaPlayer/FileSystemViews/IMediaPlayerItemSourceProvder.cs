using FluentResults;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.Services.MediaSources;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.FileSystemViews
{
    /// <summary>
    /// Acts as a source for one or more IMediaPlayerItemSource
    /// 
    /// Wraps files, internet streams and playlists
    /// </summary>
    public interface IMediaPlayerItemSourceProvder
    {
        string DisplayName { get; }
        string ImageUri { get; }
        Guid ItemID { get; }
        EmbeddedMetadataResult Metadata { get; }
        string Path { get; }
        bool SupportsMetadata { get; }

        event EventHandler<FileInfo> ImageFileChanged;
        event EventHandler<EmbeddedMetadataResult> MetadataChanged;

        Task<Result<ReadOnlyCollection<IMediaPlayerItemSource>>> GetMediaDataSourcesAsync();

        int ExpectedPlaybackIndex { get; set; }
        bool IsInPlayback { get; set; }
    }
}