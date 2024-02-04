using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback
{
    public class MediaPlaybackItemResult
    {
        public bool FileIsOpen
        {
            get;
            private set;
        }

        public MediaPlaybackItem PlaybackItem
        {
            get;
            private set;
        }

        public MediaPlaybackItemResult(bool opened, MediaPlaybackItem mss)
        {
            FileIsOpen = opened;
            PlaybackItem = mss;
        }
    }
}
