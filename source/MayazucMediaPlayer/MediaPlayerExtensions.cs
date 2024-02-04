using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Playback;

namespace MayazucMediaPlayer
{
    public static class MediaPlayerExtensions
    {
        public static async Task SeekAsync(this MediaPlayer CurrentPlayer, TimeSpan position, TimeSpan seekTimeout)
        {
            TaskCompletionSource<bool> SeekCompletedSignal = new TaskCompletionSource<bool>();

            TypedEventHandler<MediaPlaybackSession, object> seekHandler = (s, e) =>
            {
                SeekCompletedSignal.SetResult(true);
            };

            TypedEventHandler<MediaPlayer, object> mediaChangedOrEndedHandler = (s, e) =>
            {
                SeekCompletedSignal.SetResult(false);
            };

            TypedEventHandler<MediaPlayer, MediaPlayerFailedEventArgs> mediaFailed = (s, e) =>
            {
                SeekCompletedSignal.SetResult(false);
            };

            CurrentPlayer.MediaOpened += mediaChangedOrEndedHandler;
            CurrentPlayer.MediaEnded += mediaChangedOrEndedHandler;
            CurrentPlayer.MediaFailed += mediaFailed;
            CurrentPlayer.PlaybackSession.SeekCompleted += seekHandler;

            CurrentPlayer.PlaybackSession.Position = position;
            await Task.WhenAny(SeekCompletedSignal.Task, Task.Delay(seekTimeout));

            CurrentPlayer.PlaybackSession.SeekCompleted -= seekHandler;
            CurrentPlayer.MediaOpened -= mediaChangedOrEndedHandler;
            CurrentPlayer.MediaEnded -= mediaChangedOrEndedHandler;
            CurrentPlayer.MediaFailed -= mediaFailed;
        }
    }
}
