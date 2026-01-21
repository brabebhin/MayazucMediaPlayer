using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MayazucMediaPlayer.Services
{
    public partial class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void NotifyPropertyChanged(String propertyName = "")
        {
            AppState.Current.Dispatcher?.EnqueueAsync(() =>
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        protected virtual void NotifyPropertiesChanging(params string[] propertyNames)
        {
            AppState.Current.Dispatcher?.EnqueueAsync(() =>
            {
                foreach (var prop in propertyNames)
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            });
        }

        protected bool SetProperty<T>(ref T field, T newValue, string? propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            field = newValue;

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            return true;
        }
    }
}
