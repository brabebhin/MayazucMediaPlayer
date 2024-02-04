using System;

namespace MayazucMediaPlayer.MediaPlayback.PlayTo
{
    public class PlayToRecieverSourceReadyEventArgs
    {
        public IMediaPlaybackListAdapter PlaybackSource
        {
            get;
            private set;
        }

        public PlayToRecieverSourceReadyEventArgs(IMediaPlaybackListAdapter playbackSource)
        {
            PlaybackSource = playbackSource ?? throw new ArgumentNullException(nameof(playbackSource));
        }
    }
}
