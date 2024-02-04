using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;

namespace MayazucMediaPlayer.Controls
{
    public class BaseUserControl : UserControl, INotifyPropertyChanged
    {
        public DispatcherQueue DispatcherQueue { get; private set; }

        public IServiceProvider ServiceProvider
        {
            get
            {
                return AppState.Current.Services;
            }
        }

        public BaseUserControl()
        {
            DispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void NotifyPropertyChanged(String propertyName = "")
        {
            AppState.Current.Dispatcher?.EnqueueAsync(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        protected virtual void NotifyPropertyChanged(object? sender, String propertyName = "")
        {
            AppState.Current.Dispatcher?.EnqueueAsync(() =>
            {
                PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
            });
        }
    }
}
