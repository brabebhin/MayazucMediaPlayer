using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.NowPlayingViews

{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlayingList : BasePage
    {
        public override string Title => "Now playing";

        readonly AsyncLock _AddToPlaylistLock = new AsyncLock();
        public event EventHandler<NavigationRequestEventArgs> OpenInDifferentFrame;

        NowPlayingUiService dataService;
        public NowPlayingUiService DataService
        {
            get
            {
                return dataService;
            }
            private set
            {
                if (dataService != value)
                {
                    if (dataService != null)
                    {
                        dataService.NavigationRequest -= SignalNavigationRequest;
                    }

                    dataService = value;

                    DataContext = dataService;
                    dataService.NavigationRequest += SignalNavigationRequest;

                }
            }
        }

        private void SignalNavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            if (Frame == null)
            {
                OpenInDifferentFrame?.Invoke(sender, e);
            }
            else
            {
                (Frame as DependencyInjectionFrame).NotifyExternalNavigationRequest(sender, e);
            }
        }

        public NowPlayingList()
        {
            InitializeComponent();
            DataService = AppState.Current.Services.GetService<NowPlayingUiService>();
        }

        private async void AddToPlaylist(object? sender, RoutedEventArgs e)
        {
            using (await _AddToPlaylistLock.LockAsync())
            {
                (sender as Control)!.IsEnabled = false;
                try
                {
                    if (DataService.PlaybackServiceInstance.NowPlayingBackStore.Count == 0)
                    {
                        return;
                    }

                    var items = DataService.PlaybackServiceInstance.NowPlayingBackStore.Cast<MediaPlayerItemSourceUIWrapper>().ToList();

                    PlaylistPicker picker = new PlaylistPicker();
                    await picker.PickPlaylistAsync(DataService.PlaylistsService.Playlists);
                    if (picker.SelectedPlaylist != null)
                    {
                        await picker.SelectedPlaylist.Add(items.Select(x => x.MediaData));
                        PopupHelper.ShowSuccessDialog();
                    }
                }
                catch { }
                finally
                {
                    (sender as Control)!.IsEnabled = true;
                }
            }
        }

  
        protected override async Task OnInitializeStateAsync(object? parameter)
        {
            DataContext = DataService;
            await NowPlayingFileManagementControl.LoadStateInternal(DataService);
            var CurrentMusicData = (await DataService.PlaybackServiceInstance.CurrentMediaMetadata());
            //previouslyPlayedData = CurrentMusicData;
            if (CurrentMusicData.IsSuccess)
            {
            }
        }



        private async void SkipToItem(object? sender, ItemClickEventArgs e)
        {
            var t = e.ClickedItem as MediaPlayerItemSourceUIWrapper;
            var index = DataService.PlaybackServiceInstance.NowPlayingBackStore.IndexOf(t);
            await AppState.Current.MediaServiceConnector.SkipToIndex(index);
        }

        private async void RemoveFromQueue_tapped(object? sender, RoutedEventArgs e)
        {
            var mds = (sender as FrameworkElement).GetDataContextObject<MediaPlayerItemSourceUIWrapper>();

            await DataService.RemoveItemsFromPlaybackAsync(new MediaPlayerItemSourceUIWrapper[] { mds });
        }

        private async void GoToFileProperties_tapped(object? sender, RoutedEventArgs e)
        {
            try
            {
                var mds = (sender as FrameworkElement).GetDataContextObject<MediaPlayerItemSourceUIWrapper>();
                await FileDetailsPage.ShowForMediaData(mds);
            }
            catch { }
        }

        private async void AddSingleItemToPlaylist_Tapped(object? sender, RoutedEventArgs e)
        {
            var mds = sender.GetDataContextObject<MediaPlayerItemSourceUIWrapper>().MediaData;
            await AddSingleItemToPlaylist(mds);
        }

        private async Task AddSingleItemToPlaylist(IMediaPlayerItemSource mds)
        {
            var ok = await PlaylistHelpers.AddItemsToPlaylistAsync(new IMediaPlayerItemSource[] { mds }, DataService.PlaylistsService.Playlists);
            if (ok.OK)
            {
                PopupHelper.ShowInfoMessage($"Added {mds.Title} to {ok.Playlist.Title}", "Success!");
            }
            else if (ok.Error != null)
            {
                PopupHelper.ShowInfoMessage($"Could not add {mds.Title} to playlist. {ok.Error.Message}", ":(");
            }
        }

        private async void RemoveFromQueue_tapped(Microsoft.UI.Xaml.Controls.SwipeItem sender, Microsoft.UI.Xaml.Controls.SwipeItemInvokedEventArgs args)
        {
            var mds = args.SwipeControl.GetDataContextObject<MediaPlayerItemSourceUIWrapper>();
            if (mds.MediaData.Persistent)
                await DataService.RemoveItemsFromPlaybackAsync(new MediaPlayerItemSourceUIWrapper[] { mds });
        }

        private void MoveItemUp_Tapped(Microsoft.UI.Xaml.Controls.SwipeItem sender, Microsoft.UI.Xaml.Controls.SwipeItemInvokedEventArgs args)
        {
            var mds = args.SwipeControl.GetDataContextObject<MediaPlayerItemSourceUIWrapper>();
            if (mds.MediaData.Persistent)
                MoveItemUpInPlaybackList(mds);
        }

        private async void MoveItemDownInPlaybackList(MediaPlayerItemSourceUIWrapper mds)
        {
            var playerService = ServiceProvider.GetService<IBackgroundPlayer>();
            await playerService.MoveItemDownInPlaybackQueue(mds);
        }

        private async void MoveItemUpInPlaybackList(MediaPlayerItemSourceUIWrapper mds)
        {
            var playerService = ServiceProvider.GetService<IBackgroundPlayer>();
            await playerService.MoveItemUpInPlaybackQueue(mds);
        }

        private void MoveItemDown_Tapped(Microsoft.UI.Xaml.Controls.SwipeItem sender, Microsoft.UI.Xaml.Controls.SwipeItemInvokedEventArgs args)
        {
            var mds = args.SwipeControl.GetDataContextObject<MediaPlayerItemSourceUIWrapper>();
            if (mds.MediaData.Persistent)
                MoveItemDownInPlaybackList(mds);
        }

        private void MoveItemUp_Tapped(object? sender, RoutedEventArgs e)
        {
            var mds = (sender as FrameworkElement).GetDataContextObject<MediaPlayerItemSourceUIWrapper>();
            if (mds.MediaData.Persistent)
                MoveItemUpInPlaybackList(mds);
        }

        private void MoveItemDown_Tapped(object? sender, RoutedEventArgs e)
        {
            var mds = (sender as FrameworkElement).GetDataContextObject<MediaPlayerItemSourceUIWrapper>();
            MoveItemDownInPlaybackList(mds);
        }

        private void Failure_removeItem(object? sender, RoutedEventArgs e)
        {
            RemoveFromQueue_tapped(sender, e);
        }
    }
}
