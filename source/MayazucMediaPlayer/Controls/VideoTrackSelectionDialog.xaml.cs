using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class VideoTrackSelectionDialog : BaseUserControl
    {
        readonly AsyncLock trackLock = new AsyncLock();
        MediaPlaybackItem currentItem = null;

        public VideoTrackSelectionDialog()
        {
            InitializeComponent();
        }

        public async Task LoadVideoTracksAsync(MediaPlaybackItem item)
        {
            using (await trackLock.LockAsync())
            {
                if (item != null)
                {
                    if (currentItem != null)
                    {
                        currentItem.VideoTracks.SelectedIndexChanged -= VideoTracks_SelectedIndexChanged;
                        currentItem.VideoTracksChanged -= CurrentItem_VideoTracksChanged;
                    }
                    currentItem = item;

                    ProcessPlaybackItem(item);

                    currentItem.VideoTracks.SelectedIndexChanged += VideoTracks_SelectedIndexChanged;
                    currentItem.VideoTracksChanged += CurrentItem_VideoTracksChanged;
                }
            }
        }

        private void ProcessPlaybackItem(MediaPlaybackItem item)
        {
            lsvVideoStreams.SelectionChanged -= LsvVideoStreams_SelectionChanged;
            HashSet<VideoStreamPickerWrapper> items = new HashSet<VideoStreamPickerWrapper>();

            for (int i = 0; i < item.VideoTracks.Count; i++)
            {
                var ms = item.VideoTracks[i];
                items.Add(new VideoStreamPickerWrapper(ms, i + 1));
            }
            lsvVideoStreams.ItemsSource = items;
            lsvVideoStreams.SelectedIndex = item.VideoTracks.SelectedIndex;

            lsvVideoStreams.SelectionChanged += LsvVideoStreams_SelectionChanged;
        }

        private async void LsvVideoStreams_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            using (await trackLock.LockAsync())
            {
                if (currentItem != null)
                {
                    if (lsvVideoStreams.SelectedIndex >= 0)
                        currentItem.VideoTracks.SelectedIndex = lsvVideoStreams.SelectedIndex;
                }
            }
        }

        private async void CurrentItem_VideoTracksChanged(MediaPlaybackItem sender, IVectorChangedEventArgs args)
        {
            using (await trackLock.LockAsync())
            {
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    if (currentItem != null)
                        ProcessPlaybackItem(currentItem);
                });
            }
        }

        private async void VideoTracks_SelectedIndexChanged(ISingleSelectMediaTrackList sender, object args)
        {
            using (await trackLock.LockAsync())
            {
                await DispatcherQueue.EnqueueAsync(() => { lsvVideoStreams.SelectedIndex = sender.SelectedIndex; });
            }
        }
    }

    public class VideoStreamPickerWrapper
    {
        readonly VideoTrack track;
        readonly int ordinal = 0;

        public VideoStreamPickerWrapper(VideoTrack ms, int ord)
        {
            track = ms;
            ordinal = ord;
        }

        public override bool Equals(object? obj)
        {
            return obj is VideoStreamPickerWrapper wrapper &&
                   EqualityComparer<VideoTrack>.Default.Equals(track, wrapper.track) &&
                   ordinal == wrapper.ordinal;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(track, ordinal);
        }

        public string FormatString
        {
            get
            {
                return ToString();
            }
        }

        public override string ToString()
        {
            var name = track.Label;
            var encodingProperties = track.GetEncodingProperties();
            var encodingInfo = $"{encodingProperties.Width} x {encodingProperties.Height}";

            if (string.IsNullOrEmpty(name))
            {
                name = $"Video track {ordinal}";
            }

            return $"{name} : {encodingInfo}";
        }
    }
}
