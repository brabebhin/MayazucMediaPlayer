using CommunityToolkit.Mvvm.Input;
using FFmpegInteropX;
using FluentResults;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Playlists;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.VisualBasic;
using Microsoft.WindowsAPICodePack.Shell;
using Nito.AsyncEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace MayazucMediaPlayer.Controls
{
    public partial class PlaybackItemManagementUIService<T> : MainViewModelBase, IPlaybackItemManagementUIService where T : IMediaPlayerItemSourceProvder
    {
        bool playButtonIsEnabled = false;
        bool _EnqueueButtonIsEnabled = false;
        readonly AsyncLock _activationLock = new AsyncLock();

        public ListViewReorderMode ReorderMode
        {
            get
            {
                return reorderMode;
            }
            set
            {
                if (reorderMode == value) return;

                reorderMode = value;
                CanReorderItems = reorderMode == ListViewReorderMode.Enabled;
                CanSearch = !CanReorderItems;
                NotifyPropertyChanged(nameof(ReorderMode));
            }
        }

        /// <summary>
        /// Used to update listview properties
        /// </summary>
        public bool CanReorderItems
        {
            get
            {
                return _CanReorderItems;
            }
            set
            {
                if (_CanReorderItems == value) return;

                _CanReorderItems = value;
                NotifyPropertyChanged(nameof(CanReorderItems));
            }
        }

        /// <summary>
        /// Used for binding radio button (i.e. user input)
        /// </summary>
        public bool IsChangingOrder
        {
            get
            {
                return isChangingOrder;
            }
            set
            {
                if (isChangingOrder == value) return;

                isChangingOrder = value;
                NotifyPropertyChanged(nameof(IsChangingOrder));
                CanReorderItems = value;
                ReorderMode = value ? ListViewReorderMode.Enabled : ListViewReorderMode.Disabled;
            }
        }

        bool isReorderButtonEnabled = false;
        public bool IsReorderButtonEnabled
        {
            get => isReorderButtonEnabled;
            private set
            {
                if (isReorderButtonEnabled == value) return;

                isReorderButtonEnabled = value;
                NotifyPropertyChanged(nameof(IsReorderButtonEnabled));
            }
        }

        public event EventHandler<IEnumerable<ItemIndexRange>> SetSelectedItems;

        public event EventHandler<List<Object>> GetSelectedItemsRequest;



        ListViewSelectionMode selectionMode = ListViewSelectionMode.None;
        public ListViewSelectionMode SelectionMode
        {
            get
            {
                return selectionMode;
            }
            set
            {
                if (selectionMode == value) return;

                selectionMode = value;
                NotifyPropertyChanged(nameof(SelectionMode));
                SelectingPlayButtonsVisibility = selectionMode == ListViewSelectionMode.None ? Visibility.Collapsed : Visibility.Visible;
                NotSelectingPlayButtonsVisibility = selectionMode == ListViewSelectionMode.None ? Visibility.Visible : Visibility.Collapsed;
                NotifyPropertyChanged(nameof(NotSelectingPlayButtonsVisibility));
                NotifyPropertyChanged(nameof(SelectingPlayButtonsVisibility));
                IsReorderButtonEnabled = selectionMode == ListViewSelectionMode.None;
            }
        }

        public Visibility NotSelectingPlayButtonsVisibility
        {
            get;
            private set;
        } = Visibility.Visible;

        public Visibility SelectingPlayButtonsVisibility
        {
            get;
            private set;
        } = Visibility.Collapsed;

        public virtual IMediaPlayerItemSourceProvderCollection<T> Items
        {
            get;
            private set;
        } = new IMediaPlayerItemSourceProvderCollection<T>();

        public IEnumerable<T> CurrentViewItems
        {
            get
            {
                return FilterCollectionView.CurrentView.Cast<T>();
            }
        }

        public AdvancedCollectionView FilterCollectionView
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> PlayCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> PlayCommandOnlySelected
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> PlayCommandOnlyMusicFiles
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> PlayCommandOnlyVideoFiles
        {
            get;
            private set;
        }

        public IRelayCommand EnableSelection
        {
            get;
            private set;
        }

        public IRelayCommand<object> SelectAllCommandOnlyVideo
        {
            get;
            private set;
        }

        public IRelayCommand<object> SelectAllCommandOnlyAudio
        {
            get;
            private set;
        }

        public IRelayCommand<object> SelectAllCommandSelected
        {
            get;
            private set;
        }

        public IRelayCommand<object> UnselectAllCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> AddToNowPlayingCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> PlayNextCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> PlayStartingFromFileCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> PlayNextSingleFileCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> AddToNowPlayingCommandOnlySelected
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> AddToNowPlayingCommandOnlyAudio
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> AddToNowPlayingCommandOnlyVideo
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> GoToSingleItemPropertiesCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> SaveAsPlaylistCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> SaveAsPlaylistCommandOnlyMusic
        {
            get;
            private set;
        }


        public IAsyncRelayCommand<object> SaveAsPlaylistCommandOnlyVideo
        {
            get;
            private set;
        }


        public IAsyncRelayCommand<object> SaveAsPlaylistCommandOnlySelected
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> SaveAsPlaylistCommandOnlyunselected
        {
            get;
            private set;
        }


        public IAsyncRelayCommand RemoveSelectedCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand RemoveOnlyMusicCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand RemoveOnlyVideoCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> AddToExistingPlaylistCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> AddToExistingPlaylistCommandOnlyMusic
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> AddToExistingPlaylistCommandOnlyVideo
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> AddToExistingPlaylistCommandOnlySelected
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> AddToExistingPlaylistCommandOnlyUnselected
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> GoToNetworkPlaybackCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> ClearAllCommand
        {
            get; private set;
        }

        public IRelayCommand<object> RemoveSlidedItem
        {
            get;
            private set;
        }

        public IRelayCommand<object> ChangeSongOrderRequestCommand { get; private set; }
        public IAsyncRelayCommand<object> PlaySingleFileCommand { get; private set; }
        public IAsyncRelayCommand<object> EnqueueSingleFileCommand { get; private set; }
        public IAsyncRelayCommand<object> AddSingleFileToPlaylistCommand { get; private set; }
        public IRelayCommand<object> SingleItemCopyFilePath { get; private set; }
        public IRelayCommand<object> SingleItemCopyFileName { get; private set; }
        public IRelayCommand<object> CopyAlbum { get; private set; }
        public IRelayCommand<object> SingleItemCopyArtist { get; private set; }
        public IRelayCommand<object> SingleItemCopyGenre { get; private set; }
        public IAsyncRelayCommand<object> CopySingleFileToFolder { get; private set; }
        public IAsyncRelayCommand<object> CopyFileToClipboard { get; private set; }

        public bool PlayButtonIsEnabled
        {
            get
            {
                return playButtonIsEnabled;
            }
            set
            {
                if (playButtonIsEnabled == value) return;

                playButtonIsEnabled = value;
                NotifyPropertyChanged(nameof(PlayButtonIsEnabled));
            }
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

        bool _NpChangeOrderButton = true;
        public bool NpChangeOrderButton
        {
            get
            {
                return _NpChangeOrderButton;
            }
            set
            {
                if (_NpChangeOrderButton == value) return;

                _NpChangeOrderButton = value;
                NotifyPropertyChanged(nameof(NpChangeOrderButton));
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
                if (_CanSearch == value) return;
                _CanSearch = value;
                NotifyPropertyChanged(nameof(CanSearch));
            }
        }

        public PlaylistsService PlaylistsService { get; private set; }

        public int ItemsCount => Items.Count;

        public bool CommandBarEnabled { get; private set; }

        public PlaybackItemManagementUIService(DispatcherQueue dp,
            PlaybackSequenceService m,
            PlaylistsService playlistsService) : base(dp, m)
        {
            FilterCollectionView = new AdvancedCollectionView();
            PlaylistsService = playlistsService;

            InitializeMembers();
            NotifyPropertyChanged(nameof(FilterCollectionView));
        }

        private void CollectionViewModelInstance_NavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            SubmitNavigationEvent(e.PageType, e.Parameter);
        }

        private void InitializeMembers()
        {
            Items.CollectionChanged += Items_CollectionChanged;

            PlayCommand = new AsyncRelayCommand<object>(PlayClickCommandFunction);
            AddToNowPlayingCommand = new AsyncRelayCommand<object>(AddToNowPlayingCommandFunction);
            EnableSelection = new RelayCommand(SelectAllCommandFunction);
            SaveAsPlaylistCommand = new AsyncRelayCommand<object>(SaveAsPlaylistCommandFunction);
            RemoveSelectedCommand = new AsyncRelayCommand(RemoveSelectedCommandFunction);

            AddToExistingPlaylistCommand = new AsyncRelayCommand<object>(AddToExistingPlaylistCommandFunction);

            ClearAllCommand = new AsyncRelayCommand<object>(ClearContents);

            RemoveSlidedItem = new RelayCommand<object>(RemoveSlided);
            PlayCommandOnlySelected = new AsyncRelayCommand<object>(PlayCommandOnlySelectedInternal);
            PlayCommandOnlyMusicFiles = new AsyncRelayCommand<object>(PlayCommandOnlyMusicFilesInternal);
            PlayCommandOnlyVideoFiles = new AsyncRelayCommand<object>(PlayCommandOnlyVideoFilesInternal);

            AddToNowPlayingCommandOnlySelected = new AsyncRelayCommand<object>(AddToNowPlayingCommandOnlySelectedInternal);
            AddToNowPlayingCommandOnlyAudio = new AsyncRelayCommand<object>(AddToNowPlayingCommandOnlyAudioInternal);
            AddToNowPlayingCommandOnlyVideo = new AsyncRelayCommand<object>(AddToNowPlayingCommandOnlyVideoInternal);

            SaveAsPlaylistCommandOnlyMusic = new AsyncRelayCommand<object>(SaveAsPlaylistCommandOnlyMusicInternal);
            SaveAsPlaylistCommandOnlySelected = new AsyncRelayCommand<object>(SaveAsPlaylistCommandOnlySelectedInternal);
            SaveAsPlaylistCommandOnlyVideo = new AsyncRelayCommand<object>(SaveAsPlaylistCommandOnlyVideoInternal);
            SaveAsPlaylistCommandOnlyunselected = new AsyncRelayCommand<object>(SaveAsPlaylistCommandOnlyunselectedInternal);

            AddToExistingPlaylistCommandOnlyMusic = new AsyncRelayCommand<object>(AddToExistingPlaylistCommandOnlyMusicInternal);
            AddToExistingPlaylistCommandOnlySelected = new AsyncRelayCommand<object>(AddToExistingPlaylistCommandOnlySelectedInternal);
            AddToExistingPlaylistCommandOnlyVideo = new AsyncRelayCommand<object>(AddToExistingPlaylistCommandOnlyVideoInternal);
            AddToExistingPlaylistCommandOnlyUnselected = new AsyncRelayCommand<object>(AddToExistingPlaylistCommandOnlyUnselectedInternal);

            SelectAllCommandOnlyVideo = new RelayCommand<object>(SelectAllCommandOnlyVideoInternal);
            SelectAllCommandOnlyAudio = new RelayCommand<object>(SelectAllCommandOnlyAudioInternal);
            UnselectAllCommand = new RelayCommand<object>(UnselectAllCommandnternal);
            SelectAllCommandSelected = new RelayCommand<object>(SelectAllCommandSelectedInternal);

            RemoveOnlyMusicCommand = new AsyncRelayCommand(RemoveOnlyMusicCommandInternal);
            RemoveOnlyVideoCommand = new AsyncRelayCommand(RemoveOnlyVideoCommandInternal);

            FilterCollectionView.ItemsSource = Items;

            ChangeSongOrderRequestCommand = new RelayCommand<object>(ChangeOrderRequestCommandFunction);

            PlaySingleFileCommand = new AsyncRelayCommand<object>(PlaySingleFile);
            EnqueueSingleFileCommand = new AsyncRelayCommand<object>(EnqueueSingleFile);
            AddSingleFileToPlaylistCommand = new AsyncRelayCommand<object>(AddSingleFileToPlaylist);
            PlayNextCommand = new AsyncRelayCommand<object>(AddToNowPlayingNext);
            PlayNextSingleFileCommand = new AsyncRelayCommand<object>(PlayNextSingleFile);
            PlayStartingFromFileCommand = new AsyncRelayCommand<object>(PlayStartingFromFile);

            GoToSingleItemPropertiesCommand = new AsyncRelayCommand<object>(GoToItemProperties);

            SingleItemCopyFileName = new RelayCommand<object>((obj) =>
            {
                CopyMetadata(obj, x => x.DisplayName);
            });
            SingleItemCopyFilePath = new RelayCommand<object>((obj) =>
            {
                CopyMetadata(obj, x => x.Path);
            });
            CopyAlbum = new RelayCommand<object>((obj) =>
            {
                CopyMetadata(obj, x => x.Metadata.Album);
            });
            SingleItemCopyArtist = new RelayCommand<object>((obj) =>
            {
                CopyMetadata(obj, x => x.Metadata.Artist);
            });
            SingleItemCopyGenre = new RelayCommand<object>((obj) =>
            {
                CopyMetadata(obj, x => x.Metadata.Genre);
            });

            CopySingleFileToFolder = new AsyncRelayCommand<object>(CopyFileToFolderFunction);
            CopyFileToClipboard = new AsyncRelayCommand<object>(CopyFileToClipboardFunction);

            SetCommandBarEnabled();
        }

        private Task CopyFileToClipboardFunction(object arg)
        {
            try
            {
                var targetItem = (IMediaPlayerItemSourceProvder)arg;
                StringCollection paths = new StringCollection();
                paths.Add(targetItem.Path);
                System.Windows.Forms.Clipboard.SetFileDropList(paths);
            }
            catch
            {
            }

            return Task.CompletedTask;
        }

        private async Task CopyFileToFolderFunction(object arg)
        {
            try
            {
                var sourceItem = (IMediaPlayerItemSourceProvder)arg;
                var folderPicker = FileFolderPickerService.GetFolderPicker();
                var folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    _ = FileCopyService.CopyFilesToFolderAsync(new string[] { sourceItem.Path }, folder.Path);
                }
            }
            catch
            {

            }
        }

        private async Task PlayStartingFromFile(object arg)
        {
            var targetItem = (T)arg;

            await PlayFilesInternal(Items, Items.IndexOf(targetItem));
            await ClearListWithUserOptions();
        }

        private static void CopyMetadata(object obj, Func<T, string> propertySelector)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(propertySelector((T)obj));
            Clipboard.SetContent(dataPackage);
        }

        private async Task GoToItemProperties(object arg)
        {
            var file = (T)arg;
            var mediaItems = await file.GetMediaDataSourcesAsync();
            if (mediaItems.IsFailed) return;
            await FileDetailsPage.ShowForMediaData(new MediaPlayerItemSourceUIWrapper(mediaItems.Value.FirstOrDefault(), Dispatcher));
        }

        private async Task PlayNextSingleFile(object arg)
        {
            var file = (T)arg;
            await PlayFilesNext(new T[] { file });
        }

        private async Task AddSingleFileToPlaylist(object arg)
        {
            var file = (T)arg;
            await AddFilesToExistingPlaylist(new[] { file });
        }

        private async Task EnqueueSingleFile(object arg)
        {
            var file = (T)arg;
            await AddFilesToNowPlaying(new[] { file });
        }

        private async Task PlaySingleFile(object arg)
        {
            var file = (T)arg;
            await PlayFilesInternal(new[] { file }, 0);
        }

        private async void ChangeOrderRequestCommandFunction(object arg)
        {
            if (ReorderMode == ListViewReorderMode.Enabled)
            {
                ReorderMode = ListViewReorderMode.Disabled;
                await OnContentsChanged(Items.ToList().AsReadOnly());
            }
            else
            {
                ReorderMode = ListViewReorderMode.Enabled;
            }
        }

        private async Task RemoveOnlyMusicCommandInternal()
        {
            var selected = GetOnlyMusicFiles(CurrentViewItems).ToList();
            await RemoveItemsFromOriginalCollection(selected);
        }

        private async Task RemoveItemsFromOriginalCollection(List<T> selected)
        {
            using (await _activationLock.LockAsync())
            {
                foreach (var s in selected)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (Items[i].Equals(s))
                        {
                            Items.RemoveAt(i);
                        }
                    }
                }
            }

            await OnContentsChanged(Items.ToList().AsReadOnly());
        }

        private async Task RemoveOnlyVideoCommandInternal()
        {
            var selected = GetOnlyVideoFiles(CurrentViewItems).ToList();
            await RemoveItemsFromOriginalCollection(selected);
        }

        private void SelectAllCommandSelectedInternal(object arg)
        {
            SelectionMode = ListViewSelectionMode.Multiple;
            SetSelectedItems?.Invoke(this, CurrentViewItems.GetItemRanges(CurrentViewItems));
        }

        private void UnselectAllCommandnternal(object arg)
        {
            SelectionMode = ListViewSelectionMode.Multiple;
            SetSelectedItems?.Invoke(this, null);
        }

        private void SelectAllCommandOnlyAudioInternal(object arg)
        {
            SelectionMode = ListViewSelectionMode.Multiple;
            SetSelectedItems?.Invoke(this, CurrentViewItems.GetItemRanges(CurrentViewItems.Where(x => SupportedFileFormats.IsAudioFile(x.Path))));
        }

        private void SelectAllCommandOnlyVideoInternal(object arg)
        {
            SelectionMode = ListViewSelectionMode.Multiple;
            SetSelectedItems?.Invoke(this, CurrentViewItems.GetItemRanges(CurrentViewItems.Where(x => SupportedFileFormats.IsVideoFile(x.Path))));
        }

        private async Task AddToExistingPlaylistCommandOnlyUnselectedInternal(object arg)
        {
            var selected = GetSelectedItemsFromUI();
            await AddFilesToExistingPlaylist(CurrentViewItems.Except(selected));
        }

        private async Task AddToExistingPlaylistCommandOnlyVideoInternal(object arg)
        {
            var selected = GetOnlyVideoFiles(CurrentViewItems);
            await AddFilesToExistingPlaylist(selected);
        }

        private async Task AddToExistingPlaylistCommandOnlySelectedInternal(object arg)
        {
            var selected = GetSelectedItemsFromUI();
            await AddFilesToExistingPlaylist(selected);
        }

        private async Task AddToExistingPlaylistCommandOnlyMusicInternal(object arg)
        {
            var selected = GetOnlyMusicFiles(CurrentViewItems);
            await AddFilesToExistingPlaylist(selected);
        }

        private async Task SaveAsPlaylistCommandOnlyunselectedInternal(object arg)
        {
            var selected = GetSelectedItemsFromUI();
            await PrepareToSaveToPlaylist(null, CurrentViewItems.Except(selected));
        }

        private async Task SaveAsPlaylistCommandOnlyVideoInternal(object arg)
        {
            var selected = GetOnlyVideoFiles(CurrentViewItems);
            await PrepareToSaveToPlaylist(null, selected);
        }

        private async Task SaveAsPlaylistCommandOnlySelectedInternal(object arg)
        {
            var selected = GetSelectedItemsFromUI();
            await PrepareToSaveToPlaylist(null, selected);
        }

        private async Task SaveAsPlaylistCommandOnlyMusicInternal(object arg)
        {
            var selected = GetOnlyMusicFiles(CurrentViewItems);
            await PrepareToSaveToPlaylist(null, selected);
        }

        private async Task AddToNowPlayingNext(object arg)
        {
            await PlayFilesNext(CurrentViewItems);
            await ClearListWithUserOptions();
        }

        private async Task PlayFilesNext(IEnumerable<T> Items)
        {
            await AddFilesToNowPlayingInternal(Items, AppState.Current.MediaServiceConnector.EnqueueNext);
        }

        private async Task AddToNowPlayingCommandOnlyVideoInternal(object arg)
        {
            IEnumerable<T> selected = GetOnlyVideoFiles(CurrentViewItems);
            await AddFilesToNowPlaying(selected);
        }

        private async Task AddToNowPlayingCommandOnlyAudioInternal(object arg)
        {
            IEnumerable<T> selected = GetOnlyMusicFiles(CurrentViewItems);
            await AddFilesToNowPlaying(selected);
        }

        private async Task AddToNowPlayingCommandOnlySelectedInternal(object arg)
        {
            IEnumerable<T> selected = GetSelectedItemsFromUI();
            await AddFilesToNowPlaying(selected);
        }

        private async Task PlayCommandOnlyVideoFilesInternal(object arg)
        {
            IEnumerable<T> selected = GetOnlyVideoFiles(CurrentViewItems);
            await PlayFilesInternal(selected, 0);
        }

        private async Task PlayCommandOnlyMusicFilesInternal(object arg)
        {
            IEnumerable<T> selected = GetOnlyMusicFiles(CurrentViewItems);
            await PlayFilesInternal(selected, 0);
        }

        private async Task PlayCommandOnlySelectedInternal(object arg)
        {
            IEnumerable<T> selected = GetSelectedItemsFromUI();
            await PlayFilesInternal(selected, 0);
        }

        private IEnumerable<T> GetOnlyMusicFiles(IEnumerable<T> itemsToFilter)
        {
            return itemsToFilter.Where(x => SupportedFileFormats.IsAudioFile(x.Path));
        }

        private IEnumerable<T> GetOnlyVideoFiles(IEnumerable<T> itemsToFilter)
        {
            return itemsToFilter.Where(x => SupportedFileFormats.IsVideoFile(x.Path));
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PlayButtonIsEnabled = Items.Count != 0;
            EnqueueButtonIsEnabled = Items.Count != 0;
            IsReorderButtonEnabled = Items.Count != 0;
            SetCommandBarEnabled();
        }

        private void SetCommandBarEnabled()
        {
            CommandBarEnabled = Items.Count != 0;
            NotifyPropertyChanged(nameof(CommandBarEnabled));
        }

        private void RemoveSlided(object? sender)
        {
            var x = (T)sender;
            Items.RemoveWithLock<T>(x, Items);
            OnContentsChanged(Items.ToList().AsReadOnly());
        }

        private async Task ClearContents(object? sender)
        {
            using (await _activationLock.LockAsync())
            {
                Items.Clear();
            }
        }

        private async Task PlayClickCommandFunction(object? sender)
        {
            await PlayFilesInternal(CurrentViewItems, 0);
            await ClearListWithUserOptions();
        }

        private async Task ClearListWithUserOptions()
        {
            using (await _activationLock.LockAsync())
            {
                Items.Clear();
            }
        }

        protected virtual async Task PlayFilesInternal(IEnumerable<T> Items, int startIndex)
        {
            if (!Items.Any())
            {
                return;
            }


            var mediaSourcesToPlay = await PrepareForPlayback(Items);
            if (mediaSourcesToPlay.IsSuccess)
            {
                if (startIndex == 0)
                {
                    await AppState.Current.MediaServiceConnector.StartPlaybackFromBeginning(mediaSourcesToPlay.Value);
                }
                else
                {
                    await AppState.Current.MediaServiceConnector.StartPlaybackFromIndexAndPosition(mediaSourcesToPlay.Value, startIndex, 0);
                }
            }
        }

        private async Task AddToNowPlayingCommandFunction(object? sender)
        {
            await AddFilesToNowPlaying(CurrentViewItems);
            await ClearListWithUserOptions();
        }

        private async Task AddFilesToNowPlaying(IEnumerable<T> Items)
        {
            await AddFilesToNowPlayingInternal(Items, AppState.Current.MediaServiceConnector.EnqueueAtEnd);
        }

        private async Task AddFilesToNowPlayingInternal(IEnumerable<T> Items, Func<IEnumerable<IMediaPlayerItemSource>, Task> playerServiceRequest)
        {
            if (!Items.Any())
            {
                return;
            }
            try
            {
                EnqueueButtonIsEnabled = false;
                PlayButtonIsEnabled = false;
                var mediaSourcesToPlay = await PrepareForPlayback(Items);
                if (mediaSourcesToPlay.IsSuccess)
                {
                    await playerServiceRequest(mediaSourcesToPlay.Value);
                }
            }
            finally
            {
                EnqueueButtonIsEnabled = true;
                PlayButtonIsEnabled = true;
            }
        }

        private async Task<Result<IEnumerable<IMediaPlayerItemSource>>> PrepareForPlayback(IEnumerable<T> itemsToPrepare)
        {
            MediaPlayerItemSourceList mediaSourcesToPlay = new MediaPlayerItemSourceList();
            List<string> brokenFiles = new List<string>();

            foreach (T file in itemsToPrepare)
            {
                var sourceResult = await file.GetMediaDataSourcesAsync();
                if (sourceResult.IsSuccess)
                {
                    mediaSourcesToPlay.AddRange(sourceResult.Value);
                }
                else
                {
                    brokenFiles.Add(file.DisplayName);
                }
            }

            bool continuee = await SignalBrokenFiles(brokenFiles);
            if (continuee)
            {
                return Result.Ok(mediaSourcesToPlay.AsEnumerable());
            }
            else
            {
                return Result.Fail("Operation canceled");
            }
        }

        private async Task<bool> SignalBrokenFiles(List<string> brokenFiles)
        {
            bool continuee = true;
            if (brokenFiles.Count > 0)
            {
                StringBuilder errorBuilder = new StringBuilder("The following files were ignored, either because they are not supported or because they are broken: " + Environment.NewLine);
                foreach (var v in brokenFiles)
                {
                    errorBuilder.AppendLine(v);
                }
                errorBuilder.AppendLine("Do you wish to continue?");
                await MessageDialogService.Instance.ShowMessageDialogAsync(errorBuilder.ToString(), "Some files are not supported in this operation",
                new UICommand("Continue"),
                new UICommand("Cancel", (s) => { continuee = false; }));
            }

            return continuee;
        }

        private async Task AddToExistingPlaylistCommandFunction(object? sender)
        {
            await AddFilesToExistingPlaylist(CurrentViewItems);
        }

        private async Task AddFilesToExistingPlaylist(IEnumerable<T> Items)
        {
            PlaylistPicker picker = new PlaylistPicker();
            var target = await picker.PickPlaylistAsync(PlaylistsService.Playlists);

            if (target != null)
            {
                await PrepareToSaveToPlaylist(target, Items);
                await MessageDialogService.Instance.ShowMessageDialogAsync("Added to playlist", "Success");
            }
        }

        private async Task PrepareToSaveToPlaylist(PlaylistItem item, IEnumerable<T> Items)
        {
            if (!Items.Any())
            {
                return;
            }
            List<string> brokenFiles = new List<string>();

            var mediaSourcesToPlay = await PrepareForPlayback(Items);

            if (item == null && mediaSourcesToPlay.IsSuccess)
            {
                var diag = new StringInputDialog("Playlist name", "Pick a playlist name");
                await ContentDialogService.Instance.ShowAsync(diag);

                var PlayListName = diag.Result;
                if (PlayListName == null)
                {
                    return;
                }

                await PlaylistsService.AddPlaylist(PlayListName, mediaSourcesToPlay.Value);

                PopupHelper.ShowSuccessDialog();

            }
            else if (mediaSourcesToPlay.IsSuccess)
            {
                await PlaylistsService.AddToPlaylist(item, mediaSourcesToPlay.Value);
            }
        }

        private void SelectAllCommandFunction()
        {
            if (SelectionMode == ListViewSelectionMode.None)
            {
                SelectionMode = ListViewSelectionMode.Multiple;
            }
            else
            {
                SelectionMode = ListViewSelectionMode.None;
            }
        }

        private async Task SaveAsPlaylistCommandFunction(object? sender)
        {
            await PrepareToSaveToPlaylist(null, CurrentViewItems);
        }

        private async Task RemoveSelectedCommandFunction()
        {
            IEnumerable<T> selected = GetSelectedItemsFromUI();
            await RemoveItemsFromOriginalCollection(selected.ToList());
        }

        protected IEnumerable<T> GetSelectedItemsFromUI()
        {
            var selected = new List<object>();
            GetSelectedItemsRequest?.Invoke(this, selected);
            return selected.Cast<T>();
        }

        private async Task PlayFile(T file)
        {
            if (SelectionMode == ListViewSelectionMode.None && !CanReorderItems)
            {
                await PlayFilesInternal(new T[] { file }, 0);
            }
        }

        /// <summary>
        /// when items are added
        /// </summary>
        /// <param name="addedContent"></param>
        /// <returns></returns>
        protected virtual Task OnContentAdded(ReadOnlyCollection<T> addedContent)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// When items order is changed or items are removed
        /// </summary>
        /// <param name="newContent">The list of contents after removal or reorder</param>
        /// <returns></returns>
        protected virtual Task OnContentsChanged(ReadOnlyCollection<T> newContent)
        {
            return Task.CompletedTask;
        }

        private ListViewReorderMode reorderMode = ListViewReorderMode.Disabled;
        private bool _CanReorderItems;
        private bool isChangingOrder;
    }
}
