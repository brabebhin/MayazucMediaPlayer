using CommunityToolkit.Mvvm.Input;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.NowPlayingViews
{
    public partial class NowPlayingUiService : PlaybackItemManagementUIService<MediaPlayerItemSourceUIWrapper>, IDisposable
    {
        readonly AsyncLock stopPlaybackQueueLock = new AsyncLock();

        bool _shuffleButtonEnabled = true;
        private bool _clearQueueButtonEnabled = true;
        private bool _contextMenuEnabled = true;
        private bool _nowPlayingCommandBarEnabled = true;

        public bool IsItemClickEnabled
        {
            get;
            private set;
        } = true;

        public bool NowPlayingCommandBarEnabled
        {
            get => _nowPlayingCommandBarEnabled;
            set
            {
                if (_nowPlayingCommandBarEnabled == value) return;
                _nowPlayingCommandBarEnabled = value;
                NotifyPropertyChanged(nameof(NowPlayingCommandBarEnabled));
            }
        }

        public bool ContextMenuEnabled
        {
            get => _contextMenuEnabled;
            set
            {
                if (_contextMenuEnabled == value) return;
                _contextMenuEnabled = value;
                NotifyPropertyChanged(nameof(ContextMenuEnabled));
            }
        }

        public bool ClearQueueButtonEnabled
        {
            get => _clearQueueButtonEnabled;
            set
            {
                if (_clearQueueButtonEnabled == value) return;
                _clearQueueButtonEnabled = value;
                NotifyPropertyChanged(nameof(ClearQueueButtonEnabled));
            }
        }

        public AdvancedCollectionView NowPlayingCollectionViewSource
        {
            get;
            private set;
        }

        public TimeSpan CurrentSpan { get; private set; }

        public TimeSpan TrackDurationSpan { get; set; }

        public bool NpShuffleButton
        {
            get
            {
                return _shuffleButtonEnabled;
            }
            set
            {
                if (_shuffleButtonEnabled == value) return;

                _shuffleButtonEnabled = value;
                NotifyPropertyChanged(nameof(NpShuffleButton));
            }
        }

        public override IMediaPlayerItemSourceProvderCollection<MediaPlayerItemSourceUIWrapper> Items
        {
            get
            {
                return PlaybackServiceInstance.NowPlayingBackStore;
            }
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
            PlaylistsService playlistsService) : base(dispatcher, m, playlistsService, () => false)
        {
            PlaylistsService = playlistsService;
            BackgroundMediaPlayerInstance = backgroundMediaPlayerInstance;

           
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

        private async Task StopPlaybackAsync()
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
                    SelectionMode = ListViewSelectionMode.None;
                    await (AppState.Current.MediaServiceConnector.PlayerInstance).StopPlayback();

                    //diag = new MessageDialog("Please restart the app for best results.", "Obliteration complete");
                    //diag.Commands.Clear();
                    //await diag.ShowAsync();
                }
            }
        }

        public async Task RemoveItemsFromPlaybackAsync(IList<MediaPlayerItemSourceUIWrapper> selectedItems)
        {
            await (AppState.Current.MediaServiceConnector.PlayerInstance).RemoveItemsFromNowPlayingQueue(selectedItems);
        }

        protected override async Task PlayFilesInternal(IEnumerable<MediaPlayerItemSourceUIWrapper> itemsToPlay, int startIndex)
        {
            if (itemsToPlay.TryGetNonEnumeratedCount(out var count))
            {
                if (count == 1)
                {
                    await AppState.Current.MediaServiceConnector.SkipToIndex(itemsToPlay.First().ExpectedPlaybackIndex);
                }
            }
        }

        protected override async Task OnContentsChanged(ReadOnlyCollection<MediaPlayerItemSourceUIWrapper> newContent)
        {
            if(newContent.Count == 0)
            {
               await StopPlaybackAsync();
            }
            else
            {
                PlaybackServiceInstance.NowPlayingBackStore.SaveInstanceToBackStore();
            }
        }

        protected override Task OnContentAdded(ReadOnlyCollection<MediaPlayerItemSourceUIWrapper> addedContent)
        {
            PlaybackServiceInstance.NowPlayingBackStore.SaveInstanceToBackStore();
            return Task.CompletedTask; 
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
