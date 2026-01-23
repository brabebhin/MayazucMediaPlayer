using MayazucMediaPlayer.Controls;
using System;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Playlists
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistDetailsPage : BaseUserControl, IDisposable
    {
        public PlaylistsDetailsService PlaylistsDetailsService
        {
            get; private set;
        }

        public string ActualPageTitle
        {
            get;
            private set;
        }

        private bool disposedValue;

        public PlaylistDetailsPage(PlaylistItem playlistData, PlaylistsDetailsService model)
        {
            InitializeComponent();
            DataContext = PlaylistsDetailsService = model;
            PlaylistsDetailsService.PropertyChanged += Model_PropertyChanged;
            _ = LoadPlaylistItem(playlistData);
        }

        private async Task LoadPlaylistItem(PlaylistItem item)
        {
            await PlaylistsDetailsService.LoadState(item);
            await playlistContentsManagerControl.LoadStateInternal(PlaylistsDetailsService);
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlaylistsDetailsService.TitleBoxText))
            {
                ActualPageTitle = PlaylistsDetailsService.TitleBoxText;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                PlaylistsDetailsService.PropertyChanged -= Model_PropertyChanged;
                PlaylistsDetailsService.Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~PlaylistDetailsPage()
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
    }
}
