using Windows.System.Display;

namespace MayazucMediaPlayer.Services
{
    public sealed class ScreenSessionManager
    {
        static ScreenSessionManager singleton = null;
        static readonly object singletonLock = new object();


        private readonly DisplayRequest ScreenDisplayRequest = new DisplayRequest();

        private ScreenSessionManager()
        {

        }

        public void RequestKeepScreenOn()
        {
            lock (ScreenDisplayRequest)
            {
                ScreenDisplayRequest.RequestActive();
                LockScreenDisables++;
            }
        }

        public void RequestScreenOff()
        {
            lock (ScreenDisplayRequest)
            {
                if (LockScreenDisables > 0)
                {
                    ScreenDisplayRequest.RequestRelease();
                    LockScreenDisables--;
                }
            }
        }

        public void DeactivateAllLockScreenRequests()
        {
            while (LockScreenDisables > 0)
            {
                RequestScreenOff();
            }
        }


        public int LockScreenDisables
        {
            get;
            private set;
        }

        public static ScreenSessionManager Current
        {
            get
            {
                lock (singletonLock)
                {
                    if (singleton == null)
                    {
                        singleton = new ScreenSessionManager();
                    }
                    return singleton;
                }
            }
        }

        public bool IsLockScreenDisabled
        {
            get;
            private set;
        }
    }
}
