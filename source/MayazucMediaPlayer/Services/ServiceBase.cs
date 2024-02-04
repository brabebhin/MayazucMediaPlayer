﻿using CommunityToolkit.WinUI;
using MayazucMediaPlayer.Navigation;
using Microsoft.UI.Dispatching;
using System;

namespace MayazucMediaPlayer.Services
{
    public abstract class ServiceBase : ObservableObject
    {
        public event EventHandler<NavigationRequestEventArgs> NavigationRequest;

        public DispatcherQueue Dispatcher
        {
            get;
            private set;
        }

        public ServiceBase(DispatcherQueue dispatcher)
        {
            Dispatcher = dispatcher;
        }

        public void SubmitNavigationEvent(Type target, object param)
        {
            NavigationRequest?.Invoke(this, new Navigation.NavigationRequestEventArgs(target, param));
        }

        protected override void NotifyPropertyChanged(string propertyName = "")
        {
            _ = Dispatcher.EnqueueAsync(() =>
               {
                   base.NotifyPropertyChanged(propertyName);
               });
        }
    }

    public abstract class MainViewModelBase : ServiceBase
    {
        public PlaybackSequenceService PlaybackServiceInstance
        {
            get;
            protected set;
        }

        public MainViewModelBase(DispatcherQueue disp) : base(disp)
        {

        }

        public MainViewModelBase(DispatcherQueue disp, PlaybackSequenceService m) : base(disp)
        {
            PlaybackServiceInstance = m;
        }
    }
}
