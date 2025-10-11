using CommunityToolkit.Mvvm.Input;
using FluentResults;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.Runtime;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MayazucMediaPlayer.Playlists
{
    public partial class PlaylistsDetailsService : PlaybackItemManagementUIService<PlayListItemMediaSourceProvider>, IDisposable
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
                if (_NumberOfSongsText == value) return;
                _NumberOfSongsText = value;
                NotifyPropertyChanged(nameof(NumberOfSongsText));
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
                if (_PlaylistCoverSource == value) return;
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

        public PlaylistItem CurrentPlaylistItem
        {
            get;
            private set;
        }

        public IRelayCommand<object> DeleteCommand
        {
            get; private set;
        }

        public IRelayCommand<object> RenamePlaylistCommand
        {
            get;
            private set;
        }


        public PlaylistsDetailsService(DispatcherQueue dp, PlaybackSequenceService m, PlaylistsService playlistsService) : base(dp, m, playlistsService)
        {
            base.Items.CollectionChanged += Files_CollectionChanged;

            DeleteCommand = new AsyncRelayCommand<object>(DeleteClick);
            RenamePlaylistCommand = new AsyncRelayCommand<object>(RenamePlaylistAsync);

            PlaylistsService.PlaylistItemChanged += PlaybackService_PlaylistItemChanged;
        }


        private async void PlaybackService_PlaylistItemChanged(object? sender, string e)
        {
            if (CurrentPlaylistItem == sender)
            {
                await LoadState(CurrentPlaylistItem);
            }
        }


        private void Files_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var tracks = "track";
            if (base.Items.Count != 1)
            {
                tracks += "s";
            }

            NumberOfSongsText = $"{base.Items.Count} {tracks}";
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
                if (_TitleBoxText == value) return;

                _TitleBoxText = value;
                NotifyPropertyChanged(nameof(TitleBoxText));
            }
        }


        public async Task LoadState(PlaylistItem NavigationParameter)
        {
            //await SavePlaylistItemChanges();
            base.Items.Clear();
            CurrentPlaylistItem = NavigationParameter;

            var playlist = NavigationParameter as PlaylistItem;
            playlistPath = playlist.BackstorePath;
            await LoadFromPlaylist(playlist);
        }

        private async Task SavePlaylistItemChanges(IEnumerable<IMediaPlayerItemSourceProvder> items)
        {
            var former = CurrentPlaylistItem as PlaylistItem;
            List<IMediaPlayerItemSource> sources = new List<IMediaPlayerItemSource>();
            foreach (var item in items)
            {
                var result = await item.GetMediaDataSourcesAsync();
                if (result.IsSuccess)
                    sources.AddRange(result.Value);
            }
            await former.SavePlaylistAsync(sources);
        }

        private void SetMetadataElements()
        {
            var firstMds = Items.FirstOrDefault();
            if (firstMds != null)
            {
                var metadata = firstMds.Metadata;
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
                    base.Items.Add(new PlayListItemMediaSourceProvider(ss));
                }

                PlaylistCoverSource = await model.GetCoverSource();
            }
            catch
            {
                PopupHelper.ShowInfoMessage("Could not properly open file");
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

        protected override Task OnContentsChanged(ReadOnlyCollection<PlayListItemMediaSourceProvider> newContent)
        {
            base.Dispatcher.TryEnqueue(async () =>
            {
                await SavePlaylistItemChanges(newContent);
            });

            return Task.CompletedTask;
        }

        private bool disposedValue;
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
        ~PlaylistsDetailsService()
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

    public partial class PlayListItemMediaSourceProvider : IMediaPlayerItemSourceProviderBase, IMediaPlayerItemSourceProvder
    {
        private readonly IMediaPlayerItemSource data;
        private readonly Task<EmbeddedMetadataResult> metadataTask;

        public string DisplayName => data.Title;

        public string ImageUri
        {
            get
            {
                if (metadataTask.IsCompletedSuccessfully)
                    return metadataTask.Result.SavedThumbnailFile;
                else return EmbeddedMetadataResolver.GetDefaultMetadataForFile(data.MediaPath).SavedThumbnailFile;
            }
        }

        public Guid ItemID => data.ID;

        public EmbeddedMetadataResult Metadata
        {
            get
            {
                if (metadataTask.IsCompletedSuccessfully)
                    return metadataTask.Result;
                return EmbeddedMetadataResolver.GetDefaultMetadataForFile(data.MediaPath);
            }
        }

        public string Path => data.MediaPath;

        public bool SupportsMetadata => true;

        public event EventHandler<FileInfo> ImageFileChanged;
        public event EventHandler<EmbeddedMetadataResult> MetadataChanged;

        public Task<Result<ReadOnlyCollection<IMediaPlayerItemSource>>> GetMediaDataSourcesAsync()
        {
            return Task<Result<ReadOnlyCollection<IMediaPlayerItemSource>>>.FromResult(Result.Ok(new List<IMediaPlayerItemSource>() { data }.AsReadOnly()));
        }

        public PlayListItemMediaSourceProvider(IMediaPlayerItemSource _data)
        {
            data = _data;
            metadataTask = _data.GetMetadataAsync();
            metadataTask.ContinueWith(task =>
            {
                MetadataChanged?.Invoke(this, task.Result);
                ImageFileChanged?.Invoke(this, new FileInfo(task.Result.SavedThumbnailFile));
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

    }
}
