using FFmpegInteropX;
using MayazucMediaPlayer.Services.MediaSources;
using System;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback
{
    public sealed class PlaybackItemExtraData : IDisposable
    {
        private readonly object threadLock = new object();
        private volatile bool disposedValue;

        public bool Disposed
        {
            get => disposedValue;
            set
            {
                lock (threadLock)
                {
                    disposedValue = value;
                }
            }
        }

        public IMediaPlayerItemSource MediaPlayerItemSource
        {
            get;
            private set;
        }

        public FFmpegMediaSource FFmpegMediaSource
        {
            get;
            private set;
        }


        public SubtitleManager SubtitleService
        {
            get;
            private set;
        }

        public MediaPlaybackItemUIInformation PlaybackDataStreamInfo
        {
            get;
            private set;
        }

        public TimeSpan CurrentSubtitleDelay
        {
            get
            {
                return FFmpegMediaSource.Configuration.Subtitles.DefaultSubtitleDelay;
            }
            set
            {
                FFmpegMediaSource.Configuration.Subtitles.DefaultSubtitleDelay = value;
                FFmpegMediaSource.SetSubtitleDelay(value);
            }
        }

        public PlaybackItemExtraData(FFmpegMediaSource interopMss,
            MediaPlaybackItemUIInformation playbackDataStreamInfo,
            IMediaPlayerItemSource mediaPlayerItemSource,
            SubtitleManager subsManager)
        {
            MediaPlayerItemSource = mediaPlayerItemSource;
            FFmpegMediaSource = interopMss ?? throw new ArgumentNullException(nameof(interopMss));
            PlaybackDataStreamInfo = playbackDataStreamInfo ?? throw new ArgumentNullException(nameof(playbackDataStreamInfo));
            SubtitleService = subsManager;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                Disposed = true;
                FFmpegMediaSource.Dispose();
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~PlaybackItemExtraData()
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

    public static class ExtensionsExtradata
    {
        public static PlaybackItemExtraData GetExtradata(this MediaPlaybackItem item)
        {
            if (item == null)
            {
                return null;
            }

            if (item.Source == null)
            {
                return null;
            }

            return item.Source.CustomProperties[nameof(PlaybackItemExtraData)] as PlaybackItemExtraData;
        }

        private static void StoreExtradata(this MediaPlaybackItem item, PlaybackItemExtraData extradata)
        {
            item.Source.CustomProperties.Add(nameof(PlaybackItemExtraData), extradata);
        }

        public static PlaybackItemExtraData AddExtradataToPlaybackItem(this MediaPlaybackItem item,
            IMediaPlayerItemSource currentDataStorage,
            FFmpegMediaSource interopMss,
            MediaPlaybackItemUIInformation playbackDataStreamInfo)
        {
            var subsManager = new SubtitleManager(item);
            var extradata = new PlaybackItemExtraData(interopMss, playbackDataStreamInfo, currentDataStorage, subsManager);
            item.StoreExtradata(extradata);
            return extradata;
        }
    }
}
