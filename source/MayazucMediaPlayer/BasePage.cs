using CommunityToolkit.WinUI;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace MayazucMediaPlayer
{
    public abstract class BasePage : Page, INotifyPropertyChanged, IDisposable
    {
        public event EventHandler<NavigationRequestEventArgs> ExternalNavigationRequest;
        public abstract string Title { get; }
        public IBackgroundPlayer BackgroundMediaPlayerInstance
        {

            get
            {
                return ServiceProvider.GetService<IBackgroundPlayer>();
            }
        }

        private bool disposedValue;

        protected ApplicationDataModel ApplicationDataModels
        {
            get
            {
                return ServiceProvider.GetService<ApplicationDataModel>();
            }
        }

        /// <summary>
        /// Returns the currently playing playback item
        /// Accessing this property may be unsafe during media change events
        /// </summary>
        protected MediaPlaybackItem CurrentPlaybackItem
        {
            get
            {
                return AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlaybackItem;
            }
        }

        public IServiceProvider ServiceProvider
        {
            get { return AppState.Current.Services; }
        }

        public new DependencyInjectionFrame Frame
        {
            get
            {
                return base.Parent as DependencyInjectionFrame;
            }
        }

        public BasePage()
        {
            DataContext = this;
        }

        private void SubscribeEventHandlersInternal()
        {
            AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlayer.PlaybackSession.PlaybackStateChanged += Current_CurrentStateChanged;
            AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlayer.MediaEnded += Current_MediaEnded;
            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened += Current_MediaOpened;
        }

        protected virtual Task CompactOverlayRequest(MediaPlayerCompactOverlayEventArgs e)
        {
            return Task.CompletedTask;
        }

        protected virtual void NotifyExternalNavigationRequest(object? sender, NavigationRequestEventArgs args)
        {
            ExternalNavigationRequest?.Invoke(sender, args);
        }

        private void OnDispose()
        {
            UnsubscribeHandlersInternal();
            FreeMemory();

            DataContext = null;
            Content = null;
        }

        protected virtual void FreeMemory()
        {

        }

        private void UnsubscribeHandlersInternal()
        {
            AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlayer.PlaybackSession.PlaybackStateChanged -= Current_CurrentStateChanged;
            AppState.Current.MediaServiceConnector.PlayerInstance.CurrentPlayer.MediaEnded -= Current_MediaEnded;
            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened -= Current_MediaOpened;
        }

        private async void Current_MediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            await OnMediaOpened(sender, args);
        }

        private async void Current_MediaEnded(MediaPlayer sender, object args)
        {
            await OnMediaEnded();
        }

        private async void Current_CurrentStateChanged(MediaPlaybackSession sender, object args)
        {
            await OnMediaStateChanged();
        }

        protected virtual Task OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMediaEnded()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnMediaStateChanged()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override to perform one time async initialization
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnInitializeStateAsync(object? parameter)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override to get parameters passed to this instance.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>


        /// <summary>
        /// Used to finalize initialization of async resources
        /// </summary>
        /// <returns></returns>
        public async Task InitializeStateAsync(object? parameter)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                SubscribeEventHandlersInternal();
                await OnInitializeStateAsync(parameter);
            });
        }


        public event PropertyChangedEventHandler PropertyChanged;
        internal void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                OnDispose();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~BasePage()
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

        /// <summary>
        /// returns true if the page handed the back command on its own, and false otherwise
        /// </summary>
        /// <returns></returns>
        public virtual Task<bool> GoBack()
        {
            return Task.FromResult(false);
        }
    }
}
