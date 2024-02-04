using System;

namespace MayazucMediaPlayer.Navigation
{
    public class NavigationRequestEventArgs : EventArgs
    {
        public object Parameter
        {
            get; private set;
        }

        public Type PageType
        {
            get; private set;
        }

        public NavigationRequestEventArgs(Type target, object param)
        {
            PageType = target;
            Parameter = param;
        }
    }
}
