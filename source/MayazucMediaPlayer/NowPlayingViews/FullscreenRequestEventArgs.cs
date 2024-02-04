using System;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.NowPlayingViews
{
    public class FullscreenRequestEventArgs
    {
        public Func<Task> BeforeCallback { get; set; }

        public Func<Task> AfterCallback { get; set; }

        public bool IsGoingFullScreen { get; private set; }

        public FullscreenRequestEventArgs(bool isGoingFullScreen)
        {
            IsGoingFullScreen = isGoingFullScreen;
        }
    }
}