using FFmpegInteropX;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.MediaPlayback
{
    public interface IFFmpegInteropMediaSourceProvider<T>
    {
        /// <summary>
        /// This method must return a ffmpegInteropMSS object with an initialized media playback item
        /// </summary>
        /// <returns></returns>
        Task<FFmpegMediaSource> GetFFmpegInteropMssAsync(T source, bool createPlaybackItem);
    }
}
