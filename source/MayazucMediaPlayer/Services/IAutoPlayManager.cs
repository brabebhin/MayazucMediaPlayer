using FluentResults;
using MayazucMediaPlayer.Services.MediaSources;
using System.IO;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    public interface IAutoPlayManager
    {
        Task LoadAutoPlayAsync(FileInfo file);
        Task<int> CountAsync();
        Task<Result<IMediaPlayerItemSource>> GetNextFile(FileInfo file);
    }
}