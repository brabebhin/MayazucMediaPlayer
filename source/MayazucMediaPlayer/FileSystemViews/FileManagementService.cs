using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;


namespace MayazucMediaPlayer.FileSystemViews
{
    public partial class FileManagementService : PlaybackItemManagementUIService<PickedFileItem>
    {
        public IAsyncRelayCommand<object> OpenFilesCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> OpenFoldersCommand
        {
            get; private set;
        }

        public IAsyncRelayCommand<object> AddDeepFoldersCommand
        {
            get;
            private set;
        }

        readonly FileOpenPicker filePicker;
        readonly FolderPicker folderPicker;

        private IMediaPlayerItemSourceProvderCollection<PickedFileItem> files = new IMediaPlayerItemSourceProvderCollection<PickedFileItem>();

        public override IMediaPlayerItemSourceProvderCollection<PickedFileItem> Items => files;

        public FileManagementService(DispatcherQueue dispatcherQueue,
            PlaybackSequenceService playbackSequenceService,
            PlaylistsService playlistsService, Func<bool> shouldClearItemsCallback) : base(dispatcherQueue, playbackSequenceService, playlistsService, shouldClearItemsCallback)
        {
            folderPicker = FileFolderPickerService.GetFolderPicker();
            filePicker = FileFolderPickerService.GetFileOpenPicker();

            InitializeMembers();
            NotifyPropertyChanged(nameof(FilterCollectionView));
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

        private void CollectionViewModelInstance_NavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            SubmitNavigationEvent(e.PageType, e.Parameter);
        }

        private void InitializeMembers()
        {
            ConfigureFilePickers();

            OpenFilesCommand = new AsyncRelayCommand<object>(OpenFilesAsync);

            OpenFoldersCommand = new AsyncRelayCommand<object>(OpenFolderAsync);

            AddDeepFoldersCommand = new AsyncRelayCommand<object>(OpenDeepFolderAsync);
            FilterCollectionView.ItemsSource = Items;
        }

        private async Task OpenDeepFolderAsync(object arg)
        {
            folderPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            var folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
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
            }
        }

        private async Task OpenFilesAsync(object? sender)
        {
            try
            {
                var files = await filePicker.PickMultipleFilesAsync();

                if (files != null)
                {
                    await OpenFilesInternalAsync(files.Select(x => x.ToFileInfo()));
                }
            }
            finally
            {
            }
        }

        private async Task OpenFolderAsync(object? sender)
        {
            try
            {
                folderPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
                var folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    await OpenFolderAsync((folder));
                }
            }
            finally
            {
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
                var validItems = new List<PickedFileItem>();

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

                files.AddRange(validItems);

                await OnContentAdded(validItems.AsReadOnly());
            }
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

        /// <summary>
        /// when items are added
        /// </summary>
        /// <param name="addedContent"></param>
        /// <returns></returns>
        protected virtual Task OnContentAdded(ReadOnlyCollection<IMediaPlayerItemSourceProvder> addedContent)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// When items order is changed or items are removed
        /// </summary>
        /// <param name="newContent">The list of contents after removal or reorder</param>
        /// <returns></returns>
        protected virtual Task OnContentsChanged(ReadOnlyCollection<IMediaPlayerItemSourceProvder> newContent)
        {
            return Task.CompletedTask;
        }

        private ListViewReorderMode reorderMode = ListViewReorderMode.Disabled;
        private bool _CanReorderItems;
        private bool isChangingOrder;
    }
}
