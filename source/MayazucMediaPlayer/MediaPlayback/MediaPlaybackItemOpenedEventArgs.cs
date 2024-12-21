using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback
{
    public class NewMediaPlaybackItemOpenedEventArgs
    {
        public MediaPlaybackItem PlaybackItem
        {
            get;
            private set;
        }

        public MediaPlaybackItem? PreviousItem
        {
            get;
            private set;
        }

        public PlaybackItemExtraData ExtraData
        {
            get;
            private set;
        }

        public string MediaPath
        {
            get
            {
                return ExtraData?.MediaPlayerItemSource?.MediaPath ?? string.Empty;
            }
        }

        public NewMediaPlaybackItemOpenedEventArgs(MediaPlaybackItem? previous, MediaPlaybackItem itm, PlaybackItemExtraData data)
        {
            ExtraData = data;
            PlaybackItem = itm;
            PreviousItem = previous;
        }
    }
}
