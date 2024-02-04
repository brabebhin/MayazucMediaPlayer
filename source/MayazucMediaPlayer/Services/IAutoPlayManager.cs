using FluentResults;
using MayazucMediaPlayer.Services.MediaSources;
using System.IO;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    public interface IAutoPlayManager
    {
        DirectoryInfo CurrentAutoPlayFolder { get; }
        bool IsAutoPlayAvailable { get; }
        FileInfo LoadedFile { get; }
        Task<bool> AddNextFileToNowPlaying(FileInfo file);
        Task LoadAutoPlayAsync(FileInfo file);
        void Reset();
        Task<bool> IsPathLoaded(string path, bool autoMusic, bool autoVideo);
        Task<int> CountAsync();
        Task<Result<IMediaPlayerItemSource>> GetNextFile(FileInfo file);
    }
}