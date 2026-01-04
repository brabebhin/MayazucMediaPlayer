using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.VideoEffects;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback
{
    /// <summary>
    /// a proxy between the background media player task and the rest of the application.
    /// </summary>
    public class BackgroundMediaServiceConnector
    {
        public bool HasActivePlaybackSession()
        {
            return (CurrentPlaybackSession).PlaybackState != MediaPlaybackState.None;
        }

        public bool IsPlaying()
        {
            return (CurrentPlaybackSession).PlaybackState == MediaPlaybackState.Playing;
        }

        public void Initialize(IServiceProvider services, IntPtr hwnd, ulong windowId)
        {
            if (PlayerInstance == null)
            {
                PlayerInstance = (IBackgroundPlayer)services.GetService(typeof(IBackgroundPlayer));
                PlayerInstance.Initialize(hwnd, windowId);
            }
        }


        public ManagedVideoEffectProcessorConfiguration VideoEffectsConfiguration
        {
            get
            {
                return PlayerInstance.VideoEffectsConfiguration;
            }
        }

        public MediaPlayer CurrentPlayer
        {
            get
            {
                return PlayerInstance.CurrentPlayer;
            }
        }


        public MediaPlaybackSession CurrentPlaybackSession
        {
            get
            {
                return PlayerInstance.CurrentPlayer.PlaybackSession;
            }
        }

        public IBackgroundPlayer PlayerInstance { get; private set; }

        public event EventHandler<MediaPlayerCompactOverlayEventArgs> CompactOverlayRequest;
        public void NotifyViewMode(bool isFullPlayer, UserControl element)
        {
            CompactOverlayRequest?.Invoke(null, new MediaPlayerCompactOverlayEventArgs(isFullPlayer, element));
        }

        public void NotifyAudioBalanceChanged(double value)
        {
            CurrentPlayer.AudioBalance = value;
        }

        public async Task NotifyResetFiltering(bool enabled)
        {
            await (PlayerInstance).ResetFiltering(enabled);
        }

        internal async Task SetActiveSubtitle(MediaPlaybackItem currentPlaybackItem, uint trackIndex, int? playbackItemHash = null)
        {
            await (PlayerInstance).SetActiveSubtitleAsync(currentPlaybackItem, trackIndex, playbackItemHash);
        }

        internal async Task SetDisabledSubtitle(MediaPlaybackItem currentPlaybackItem, uint trackIndex, int? playbackItemHash = null)
        {
            await (PlayerInstance).SetDisableSubtitleAsync(currentPlaybackItem, trackIndex, playbackItemHash);
        }

        internal event EventHandler<TimeSpan> SubtitleDelayChanged;
        internal async Task<TimeSpan> QuickenSubtitles()
        {
            var returnvalue = await (PlayerInstance).QuickenSubtitles();
            SubtitleDelayChanged?.Invoke(PlayerInstance, returnvalue);
            return returnvalue;
        }

        internal async Task<TimeSpan> DelaySubtitles()
        {
            var returnvalue = await (PlayerInstance).DelaySubtitles();
            SubtitleDelayChanged?.Invoke(PlayerInstance, returnvalue);
            return returnvalue;
        }

        public readonly int DefaultPlaybackSpeedIndex = 10;
        public readonly IList<double> AllowedPlaybackRates = new double[] { 0.125, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 2, 3, 4 };
        public event EventHandler<double> ExternalPlaybackRateChanged;

        /// <summary>
        /// adds to now playing;
        /// </summary>
        /// <param name="FilesToAdd"></param>
        public async Task EnqueueAtEnd(IEnumerable<IMediaPlayerItemSource> FilesToAdd)
        {
            await (PlayerInstance).AddToNowPlaying(FilesToAdd);
        }

        /// <summary>
        /// adds to now playing;
        /// </summary>
        /// <param name="FilesToAdd"></param>
        public async Task EnqueueNext(IEnumerable<IMediaPlayerItemSource> FilesToAdd)
        {
            await (PlayerInstance).EnqueueNext(FilesToAdd);
        }

        public async Task AddToNowPlaying(IMediaPlayerItemSource data)
        {
            await EnqueueAtEnd(new IMediaPlayerItemSource[] { data });
        }

        /// <summary>
        /// queues a new list; removes old now playing
        /// </summary>
        public async Task StartPlaybackFromBeginning(IEnumerable<IMediaPlayerItemSource> FilesToAdd)
        {
            await (PlayerInstance).StartPlaybackFromIndexAndPosition(FilesToAdd, 0, 0);
        }

        public async Task StartPlaybackFromBeginning(IMediaPlayerItemSource FilesToAdd)
        {
            await StartPlaybackFromBeginning(new IMediaPlayerItemSource[] { FilesToAdd });
        }

        public async Task StartPlaybackFromIndexAndPosition(IEnumerable<IMediaPlayerItemSource> FilesToAdd, int index, long position)
        {
            await (PlayerInstance).StartPlaybackFromIndexAndPosition(FilesToAdd, index, position);
        }

        public async Task SkipNext()
        {
            await (PlayerInstance).SkipNext();
        }

        public async Task SkipPrevious()
        {
            await (PlayerInstance).SkipPrevious();
        }

        public async Task SkipToIndex(int index)
        {
            await (PlayerInstance).SkipToIndex(index);
        }

        public async Task PlayPauseAutoSwitch()
        {
            await (PlayerInstance).PlayPauseAsync();
        }

        public async Task<object> SkipSecondsBack(long toSeekSeconds)
        {
            if (CurrentPlaybackSession != null)
            {
                var session = CurrentPlaybackSession;

                await (PlayerInstance).Seek(session.Position.Subtract(TimeSpan.FromSeconds(toSeekSeconds)), true);
                return true;
            }
            return false;
        }

        public async Task<object> QuickenPlaybackRate()
        {
            var session = CurrentPlaybackSession;

            int index = GetPlaybackRateIndex();
            index++;
            if (index >= AllowedPlaybackRates.Count)
            {
                index = AllowedPlaybackRates.Count - 1;
            }
            session.PlaybackRate = AllowedPlaybackRates[index];

            ExternalPlaybackRateChanged?.Invoke(null, session.PlaybackRate);
            return true;
        }

        public async Task<object> SlowerPlaybackRate()
        {
            var session = CurrentPlaybackSession;
            int index = GetPlaybackRateIndex();
            index--;
            if (index < 0) index = 0;
            session.PlaybackRate = AllowedPlaybackRates[index];

            ExternalPlaybackRateChanged?.Invoke(null, session.PlaybackRate);

            return true;
        }

        private int GetPlaybackRateIndex()
        {
            var playbackRate = (CurrentPlaybackSession).PlaybackRate;
            var index = AllowedPlaybackRates.IndexOf(playbackRate);

            if (index < 0) index = 0;

            return index;
        }

        public async Task<object> SkipSecondsForth(long toSeekSeconds)
        {
            var seekSpan = TimeSpan.FromSeconds(toSeekSeconds);
            var playbackSession = CurrentPlaybackSession;
            if (playbackSession.NaturalDuration - playbackSession.Position > seekSpan)
                await (PlayerInstance).Seek(playbackSession.Position.Add(seekSpan), true);
            else await (PlayerInstance).Seek(playbackSession.NaturalDuration - TimeSpan.FromSeconds(0.1), true);
            return true;
        }

        public async Task SetRepeatMode(string tag)
        {
            var desiredRepeatMode = "";
            switch (tag)
            {
                case Constants.RepeatAll:

                    desiredRepeatMode = tag; break;
                case Constants.RepeatOne:

                    desiredRepeatMode = tag; break;
                case Constants.RepeatNone:

                    desiredRepeatMode = tag; break;
                default: throw new ArgumentException("invalid repeat mode");
            }
            if (desiredRepeatMode != SettingsService.Instance.RepeatMode)
            {
                SettingsService.Instance.RepeatMode = tag;
                await (PlayerInstance).RepeatModeChanged();
            }
        }

        public async Task SetShuffleMode(bool mode)
        {
            if (SettingsService.Instance.ShuffleMode != mode)
            {
                SettingsService.Instance.ShuffleMode = mode;
                await (PlayerInstance).ShuffleModeChanged();
            }
        }

        internal async Task SkipToQueueItem(IMediaPlayerItemSource mediaData)
        {
            await (PlayerInstance).SkipToQueueItem(mediaData);
        }

        internal double GetNormalizedPosition()
        {
            if (HasActivePlaybackSession() && PlayerInstance.CurrentPlaybackItem != null && CurrentPlayer.PlaybackSession.NaturalDuration.TotalSeconds != 0)
            {
                //100 -> naturalDuration
                //x  --> position

                return (CurrentPlayer.Position.TotalSeconds * 100) / CurrentPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
            }

            return 0;
        }
    }
}
