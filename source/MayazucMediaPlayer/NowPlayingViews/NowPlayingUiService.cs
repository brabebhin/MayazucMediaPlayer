using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI;
using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.NowPlayingViews
{
    public partial class NowPlayingUiService : PlaybackItemManagementUIService<MediaPlayerItemSourceUIWrapper>, IDisposable
    {
        readonly AsyncLock stopPlaybackQueueLock = new AsyncLock();
        string npSelectLabel = "Select... ";
        bool _CanReorderItems;

        ListViewReorderMode reorderMode = ListViewReorderMode.Disabled;
        ListViewSelectionMode selectionMode = ListViewSelectionMode.None;

        public event EventHandler ClearSelectionRequest;

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
                if (_SelectButtonChecked == value) return;

                _SelectButtonChecked = value;
                NotifyPropertyChanged(nameof(SelectButtonChecked));
            }
        }

        public bool NowPlayingCommandBarEnabled
        {
            get => _NowPlayingCommandBarEnabled;
            set
            {
                if (_NowPlayingCommandBarEnabled == value) return;
                _NowPlayingCommandBarEnabled = value;
                NotifyPropertyChanged(nameof(NowPlayingCommandBarEnabled));
            }
        }

        public bool ContextMenuEnabled
        {
            get => _ContextMenuEnabled;
            set
            {
                if (_ContextMenuEnabled == value) return;
                _ContextMenuEnabled = value;
                NotifyPropertyChanged(nameof(ContextMenuEnabled));
            }
        }

        public bool ClearQueueButtonEnabled
        {
            get => _ClearQueueButtonEnabled;
            set
            {
                if (_ClearQueueButtonEnabled == value) return;
                _ClearQueueButtonEnabled = value;
                NotifyPropertyChanged(nameof(ClearQueueButtonEnabled));
            }
        }
        public bool RemoveSelectedButtonEnabled
        {
            get => _RemoveSelectedButtonEnabled;
            set
            {
                if (_RemoveSelectedButtonEnabled == value) return;
                _RemoveSelectedButtonEnabled = value;
                NotifyPropertyChanged(nameof(RemoveSelectedButtonEnabled));
            }
        }
        public bool UnselectButtonEnabled
        {
            get => _UnselectButtonEnabled; set
            {
                if (_UnselectButtonEnabled == value) return;
                _UnselectButtonEnabled = value;
                NotifyPropertyChanged(nameof(UnselectButtonEnabled));
            }
        }
        public bool SelectButtonEnabled
        {
            get => _SelectButtonEnabled;
            set
            {
                if (_SelectButtonEnabled == value) return;
                _SelectButtonEnabled = value;
                NotifyPropertyChanged(nameof(SelectButtonEnabled));
            }
        }
        public bool AddToPlaylistButtonEnabled
        {
            get => _AddToPlaylistButtonEnabled;
            set
            {
                if (_AddToPlaylistButtonEnabled == value) return;
                _AddToPlaylistButtonEnabled = value;
                NotifyPropertyChanged(nameof(AddToPlaylistButtonEnabled));
            }
        }
        public bool SaveButtonEnabled
        {
            get => _SaveButtonEnabled;
            set
            {
                if (_SaveButtonEnabled == value) return;

                _SaveButtonEnabled = value;
                NotifyPropertyChanged(nameof(SaveButtonEnabled));
            }
        }

        public bool MiniPlayerToggled
        {
            get
            {
                return miniPlayerToggled;
            }
            set
            {
                if (miniPlayerToggled == value) return;

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
                if (_NowPlayingSearchFilter == value) return;

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
                if (npShuffleButton == value) return;

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
                if (npChangeSongButton == value) return;

                npChangeSongButton = value;
                NotifyPropertyChanged(nameof(NpChangeSongButton));
            }
        }

        public override IMediaPlayerItemSourceProvderCollection<MediaPlayerItemSourceUIWrapper> Items
        {
            get
            {
                return PlaybackServiceInstance.NowPlayingBackStore;
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

        public AsyncRelayCommand DecreasePlaybackRate
        {
            get;
            private set;
        }

        public IBackgroundPlayer BackgroundMediaPlayerInstance
        {
            get;
            private set;
        }

        public PlaylistsService PlaylistsService { get; private set; }

        public NowPlayingUiService(DispatcherQueue dispatcher,
            PlaybackSequenceService m,
            IBackgroundPlayer backgroundMediaPlayerInstance,
            PlaylistsService playlistsService) : base(dispatcher, m, playlistsService)
        {
            PlaylistsService = playlistsService;
            BackgroundMediaPlayerInstance = backgroundMediaPlayerInstance;
            SaveNowPlayingAsPlaylistCommand = new AsyncRelayCommand<object>(SaveAsPlaylistCommandFunction);
            ShuffleRequestClickCommand = new AsyncRelayCommand<object>(ShuffleRequestCommandFunction);

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

        private void UnSelectAllRequestCommandFunction(object? sender)
        {
            ClearSelectionRequest?.Invoke(this, new EventArgs());
        }

        private async Task RemoveFromPlaybackRequestCommandFunction(object? sender)
        {
            await RemoveFromPlayback(sender);
        }

        public async Task RemoveFromPlayback(object? sender)
        {
            var selectedItems = new List<MediaPlayerItemSourceUIWrapper>();
            //GetSelectedItemsRequest?.Invoke(this, selectedItems);
            if (selectedItems.Count == Items.Count)
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
        ~NowPlayingUiService()
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
