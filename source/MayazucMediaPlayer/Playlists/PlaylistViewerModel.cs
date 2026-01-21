using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MayazucMediaPlayer.Playlists
{
    public partial class PlaylistViewerModel : MainViewModelBase
    {
        readonly AsyncLock randomPlaylistLock = new AsyncLock();

        public event EventHandler<List<PlaylistItem>> GetSelectedItemsRequest;

        bool _EnqueueButtonIsEnabled = false;
        bool _PlayButtonIsEnabled, _AddToPlaylistButtonIsEnabled, _SaveAsPlaylistButtonIsEnabled, _DeleteButtonIsEnabled;
        Visibility _SearchGridVisibility = Visibility.Collapsed;

        public IRelayCommand<object> PlayCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> AddToNowPlayingCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> SaveToExistingPlaylistCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> DeleteSelectedPlaylistsCommand
        {
            get;
            private set;
        }


        public IRelayCommand<object> SetSelectionModeCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> ShowSearchGridCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> DeleteSinglePlaylist
        {
            get;
            private set;
        }

        public IRelayCommand<object> CreateEmptyPlaylistCommand
        {
            get;
            private set;
        }

        public event EventHandler<PlaylistItem> OnPlaylistItemDeleted;

        public ObservableCollection<PlaylistItem> Playlists
        {
            get
            {
                return PlaylistsService.Playlists;
            }
        }

        public PlaylistsService PlaylistsService
        {
            get;
            private set;
        }

        public AdvancedCollectionView PlaylistsView
        {
            get;
            private set;
        }

        public PlaylistViewerModel(DispatcherQueue disp, PlaybackSequenceService playbackSeqService, PlaylistsService playlistService) : base(disp, playbackSeqService)
        {
            PlaylistsService = playlistService;
            PlaylistsView = new AdvancedCollectionView();
            PlaylistsView.ItemsSource = Playlists;
            ShowSearchGridCommand = new RelayCommand<object>(ShowHideSearchGrid);
            SetSelectionModeCommand = new RelayCommand<object>(SelectAllCommand);
            DeleteSelectedPlaylistsCommand = new AsyncRelayCommand<object>(DeleteSelectedCommand);
            SaveToExistingPlaylistCommand = new AsyncRelayCommand<object>(SaveToExistingPlaylist);
            PlayCommand = new AsyncRelayCommand<object>(PlayCommandAsync);
            AddToNowPlayingCommand = new AsyncRelayCommand<object>(AddToNowPlayingAsyncCommand);
            DeleteSinglePlaylist = new AsyncRelayCommand<object>(DeleteSinglePlaylistAsync);
            CreateEmptyPlaylistCommand = new AsyncRelayCommand<object>(CreateEmptyPlaylist);
        }

        private async Task CreateEmptyPlaylist(object args)
        {
            await Dispatcher.EnqueueAsync(async () =>
            {
                var playlistNamePicker = new StringInputDialog("New playlist", "Write the playlist's name");
                var invalidChars = System.IO.Path.GetInvalidFileNameChars().Union(new char[] { '.' });
                HashSet<string> knownPlaylistTitles = new HashSet<string>(Playlists.Select(x => x.Title.ToLowerInvariant()));
                playlistNamePicker.Validator = (s) =>
                {
                    return !s.Any(x => invalidChars.Contains(x)) && !knownPlaylistTitles.Contains(s.ToLowerInvariant() + PlaylistItem.FileExtension);
                };
                await ContentDialogService.Instance.ShowAsync(playlistNamePicker);

                if (!string.IsNullOrWhiteSpace(playlistNamePicker.Result))
                {
                    var name = playlistNamePicker.Result;
                    await PlaylistsService.AddPlaylist(name, Enumerable.Empty<IMediaPlayerItemSource>());
                }
            });
        }

        private async Task DeleteSinglePlaylistAsync(object arg)
        {
            var playlisItem = arg as PlaylistItem;
            if (playlisItem != null)
            {
                await PlaylistsService.RemovePlaylist(playlisItem);
            }
            OnPlaylistItemDeleted?.Invoke(this, playlisItem);
        }

        public void LoadState(object navigationparam)
        {
            NavigationParam = navigationparam;
            LoadData();
        }

        private async Task AddToNowPlayingAsyncCommand(object? sender)
        {
            EnqueueButtonIsEnabled = false;
            try
            {
                MediaPlayerItemSourceList storages = new MediaPlayerItemSourceList();
                if (sender is PlaylistItem)
                {
                    var mds = await (sender as PlaylistItem)?.GetMediaDataSourcesAsync();
                    if (mds.Count == 0)
                    {
                        await Dispatcher.EnqueueAsync(() =>
                        {
                            PopupHelper.ShowInfoMessage("Empty playlist...");
                        });

                    }
                    else
                    {
                        storages.AddRange(mds);
                    }
                }
                else
                {
                    List<PlaylistItem> SelectedPlaylists = GetSelectedItems();
                    foreach (PlaylistItem itm in SelectedPlaylists)
                    {
                        var s = await itm.GetMediaDataSourcesAsync();
                        storages.AddRange(s);
                    }
                }
                await AppState.Current.MediaServiceConnector.EnqueueAtEnd(storages);
            }
            catch { }
            finally
            {
                EnqueueButtonIsEnabled = true;
            }

        }

        private void SelectAllCommand(object? sender)
        {
            ListViewSelectionMode defaultSelectionMode = GetDefaultSelectionMode();
            var f = (sender as AppBarToggleButton);
            if (DisplayListSelectionMode == defaultSelectionMode)
            {
                DisplayListSelectionMode = ListViewSelectionMode.Multiple;
            }
            else
            {
                DisplayListSelectionMode = defaultSelectionMode;
                ChangeButtonAvailabilityButtons(false);
            }
            f.IsChecked = DisplayListSelectionMode == ListViewSelectionMode.Multiple;
        }

        private ListViewSelectionMode GetDefaultSelectionMode()
        {
            return ListViewSelectionMode.None;
        }

        private async Task PlayCommandAsync(object? sender)
        {
            PlayButtonIsEnabled = false;
            try
            {
                MediaPlayerItemSourceList thingsToPlay = new MediaPlayerItemSourceList();
                if (sender is PlaylistItem)
                {
                    var mds = await (sender as PlaylistItem)?.GetMediaDataSourcesAsync();
                    if (mds.Count == 0)
                    {
                        await Dispatcher.EnqueueAsync(() =>
                        {
                            PopupHelper.ShowInfoMessage("Empty playlist...");
                        });

                    }
                    else
                    {
                        thingsToPlay.AddRange(mds);
                    }
                }
                else
                {
                    List<PlaylistItem> SelectedPlaylists = GetSelectedItems();
                    foreach (PlaylistItem itm in SelectedPlaylists)
                    {
                        var s = await itm.GetMediaDataSourcesAsync();
                        foreach (var ss in s)
                        {

                            thingsToPlay.Add(ss);
                        }
                    }
                }

                if (NavigationParam is PlaylistItem)
                {
                    await AppState.Current.MediaServiceConnector.StartPlaybackFromBeginning(thingsToPlay);
                }
                else
                {
                    await AppState.Current.MediaServiceConnector.StartPlaybackFromBeginning(thingsToPlay);
                }
            }
            catch { }
            finally
            {
                PlayButtonIsEnabled = true;
            }

        }

        private List<PlaylistItem> GetSelectedItems()
        {
            var SelectedPlaylists = new List<PlaylistItem>();
            GetSelectedItemsRequest?.Invoke(this, SelectedPlaylists);
            return SelectedPlaylists;
        }

        public void ShowHideSearchGrid(object? sender)
        {
            if (SearchGridVisibility == Visibility.Visible)
            {
                SearchGridVisibility = Visibility.Collapsed;
            }
            else
            {
                SearchGridVisibility = Visibility.Visible;
            }
        }

        private async Task SaveToExistingPlaylist(object? sender)
        {
            MediaPlayerItemSourceList NewPlaylist = new MediaPlayerItemSourceList();
            //(ApplicationSessionState.Current.Models.NowPlaying).Clear();
            var SelectedPlaylists = new List<PlaylistItem>();
            GetSelectedItemsRequest?.Invoke(this, SelectedPlaylists);

            foreach (PlaylistItem itm in SelectedPlaylists)
            {

                var s = await itm.GetMediaDataSourcesAsync();
                foreach (var ss in s)
                {
                    NewPlaylist.Add(ss);
                }
            }

            PlaylistPicker picker = new PlaylistPicker();
            var target = await picker.PickPlaylistAsync(PlaylistsService.Playlists);

            if (target != null)
            {
                await PlaylistsService.AddToPlaylist(target, NewPlaylist);
                await MessageDialogService.Instance.ShowMessageDialogAsync("Added to playlist", "Success");
            }
        }

        private async Task DeleteSelectedCommand(object? sender)
        {
            try
            {
                var dataz = GetSelectedItems();

                if (dataz.Any())
                {
                    bool delete = false;

                    await MessageDialogService.Instance.ShowMessageDialogAsync("Are you sure you want to delete those files?", "Confirm deletion",
                    new UICommand("Yes", new UICommandInvokedHandler((s) =>
                    {
                        delete = true;
                    })),
                    new UICommand("No"));

                    if (delete)
                    {
                        var c = dataz.ToList();
                        for (int i = 0; i < c.Count; i++)
                        {
                            await PlaylistsService.RemovePlaylist(c[i]);
                        }
                    }
                }
            }
            catch { }

        }

        public void ChangeButtonAvailabilityButtons(bool isEnabled)
        {
            PlayButtonIsEnabled = isEnabled;
            EnqueueButtonIsEnabled = isEnabled;
            AddToPlaylistButtonIsEnabled = isEnabled;
            SaveAsPlaylistButtonIsEnabled = isEnabled;
            DeleteButtonIsEnabled = isEnabled;
        }

        public void LoadData()
        {
            ChangeButtonAvailabilityButtons(false);
        }

        public bool EnqueueButtonIsEnabled
        {
            get
            {
                return _EnqueueButtonIsEnabled;
            }
            set
            {
                if (_EnqueueButtonIsEnabled == value) return;

                _EnqueueButtonIsEnabled = value;

                NotifyPropertyChanged(nameof(EnqueueButtonIsEnabled));
            }
        }

        ListViewSelectionMode _DisplayListSelectionMode = ListViewSelectionMode.None;
        public ListViewSelectionMode DisplayListSelectionMode
        {
            get
            {
                return _DisplayListSelectionMode;
            }
            private set
            {
                if (_DisplayListSelectionMode == value) return;

                _DisplayListSelectionMode = value;
                NotifyPropertyChanged(nameof(DisplayListSelectionMode));
            }
        }

        public bool PlayButtonIsEnabled
        {
            get
            {
                return _PlayButtonIsEnabled;
            }
            set
            {
                if (_PlayButtonIsEnabled == value) return;

                _PlayButtonIsEnabled = value;

                NotifyPropertyChanged(nameof(PlayButtonIsEnabled));
            }
        }

        public bool AddToPlaylistButtonIsEnabled
        {
            get
            {
                return _AddToPlaylistButtonIsEnabled;
            }
            set
            {
                if (_AddToPlaylistButtonIsEnabled == value) return;

                _AddToPlaylistButtonIsEnabled = value;

                NotifyPropertyChanged(nameof(AddToPlaylistButtonIsEnabled));
            }
        }

        public bool SaveAsPlaylistButtonIsEnabled
        {
            get
            {
                return _SaveAsPlaylistButtonIsEnabled;
            }
            set
            {
                if (_SaveAsPlaylistButtonIsEnabled == value) return;

                _SaveAsPlaylistButtonIsEnabled = value;
                NotifyPropertyChanged(nameof(SaveAsPlaylistButtonIsEnabled));
            }
        }

        public bool DeleteButtonIsEnabled
        {
            get
            {
                return _DeleteButtonIsEnabled;
            }
            set
            {
                if (_DeleteButtonIsEnabled == value) return;

                _DeleteButtonIsEnabled = value;
                NotifyPropertyChanged(nameof(DeleteButtonIsEnabled));
            }
        }

        public object NavigationParam { get; private set; }

        public Visibility SearchGridVisibility
        {
            get
            {
                return _SearchGridVisibility;
            }
            set
            {
                if (_SearchGridVisibility == value) return;

                _SearchGridVisibility = value;
                NotifyPropertyChanged(nameof(SearchGridVisibility));
            }
        }

        public bool DataLoaded { get; private set; }
    }
}
