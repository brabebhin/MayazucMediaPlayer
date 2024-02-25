using FFmpegInteropX;
using MayazucMediaPlayer.Helpers;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.VideoEffects;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Media.PlayTo;

namespace MayazucMediaPlayer.MediaPlayback.PlayTo
{
    class PlayToMediaPlaybackListAdapter : IMediaPlaybackListAdapter
    {
        readonly AsyncLock _lock = new AsyncLock();
        SourceChangeRequestedEventArgs Source { get; set; }

        public BusyFlag IsBusy
        {
            get; private set;
        } = new BusyFlag();

        public event EventHandler AttachingToMediaPlayer;
        public event EventHandler<MayazucCurrentMediaPlaybackItemChangedEventArgs> CurrentPlaybackItemChanged;
        public event EventHandler ItemCreationFailed;
        public event EventHandler Disposing;

        public IFFmpegInteropMediaSourceProvider<SourceChangeRequestedEventArgs> ItemBuilder;
        FFmpegMediaSource ffmpeginteropMss;
        private bool disposedValue;

        public MediaPlayer CurrentPlayer
        {
            get;
            private set;
        }

        public PlayToReceiver Connector
        {
            get;
            private set;
        }

        public PlaybackSequenceService PlaybackServiceInstance
        {
            get;
            private set;
        }

        public MediaPlaybackItem CurrentPlaybackItem { get; private set; }

        public bool LocalSource => false;

        public PlayToMediaPlaybackListAdapter(MediaPlayer player,
            IFFmpegInteropMediaSourceProvider<SourceChangeRequestedEventArgs> itemBuilder,
            SourceChangeRequestedEventArgs source,
            PlayToReceiver connector,
            PlaybackSequenceService playbackServiceInstance)
        {
            CurrentPlayer = player;
            ItemBuilder = itemBuilder;
            Source = source;
            Connector = connector;
            PlaybackServiceInstance = playbackServiceInstance;
        }

        public Task<bool> MoveToNextItem(MediaPlaybackItem CurrentItem, bool userAction, bool incrementIndex)
        {
            Connector.NotifyEnded();
            return Task.FromResult(true);
        }

        public Task ReloadNextItemAsync(MediaPlaybackItem CurrenItem, bool userAction, bool incrementIndex, int currentIndex)
        {
            return Task.CompletedTask;
        }

        internal async Task<bool> LoadPlaybackItemAsync()
        {
            using (IsBusy.SetBusy())
            {
                try
                {

                    var metadata = new EmbeddedMetadataResult(Source.Album, Source.Author, Source.Genre, Source.Title);
                    var data = IMediaPlayerItemSourceFactory.Get(Source);

                    ffmpeginteropMss = await data.GetFFmpegMediaSourceAsync();
                    CurrentPlaybackItem = ffmpeginteropMss.CreateMediaPlaybackItem();

                    var PlaybackDataStreamInfo = MediaPlaybackItemUIInformation.Create(ffmpeginteropMss, data);
                    PlaybackItemExtraData extradata = CurrentPlaybackItem.AddExtradataToPlaybackItem(data, ffmpeginteropMss, PlaybackDataStreamInfo);

                    var ffmpegThumbnail = Source.Thumbnail;

                    var subRequest = await data.PrepareSubtitles();
                    await extradata.SubtitleService.PrepareSubtitles(subRequest);
                    await MediaPlaybackItemDisplayPropertiesHelper.SetPlaybackItemMediaProperties(CurrentPlaybackItem, data);
                    return true;
                }
                catch
                {
                    ItemCreationFailed?.Invoke(this, new EventArgs());
                    return false;
                }
            }
        }

        public void RemoveFFmpegAudioEffects()
        {
            ffmpeginteropMss.ClearFFmpegAudioFilters();
        }

        public void RemoveFFmpegVideoEffects()
        {
            ffmpeginteropMss.ClearFFmpegVideoFilters();
        }

        public Task<bool> ResetPlayback(MediaPlaybackItem CurrentItem, bool userAction, bool shiftIndex, bool incrementIndex)
        {
            CurrentPlayer.Source = CurrentPlaybackItem;
            return Task.FromResult(true);
        }

        public void SetFFmpegAudioEffects(IReadOnlyList<AvEffectDefinition> audioEffects)
        {
            ffmpeginteropMss.SetFFmpegAudioFilters(audioEffects.GetFFmpegFilterJoinedFilterDef());
        }

        public void SetFFmpegVideoEffects(IReadOnlyList<AvEffectDefinition> videoEffects)
        {
            ffmpeginteropMss.SetFFmpegVideoFilters(videoEffects.GetFFmpegFilterJoinedFilterDef());
        }

        public async Task<bool> Start(MediaPlaybackItem initialItem)
        {
            return await StartInternal();
        }

        private async Task<bool> StartInternal()
        {
            using (await _lock.LockAsync())
            {
                if (CurrentPlaybackItem == null)
                {
                    await LoadPlaybackItemAsync();
                }

                AttachingToMediaPlayer?.Invoke(this, new EventArgs());
                CurrentPlayer.MediaOpened += CurrentPlayer_MediaOpened;
                CurrentPlayer.Source = CurrentPlaybackItem;
                return true;
            }
        }

        private async void CurrentPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            SettingsWrapper.PlaybackIndex = 0;
            await PlaybackServiceInstance.EnqueueNewPlaylistAsync(new IMediaPlayerItemSource[] { CurrentPlaybackItem?.GetExtradata().MediaPlayerItemSource });
            CurrentPlaybackItemChanged?.Invoke(sender, new MayazucCurrentMediaPlaybackItemChangedEventArgs(CurrentPlaybackItem, MediaPlaybackItemChangedReason.InitialItem, this));
        }

        public void Stop()
        {
            Disposing?.Invoke(this, new EventArgs());
            CurrentPlaybackItem?.Source.Dispose();
            ffmpeginteropMss?.Dispose();
            CurrentPlayer.Source = null;
            CurrentPlayer.MediaOpened -= CurrentPlayer_MediaOpened;
        }

        public Task<bool> BackstoreHasItems()
        {
            return Task.FromResult(true);
        }

        public Task<bool> CanGoBack()
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// TODO: reimplement.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public Task<bool> ApplyMCVideoEffect(VideoEffectProcessorConfiguration configuration)
        {
            return Task.FromResult(false);
        }

        public MediaPlaybackItem DetachCurrentItem()
        {
            var toReturn = CurrentPlaybackItem;
            CurrentPlaybackItem = null;
            ffmpeginteropMss = null;
            return toReturn;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                ffmpeginteropMss?.Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~PlayToMediaPlaybackListAdapter()
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

        public Task<bool> Start(int index)
        {
            return StartInternal();
        }
    }
}
