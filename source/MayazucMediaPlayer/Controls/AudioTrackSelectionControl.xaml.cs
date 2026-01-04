using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class AudioTrackSelectionControl : BaseUserControl
    {
        readonly AsyncLock trackLock = new AsyncLock();
        MediaPlaybackItem currentItem = null;

        public AudioTrackSelectionControl()
        {
            InitializeComponent();
        }

        public async Task LoadAudioTracksAsync(MediaPlaybackItem item)
        {
            using (await trackLock.LockAsync())
            {
                if (item != null)
                {
                    if (currentItem != null)
                    {
                        currentItem.VideoTracks.SelectedIndexChanged -= AudioTracks_SelectedIndexChanged;
                        currentItem.VideoTracksChanged -= CurrentItem_VideoTracksChanged;
                    }
                    currentItem = item;

                    ProcessPlaybackItem(item);

                    currentItem.VideoTracks.SelectedIndexChanged += AudioTracks_SelectedIndexChanged;
                    currentItem.VideoTracksChanged += CurrentItem_VideoTracksChanged;
                }
            }
        }

        private void ProcessPlaybackItem(MediaPlaybackItem item)
        {
            lsvAudioStreams.SelectionChanged -= LsvVideoStreams_SelectionChanged;
            HashSet<AudioStreamPickerWrapper> items = new HashSet<AudioStreamPickerWrapper>();

            for (int i = 0; i < item.AudioTracks.Count; i++)
            {
                var ms = item.AudioTracks[i];
                items.Add(new AudioStreamPickerWrapper(ms, i + 1));
            }
            lsvAudioStreams.ItemsSource = items;
            lsvAudioStreams.SelectedIndex = item.AudioTracks.SelectedIndex;

            lsvAudioStreams.SelectionChanged += LsvVideoStreams_SelectionChanged;
        }

        private async void LsvVideoStreams_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            using (await trackLock.LockAsync())
            {
                if (currentItem != null)
                {
                    if (lsvAudioStreams.SelectedIndex >= 0)
                        currentItem.VideoTracks.SelectedIndex = lsvAudioStreams.SelectedIndex;
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

        private async void AudioTracks_SelectedIndexChanged(ISingleSelectMediaTrackList sender, object args)
        {
            using (await trackLock.LockAsync())
            {
                await DispatcherQueue.EnqueueAsync(() => { lsvAudioStreams.SelectedIndex = sender.SelectedIndex; });
            }
        }


    }

    internal class AudioStreamPickerWrapper
    {
        readonly AudioTrack track;
        readonly int ordinal = 0;

        public AudioStreamPickerWrapper(AudioTrack ms, int ord)
        {
            track = ms;
            ordinal = ord;
        }

        public override bool Equals(object? obj)
        {
            return obj is AudioStreamPickerWrapper wrapper &&
                   EqualityComparer<AudioTrack>.Default.Equals(track, wrapper.track) &&
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
            var encodingInfo = $"{encodingProperties.SampleRate} Hz, {encodingProperties.ChannelCount} Channels, {encodingProperties.Bitrate} bits/s";

            if (string.IsNullOrEmpty(name))
            {
                name = $"Track {ordinal}";
            }

            return $"{name} : {encodingInfo}";
        }
    }
}
