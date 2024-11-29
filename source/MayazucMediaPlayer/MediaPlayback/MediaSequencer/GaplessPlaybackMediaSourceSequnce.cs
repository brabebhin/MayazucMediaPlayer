using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback.MediaSequencer
{
    /// <summary>
    /// A media source sequencer that uses a MediaPlaybackList as backing store for gapless playback
    /// </summary>
    public partial class GaplessPlaybackMediaSourceSequnce : IMediaSourceSequencer
    {
        bool started = false;
        public MediaPlaybackItem CurrentItem
        {
            get
            {
                if (MediaPlaybackList != null)
                    return MediaPlaybackList.CurrentItem;
                return null;
            }
        }

        public event EventHandler SequenceEnded;
        public event EventHandler<MediaOpenedEventArgs> MediaOpened;
        public event EventHandler<MediaPlaybackItem> MediaFailed;

        MediaPlayer player;
        MediaPlayer Player
        {
            get => player;
            set
            {
                if (player != null)
                {
                    player.MediaEnded -= Player_MediaEnded;
                    player.RemoveAllEffects();
                }
                player = value;
                if (player != null)
                {
                    player.RemoveAllEffects();
                    player.MediaEnded += Player_MediaEnded;
                }
            }
        }

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            if (!started) return;
            SequenceEnded?.Invoke(this, new EventArgs());
        }

        MediaPlaybackList MediaPlaybackList { get; set; } = new MediaPlaybackList();

        public GaplessPlaybackMediaSourceSequnce()
        {
            AttachPlaybackListEvents();
            MediaPlaybackList.ShuffleEnabled = false;
            MediaPlaybackList.AutoRepeatEnabled = false;
        }

        private void AttachPlaybackListEvents()
        {
            MediaPlaybackList.CurrentItemChanged += MediaPlaybackList_CurrentItemChanged;
            MediaPlaybackList.ItemFailed += MediaPlaybackList_ItemFailed;
        }

        private void DetachPlaybackListEvents()
        {
            MediaPlaybackList.CurrentItemChanged -= MediaPlaybackList_CurrentItemChanged;
            MediaPlaybackList.ItemFailed -= MediaPlaybackList_ItemFailed;
        }

        private void MediaPlaybackList_ItemFailed(MediaPlaybackList sender, MediaPlaybackItemFailedEventArgs args)
        {
            MediaFailed?.Invoke(this, args.Item);
        }

        private void MediaPlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (args.NewItem != null)
                MediaOpened?.Invoke(this, new MediaOpenedEventArgs(MediaOpenedEventReason.MediaPlaybackListItemChanged, new NewMediaPlaybackItemOpenedEventArgs(args.OldItem, args.NewItem, args.NewItem.GetExtradata())));
        }

        public Task AddItem(MediaPlaybackItem mediaPlaybackItem)
        {
            MediaPlaybackList.Items.Add(mediaPlaybackItem);
            return Task.CompletedTask;
        }

        public Task Start(MediaPlayer player, TimeSpan startPosition)
        {
            Player = player;
            Player.Source = null;
            Player.RemoveAllEffects();

            Player.Source = MediaPlaybackList;
            started = true;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (Player != null)
                Player.Source = null;
            Player = null;
            started = false;

            foreach (var item in MediaPlaybackList.Items)
            {
                item.GetExtradata().Dispose();
            }
            MediaPlaybackList.Items.Clear();
        }

        public Task<MediaPlaybackItem> ReloadNextItem(MediaPlaybackItem nextItem)
        {
            var nextIndex = (int)MediaPlaybackList.CurrentItemIndex + 1;

            if (MediaPlaybackList.Items.Count <= nextIndex) return Task.FromResult((MediaPlaybackItem)null);

            var itemToReturn = MediaPlaybackList.Items[nextIndex];
            MediaPlaybackList.Items.RemoveAt(nextIndex);

            if (nextItem != null)
            {
                MediaPlaybackList.Items.Add(nextItem);
            }

            return Task.FromResult(itemToReturn);
        }

        public IList<MediaPlaybackItem> Trim()
        {
            var currentItem = CurrentItem;
            List<MediaPlaybackItem> trimmedItems = new List<MediaPlaybackItem>();
            if (currentItem != null)
            {
                while (MediaPlaybackList.Items.Count > 0 && MediaPlaybackList.Items[0] != currentItem)
                {
                    try
                    {
                        trimmedItems.Add(MediaPlaybackList.Items[0]);
                        MediaPlaybackList.Items.RemoveAt(0);
                    }
                    catch { }
                }
            }

            return trimmedItems;
        }

        public void ApplyFFmpegAudioEffects(IReadOnlyList<AvEffectDefinition> audioEffects)
        {
            if (audioEffects == null) return;

            foreach (var item in MediaPlaybackList.Items)
                item.GetExtradata().FFmpegMediaSource.SetFFmpegAudioFilters(audioEffects.GetFFmpegFilterJoinedFilterDef());
        }

        public void RemoveFFmpegAudioEffects()
        {
            foreach (var item in MediaPlaybackList.Items)
                item.GetExtradata().FFmpegMediaSource?.ClearFFmpegAudioFilters();
        }

        public void ApplyFFmpegVideoEffects(IReadOnlyList<AvEffectDefinition> videoEffects)
        {
            if (videoEffects == null) return;

            foreach (var item in MediaPlaybackList.Items)
                item.GetExtradata().FFmpegMediaSource.SetFFmpegVideoFilters(videoEffects.GetFFmpegFilterJoinedFilterDef());
        }

        public void RemoveFFmpegVideoEffects()
        {
            foreach (var item in MediaPlaybackList.Items)
                item.GetExtradata().FFmpegMediaSource?.ClearFFmpegVideoFilters();
        }

        public MediaPlaybackItem DetachCurrentItem()
        {
            DetachPlaybackListEvents();
            System.Diagnostics.Debug.WriteLine($"Detaching current item: List contains {MediaPlaybackList.Items.Count}");
            var currentItem = MediaPlaybackList.CurrentItem;
            MediaPlaybackList.Items.Remove(currentItem);
            AttachPlaybackListEvents();
            return currentItem;
        }
    }
}
