using CommunityToolkit.WinUI;
using FluentResults;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Playlists;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    public class PlaybackSequenceService : ObservableObject
    {
        public const int AutoResetEventMSTimeout = 20 * 1000;
        public static AutoResetEvent PlaylistReadWriteLock { get; private set; } = new AutoResetEvent(true);
        public static object objLock = new object();
        public static bool newVersion = false;
        public const int AddToNowPlayingAtTheEnd = -1;

        public PlaybackSequence NowPlayingBackStore { get; private set; }


        public ObservableCollection<PlaylistItem> NowPlayingHistoryPlaylists
        {
            get;
            set;
        }

        public DispatcherQueue Dispatcher
        {
            get;
            private set;
        }


        public PlaybackSequenceService(DispatcherQueue disp, IPlaybackSequenceProviderFactory nowPlayingSequenceProvider)
        {
            Dispatcher = disp;
            NowPlayingBackStore = new NowPlayingPlaybackSequence(Dispatcher, nowPlayingSequenceProvider);
        }

        public async Task<int> RandomizeNowPlayingAsync(int oldIndex)
        {
            var newIndex = NowPlayingBackStore.RandomizeMusicDataStorage(oldIndex);
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

        public async Task HandleMediaOpened(Windows.Media.Playback.MediaPlayer sender, MediaOpenedEventArgs args)
        {
            var index = SettingsWrapper.PlaybackIndex;

            if (args.Reason == MediaOpenedEventReason.MediaPlaybackListItemChanged)
            {
                for (int i = 0; i < NowPlayingBackStore.Count; i++)
                {
                    NowPlayingBackStore[i].IsInPlayback = i == index;
                }
            }
        }

        public Task FillDataAsync()
        {
            return Task.WhenAll(LoadNowPlaying());
        }

        public void FillData()
        {
            NowPlayingBackStore.LoadSequence();
        }

        public Task LoadNowPlaying()
        {
            return NowPlayingBackStore.LoadSequenceAsync();
        }

        public async Task<Result<MediaPlayerItemSourceUIWrapper>> CurrentMediaMetadata()
        {
            var playerInstance = AppState.Current.MediaServiceConnector.PlayerInstance;
            if (playerInstance != null && playerInstance.CurrentPlaybackData != null)
            {
                var currentPlaybackData = playerInstance.CurrentPlaybackData;
                return Result.Ok(new MediaPlayerItemSourceUIWrapper(currentPlaybackData, Dispatcher));
            }
            else
            {
                var index = SettingsWrapper.PlaybackIndex;

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
            await Dispatcher.EnqueueAsync(() =>
            {
                NowPlayingBackStore.Switch(source, destination);
            });

        }
    }
}
