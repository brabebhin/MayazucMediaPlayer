using Microsoft.UI.Xaml.Controls;

namespace MayazucMediaPlayer.MediaPlayback
{
    public class MediaPlayerCompactOverlayEventArgs
    {
        public bool IsNormalOverlayRequested
        {
            get;
            private set;
        }

        public MediaPlayerElement MediaPlayerElementInstance
        {
            get;
            private set;
        }

        public MediaPlayerCompactOverlayEventArgs(bool compact, MediaPlayerElement element)
        {
            IsNormalOverlayRequested = compact;
            MediaPlayerElementInstance = element;
        }
    }
}
