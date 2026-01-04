using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;

namespace MayazucMediaPlayer.FileSystemViews
{
    public class QuickPlayService
    {
        readonly FolderPicker folderPicker = FileFolderPickerService.GetFolderPicker();
        readonly FileOpenPicker filePicker = FileFolderPickerService.GetFileOpenPicker();

        public QuickPlayService() { }

        public async Task PlayFilesAsync()
        {
            Utilities.ConfigureFilePicker(filePicker, SupportedFileTypesConfiguration.AllFiles);

            filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            filePicker.ViewMode = PickerViewMode.List;

            var files = await filePicker.PickMultipleFilesAsync();
            if (files != null)
            {
                await ProcessAndPlayFiles(files.Select(x => x.ToFileInfo()));
            }
        }

        public async Task PlayShallowFolderAsync()
        {
            StorageFolder folder = await PickFolderAsync();

            if (folder != null)
            {
                var files = await folder.GetFilesAsync();
                await ProcessAndPlayFiles(files.Select(x => x.ToFileInfo()));
            }
        }

        public async Task PlayDeepFolderAsync()
        {
            StorageFolder folder = await PickFolderAsync();

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

                await ProcessAndPlayFiles(files.Select(x => x.ToFileInfo()));

            }
        }

        public async Task PlayItems(IEnumerable<IStorageItem> items)
        {
            FileInfoCollection collection = PrepareFileInfos(items);
            await ProcessAndPlayFiles(collection);
        }

        private static FileInfoCollection PrepareFileInfos(IEnumerable<IStorageItem> items)
        {
            FileInfoCollection collection = new FileInfoCollection();
            foreach (var item in items)
            {
                if (item is StorageFile)
                {
                    collection.Add((item as StorageFile).ToFileInfo());
                }
                if (item is StorageFolder)
                {
                    collection.AddRange((item as StorageFolder).ToDirectoryInfo().EnumerateFiles());
                }
            }

            return collection;
        }

        public async Task EnqueueItems(IEnumerable<IStorageItem> items)
        {
            FileInfoCollection collection = PrepareFileInfos(items);
            List<IMediaPlayerItemSource> itemsToPlay = await PrepareMediaSources(collection);
            await AppState.Current.MediaServiceConnector.EnqueueAtEnd(itemsToPlay);
        }

        public async Task AddItemsToPlaylist(IEnumerable<IStorageItem> items)
        {
            FileInfoCollection collection = PrepareFileInfos(items);
            List<IMediaPlayerItemSource> itemsToPlay = await PrepareMediaSources(collection);
            await PlaylistHelpers.AddItemsToPlaylistAsync(itemsToPlay, AppState.Current.Services.GetService<PlaylistsService>().Playlists);
        }

        private static async Task ProcessAndPlayFiles(IEnumerable<FileInfo> files)
        {
            List<IMediaPlayerItemSource> itemsToPlay = await PrepareMediaSources(files);

            await AppState.Current.MediaServiceConnector.StartPlaybackFromBeginning(itemsToPlay);
        }

        private static async Task<List<IMediaPlayerItemSource>> PrepareMediaSources(IEnumerable<FileInfo> files)
        {
            List<IMediaPlayerItemSource> itemsToPlay = new List<IMediaPlayerItemSource>();

            foreach (var f in files)
            {
                var fileOpenResult = await (new PickedFileItem(f)).GetMediaDataSourcesAsync();
                if (fileOpenResult.IsSuccess)
                    itemsToPlay.AddRange(fileOpenResult.Value);
            }

            return itemsToPlay;
        }

        private async Task<StorageFolder> PickFolderAsync()
        {
            folderPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            var folder = await folderPicker.PickSingleFolderAsync();
            return folder;
        }
    }
}
