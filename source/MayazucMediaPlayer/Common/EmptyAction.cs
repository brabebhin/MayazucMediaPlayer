using System;

namespace MayazucMediaPlayer.Common
{
    public static class EmptyCallbacks
    {
        public static Action<object> Action
        {
            get
            {
                return (value) => { };
            }
        }

        public static Func<object> Func
        {
            get
            {
                return () => { return new object(); };
            }
        }
    }
}
