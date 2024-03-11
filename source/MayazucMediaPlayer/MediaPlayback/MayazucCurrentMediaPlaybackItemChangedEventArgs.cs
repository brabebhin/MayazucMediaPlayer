using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback
{
    public class MayazucCurrentMediaPlaybackItemChangedEventArgs
    {
        //
        // Summary:
        //     Gets the new current MediaPlaybackItem.
        //
        // Returns:
        //     The new current MediaPlaybackItem.
        public MediaPlaybackItem NewItem { get; private set; }

        //
        // Summary:
        //     Gets the reason why the current MediaPlaybackItem in a MediaPlaybackList changed,
        //     such as if the previous item completed playback successfully or if there was
        //     an error playing back the previous item.
        //
        // Returns:
        //     The reason why the current MediaPlaybackItem in a MediaPlaybackList changed.
        public MediaPlaybackItemChangedReason Reason { get; private set; }

        public IMediaPlaybackListAdapter MediaPlaybackListAdapter { get; private set; }

        public MayazucCurrentMediaPlaybackItemChangedEventArgs(MediaPlaybackItem newItem, MediaPlaybackItemChangedReason reason, IMediaPlaybackListAdapter mediaPlaybackListAdapter)
        {
            NewItem = newItem;
            Reason = reason;
            MediaPlaybackListAdapter = mediaPlaybackListAdapter;
        }
    }
}
