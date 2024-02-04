using MayazucMediaPlayer.Services.MediaSources;
using System;
using System.Collections.Generic;

namespace MayazucMediaPlayer.Services
{
    public interface IPlaybackSequenceProvider
    {
        event EventHandler<IEnumerable<IMediaPlayerItemSource>> NewPlaybackQueue;
        event EventHandler<IEnumerable<IMediaPlayerItemSource>> PlaybackQueueChanged;

        void AddToNowPlaying(IEnumerable<IMediaPlayerItemSource> ToAdd);
        void ClearNowPlaying();
        void CreateNewPlaybackQueue(IEnumerable<IMediaPlayerItemSource> ToAdd);
        IReadOnlyCollection<IMediaPlayerItemSource> GetPlaybackQueue();
    }
}