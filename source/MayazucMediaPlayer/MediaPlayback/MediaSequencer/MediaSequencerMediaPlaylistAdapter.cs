using CommunityToolkit.WinUI;
using MayazucMediaPlayer.Helpers;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using MayazucNativeFramework;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback.MediaSequencer
{
    public partial class MediaSequencerMediaPlaylistAdapter : IMediaPlaybackListAdapter
    {
        public BusyFlag IsBusy { get; private set; } = new BusyFlag();

        public MediaPlaybackItem CurrentPlaybackItem
        {
            get
            {
                if (currentbackStore == null)
                    return null;
                return currentbackStore.CurrentItem;
            }
        }

        private readonly MediaPlaybackQueueProvider playbackQueueProvider;
        private readonly PlaybackSequenceService PlaybackModelsInstance;
        readonly IFFmpegInteropMediaSourceProvider<IMediaPlayerItemSource> playbackItemProvider;
        readonly MediaPlayer player;

        IReadOnlyList<AvEffectDefinition> currentAudioEffects;
        IReadOnlyList<AvEffectDefinition> currentVideoEffects;

        readonly DispatcherQueue dispatcher;
        readonly CommandDispatcher mediaCommandsDispatcher;

        readonly VideoEffectProcessorConfiguration VideoEffectsConfiguration;
        readonly ulong WindowId = 0;

        public MediaSequencerMediaPlaylistAdapter(
            PlaybackSequenceService playbackQueueService,
            IFFmpegInteropMediaSourceProvider<IMediaPlayerItemSource> provider,
            MediaPlayer player,
            DispatcherQueue dispatcher,
            CommandDispatcher mediaCommandsDispatcher,
            VideoEffectProcessorConfiguration videoEffectsConfiguration,
            ulong windowId)
        {
            PlaybackModelsInstance = playbackQueueService ?? throw new ArgumentNullException(nameof(playbackQueueService));
            playbackItemProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            this.mediaCommandsDispatcher = mediaCommandsDispatcher ?? throw new ArgumentNullException(nameof(mediaCommandsDispatcher));
            PlaybackModelsInstance.NowPlayingBackStore.CollectionChanged += NowPlayingBackStore_CollectionChanged;
            playbackQueueProvider = new MediaPlaybackQueueProvider(PlaybackModelsInstance.NowPlayingBackStore);
            VideoEffectsConfiguration = videoEffectsConfiguration;
            WindowId = windowId;
        }

        private void NowPlayingBackStore_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LocalSource = !PlaybackModelsInstance.NowPlayingBackStore.All(x => x.MediaData.HasExternalSource);
        }

        private IMediaSourceSequencer nextBackStore;
        private IMediaSourceSequencer currentbackStore;
        private bool disposedValue;

        public IMediaSourceSequencer CurrentBackStore
        {
            get => currentbackStore;
            set
            {
                if (currentbackStore != null)
                {
                    currentbackStore.SequenceEnded -= BackStore_SequenceEnded;
                    currentbackStore.MediaOpened -= BackStore_MediaOpened;
                    currentbackStore.MediaFailed -= BackStore_MediaFailed;
                    currentbackStore.Dispose();
                }

                currentbackStore = value;

                if (currentbackStore != null)
                {
                    currentbackStore.ApplyFFmpegAudioEffects(currentAudioEffects);
                    currentbackStore.ApplyFFmpegVideoEffects(currentVideoEffects);
                    currentbackStore.SequenceEnded += BackStore_SequenceEnded;
                    currentbackStore.MediaOpened += BackStore_MediaOpened;
                    currentbackStore.MediaFailed += BackStore_MediaFailed;
                }
            }
        }

        public bool LocalSource
        {
            get;
            private set;
        }

        private async void BackStore_MediaFailed(object? sender, MediaPlaybackItem e)
        {
            await mediaCommandsDispatcher.EnqueueAsync(async () =>
            {
                await MoveToNextItem(e, true, true);
            });
        }

        private async void BackStore_MediaOpened(object? sender, MediaOpenedEventArgs e)
        {
            await mediaCommandsDispatcher.EnqueueAsync(async () =>
            {
                using (IsBusy.SetBusy())
                {
                    var currentBackStore = sender as IMediaSourceSequencer;
                    var currentIndex = e.Data.PlaybackItem.GetExtradata().MediaPlayerItemSource.ExpectedPlaybackIndex;
                    var nextItem = await GetNextItem(currentIndex: currentIndex, userAction: false, changeIndex: true);
                    if (nextItem == null)
                        return;
                    IMediaSourceSequencer targetBackstore = InitializeBackstoreForItem(nextItem);

                    await targetBackstore.AddItem(nextItem);
                    var trimmedItems = currentBackStore.Trim();
                    foreach (var item in trimmedItems)
                    {
                        var interopMss = item.GetExtradata().FFmpegMediaSource;
                        item.GetExtradata()?.Dispose();
                    }
                }
            });

            CurrentPlaybackItemChanged?.Invoke(this, new MayazucCurrentMediaPlaybackItemChangedEventArgs(e.Data.PlaybackItem, MediaPlaybackItemChangedReason.EndOfStream, this));
        }

        private IMediaSourceSequencer InitializeBackstoreForItem(MediaPlaybackItem nextItem)
        {
            var nextBackStoreInternal = GetOrCreateMediaSequencer(nextItem);
            var targetBackstore = CurrentBackStore;

            if (nextBackStoreInternal != CurrentBackStore)
            {
                nextBackStore = nextBackStoreInternal;
                targetBackstore = nextBackStoreInternal;
            }

            return targetBackstore;
        }

        private void BackStore_SequenceEnded(object? sender, EventArgs e)
        {
            mediaCommandsDispatcher.EnqueueAsync(async () =>
            {
                await TrySwitchSequences();
            });
        }

        private async Task TrySwitchSequences()
        {
            if (nextBackStore != null)
            {
                CurrentBackStore = nextBackStore;
                AttachingToMediaPlayer?.Invoke(this, new EventArgs());
                await CurrentBackStore.Start(player, TimeSpan.Zero);
            }
        }

        public event EventHandler AttachingToMediaPlayer;
        public event EventHandler<MayazucCurrentMediaPlaybackItemChangedEventArgs> CurrentPlaybackItemChanged;
        public event EventHandler ItemCreationFailed;
        public event EventHandler Disposing;

        public Task<bool> BackstoreHasItems()
        {
            var count = PlaybackModelsInstance.NowPlayingBackStore.Count;
            return Task.FromResult(count > 0);
        }

        public Task<bool> CanGoBack()
        {
            return Task.FromResult(true);
        }


        public async Task<bool> MoveToNextItem(MediaPlaybackItem CurrentItem,
            bool userAction,
            bool incrementIndex)
        {
            using (IsBusy.SetBusy())
            {
                var nextItem = await GetNextItem(SettingsService.Instance.PlaybackIndex, userAction, incrementIndex);

                return await Start(nextItem);
            }
        }

        public async Task ReloadNextItemAsync(MediaPlaybackItem CurrenItem,
            bool userAction,
            bool incrementIndex,
            int currentIndex)
        {
            using (IsBusy.SetBusy())
            {

                if (CurrentBackStore == null)
                    await Start(currentIndex);

                var nextItem = await GetNextItem(CurrenItem != null ? PlaybackModelsInstance.NowPlayingBackStore.IndexOfMediaData(CurrenItem.GetExtradata().MediaPlayerItemSource) : currentIndex, userAction, incrementIndex);
                //reset the backstore if necessary
                ResetNextBackstore();
                // get the next backstore
                var targetBackStore = InitializeBackstoreForItem(nextItem);
                var oldItem = await targetBackStore.ReloadNextItem(nextItem);
                oldItem?.GetExtradata()?.Dispose();
                oldItem?.Source?.Dispose();
            }
        }

        private void ResetNextBackstore()
        {
            nextBackStore?.Dispose();
            nextBackStore = null;
        }

        public void RemoveFFmpegAudioEffects()
        {
            currentAudioEffects = null;
            nextBackStore?.RemoveFFmpegAudioEffects();
            currentbackStore?.RemoveFFmpegAudioEffects();
        }

        public void RemoveFFmpegVideoEffects()
        {
            currentVideoEffects = null;
            nextBackStore?.RemoveFFmpegVideoEffects();
            currentbackStore?.RemoveFFmpegVideoEffects();
        }

        public Task<bool> ResetPlayback(MediaPlaybackItem CurrentItem,
            bool userAction,
            bool shiftIndex,
            bool incrementIndex)
        {
            throw new NotImplementedException();
        }

        public void SetFFmpegVideoEffects(IReadOnlyList<AvEffectDefinition> videoEffects)
        {
            currentVideoEffects = videoEffects;
            nextBackStore?.ApplyFFmpegVideoEffects(videoEffects);
            currentbackStore?.ApplyFFmpegVideoEffects(videoEffects);
        }

        public void SetFFmpegAudioEffects(IReadOnlyList<AvEffectDefinition> audioEffects)
        {
            currentAudioEffects = audioEffects;
            nextBackStore?.ApplyFFmpegAudioEffects(audioEffects);
            currentbackStore?.ApplyFFmpegAudioEffects(audioEffects);
        }

        public async Task<bool> Start(MediaPlaybackItem initialItem)
        {
            return await StartInternal(initialItem);
        }

        public async Task<bool> Start(int index)
        {
            using (IsBusy.SetBusy())
            {
                MediaPlaybackItem startingItem = await GetDefaultStartingItem(index);
                if (startingItem == null) return false;

                return await StartInternal(startingItem);

                async Task<MediaPlaybackItem> GetDefaultStartingItem(int index)
                {
                    var mediaData = await playbackQueueProvider.GetStartingItem(index);
                    if (mediaData == null)
                    {
                        ItemCreationFailed?.Invoke(this, new EventArgs());
                        return await Task.FromResult(default(MediaPlaybackItem));
                    }
                    var ffmpegInteropMss = await DispatcherQueueExtensions.EnqueueAsync(dispatcher, async () =>
                    {
                        return await playbackItemProvider.GetFFmpegInteropMssAsync(mediaData, true, WindowId);
                    });

                    if (ffmpegInteropMss == null)
                    {
                        ItemCreationFailed?.Invoke(this, new EventArgs());
                        return await Task.FromResult(default(MediaPlaybackItem));
                    }
                    PlaybackItemExtraData extraData = ffmpegInteropMss.PlaybackItem.AddExtradataToPlaybackItem(mediaData, ffmpegInteropMss, MediaPlaybackItemUIInformation.Create(ffmpegInteropMss, mediaData));

                    await MediaPlaybackItemDisplayPropertiesHelper.SetPlaybackItemMediaProperties(ffmpegInteropMss.PlaybackItem, mediaData);
                    await extraData.SubtitleService.PrepareSubtitles(await mediaData.PrepareSubtitles());
                    await ffmpegInteropMss.PlaybackItem.Source.OpenAsync();
                    ffmpegInteropMss.PlaybackItem.Source.MediaStreamSource.MaxSupportedPlaybackRate = 2;

                    return ffmpegInteropMss.PlaybackItem;
                }
            }
        }

        private async Task<bool> StartInternal(MediaPlaybackItem startingItem)
        {
            CurrentBackStore?.Dispose();
            nextBackStore?.Dispose();

            CurrentBackStore = null;
            nextBackStore = null;

            CurrentBackStore = GetOrCreateMediaSequencer(startingItem);
            if (CurrentBackStore == null) return false;
            await CurrentBackStore.AddItem(startingItem);
            await dispatcher.EnqueueAsync(async () =>
            {
                await CurrentBackStore.Start(player, TimeSpan.Zero);
            });
            return true;
        }

        private async Task<MediaPlaybackItem?> GetNextItem(int currentIndex,
            bool userAction,
            bool changeIndex)
        {
            int retryAttempt = 0;
            do
            {
                retryAttempt++;

                var mediaData = await playbackQueueProvider.GetNextMediaData(currentIndex, userAction, changeIndex);
                if (mediaData == null) return null;

                if (!mediaData.IsAvailable())
                {
                    ItemCreationFailed?.Invoke(this, new EventArgs());
                    userAction = true;
                    changeIndex = true;
                    continue;
                }

                var ffmpegInteropMss = await dispatcher.EnqueueAsync(async () =>
                {
                    return await playbackItemProvider.GetFFmpegInteropMssAsync(mediaData, true, WindowId);
                });

                if (ffmpegInteropMss == null)
                {
                    userAction = true;
                    changeIndex = true;
                    continue;
                }

                PlaybackItemExtraData extraData = ffmpegInteropMss.PlaybackItem.AddExtradataToPlaybackItem(mediaData, ffmpegInteropMss, MediaPlaybackItemUIInformation.Create(ffmpegInteropMss, mediaData));

                await MediaPlaybackItemDisplayPropertiesHelper.SetPlaybackItemMediaProperties(ffmpegInteropMss.PlaybackItem, mediaData);
                var tags = ffmpegInteropMss.MetadataTags;
                foreach (var t in tags)
                {
                    System.Diagnostics.Debug.WriteLine($"{t.Key} - {string.Join(' ', t.Value)}");
                }
                await extraData.SubtitleService.PrepareSubtitles(await mediaData.PrepareSubtitles());
                ffmpegInteropMss.PlaybackItem.Source.MediaStreamSource.MaxSupportedPlaybackRate = 2;
                return ffmpegInteropMss.PlaybackItem;
            }
            while (retryAttempt < 5);
            return null;
        }

        private IMediaSourceSequencer GetOrCreateMediaSequencer(MediaPlaybackItem item)
        {
            var isVideo = item.IsVideo();
            var hasVideoEffects = VideoEffectsConfiguration.MasterSwitch;
            if (isVideo && hasVideoEffects)
            {
                var hasSeqSource = currentbackStore != null && currentbackStore is VideoEffectSequentialMediaSourceSequence;
                if (!hasSeqSource)
                    return new VideoEffectSequentialMediaSourceSequence(VideoEffectsConfiguration);
                else return currentbackStore;
            }
            else
            {
                var hasGapless = currentbackStore != null && currentbackStore is GaplessPlaybackMediaSourceSequnce;
                if (!hasGapless)
                    return new GaplessPlaybackMediaSourceSequnce();
                else return currentbackStore;
            }
        }

        public void Stop()
        {
            using (IsBusy.SetBusy())
            {
                Disposing?.Invoke(this, new EventArgs());
                CurrentBackStore?.Dispose();
                CurrentBackStore = null;
            }
        }

        /// <summary>
        /// not supported here
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Task<bool> ApplyMCVideoEffect(VideoEffectProcessorConfiguration configuration)
        {
            using (IsBusy.SetBusy())
            {
                var currentPlaybackItem = currentbackStore.CurrentItem;

                if (!currentPlaybackItem.IsVideo()) return Task.FromResult(false);
                //if current item is video, switch to sequential source and apply effect
                nextBackStore?.Dispose();
                nextBackStore = null;
                GetOrCreateMediaSequencer(currentPlaybackItem);
                return Task.FromResult(true);
            }
        }

        public MediaPlaybackItem DetachCurrentItem()
        {
            return currentbackStore.DetachCurrentItem();
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disposing?.Invoke(this, new EventArgs());

                    // TODO: dispose managed state (managed objects)
                }
                currentbackStore?.Dispose();
                nextBackStore?.Dispose();
                PlaybackModelsInstance.NowPlayingBackStore.CollectionChanged -= NowPlayingBackStore_CollectionChanged;

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~MediaSequencerMediaPlaylistAdapter()
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
}

