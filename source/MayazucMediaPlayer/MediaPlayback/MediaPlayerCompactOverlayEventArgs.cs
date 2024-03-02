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

        public UserControl MediaPlayerElementInstance
        {
            get;
            private set;
        }

        public MediaPlayerCompactOverlayEventArgs(bool compact, UserControl element)
        {
            IsNormalOverlayRequested = compact;
            MediaPlayerElementInstance = element;
        }
    }
}
