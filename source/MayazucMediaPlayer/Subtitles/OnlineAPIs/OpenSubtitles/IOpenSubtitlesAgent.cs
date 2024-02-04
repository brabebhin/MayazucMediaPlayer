using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Users;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles
{
    public interface IOpenSubtitlesAgent : ILoginProvider, IDisposable
    {
        Task<FileInfo?> AutoDownloadSubtitleAsync(SubtitleRequest request);
        Task<FileInfo?> DownloadSubtitleAsync(IOSDBSubtitleInfo info);
        Task<ReadOnlyCollection<IOSDBSubtitleInfo>> SearchSubtitlesAsync(SubtitleRequest request);
    }
}