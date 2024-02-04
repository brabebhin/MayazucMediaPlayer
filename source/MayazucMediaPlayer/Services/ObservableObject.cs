using CommunityToolkit.WinUI;
using System;
using System.ComponentModel;
using System.Windows.Markup;

namespace MayazucMediaPlayer.Services
{
    public class ObservableObject : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        protected virtual void NotifyPropertyChanged(String propertyName = "")
        {
            AppState.Current.Dispatcher?.EnqueueAsync(() =>
            {
                base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            });
        }
    }
}
