using MayazucNativeFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback.MediaSequencer
{
    /// <summary>
    /// A media sequencer that plays each item individually
    /// </summary>
    public class VideoEffectSequentialMediaSourceSequence : IMediaSourceSequencer
    {
        bool started = false;
        readonly Queue<MediaPlaybackItem> items = new Queue<MediaPlaybackItem>();
        readonly Queue<MediaPlaybackItem> oldItems = new Queue<MediaPlaybackItem>();
        public MediaPlaybackItem? CurrentItem
        {
            get
            {
                if (Player != null)
                    return Player.Source as MediaPlaybackItem;
                return null;
            }
        }

        readonly bool hadAutoPlay = false;
        public event EventHandler? SequenceEnded;
        public event EventHandler<MediaOpenedEventArgs>? MediaOpened;
        public event EventHandler<MediaPlaybackItem>? MediaFailed;
        MediaPlayer? player;
        readonly VideoEffectProcessorConfiguration VideoEffectsConfiguration;

        MediaPlayer? Player
        {
            get => player;
            set
            {
                if (player != null)
                {
                    DetachMediaPlayerEvents();
                    player.RemoveAllEffects();
                    //player.AutoPlay = hadAutoPlay;
                }
                player = value;
                if (player != null)
                {
                    //hadAutoPlay = player.AutoPlay;
                    //player.AutoPlay = true;
                    AttachMediaPlayerEvents();
                }
            }
        }

        private void AttachMediaPlayerEvents()
        {
            player.MediaFailed += Player_MediaFailed;
            player.MediaEnded += Player_MediaEnded;
            player.MediaOpened += Player_MediaOpened;
        }

        private void DetachMediaPlayerEvents()
        {
            player.MediaEnded -= Player_MediaEnded;
            player.MediaOpened -= Player_MediaOpened;
            player.MediaFailed -= Player_MediaFailed;
        }

        public VideoEffectSequentialMediaSourceSequence(VideoEffectProcessorConfiguration videoEffectConfig)
        {
            VideoEffectsConfiguration = videoEffectConfig;
        }

        private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            if (!started || CurrentItem == null) return;

            MediaFailed?.Invoke(this, CurrentItem);
        }

        private void Player_MediaOpened(MediaPlayer sender, object args)
        {
            if (!started) return;

            var item = CurrentItem!;
            MediaOpened?.Invoke(this, new MediaOpenedEventArgs(MediaOpenedEventReason.MediaPlaybackListItemChanged, new NewMediaPlaybackItemOpenedEventArgs(null, item, item.GetExtradata())));
        }

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            if (!started || player == null) return;

            if (items.TryDequeue(out var nextItem))
            {
                var oldItem = player.Source as MediaPlaybackItem;
                player.Source = nextItem;
                if (oldItem != null)
                {
                    oldItems.Enqueue(oldItem);
                }
            }
            else
            {
                SequenceEnded?.Invoke(this, new EventArgs());
            }
        }

        public Task AddItem(MediaPlaybackItem mediaPlaybackItem)
        {
            items.Enqueue(mediaPlaybackItem);
            return Task.CompletedTask;
        }

        public Task Start(MediaPlayer player, TimeSpan startPosition)
        {
            Player = player;
            Player.Source = items.Dequeue();
            started = true;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (Player != null)
            {
                CurrentItem?.GetExtradata()?.Dispose();
                Player.Source = null;
            }
            Player = null;
            started = false;
            foreach (var item in items.Concat(oldItems))
                item.GetExtradata().Dispose();
            items.Clear();
            oldItems.Clear();
        }

        public Task<MediaPlaybackItem?> ReloadNextItem(MediaPlaybackItem nextItem)
        {
            items.TryDequeue(out var itemRemoved);
            if (nextItem != null)
                items.Enqueue(nextItem);
            return Task.FromResult(itemRemoved);
        }

        public IList<MediaPlaybackItem> Trim()
        {
            //this one doesn't need trimming
            List<MediaPlaybackItem> trimmedItems = new List<MediaPlaybackItem>(oldItems);
            oldItems.Clear();
            return trimmedItems;
        }

        public void ApplyFFmpegAudioEffects(IReadOnlyList<AvEffectDefinition> audioEffects)
        {
            if (audioEffects == null)
            {
                RemoveFFmpegAudioEffects();
                return;
            }
            CurrentItem?.GetExtradata()?.FFmpegMediaSource?.SetFFmpegAudioFilters(audioEffects.GetFFmpegFilterJoinedFilterDef());
            foreach (var item in items)
                item.GetExtradata()?.FFmpegMediaSource?.SetFFmpegAudioFilters(audioEffects.GetFFmpegFilterJoinedFilterDef());
        }

        public void RemoveFFmpegAudioEffects()
        {
            CurrentItem?.GetExtradata()?.FFmpegMediaSource?.ClearFFmpegAudioFilters();
            foreach (var item in items)
                item.GetExtradata()?.FFmpegMediaSource?.ClearFFmpegAudioFilters();
        }

        public void ApplyFFmpegVideoEffects(IReadOnlyList<AvEffectDefinition> videoEffects)
        {
            if (videoEffects == null)
            {
                RemoveFFmpegVideoEffects();
                return;
            }
            CurrentItem?.GetExtradata()?.FFmpegMediaSource?.SetFFmpegAudioFilters(videoEffects.GetFFmpegFilterJoinedFilterDef());
            foreach (var item in items)
                item.GetExtradata()?.FFmpegMediaSource?.SetFFmpegAudioFilters(videoEffects.GetFFmpegFilterJoinedFilterDef());

        }

        public void RemoveFFmpegVideoEffects()
        {
            CurrentItem?.GetExtradata()?.FFmpegMediaSource?.ClearFFmpegVideoFilters();
            foreach (var item in items)
                item.GetExtradata()?.FFmpegMediaSource?.ClearFFmpegVideoFilters();
        }

        public MediaPlaybackItem? DetachCurrentItem()
        {
            DetachMediaPlayerEvents();
            var oldItem = CurrentItem;
            if (player != null)
                player.Source = null;
            AttachMediaPlayerEvents();
            return oldItem;
        }
    }
}
