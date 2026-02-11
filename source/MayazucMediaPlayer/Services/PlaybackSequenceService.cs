using FluentResults;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    public partial class PlaybackSequenceService : ObservableObject
    {
        public const int AddToNowPlayingAtTheEnd = -1;

        public PlaybackSequence NowPlayingBackStore { get; private set; }

        public DispatcherQueue DispatcherQueue
        {
            get;
            private set;
        }

        public PlaybackSequenceService(DispatcherQueue dispatcherQueue, IPlaybackSequenceProviderFactory nowPlayingSequenceProvider)
        {
            DispatcherQueue = dispatcherQueue;
            NowPlayingBackStore = new NowPlayingPlaybackSequence(DispatcherQueue, nowPlayingSequenceProvider);
        }

        public async Task<int> RandomizeSequence(int oldIndex)
        {
            var newIndex = NowPlayingBackStore.Randomize(oldIndex);
            return newIndex;
        }

        public async Task AddToNowPlayingAsync(IEnumerable<IMediaPlayerItemSource> mediaDatas, int index)
        {
            await NowPlayingBackStore.AddToSequenceAsync(mediaDatas, index);
        }

        public async Task EnqueueNewPlaylistAsync(IEnumerable<IMediaPlayerItemSource> mediaDatas)
        {
            await NowPlayingBackStore.SetSequence(mediaDatas);
        }

        public Task HandleMediaOpened(Windows.Media.Playback.MediaPlayer sender, MediaOpenedEventArgs args)
        {
            var index = SettingsService.Instance.PlaybackIndex;

            if (args.Reason == MediaOpenedEventReason.MediaPlaybackListItemChanged)
            {
                NowPlayingBackStore.NotifyPlayingMediaPath(args.Data.MediaPath);
            }

            return Task.CompletedTask;
        }

        public void FillData()
        {
            NowPlayingBackStore.LoadSequence();
        }

        public Task LoadSequence()
        {
            return NowPlayingBackStore.LoadSequenceAsync();
        }

        public async Task<Result<MediaPlayerItemSourceUIWrapper>> CurrentMediaMetadata()
        {
            var playerInstance = AppState.Current.MediaServiceConnector.PlayerInstance;
            if (playerInstance != null && playerInstance.CurrentPlaybackData != null)
            {
                var currentPlaybackData = playerInstance.CurrentPlaybackData;
                return Result.Ok(new MediaPlayerItemSourceUIWrapper(currentPlaybackData, DispatcherQueue));
            }
            else
            {
                var index = SettingsService.Instance.PlaybackIndex;

                var storedCount = NowPlayingBackStore.Count;

                return await NowPlayingBackStore.GetMediaDataItemAtIndex(storedCount > index ? index : 0);
            }
        }

        public async Task RemoveItemsFromNowPlayingAsync(IEnumerable<MediaPlayerItemSourceUIWrapper> items)
        {
            await NowPlayingBackStore.RemoveItemsFromSequenceAsync(items);
        }

        public async Task SwitchItemInPlaybackQueue(int source, int destination)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                NowPlayingBackStore.Switch(source, destination);
            });

        }
    }
}
