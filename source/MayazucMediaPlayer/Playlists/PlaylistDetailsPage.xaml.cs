using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;

namespace MayazucMediaPlayer.Playlists
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistDetailsPage : BaseUserControl, IDisposable
    {
        public PlaylistsDetailsViewModel Model
        {
            get; private set;
        }

        public string ActualPageTitle
        {
            get;
            private set;
        }

        private bool disposedValue;

        public PlaylistDetailsPage(PlaylistItem playlistData, PlaylistsDetailsViewModel model)
        {
            InitializeComponent();
            COntentPresenter.SelectionChanged += COntentPresenter_SelectionChanged;
            DataContext = Model = model;

            Model.GetSelectedItemsRequest += Model_GetSelectedItemsRequest;
            Model.NavigationRequest += Model_NavigationRequest;
            Model.PropertyChanged += Model_PropertyChanged;
            _ = Model.LoadState(playlistData);
        }


        private void Model_NavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            //this.Frame.NavigateAsync(e.PageType, e.Parameter);
        }

        private void Model_GetSelectedItemsRequest(object? sender, List<object> e)
        {
            e.AddRange(COntentPresenter.SelectedItems);
        }

        private void Model_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.TitleBoxText))
            {
                ActualPageTitle = Model.TitleBoxText;
            }
        }

        void COntentPresenter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (COntentPresenter.SelectedItems.Count > 0)
            {
                RemoveSelected.IsEnabled = true;
            }
            else
            {
                RemoveSelected.IsEnabled = false;
            }
        }

        private async void GoToDetailsPage(object? sender, TappedRoutedEventArgs e)
        {
            if (Model.ContentPresenterSelectionMode == ListViewSelectionMode.None)
            {
                try
                {
                    var mds = (sender as FrameworkElement).GetDataContextObject<MediaPlayerItemSourceUIWrapper>();
                    await FileDetailsPage.ShowForMediaData(mds);
                }
                catch
                {
                    //file could not be loaded
                }
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
                Model.GetSelectedItemsRequest -= Model_GetSelectedItemsRequest;
                Model.NavigationRequest -= Model_NavigationRequest;
                Model.PropertyChanged -= Model_PropertyChanged;
                Model.Dispose();
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
