using FFmpegInteropX;
using System;
using System.Threading.Tasks;
using Windows.Media.PlayTo;

namespace MayazucMediaPlayer.MediaPlayback.PlayTo
{
    class PlayToMediaPlaybackItemBuilder : IFFmpegInteropMediaSourceProvider<SourceChangeRequestedEventArgs>
    {
        public async Task<FFmpegMediaSource> GetFFmpegInteropMssAsync(SourceChangeRequestedEventArgs source, bool createPlaybackItem, ulong windowId)
        {
            var ffmpegInteropTask = FFmpegMediaSource.CreateFromStreamAsync(source.Stream, new MediaSourceConfig(), windowId);

            var ffmpegInteropmss = await ffmpegInteropTask;
            if (createPlaybackItem)
            {
                ffmpegInteropmss.CreateMediaPlaybackItem();
            }
            return ffmpegInteropmss;
        }
    }
}
