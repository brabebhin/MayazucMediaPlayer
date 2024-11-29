using CommunityToolkit.WinUI;
using MayazucMediaPlayer.Converters;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;

namespace MayazucMediaPlayer.Controls
{
    public partial class BaseUserControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        private bool disposedValue;

        public IServiceProvider ServiceProvider
        {
            get
            {
                return AppState.Current.Services;
            }
        }

        public BaseUserControl()
        {
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

        protected virtual void OnDispose(bool disposing)
        {

        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                OnDispose(disposing);
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~BaseUserControl()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ConverterLocator Converters { get; private set; } = new ConverterLocator();

        public bool DirectBindBack(bool value) => value;

    }
}
