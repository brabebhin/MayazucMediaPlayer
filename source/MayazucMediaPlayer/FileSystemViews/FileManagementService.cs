﻿using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.UI;
using FFmpegInteropX;
using FluentResults;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Playlists;
using MayazucMediaPlayer.Runtime;
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


namespace MayazucMediaPlayer.FileSystemViews
{
    public class FileManagementService : MainViewModelBase
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
                isReorderButtonEnabled = value;
                NotifyPropertyChanged(nameof(IsReorderButtonEnabled));
            }
        }

        bool fileFolderPickerBusy = false;
        public event EventHandler<IEnumerable<ItemIndexRange>> SetSelectedItems;

        public event EventHandler<List<Object>> GetSelectedItemsRequest;

        readonly FileOpenPicker filePicker;
        readonly FolderPicker folderPicker;

        ListViewSelectionMode selectionMode = ListViewSelectionMode.None;
        public ListViewSelectionMode SelectionMode
        {
            get
            {
                return selectionMode;
            }
            set
            {
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

        public IMediaPlayerItemSourceProvderCollection Items
        {
            get;
            private set;
        }

        public IEnumerable<IMediaPlayerItemSourceProvder> CurrentViewItems
        {
            get
            {
                return (FilterCollectionView.Source as IEnumerable).Cast<IMediaPlayerItemSourceProvder>();
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

        public IAsyncRelayCommand<object> OpenFilesCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> OpenFoldersCommand
        {
            get; private set;

        }

        public IAsyncRelayCommand<object> ClearAllCommand
        {
            get; private set;
        }

        public IAsyncRelayCommand<object> AddDeepFoldersCommand
        {
            get;
            private set;
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
                _CanSearch = value;
                NotifyPropertyChanged(nameof(CanSearch));
            }
        }

        public PlaylistsService PlaylistsService { get; private set; }


        public FileManagementService(DispatcherQueue dp,
            PlaybackSequenceService m,
            PlaylistsService playlistsService) : base(dp, m)
        {
            FilterCollectionView = new AdvancedCollectionView(Items);
            PlaylistsService = playlistsService;

            folderPicker = FileFolderPickerService.GetFolderPicker();
            filePicker = FileFolderPickerService.GetFileOpenPicker();

            InitializeMembers();
            NotifyPropertyChanged(nameof(FilterCollectionView));
        }

        private void CollectionViewModelInstance_NavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            SubmitNavigationEvent(e.PageType, e.Parameter);
        }

        private void InitializeMembers()
        {
            Items = new IMediaPlayerItemSourceProvderCollection();
            Items.CollectionChanged += Items_CollectionChanged;
            ConfigureFilePickers();


            PlayCommand = new AsyncRelayCommand<object>(PlayClickCommandFunction);
            AddToNowPlayingCommand = new AsyncRelayCommand<object>(AddToNowPlayingCommandFunction);
            EnableSelection = new RelayCommand(SelectAllCommandFunction);
            SaveAsPlaylistCommand = new AsyncRelayCommand<object>(SaveAsPlaylistCommandFunction);
            RemoveSelectedCommand = new AsyncRelayCommand(RemoveSelectedCommandFunction);

            AddToExistingPlaylistCommand = new AsyncRelayCommand<object>(AddToExistingPlaylistCommandFunction);

            ClearAllCommand = new AsyncRelayCommand<object>(ClearContents);
            OpenFilesCommand = new AsyncRelayCommand<object>(OpenFilesAsync);

            OpenFoldersCommand = new AsyncRelayCommand<object>(OpenFolderAsync);

            AddDeepFoldersCommand = new AsyncRelayCommand<object>(OpenDeepFolderAsync);

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

            FilterCollectionView.Source = Items;

            ChangeSongOrderRequestCommand = new RelayCommand<object>(ChangeSongRequestCommandFunction);

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
        }

        private Task CopyFileToClipboardFunction(object arg)
        {
            try
            {
                var targetItem = arg as IMediaPlayerItemSourceProvder;
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
                var sourceItem = arg as IMediaPlayerItemSourceProvder;
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
            var targetItem = arg as IMediaPlayerItemSourceProvder;

            await PlayFilesInternal(Items, Items.IndexOf(targetItem));
            await ClearListWithUserOptions();
        }

        private static void CopyMetadata(object obj, Func<IMediaPlayerItemSourceProvder, string> propertySelector)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(propertySelector((IMediaPlayerItemSourceProvder)obj));
            Clipboard.SetContent(dataPackage);
        }

        private async Task GoToItemProperties(object arg)
        {
            var file = (IMediaPlayerItemSourceProvder)arg;
            var mediaItems = await file.GetMediaDataSourcesAsync();
            if (mediaItems.IsFailed) return;
            await FileDetailsPage.ShowForMediaData(new MediaPlayerItemSourceUIWrapper(mediaItems.Value.FirstOrDefault(), Dispatcher));
        }

        private async Task PlayNextSingleFile(object arg)
        {
            var file = (IMediaPlayerItemSourceProvder)arg;
            await PlayFilesNext(new IMediaPlayerItemSourceProvder[] { file });
        }

        private async Task AddSingleFileToPlaylist(object arg)
        {
            var file = (IMediaPlayerItemSourceProvder)arg;
            await AddFilesToExistingPlaylist(new[] { file });
        }

        private async Task EnqueueSingleFile(object arg)
        {
            var file = (IMediaPlayerItemSourceProvder)arg;
            await AddFilesToNowPlaying(new[] { file });
        }

        private async Task PlaySingleFile(object arg)
        {
            var file = (IMediaPlayerItemSourceProvder)arg;
            await PlayFilesInternal(new[] { file }, 0);
        }

        private void ChangeSongRequestCommandFunction(object arg)
        {
            if (ReorderMode == ListViewReorderMode.Enabled)
            {
                ReorderMode = ListViewReorderMode.Disabled;
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

        private async Task RemoveItemsFromOriginalCollection(List<IMediaPlayerItemSourceProvder> selected)
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

        private async Task PlayFilesNext(IEnumerable<IMediaPlayerItemSourceProvder> Items)
        {
            await AddFilesToNowPlayingInternal(Items, AppState.Current.MediaServiceConnector.EnqueueNext);
        }

        private async Task AddToNowPlayingCommandOnlyVideoInternal(object arg)
        {
            IEnumerable<IMediaPlayerItemSourceProvder> selected = GetOnlyVideoFiles(CurrentViewItems);
            await AddFilesToNowPlaying(selected);
        }

        private async Task AddToNowPlayingCommandOnlyAudioInternal(object arg)
        {
            IEnumerable<IMediaPlayerItemSourceProvder> selected = GetOnlyMusicFiles(CurrentViewItems);
            await AddFilesToNowPlaying(selected);
        }

        private async Task AddToNowPlayingCommandOnlySelectedInternal(object arg)
        {
            IEnumerable<IMediaPlayerItemSourceProvder> selected = GetSelectedItemsFromUI();
            await AddFilesToNowPlaying(selected);
        }

        private async Task PlayCommandOnlyVideoFilesInternal(object arg)
        {
            IEnumerable<IMediaPlayerItemSourceProvder> selected = GetOnlyVideoFiles(CurrentViewItems);
            await PlayFilesInternal(selected, 0);
        }

        private async Task PlayCommandOnlyMusicFilesInternal(object arg)
        {
            IEnumerable<IMediaPlayerItemSourceProvder> selected = GetOnlyMusicFiles(CurrentViewItems);
            await PlayFilesInternal(selected, 0);
        }

        private async Task PlayCommandOnlySelectedInternal(object arg)
        {
            IEnumerable<IMediaPlayerItemSourceProvder> selected = GetSelectedItemsFromUI();
            await PlayFilesInternal(selected, 0);
        }

        private IEnumerable<IMediaPlayerItemSourceProvder> GetOnlyMusicFiles(IEnumerable<IMediaPlayerItemSourceProvder> itemsToFilter)
        {
            return itemsToFilter.Where(x => SupportedFileFormats.IsAudioFile(x.Path));
        }

        private IEnumerable<IMediaPlayerItemSourceProvder> GetOnlyVideoFiles(IEnumerable<IMediaPlayerItemSourceProvder> itemsToFilter)
        {
            return itemsToFilter.Where(x => SupportedFileFormats.IsVideoFile(x.Path));
        }

        private void Items_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PlayButtonIsEnabled = Items.Count != 0;
            EnqueueButtonIsEnabled = Items.Count != 0;
            IsReorderButtonEnabled = Items.Count != 0;
        }

        private void RemoveSlided(object? sender)
        {
            var x = sender as IMediaPlayerItemSourceProvder;
            Items.Remove(x, Items);
        }

        private async Task OpenDeepFolderAsync(object arg)
        {
            folderPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            var folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                await GlobalProgressBarUtilities.ShowProgressBar("Searching...");

                QueryOptions opts = new QueryOptions();
                foreach (var str in SupportedFileFormats.AllSupportedFileFormats)
                {
                    opts.FileTypeFilter.Add(str);
                }

                opts.FolderDepth = FolderDepth.Deep;
                opts.IndexerOption = IndexerOption.UseIndexerWhenAvailable;

                var targetFolder = folder;
                var qfiles = targetFolder.CreateFileQuery();
                qfiles.ApplyNewQueryOptions(opts);

                var files = await qfiles.GetFilesAsync();
                await OpenFilesInternalAsync(files.Select(x => x.ToFileInfo()));

                await GlobalProgressBarUtilities.HideProgressBar();
            }
        }

        private void ConfigureFilePickers()
        {
            try
            {
                Utilities.ConfigureFilePicker(filePicker, SupportedFileTypesConfiguration.AllFiles);

                filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
                filePicker.ViewMode = PickerViewMode.List;

                foreach (string ext in SupportedFileFormats.AllSupportedFileFormats)
                {
                    filePicker.FileTypeFilter.Add(ext);
                    folderPicker.FileTypeFilter.Add(ext);
                }
            }
            catch { }
        }


        private async Task OpenFilesAsync(object? sender)
        {
            if (!fileFolderPickerBusy)
            {
                try
                {
                    fileFolderPickerBusy = true;

                    var files = await filePicker.PickMultipleFilesAsync();

                    if (files != null)
                    {
                        await OpenFilesInternalAsync(files.Select(x => x.ToFileInfo()));
                    }
                }
                finally
                {
                    fileFolderPickerBusy = false;
                }
            }
        }

        private async Task OpenFolderAsync(object? sender)
        {
            if (!fileFolderPickerBusy)
            {
                try
                {
                    fileFolderPickerBusy = true;
                    folderPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
                    var folder = await folderPicker.PickSingleFolderAsync();
                    if (folder != null)
                    {
                        await OpenFolderAsync((folder));
                    }
                }
                finally
                {
                    fileFolderPickerBusy = false;
                }
            }
        }

        private async Task ClearContents(object? sender)
        {
            using (await _activationLock.LockAsync())
            {
                Items.Clear();
            }
        }

        public async Task OpenFolderAsync(StorageFolder args)
        {
            if (args != null)
            {
                var files = await args.GetFilesAsync();

                await OpenFilesInternalAsync(files.Select(x => x.ToFileInfo()));
            }
        }

        private async Task OpenFilesInternalAsync(IEnumerable<FileInfo> args)
        {
            if (args.Any())
            {
                var validItems = new List<IMediaPlayerItemSourceProvder>();

                await Task.Run(() =>
                {
                    var totalFiles = new List<FileInfo>(args);
                    foreach (var file in totalFiles)
                    {
                        var ext = file.Extension.ToLowerInvariant();
                        if (SupportedFileFormats.AllSupportedFileFormats.Contains(ext))
                        {
                            var x = new PickedFileItem(file);
                            validItems.Add(x);
                        }
                    }
                });
                using (await _activationLock.LockAsync())
                {
                    var cacheOnlyAa = SettingsService.Instance.OnlyUseCacheInFilePicker;
                    Items.AddRange(validItems);
                }
            }
        }

        private async Task PlayClickCommandFunction(object? sender)
        {
            await PlayFilesInternal(CurrentViewItems, 0);
            await ClearListWithUserOptions();
        }

        private async Task ClearListWithUserOptions()
        {
            if (SettingsService.Instance.AutoClearFilePicker)
            {
                using (await _activationLock.LockAsync())
                {
                    Items.Clear();
                }
            }
        }

        private async Task PlayFilesInternal(IEnumerable<IMediaPlayerItemSourceProvder> Items, int startIndex)
        {
            try
            {
                if (!Items.Any())
                {
                    return;
                }

                await GlobalProgressBarUtilities.ShowProgressBar("Loading...");
                PlayButtonIsEnabled = false;

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
            finally
            {
                PlayButtonIsEnabled = true;

                await GlobalProgressBarUtilities.HideProgressBar();
                //Frame.BackStack.Clear();
            }
        }

        private async Task AddToNowPlayingCommandFunction(object? sender)
        {
            await AddFilesToNowPlaying(CurrentViewItems);
            await ClearListWithUserOptions();
        }

        private async Task AddFilesToNowPlaying(IEnumerable<IMediaPlayerItemSourceProvder> Items)
        {
            await AddFilesToNowPlayingInternal(Items, AppState.Current.MediaServiceConnector.EnqueueAtEnd);
        }

        private async Task AddFilesToNowPlayingInternal(IEnumerable<IMediaPlayerItemSourceProvder> Items, Func<IEnumerable<IMediaPlayerItemSource>, Task> playerServiceRequest)
        {
            if (!Items.Any())
            {
                return;
            }
            try
            {
                await GlobalProgressBarUtilities.ShowProgressBar("Loading...");
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
                await GlobalProgressBarUtilities.HideProgressBar();
            }
        }

        private async Task<Result<IEnumerable<IMediaPlayerItemSource>>> PrepareForPlayback(IEnumerable<IMediaPlayerItemSourceProvder> itemsToPrepare)
        {
            MediaPlayerItemSourceList mediaSourcesToPlay = new MediaPlayerItemSourceList();
            List<string> brokenFiles = new List<string>();

            foreach (IMediaPlayerItemSourceProvder file in itemsToPrepare)
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

        private async Task AddFilesToExistingPlaylist(IEnumerable<IMediaPlayerItemSourceProvder> Items)
        {
            await GlobalProgressBarUtilities.ShowProgressBar("Saving");
            PlaylistPicker picker = new PlaylistPicker();
            var target = await picker.PickPlaylistAsync(PlaylistsService.Playlists);

            if (target != null)
            {
                await PrepareToSaveToPlaylist(target, Items);
                await MessageDialogService.Instance.ShowMessageDialogAsync("Added to playlist", "Success");
            }
            await GlobalProgressBarUtilities.HideProgressBar();
        }

        private async Task PrepareToSaveToPlaylist(PlaylistItem item, IEnumerable<IMediaPlayerItemSourceProvder> Items)
        {
            try
            {
                await GlobalProgressBarUtilities.ShowProgressBar("Preparing...");
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
            finally
            {
                await GlobalProgressBarUtilities.HideProgressBar();
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
            IEnumerable<IMediaPlayerItemSourceProvder> selected = GetSelectedItemsFromUI();
            await RemoveItemsFromOriginalCollection(selected.ToList());
        }

        private IEnumerable<IMediaPlayerItemSourceProvder> GetSelectedItemsFromUI()
        {
            var selected = new List<object>();
            GetSelectedItemsRequest?.Invoke(this, selected);
            return selected.Cast<IMediaPlayerItemSourceProvder>();
        }

        public async Task HandleDragDropActivation(IEnumerable<FileInfo> _items)
        {
            if (_items.Any())
            {
                await OpenFilesInternalAsync(_items);
            }
        }

        private async Task HandleCombinedStorageFileFolderActivation(IEnumerable<IStorageItem> _items)
        {
            foreach (var item in _items)
            {
                if (item is StorageFolder)
                {
                    var folder = item as StorageFolder;
                    var files = await folder.GetFilesAsync();
                    await OpenFilesInternalAsync(files.Select(x => x.ToFileInfo()));
                }
                else if (item is StorageFile)
                {
                    var file = item as StorageFile;

                    await OpenFilesInternalAsync(new FileInfo[] { file.ToFileInfo() });
                }
            }
        }

        private async Task PlayFile(IMediaPlayerItemSourceProvder file)
        {
            if (SelectionMode == ListViewSelectionMode.None && !CanReorderItems)
            {
                await PlayFilesInternal(new IMediaPlayerItemSourceProvder[] { file }, 0);
            }
        }

        private ListViewReorderMode reorderMode = ListViewReorderMode.Disabled;
        private bool _CanReorderItems;
        private bool isChangingOrder;
    }
}
