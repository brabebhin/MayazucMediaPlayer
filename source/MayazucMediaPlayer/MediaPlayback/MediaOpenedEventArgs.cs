using System;

namespace MayazucMediaPlayer.MediaPlayback
{
    public class MediaOpenedEventArgs : EventArgs
    {
        public MediaOpenedEventReason Reason
        {
            get;
            private set;
        }

        public NewMediaPlaybackItemOpenedEventArgs EventData
        {
            get;
            private set;
        }

        public MediaOpenedEventArgs(MediaOpenedEventReason reason, NewMediaPlaybackItemOpenedEventArgs e)
        {
            Reason = reason;
            EventData = e;
        }
    }


    public enum MediaOpenedEventReason
    {
        MediaPlayerObjectRequested,
        MediaPlaybackListItemChanged,
        CueLoad
    }
}
