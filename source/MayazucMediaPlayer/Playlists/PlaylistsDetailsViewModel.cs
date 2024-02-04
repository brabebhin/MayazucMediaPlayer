using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.Runtime;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MayazucMediaPlayer.Playlists
{
    public class PlaylistsDetailsViewModel : MainViewModelBase, IDisposable
    {
        string _NumberOfSongsText;
        public string NumberOfSongsText
        {
            get
            {
                return _NumberOfSongsText;
            }
            set
            {
                _NumberOfSongsText = value;
                NotifyPropertyChanged(nameof(NumberOfSongsText));
            }
        }

        bool _writeable;
        public bool Writeable
        {
            get => _writeable;
            set
            {
                _writeable = value;
                DeleteIsEnabled = _writeable;
                SelectButtonIsEnabled = _writeable;
                RenameIsEnabled = _writeable;
                NpChangeSongButton = _writeable;
            }
        }

        bool isChangingOrder;
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

        bool npChangeSongButton = false;
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

        string _PlaylistCoverSource;
        public string PlaylistCoverSource
        {
            get
            {
                return _PlaylistCoverSource;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _PlaylistCoverSource = AssetsPaths.PlaylistCoverPlaceholder;
                }
                else
                {
                    _PlaylistCoverSource = value;
                }

                NotifyPropertyChanged(nameof(PlaylistCoverSource));
            }
        }

        string playlistPath;
        string _TitleBoxText;
        bool _DeleteIsEnabled = true;
        bool _RemoveSelectedIsEnabled = true;
        bool _SelectButtonIsEnabled = true;

        ListViewSelectionMode _COntentPresenterSelectionMode = ListViewSelectionMode.None;
        ListViewReorderMode reorderMode = ListViewReorderMode.Disabled;

        public PlaylistItem CurrentPlaylistItem
        {
            get;
            private set;
        }

        public event EventHandler<List<object>> GetSelectedItemsRequest;

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
                NotifyPropertyChanged(nameof(ReorderMode));
            }
        }

        bool _CanReorderItems;
        public bool CanReorderItems
        {
            get
            {
                return _CanReorderItems;
            }
            set
            {
                _CanReorderItems = value;
                NotifyPropertyChanged(nameof(CanReorderItems));
            }
        }

        public CommandBase ChangeSongOrderRequestCommand
        {
            get;
            private set;
        }

        public CommandBase PlayButtonCommand
        {
            get; private set;
        }

        public CommandBase EnqueueButtonCommand
        {
            get; private set;
        }

        public CommandBase SelectButtonCommand
        {
            get; private set;
        }

        public CommandBase AddToPlaylistButtonCommand
        {
            get; private set;
        }

        public CommandBase DeleteCommand
        {
            get; private set;
        }

        public CommandBase RemoveSelectedCommand
        {
            get; private set;
        }

        public CommandBase RenamePlaylistCommand
        {
            get;
            private set;
        }

        public ObservableCollection<MediaPlayerItemSourceUIWrapper> Files { get; set; } = new ObservableCollection<MediaPlayerItemSourceUIWrapper>();

        public PlaylistsService PlaylistsService { get; private set; }


        public PlaylistsDetailsViewModel(DispatcherQueue dp, PlaybackSequenceService m, PlaylistsService playlistsService) : base(dp, m)
        {
            PlaylistsService = playlistsService;
            Files.CollectionChanged += Files_CollectionChanged;
            PlayButtonCommand = new AsyncRelayCommand(PlayCLick);
            EnqueueButtonCommand = new AsyncRelayCommand(AddToNowPlaying);

            SelectButtonCommand = new RelayCommand(SelectTapped);
            AddToPlaylistButtonCommand = new AsyncRelayCommand(AddToExistingPlaylist);
            DeleteCommand = new AsyncRelayCommand(DeleteClick);
            RemoveSelectedCommand = new AsyncRelayCommand(RemoveSelectedItems);
            RenamePlaylistCommand = new AsyncRelayCommand(RenamePlaylistAsync);

            ChangeSongOrderRequestCommand = new AsyncRelayCommand(ChangeSongRequestCommandFunction);
            PlaylistsService.PlaylistItemChanged += PlaybackService_PlaylistItemChanged;
        }


        private async void PlaybackService_PlaylistItemChanged(object? sender, string e)
        {
            if (CurrentPlaylistItem == sender)
            {
                await LoadState(CurrentPlaylistItem);
            }
        }

        private async Task ChangeSongRequestCommandFunction(object? sender)
        {
            IsChangingOrder = true;
            //NpShuffleButton = false;
            try
            {
                await HandleReorderRequest(sender);
            }
            finally
            {

                IsChangingOrder = false;
                //NpShuffleButton = true;
            }
        }

        public async Task HandleReorderRequest(object? sender)
        {
            if (ReorderMode == ListViewReorderMode.Enabled)
            {
                ReorderMode = ListViewReorderMode.Disabled;
                IsChangingOrder = false;
                await SavePlaylistItemChanges();
                //await (await AppState.Current.BackgroundMediaServiceCurrent.PlayerInstance).SavePlaylistReorderAsync();
                (sender as AppBarToggleButton).Label = "Reorder";
            }
            else
            {
                ReorderMode = ListViewReorderMode.Enabled;
                IsChangingOrder = true;
                (sender as AppBarToggleButton).Label = "Save reorder";
            }
        }

        private void Files_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var tracks = "track";
            if (Files.Count != 1)
            {
                tracks += "s";
            }

            NumberOfSongsText = $"{Files.Count} {tracks}";
        }

        private async Task RenamePlaylistAsync(object arg)
        {
            if (CurrentPlaylistItem.Writeable)
            {
                await RenamePlaylistItemAsync();
            }
            else
            {
                await MessageDialogService.Instance.ShowMessageDialogAsync("This playlist is read only as it was automatically generated for you. As such, it can't be renamed", "About renaming...");
            }
        }


        private async Task RenamePlaylistItemAsync()
        {
            var invalidNameChars = System.IO.Path.GetInvalidFileNameChars();
            var inputDialog = new StringInputDialog("Rename playlist", "Insert a new name for your playlist");
            inputDialog.Validator = (s) => { return s.All(x => !invalidNameChars.Contains(x)); };
            await ContentDialogService.Instance.ShowAsync(inputDialog);

            var result = inputDialog.Result;
            if (result != null)
            {
                var item = CurrentPlaylistItem as PlaylistItem;
                //save old path before renaming
                var oldPath = item.BackstorePath;
                var renameResult = await PlaylistsService.RenamePlaylistAsync(item, result);
                if (renameResult == false)
                {
                    PopupHelper.ShowInfoMessage("Failed to rename playlist. Make sure there isn't another playlist with the same name, or that the name is not too long");
                }
                else
                {
                    PopupHelper.ShowSuccessDialog();
                    TitleBoxText = result;
                }
            }
        }


        public string TitleBoxText
        {
            get
            {
                return _TitleBoxText;
            }

            set
            {
                _TitleBoxText = value;
                NotifyPropertyChanged(nameof(TitleBoxText));
            }
        }

        public bool DeleteIsEnabled
        {
            get
            {
                return _DeleteIsEnabled;
            }

            set
            {
                _DeleteIsEnabled = value;
                NotifyPropertyChanged(nameof(DeleteIsEnabled));
            }
        }

        public bool RemoveSelectedIsEnabled
        {
            get
            {
                return _RemoveSelectedIsEnabled;
            }

            set
            {
                _RemoveSelectedIsEnabled = value;
                NotifyPropertyChanged(nameof(RemoveSelectedIsEnabled));
            }
        }

        public bool SelectButtonIsEnabled
        {
            get
            {
                return _SelectButtonIsEnabled;
            }

            set
            {
                _SelectButtonIsEnabled = value;
                NotifyPropertyChanged(nameof(SelectButtonIsEnabled));
            }
        }

        public ListViewSelectionMode ContentPresenterSelectionMode
        {
            get
            {
                return _COntentPresenterSelectionMode;
            }

            set
            {
                _COntentPresenterSelectionMode = value;
                NotifyPropertyChanged(nameof(ContentPresenterSelectionMode));
            }
        }

        bool renameEnabled;
        private bool disposedValue;

        public bool RenameIsEnabled
        {
            get
            {
                return renameEnabled;
            }
            set
            {
                renameEnabled = value;
                NotifyPropertyChanged(nameof(RenameIsEnabled));
            }
        }

        public async Task LoadState(PlaylistItem NavigationParameter)
        {
            await GlobalProgressBarUtilities.ShowProgressBar("Loading...");
            //await SavePlaylistItemChanges();
            Files.Clear();
            CurrentPlaylistItem = NavigationParameter;

            var playlist = NavigationParameter as PlaylistItem;
            playlistPath = playlist.BackstorePath;
            await LoadFromPlaylist(playlist);

            await GlobalProgressBarUtilities.HideProgressBar();

            _ = Files.ForEachAsync(4, (x) =>
             {
                 return x.LoadMediaThumbnailAsync(true);
             });
        }

        private async Task SavePlaylistItemChanges()
        {
            if (CurrentPlaylistItem is PlaylistItem)
            {
                var former = CurrentPlaylistItem as PlaylistItem;
                await former.SavePlaylistAsync(Files.Select(x => x.MediaData));
            }
        }

        private async Task SetMetadataElements()
        {
            var firstMds = (Files.FirstOrDefault())?.MediaData;
            if (firstMds != null)
            {
                var metadata = await firstMds.GetMetadataAsync();
                if (!metadata.HasSavedThumbnailFile())
                {
                    PlaylistCoverSource = AssetsPaths.PlaylistCoverPlaceholder;
                }
                else
                {
                    PlaylistCoverSource = metadata.SavedThumbnailFile;
                }
            }
        }

        private async Task LoadFromPlaylist(PlaylistItem model)
        {
            try
            {
                TitleBoxText = model.Title;

                var x = await model.GetMediaDataSourcesAsync();

                foreach (var ss in x)
                {
                    AddMediaDataToObservableCollection(ss);
                }

                Writeable = model.Writeable;

                RemoveSelectedIsEnabled = false;
                PlaylistCoverSource = await model.GetCoverSource();
            }
            catch
            {
                PopupHelper.ShowInfoMessage("Could not properly open file");
            }
        }

        private async Task PlayCLick(object? sender)
        {
            var PlayButton = sender as AppBarButton;
            PlayButton.IsEnabled = false;
            try
            {
                await AppState.Current.MediaServiceConnector.StartPlaybackFromBeginning(Files.Select(x => x.MediaData));
            }
            catch { }
            finally
            {
                PlayButton.IsEnabled = true;
            }
        }


        /// <summary>
        /// this is actually add to now playing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task AddToNowPlaying(object? sender)
        {
            var EnqueueButton = sender as AppBarButton;
            EnqueueButton.IsEnabled = false;
            try
            {
                await AppState.Current.MediaServiceConnector.EnqueueAtEnd(Files.Select(x => x.MediaData));
            }
            catch { }
            finally
            {
                EnqueueButton.IsEnabled = true;
            }
        }

        private async Task DeleteClick(object? sender)
        {
            await MessageDialogService.Instance.ShowMessageDialogAsync("Are you sure you want to delete this item?", "Confirm",
            new UICommand("Yes", new UICommandInvokedHandler(AcceptDelete)),
            new UICommand("No"));
        }

        private async void AcceptDelete(IUICommand command)
        {
            var mod = CurrentPlaylistItem as PlaylistItem;
            await PlaylistsService.RemovePlaylist(mod);

            SubmitNavigationEvent(typeof(FileFolderPickerPage), null);
        }

        private async Task RemoveSelectedItems(object? sender)
        {
            List<object> selectedItems = new List<object>();
            GetSelectedItemsRequest?.Invoke(this, selectedItems);
            foreach (var v in selectedItems)
            {
                if (v is MediaPlayerItemSourceUIWrapper)
                {
                    Files.Remove(v as MediaPlayerItemSourceUIWrapper);
                }
            }
            await SavePlaylistItemChanges();
            await SetMetadataElements();
        }

        private void SelectTapped(object? sender)
        {
            var button = (sender as AppBarToggleButton);
            if (ContentPresenterSelectionMode == ListViewSelectionMode.None)
            {
                button.Label = "Unselect";
                ContentPresenterSelectionMode = ListViewSelectionMode.Multiple;
            }
            else
            {
                button.Label = "Select";
                ContentPresenterSelectionMode = ListViewSelectionMode.None;
                //RemoveSelected.IsEnabled = false;
            }

            button.IsChecked = ContentPresenterSelectionMode == ListViewSelectionMode.Multiple;
        }

        private async Task AddToExistingPlaylist(object? sender)
        {
            var diag = new PlaylistPicker();
            var target = await diag.PickPlaylistAsync(PlaylistsService.Playlists);
            if (target != null)
            {
                await PlaylistsService.AddToPlaylist(target, Files.Select(x => x.MediaData));

                await MessageDialogService.Instance.ShowMessageDialogAsync("Added to playlist", "Success");
                if (CurrentPlaylistItem is PlaylistItem)
                {
                    if (target.BackstorePath == (CurrentPlaylistItem as PlaylistItem).BackstorePath)
                    {
                        Files.Clear();
                        var newmds = await target.GetMediaDataSourcesAsync();
                        foreach (var x in newmds)
                        {
                            AddMediaDataToObservableCollection(x);
                        }
                        await SetMetadataElements();
                    }
                }
            }
        }

        private void AddMediaDataToObservableCollection(IMediaPlayerItemSource x)
        {
            var mds = new MediaPlayerItemSourceUIWrapper(x, Dispatcher);
            Files.Add(mds);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                PlaylistsService.PlaylistItemChanged -= PlaybackService_PlaylistItemChanged;

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~PlaylistsDetailsViewModel()
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

    public enum DisplayItemType
    {
        PlaylistItem,
        StorageFile,
        JukeBoxItem
    }
}
