using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback.MediaSequencer
{
    /// <summary>
    /// Base class for media sequencers
    /// </summary>
    public interface IMediaSourceSequencer : IDisposable
    {
        Task AddItem(MediaPlaybackItem mediaPlaybackItem);
        event EventHandler SequenceEnded;
        event EventHandler<MediaOpenedEventArgs> MediaOpened;
        event EventHandler<MediaPlaybackItem> MediaFailed;
        MediaPlaybackItem? CurrentItem { get; }
        Task Start(MediaPlayer player, TimeSpan startPosition);

        /// <summary>
        /// Replaces the next item in the list with the value provided as parameter
        /// </summary>
        /// <param name="nextItem">Thew</param>
        /// <returns>the item that was removed from the list</returns>
        Task<MediaPlaybackItem?> ReloadNextItem(MediaPlaybackItem nextItem);

        /// <summary>
        /// Resets the list of items that were played.
        /// Returns the list to caller
        /// </summary>
        /// <returns></returns>
        IList<MediaPlaybackItem> Trim();

        void ApplyFFmpegAudioEffects(IReadOnlyList<AvEffectDefinition> audioEffects);

        void RemoveFFmpegAudioEffects();

        void ApplyFFmpegVideoEffects(IReadOnlyList<AvEffectDefinition> videoEffects);

        void RemoveFFmpegVideoEffects();
        MediaPlaybackItem? DetachCurrentItem();
    }
}
