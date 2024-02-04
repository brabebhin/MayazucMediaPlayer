using FFmpegInteropX;
using System;
using System.Threading.Tasks;
using Windows.Media.PlayTo;

namespace MayazucMediaPlayer.MediaPlayback.PlayTo
{
    class PlayToMediaPlaybackItemBuilder : IFFmpegInteropMediaSourceProvider<SourceChangeRequestedEventArgs>
    {
        public async Task<FFmpegMediaSource> GetFFmpegInteropMssAsync(SourceChangeRequestedEventArgs source, bool createPlaybackItem)
        {
            var ffmpegInteropTask = FFmpegMediaSource.CreateFromStreamAsync(source.Stream);

            var ffmpegInteropmss = await ffmpegInteropTask;
            if (createPlaybackItem)
            {
                ffmpegInteropmss.CreateMediaPlaybackItem();
            }
            return ffmpegInteropmss;
        }
    }
}
