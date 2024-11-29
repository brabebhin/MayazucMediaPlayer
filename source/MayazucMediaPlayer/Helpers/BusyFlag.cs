using System;

namespace MayazucMediaPlayer.Helpers
{
    public sealed partial class BusyFlag : IDisposable
    {
        public event EventHandler<bool> StateChanged;
        public bool IsBusy
        {
            get;
            private set;
        }

        public BusyFlag SetBusy()
        {
            IsBusy = true;
            StateChanged?.Invoke(this, IsBusy);
            return this;
        }

        public void Dispose()
        {
            IsBusy = false;
            StateChanged?.Invoke(this, IsBusy);
        }
    }
}
