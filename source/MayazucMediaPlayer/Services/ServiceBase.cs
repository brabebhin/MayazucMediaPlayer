using MayazucMediaPlayer.Navigation;
using Microsoft.UI.Dispatching;
using System;

namespace MayazucMediaPlayer.Services
{
    public abstract class ServiceBase : ObservableObject
    {
        public DispatcherQueue Dispatcher
        {
            get;
            private set;
        }

        public ServiceBase(DispatcherQueue dispatcher)
        {
            Dispatcher = dispatcher;
        }

        protected override void NotifyPropertyChanged(string propertyName = "")
        {
            _ = Dispatcher.EnqueueAsync(() =>
               {
                   base.NotifyPropertyChanged(propertyName);
               });
        }
    }
}
