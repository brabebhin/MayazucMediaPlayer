using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.NowPlayingViews
{
    public class NowPlayingHomeViewModel : MainViewModelBase, IDisposable
    {
        readonly AsyncLock stopPlaybackQueueLock = new AsyncLock();
        string npSelectLabel = "Select... ";
        bool _CanReorderItems;

        ListViewReorderMode reorderMode = ListViewReorderMode.Disabled;
        ListViewSelectionMode selectionMode = ListViewSelectionMode.None;

        public event EventHandler<IReadOnlyCollection<object>> SetSelectedItems;
        public event EventHandler ClearSelectionRequest;
        public event EventHandler<List<MediaPlayerItemSourceUIWrapper>> GetSelectedItemsRequest;

        bool npShuffleButton = true;
        bool npChangeSongButton = true;
        bool isChangingOrder;
        bool miniPlayerToggled = false;
        private bool _ClearQueueButtonEnabled = true;
        private bool _RemoveSelectedButtonEnabled = false;
        private bool _UnselectButtonEnabled = false;
        private bool _SelectButtonEnabled = true;
        private bool _AddToPlaylistButtonEnabled = true;
        private bool _SaveButtonEnabled = true;
        private bool _ContextMenuEnabled = true;
        private bool _NowPlayingCommandBarEnabled = true;
        private bool _SelectButtonChecked = false;

        public bool IsItemClickEnabled
        {
            get;
            private set;
        } = true;

        public bool SelectButtonChecked
        {
            get => _SelectButtonChecked;
            set
            {
                _SelectButtonChecked = value;
                NotifyPropertyChanged(nameof(SelectButtonChecked));
            }
        }

        public bool NowPlayingCommandBarEnabled
        {
            get => _NowPlayingCommandBarEnabled;
            set
            {
                _NowPlayingCommandBarEnabled = value;
                NotifyPropertyChanged(nameof(NowPlayingCommandBarEnabled));
            }
        }

        public bool ContextMenuEnabled
        {
            get => _ContextMenuEnabled;
            set
            {
                _ContextMenuEnabled = value;
                NotifyPropertyChanged(nameof(ContextMenuEnabled));
            }
        }

        public bool ClearQueueButtonEnabled { get => _ClearQueueButtonEnabled; set { _ClearQueueButtonEnabled = value; NotifyPropertyChanged(nameof(ClearQueueButtonEnabled)); } }
        public bool RemoveSelectedButtonEnabled
        {
            get => _RemoveSelectedButtonEnabled;
            set
            {
                _RemoveSelectedButtonEnabled = value;
                NotifyPropertyChanged(nameof(RemoveSelectedButtonEnabled));
            }
        }
        public bool UnselectButtonEnabled
        {
            get => _UnselectButtonEnabled; set
            {
                _UnselectButtonEnabled = value;
                NotifyPropertyChanged(nameof(UnselectButtonEnabled));
            }
        }
        public bool SelectButtonEnabled
        {
            get => _SelectButtonEnabled;
            set
            {
                _SelectButtonEnabled = value;
                NotifyPropertyChanged(nameof(SelectButtonEnabled));
            }
        }
        public bool AddToPlaylistButtonEnabled { get => _AddToPlaylistButtonEnabled; set { _AddToPlaylistButtonEnabled = value; NotifyPropertyChanged(nameof(AddToPlaylistButtonEnabled)); } }
        public bool SaveButtonEnabled { get => _SaveButtonEnabled; set { _SaveButtonEnabled = value; NotifyPropertyChanged(nameof(SaveButtonEnabled)); } }

        public bool MiniPlayerToggled
        {
            get
            {
                return miniPlayerToggled;
            }
            set
            {
                miniPlayerToggled = value;
                NotifyPropertyChanged(nameof(MiniPlayerToggled));
            }
        }

        public AdvancedCollectionView NowPlayingCollectionViewSource
        {
            get;
            private set;
        }

        public TimeSpan CurrentSpan { get; private set; }

        public TimeSpan TrackDurationSpan { get; set; }

        string _NowPlayingSearchFilter = string.Empty;
        public string NowPlayingSearchFilter
        {
            get
            {
                return _NowPlayingSearchFilter;
            }
            set
            {
                _NowPlayingSearchFilter = value;
                NotifyPropertyChanged(nameof(NowPlayingSearchFilter));
                if (string.IsNullOrWhiteSpace(_NowPlayingSearchFilter))
                {
                    //NowPlayingCollectionViewSource.Source = Models.NowPlayingFilter(null);
                    NpChangeSongButton = true;
                }
                else
                {
                    NpChangeSongButton = false;
                }
            }
        }

        public bool NpShuffleButton
        {
            get
            {
                return npShuffleButton;
            }
            set
            {
                npShuffleButton = value;
                NotifyPropertyChanged(nameof(NpShuffleButton));
            }
        }

        public bool NpChangeSongButton
        {
            get
            {
                return npChangeSongButton;
            }
            set
            {
                npChangeSongButton = value;
                NotifyPropertyChanged(nameof(NpChangeSongButton));
            }
        }

        public MediaDataStorageUIWrapperCollection NowPlaying
        {
            get
            {
                return PlaybackServiceInstance.NowPlayingBackStore;
            }
        }



        bool _CanSearch = true;
        public bool CanSearch
        {
            get
            {
                return _CanSearch;
            }
            set
            {
                _CanSearch = value;
                NotifyPropertyChanged(nameof(CanSearch));
            }
        }

        public IRelayCommand<object> ClearPlaybackQueueCommand
        {
            get;
            private set;
        }


        public IRelayCommand<object> SaveNowPlayingAsPlaylistCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> ShuffleRequestClickCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> ChangeSongOrderRequestCommand
        {
            get;
            private set;
        }



        public IRelayCommand<object> SelectRequestCommand
        {
            get;
            private set;
        }


        public IRelayCommand<object> ClearSelectionCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> RemoveCommand
        {
            get;
            private set;
        }

        public IRelayCommand SkipBackFrames
        {
            get;
            private set;
        }

        public IRelayCommand SkipForwardFrames
        {
            get;
            private set;
        }

        public IRelayCommand IncreasePlaybackRate
        {
            get;
            private set;
        }

        public AsyncRelayCommand DecreasePlaybackRate
        {
            get;
            private set;
        }


        public ListViewReorderMode ReorderMode
        {
            get
            {
                return reorderMode;
            }
            set
            {
                reorderMode = value;
                CanReorderItems = reorderMode == ListViewReorderMode.Enabled;
                EnableButtonsForReorder(reorderMode == ListViewReorderMode.Disabled);

                NotifyPropertyChanged(nameof(ReorderMode));
                if (CanReorderItems)
                {
                    NowPlayingSearchFilter = null;
                }
                CanSearch = !CanReorderItems;
            }
        }

        public bool IsChangingOrder
        {
            get
            {
                return isChangingOrder;
            }
            set
            {
                isChangingOrder = value;
                NotifyPropertyChanged(nameof(IsChangingOrder));
            }
        }

        public ListViewSelectionMode SelectionMode
        {
            get
            {
                return selectionMode;
            }
            set
            {
                selectionMode = value;
                var enableButtons = value == ListViewSelectionMode.None;

                ClearQueueButtonEnabled = enableButtons;
                RemoveSelectedButtonEnabled = enableButtons;

                AddToPlaylistButtonEnabled = enableButtons;
                SaveButtonEnabled = enableButtons;
                NpShuffleButton = enableButtons;
                NpChangeSongButton = enableButtons;
                CanReorderItems = false;
                ContextMenuEnabled = enableButtons;
                IsItemClickEnabled = enableButtons;

                NotifyPropertyChanged(nameof(SelectionMode));
                NotifyPropertyChanged(nameof(IsItemClickEnabled));

            }
        }

        public string NpSelectLabel
        {
            get
            {
                return npSelectLabel;
            }
            set
            {
                npSelectLabel = value;
                NotifyPropertyChanged(nameof(NpSelectLabel));
            }
        }

        public bool CanReorderItems
        {
            get
            {
                return _CanReorderItems;
            }
            set
            {
                SetCanReorderModeAndNotify(value);
            }
        }

        private void EnableButtonsForReorder(bool value)
        {
            ContextMenuEnabled = value;
            ClearQueueButtonEnabled = value;
            RemoveSelectedButtonEnabled = value;
            UnselectButtonEnabled = value;
            SelectButtonEnabled = value;
            AddToPlaylistButtonEnabled = value;
            SaveButtonEnabled = value;
            NpShuffleButton = value;
            IsItemClickEnabled = value;
            NotifyPropertyChanged(nameof(IsItemClickEnabled));
        }

        private void SetCanReorderModeAndNotify(bool value)
        {
            _CanReorderItems = value;
            NotifyPropertyChanged(nameof(CanReorderItems));
        }

        public IBackgroundPlayer BackgroundMediaPlayerInstance
        {
            get;
            private set;
        }

        public PlaylistsService PlaylistsService { get; private set; }

        public NowPlayingHomeViewModel(DispatcherQueue dispatcher,
            PlaybackSequenceService m,
            IBackgroundPlayer backgroundMediaPlayerInstance,
            PlaylistsService playlistsService) : base(dispatcher, m)
        {
            PlaylistsService = playlistsService;
            NowPlayingCollectionViewSource = new AdvancedCollectionView(NowPlaying);
            BackgroundMediaPlayerInstance = backgroundMediaPlayerInstance;
            SaveNowPlayingAsPlaylistCommand = new AsyncRelayCommand<object>(SaveAsPlaylistCommandFunction);
            ShuffleRequestClickCommand = new AsyncRelayCommand<object>(ShuffleRequestCommandFunction);
            ChangeSongOrderRequestCommand = new AsyncRelayCommand<object>(ChangeSongRequestCommandFunction);
            SelectRequestCommand = new RelayCommand<object>(SelectionRequestCommandFunction);
            ClearSelectionCommand = new RelayCommand<object>(UnSelectAllRequestCommandFunction);
            RemoveCommand = new AsyncRelayCommand<object>(RemoveFromPlaybackRequestCommandFunction);
            SkipBackFrames = new AsyncRelayCommand(async () =>
            {
                await AppState.Current.MediaServiceConnector.SkipSecondsBack(Constants.JumpBackSeconds);
            });
            SkipForwardFrames = new AsyncRelayCommand(async () =>
            {
                await AppState.Current.MediaServiceConnector.SkipSecondsForth(Constants.JumpAheadSeconds);
            });
            ClearPlaybackQueueCommand = new AsyncRelayCommand<object>(StopPlaybackAsync);

            IncreasePlaybackRate = new AsyncRelayCommand(async () =>
            {
                await AppState.Current.MediaServiceConnector.QuickenPlaybackRate();
            });

            DecreasePlaybackRate = new AsyncRelayCommand(async () =>
            {
                await AppState.Current.MediaServiceConnector.SlowerPlaybackRate();
            });

            base.PlaybackServiceInstance.NowPlayingBackStore.CollectionChanged += NowPlayingBackStore_CollectionChanged;
            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened += PlayerInstance_OnMediaOpened;
            CheckCommandBarEnabled();
        }

        private void PlayerInstance_OnMediaOpened(Windows.Media.Playback.MediaPlayer sender, MediaOpenedEventArgs args)
        {
            CheckCommandBarEnabled();

        }

        private void NowPlayingBackStore_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CheckCommandBarEnabled();
        }

        private void CheckCommandBarEnabled()
        {
            if (AppState.Current.MediaServiceConnector.PlayerInstance.LocalSource)
            {
                NowPlayingCommandBarEnabled = base.PlaybackServiceInstance.NowPlayingBackStore.Count(x => x.MediaData.Persistent) > 0;
            }
            else
            {
                NowPlayingCommandBarEnabled = false;
            }

        }

        private async Task StopPlaybackAsync(object arg)
        {
            using (await stopPlaybackQueueLock.LockAsync())
            {
                bool proceed = true;

                //MessageDialog diag = new MessageDialog("This will stop playback and erase queue.", "Are you sure?");
                //diag.Commands.Add(new UICommand("Yes", (d) => { proceed = true; }));
                //diag.Commands.Add(new UICommand("No", (d) => { proceed = false; }));
                //await diag.ShowAsync();

                if (proceed)
                {
                    SelectButtonChecked = false;
                    SelectionMode = ListViewSelectionMode.None;
                    await (AppState.Current.MediaServiceConnector.PlayerInstance).StopPlayback();

                    //diag = new MessageDialog("Please restart the app for best results.", "Obliteration complete");
                    //diag.Commands.Clear();
                    //await diag.ShowAsync();
                }
            }
        }

        private async Task SaveAsPlaylistCommandFunction(object? sender)
        {
            var diag = new StringInputDialog("Playlist name", "Pick a playlist name");
            await ContentDialogService.Instance.ShowAsync(diag);

            var PlayListName = diag.Result;
            if (PlayListName == null)
            {
                return;
            }

            await PlaylistsService.AddPlaylist(PlayListName, PlaybackServiceInstance.NowPlayingBackStore.Select(x => x.MediaData));

            PopupHelper.ShowSuccessDialog();

        }

        private async Task ShuffleRequestCommandFunction(object? sender)
        {
            NpShuffleButton = false;
            NpChangeSongButton = false;
            IsChangingOrder = true;
            await RandomizeNowPlaying(sender);
            NotifyPropertyChanged(nameof(NowPlayingCollectionViewSource));
            //SetFlipViewSelectedIndex();

            IsChangingOrder = false;
            NpShuffleButton = true;
            NpChangeSongButton = true;
        }

        public async Task RandomizeNowPlaying(object? sender)
        {
            IsChangingOrder = true;
            try
            {
                await GlobalProgressBarUtilities.ShowProgressBar("Please wait");

                await (AppState.Current.MediaServiceConnector.PlayerInstance).RandomizeNowPlayingQueue();
                //var oldIndex = SettingsWrapper.Instance.PlaybackIndex;
                //var newIndex = Models.NowPlaying.RandomizeMusicDataStorage(oldIndex);
                //SettingsWrapper.Instance.PlaybackIndex = newIndex;

                await GlobalProgressBarUtilities.HideProgressBar();
                //AppState.Current.MediaServiceConnector.SendNewNowPlaying(Models.NowPlaying.Select(x => x.MediaData).ToArray());
            }
            finally
            {
                IsChangingOrder = false;
            }
        }

        private async Task ChangeSongRequestCommandFunction(object? sender)
        {

            try
            {
                await HandleReorderRequest(sender);
            }
            catch
            {
            }
        }

        public async Task HandleReorderRequest(object? sender)
        {
            if (ReorderMode == ListViewReorderMode.Enabled)
            {
                ReorderMode = ListViewReorderMode.Disabled;
                IsChangingOrder = false;
                await (AppState.Current.MediaServiceConnector.PlayerInstance).SavePlaylistReorderAsync();
                (sender as AppBarToggleButton).Label = "Reorder";
                NpShuffleButton = true;
            }
            else
            {
                ReorderMode = ListViewReorderMode.Enabled;
                IsChangingOrder = true;
                (sender as AppBarToggleButton).Label = "Save reorder";
                NpShuffleButton = false;
            }
        }

        private void SelectionRequestCommandFunction(object? sender)
        {
            if (SelectionMode == ListViewSelectionMode.Multiple)
            {
                SelectionMode = ListViewSelectionMode.None;
                (sender as AppBarToggleButton).Label = "Select... ";
                (sender as AppBarToggleButton).IsChecked = false;
            }
            else
            {
                SelectionMode = ListViewSelectionMode.Multiple;
                NpSelectLabel = "Disable select...";

                (sender as AppBarToggleButton).IsChecked = true;
            }
        }

        private void UnSelectAllRequestCommandFunction(object? sender)
        {
            ClearSelectionRequest?.Invoke(this, new EventArgs());
        }

        private void SelectAllRequestCommandFunction(object? sender)
        {
            //NowPlayingListSelectedItems.Clear();
            var NowPlayingListSelectedItems = new List<Object>();
            SelectionMode = ListViewSelectionMode.Multiple;
            foreach (var item in NowPlaying)
            {
                NowPlayingListSelectedItems.Add(item);
            }
            SetSelectedItems?.Invoke(this, NowPlayingListSelectedItems.AsReadOnly());
        }

        private async Task RemoveFromPlaybackRequestCommandFunction(object? sender)
        {
            await RemoveFromPlayback(sender);
        }

        public async Task RemoveFromPlayback(object? sender)
        {
            var selectedItems = new List<MediaPlayerItemSourceUIWrapper>();
            GetSelectedItemsRequest?.Invoke(this, selectedItems);
            if (selectedItems.Count == NowPlaying.Count)
            {
                await StopPlaybackAsync(sender);
            }
            else
            {
                await RemoveItemsFromPlaybackAsync(selectedItems);
            }
        }

        public async Task RemoveItemsFromPlaybackAsync(IList<MediaPlayerItemSourceUIWrapper> selectedItems)
        {
            await (AppState.Current.MediaServiceConnector.PlayerInstance).RemoveItemsFromNowPlayingQueue(selectedItems);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                    // TODO: dispose managed state (managed objects).
                }

                PlaybackServiceInstance.NowPlayingBackStore.CollectionChanged -= NowPlayingBackStore_CollectionChanged;
                PlaybackServiceInstance = null;

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~NowPlayingHomeViewModel()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
