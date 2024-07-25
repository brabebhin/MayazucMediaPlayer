using CommunityToolkit.WinUI;
using FluentResults;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    /// <summary>
    /// Wraps a playback sequence and its disk storage
    /// </summary>
    public class PlaybackSequence : MediaDataStorageUIWrapperCollection, IPlaybackSequence
    {
        private readonly AutoPlayManager autoPlayManager = new AutoPlayManager(null);
        readonly AsyncLock asyncLock = new AsyncLock();
        private readonly DispatcherQueue Dispatcher;
        public const int AddToNowPlayingAtTheEnd = -1;

        private IPlaybackSequenceProvider NowPlayingManager
        {
            get;
            set;
        }

        protected string SequenceBackstoreFile
        {
            get;
            private set;
        }

        public PlaybackSequence(string backStoreFilename, DispatcherQueue _dispatcherQueue,
            IPlaybackSequenceProviderFactory nowPlayingSequenceProviderFactory)
        {
            SequenceBackstoreFile = backStoreFilename;
            Dispatcher = _dispatcherQueue;
            NowPlayingManager = nowPlayingSequenceProviderFactory.GetPlaybackSequence(SequenceBackstoreFile);
        }

        public async Task<Result<MediaPlayerItemSourceUIWrapper>> GetMediaDataItemAtIndex(int index)
        {
            using (await asyncLock.LockAsync())
            {
                var hasIndexResult = await HasIndex(index);

                switch (hasIndexResult.Result)
                {
                    case IndexResult.Cached: return Result.Ok(Items[index]);
                    case IndexResult.AutoPlay:
                        {
                            var autoPlayMds = await autoPlayManager.GetNextFile(hasIndexResult.AutoPlayStorageFile);
                            if (autoPlayMds.IsFailed) return await Task.FromResult(Result.Fail("Could not get next file"));

                            await AddToSequenceAsync(new IMediaPlayerItemSource[] { autoPlayMds.Value }, index);

                            return Result.Ok(Items[index]);
                        }
                    case IndexResult.None: return await Task.FromResult(Result.Fail("Could not find index"));
                }

                return await Task.FromResult(Result.Fail("General fail"));
            }
        }

        public async Task AddToSequenceAsync(IEnumerable<IMediaPlayerItemSource> mediaDatas, int index)
        {
            await Dispatcher.EnqueueAsync(() =>
            {
                var newMembers = mediaDatas.Select(x => new MediaPlayerItemSourceUIWrapper(x, Dispatcher)).ToList();
                AddToSequence(newMembers, index);
                NumberNowPlayingInternal();
                LoadNowPlayingThumbnails(newMembers);
                SaveInstanceToBackstore();
            });
        }

        public int Randomize(int observableIndex)
        {
            var retValue = this.Randomize<MediaPlayerItemSourceUIWrapper>(observableIndex);
            NumberNowPlayingInternal();
            SaveInstanceToBackstore();

            return retValue;
        }

        public async Task SetSequence(IEnumerable<IMediaPlayerItemSource> mediaDatas)
        {
            await Dispatcher.EnqueueAsync(() =>
            {
                HandleNewSequence(mediaDatas);
            });

            SaveInstanceToBackstore();
        }

        public async Task RemoveItemsFromSequenceAsync(IEnumerable<MediaPlayerItemSourceUIWrapper> items)
        {
            await Dispatcher.EnqueueAsync(() =>
            {
                foreach (var mds in items)
                {
                    Remove(mds);
                }
                NumberNowPlayingInternal();
            });

            SaveInstanceToBackstore();
        }

        public Task LoadSequenceAsync()
        {
            return Dispatcher.EnqueueAsync(() =>
            {
                LoadSequence();
            });
        }

        public void LoadSequence()
        {
            Clear();
            IReadOnlyCollection<IMediaPlayerItemSource> files = null;
            try
            {
                files = NowPlayingManager.GetPlaybackQueue();


                if (files != null)
                {
                    this.AddRange(files.Select(x => new MediaPlayerItemSourceUIWrapper(x, Dispatcher)));
                }
                NumberNowPlayingInternal();
                LoadNowPlayingThumbnails(this);
            }
            catch { }
        }

        public void Switch(int source, int destination)
        {
            this.Switch<MediaPlayerItemSourceUIWrapper>(source, destination);
            NumberNowPlayingInternal();
            SaveInstanceToBackstore();
        }

        private void HandleNewSequence(IEnumerable<IMediaPlayerItemSource> e)
        {
            Clear();
            this.AddRange(e.Select(x => new MediaPlayerItemSourceUIWrapper(x, Dispatcher)));
            NumberNowPlayingInternal();
            LoadNowPlayingThumbnails(this);
        }

        private void NumberNowPlayingInternal()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].TrackNumber = i + 1;
                this[i].MediaData.ExpectedPlaybackIndex = i;
            }
        }

        private void LoadNowPlayingThumbnails(IEnumerable<MediaPlayerItemSourceUIWrapper> items)
        {
            var copy = new List<MediaPlayerItemSourceUIWrapper>(items);
            copy.ForEachAsync(4, async (t) =>
            {
                try
                {
                    await t.LoadMediaThumbnailAsync();
                }
                catch
                {

                }
            });
        }

        private async Task<HasIndexResult> HasIndex(int index)
        {
            if (Count == 0) return new HasIndexResult(IndexResult.None);
            //the index was inside the existing collection, return true
            if (index < Count) return new HasIndexResult(IndexResult.Cached);

            var aMusic = SettingsService.Instance.AutoPlayMusic;
            var aVideo = SettingsService.Instance.AutoPlayVideo;

            //auto play only works if requested index is just 1 outside the count
            if ((aMusic || aVideo) && index == Count)
            {
                var lastItem = Items[Count - 1];
                var file = new FileInfo(lastItem.MediaData.MediaPath);
                await autoPlayManager.LoadAutoPlayAsync(file);

                if ((Count + await autoPlayManager.CountAsync()) > index) return new HasIndexResult(IndexResult.AutoPlay, file);
            }

            return new HasIndexResult(IndexResult.None);
        }

        private void AddToSequence(IEnumerable<MediaPlayerItemSourceUIWrapper> dataz, int index)
        {
            if (index == AddToNowPlayingAtTheEnd)
                this.AddRange(dataz);
            else
                this.InsertRange(dataz, index);
        }

        private void SaveInstanceToBackstore()
        {
            NowPlayingManager.CreateNewPlaybackQueue(this.Select(x => x.MediaData).Where(x => x.Persistent));
        }

        public void ClearSequence()
        {
            Clear();
            NowPlayingManager.ClearNowPlaying();
        }

        private class HasIndexResult
        {
            public IndexResult Result { get; private set; }

            public FileInfo AutoPlayStorageFile { get; private set; }

            public HasIndexResult(IndexResult result, FileInfo autoPlayStorageFile)
            {
                Result = result;
                AutoPlayStorageFile = autoPlayStorageFile;
            }

            public HasIndexResult(IndexResult result)
            {
                Result = result;
            }
        }

        private enum IndexResult
        {
            Cached = 0,
            AutoPlay = 1,
            None = 2
        }
    }

    public class NowPlayingPlaybackSequence : PlaybackSequence
    {
        public NowPlayingPlaybackSequence(DispatcherQueue _dispatcherQueue, IPlaybackSequenceProviderFactory nowPlayingSequenceProviderFactory) : base(JsonPlaybackSequenceFactory.NowPlayingPlaybackSequenceStore, _dispatcherQueue, nowPlayingSequenceProviderFactory)
        {
        }
    }
}
