using FFmpegInteropX;
using MayazucMediaPlayer.AudioEffects;
using MayazucMediaPlayer.Helpers;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.VideoEffects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback
{
    public interface IBackgroundPlayer : IDisposable
    {
        VideoEffectProcessorConfiguration VideoEffectsConfiguration { get; }
        PlaybackSequenceService PlaybackQueueService { get; }
        IMediaPlayerItemSource CurrentPlaybackData { get; }
        MediaPlaybackItem CurrentPlaybackItem { get; }
        MediaPlayer CurrentPlayer { get; }
        FFmpegMediaSource FfmpegInteropInstance { get; }

        event TypedEventHandler<MediaPlayer, MediaOpenedEventArgs> OnMediaOpened;

        event EventHandler<object> OnNullPlaybackItem;

        /// <summary>
        /// DONE
        /// Strategy: reset filtering on existing media playback items
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        Task ResetFiltering(bool enabled);

        /// <summary>
        /// Strategy: start timer to stop music at specific time
        /// TODO: maybe there is a better way to do this, maybe with silent notifications?
        /// </summary>
        /// <returns></returns>
        Task HandleStopMusicOnTimer();

        /// <summary>
        /// DONE
        /// Strategy: Clear queue, destory any media source
        /// </summary>
        Task StopPlayback();

        /// <summary>
        /// DONE
        /// Strategy: only to be used while seeking from external sources (like seekbar, jump previous/next)
        /// </summary>
        /// <param name="position"></param>
        /// <param name="userAction"></param>
        /// <returns></returns>
        Task Seek(TimeSpan position, bool userAction);

        /// <summary>
        /// DONE
        /// Strategy: play is state is paused, pause if state is playing.
        /// Strategy: if force start => Resume from saved position.
        /// </summary>
        /// <param name="forceStart"></param>
        /// <returns></returns>
        Task PlayPauseAsync(bool forceStart = true);

        /// <summary>
        /// DONE
        /// Strategy: Resume from saved position
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task ResumeAsync(ResumeRequest request);

        /// <summary>
        /// DONE
        /// Strategy: if there's already a track playing, skip to the next one.
        /// Strategy: skip to next index if not playing.
        /// </summary>
        /// <returns></returns>
        Task SkipNext();

        /// <summary>
        /// DONE
        /// Strategy: if there's already a track playing, and time is less than 5 seconds, skip previous
        /// Strategy: if there's already a track playing, and time is bigger than 5 seconds, seek to 0
        /// Strategy: skip to previous index if not playing.
        /// </summary>
        /// <returns></returns>
        Task SkipPrevious();

        /// <summary>
        /// DONE
        /// Strategy: skip to given index
        /// </summary>
        /// <returns></returns>
        Task SkipToIndex(int index);

        /// <summary>
        /// DONE
        /// cross-concearn: set to busy when doing other strategies.
        /// </summary>
        BusyFlag DataBusyFlag { get; }

        /// <summary>
        /// DONE: Strategy: reload next track in playback list
        /// </summary>
        /// <returns></returns>
        Task RepeatModeChanged();

        /// <summary>
        /// DONE: Strategy: reload next track in playback list
        /// </summary>
        /// <returns></returns>
        Task ShuffleModeChanged();

        /// <summary>
        /// DONE: Strategy: reload next track in playback list
        /// </summary>
        /// <returns></returns>
        Task AutoPlayChanged();

        /// <summary>
        /// DONE Strategy: delay subtitles in current track
        /// </summary>
        /// <returns></returns>
        Task<TimeSpan> DelaySubtitles();

        /// <summary>
        /// DONE Strategy: quick subtitles in current track
        /// </summary>
        /// <returns></returns>
        Task<TimeSpan> QuickenSubtitles();

        /// <summary>
        /// DONE Strategy: Resume the commands dispatcher
        /// </summary>
        void ResumeDispatcher();

        /// <summary>
        /// DONE: Strategy: if playing, destory source, recreate source, recreate current item, seek to position, play
        /// </summary>
        /// <returns></returns>
        Task RestartCurrentItemAtCurrentPosition();

        /// <summary>
        /// Cross-concearn: start the play to reciever
        /// </summary>
        /// <returns></returns>
        Task StartDlnaSink();

        /// <summary>
        /// DONE: Strategy: Stop the play to reciever, reload the saved playback queue. Do not start playing
        /// </summary>
        /// <returns></returns>
        Task ForceDisconnectDlnaAsync();

        /// <summary>
        /// Cross-concearn: rise when playback queue changes between local and DLNA
        /// </summary>
        event EventHandler<PlaybackQueueTypeChangedArgs> PlaybackQueueTypeChanged;

        /// <summary>
        /// Cross-concearn: set active subtitles on current item
        /// </summary>
        /// <param name="currentPlaybackItem"></param>
        /// <param name="trackIndex"></param>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        Task SetActiveSubtitleAsync(MediaPlaybackItem currentPlaybackItem, uint trackIndex, int? hashCode = null);

        /// <summary>
        /// Do initialization things
        /// </summary>
        /// <returns></returns>
        void InitializeAsync(IntPtr hwnd);

        /// <summary>
        /// Cross-concearn: disable given subtitles in playback item
        /// </summary>
        /// <param name="currentPlaybackItem"></param>
        /// <param name="trackIndex"></param>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        Task SetDisableSubtitleAsync(MediaPlaybackItem currentPlaybackItem, uint trackIndex, int? hashCode = null);

        /// <summary>
        /// Apply track numbering to playback queue
        /// </summary>
        /// <returns></returns>
        //Task NumberNowPlayingQueue();

        /// <summary>
        ///DONE Remove items from playback queue.
        /// Strategy: if current item was removed, start playback from 0, reload next item
        /// Strategy: if current item was not removed, reload next item
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task RemoveItemsFromNowPlayingQueue(IEnumerable<MediaPlayerItemSourceUIWrapper> items);

        /// <summary>
        /// DONE
        /// Randomize the playback queue, reload next item
        /// </summary>
        /// <returns></returns>
        Task RandomizeNowPlayingQueue();

        /// <summary>
        /// DONE
        /// Strategy: if playback queue is empty => same as enqueue new list
        /// Strategy: if playback queue is not empty => Add items to playback queue, reload next item
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task AddToNowPlaying(IEnumerable<IMediaPlayerItemSource> FilesToAdd);

        /// <summary>
        /// DONE
        /// Strategy: if playback queue is empty => same as enqueue new list
        /// Strategy: if playback queue is not empty => Add items to playback queue, reload next item
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task EnqueueNext(IEnumerable<IMediaPlayerItemSource> FilesToAdd);

        /// <summary>
        /// DONE: Strategy: clear old queue, add new items to now playing, recreate source, skip to index, seek to start position, play
        /// </summary>
        /// <param name="FilesToAdd"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        Task StartPlaybackFromIndexAndPosition(IEnumerable<IMediaPlayerItemSource> FilesToAdd, int index, long position);

        /// <summary>
        /// Strategy: save new playback queue to disk, reload next time
        /// </summary>
        /// <returns></returns>
        Task SavePlaylistReorderAsync();
        /// <summary>
        /// Move an item up in playback queue:
        /// Strategy: Reload next item
        /// </summary>
        /// <param name="mds"></param>
        /// <returns></returns>
        Task MoveItemUpInPlaybackQueue(MediaPlayerItemSourceUIWrapper mds);
        /// <summary>
        /// Move an item down in playback queue
        /// Strategy: Reload next item
        /// </summary>
        /// <param name="mds"></param>
        /// <returns></returns>
        Task MoveItemDownInPlaybackQueue(MediaPlayerItemSourceUIWrapper mds);

        Task SetEqualizerConfiguration(EqualizerConfiguration configuration);

        Task AddPresetToConfiguration(EqualizerConfiguration configuration, AudioEqualizerPreset preset);
        Task DeletePresetFromConfiguration(EqualizerConfiguration configuration, AudioEqualizerPreset preset);

        Task<EqualizerConfiguration> GetCurrentEqualizerConfiguration();
        Task<EqualizerConfigurationDeletionResult> DeleteEqualizerConfiguration(EqualizerConfiguration config);
        Task AddEqualizerConfiguration(EqualizerConfiguration config);

        /// <summary>
        /// If the item is contained in playback queue, skip to index
        /// Otherwise it enqueues the item at the end and skips to index
        /// </summary>
        /// <param name="mediaData"></param>
        /// <returns></returns>
        Task SkipToQueueItem(IMediaPlayerItemSource mediaData);
    }

    public class NewQueueRequest
    {
        public bool Resume { get; set; }

        public bool IsShuffle { get; set; }

        public int StartIndex { get; set; }

        public long StartTime { get; set; }
    }

    public class ResumeRequest
    {
        public int StartIndex { get; set; }

        public long StartTime { get; set; }

        public MediaPlaybackItem InitialItem { get; set; }
    }

    public class PlaybackQueueTypeChangedArgs
    {
        public QueueType PlaybackType
        {
            get;
            private set;
        }

        public PlaybackQueueTypeChangedArgs(QueueType playbackType)
        {
            PlaybackType = playbackType;
        }
    }

    public enum QueueType
    {
        Local,
        DLNA
    }

    public enum EqualizerConfigurationDeletionResult
    {
        Success = 0,
        FailedInUse = 1,
        GeneralFailure = 2
    }
}

