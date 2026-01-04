using MayazucNativeFramework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback
{
    public interface IMediaPlaybackListAdapter : IDisposable
    {
        bool LocalSource { get; }

        event EventHandler AttachingToMediaPlayer;

        event EventHandler<MayazucCurrentMediaPlaybackItemChangedEventArgs> CurrentPlaybackItemChanged;

        event EventHandler ItemCreationFailed;

        event EventHandler Disposing;

        Task<bool> MoveToNextItem(MediaPlaybackItem CurrentItem, bool userAction, bool changeIndex);
        Task ReloadNextItemAsync(MediaPlaybackItem CurrenItem, bool userAction, bool changeIndex, int currentIndex);
        void RemoveFFmpegAudioEffects();
        void RemoveFFmpegVideoEffects();
        void SetFFmpegAudioEffects(IReadOnlyList<AvEffectDefinition> audioEffects);
        void SetFFmpegVideoEffects(IReadOnlyList<AvEffectDefinition> videoEffects);
        /// <summary>
        /// Starts playback at the specified index.
        /// </summary>
        /// <param name="index">the index to start playback from</param>
        /// <returns></returns>
        Task<bool> Start(int index);
        /// <summary>
        /// Starts playback with the specified MediaPlaybackItem
        /// /// </summary>
        /// <param name="initialItem">The next item will be loaded based on current index</param>
        /// <returns></returns>
        Task<bool> Start(MediaPlaybackItem initialItem);

        void Stop();
        Task<bool> BackstoreHasItems();

        Task<bool> CanGoBack();

        MediaPlaybackItem CurrentPlaybackItem { get; }

        Task<bool> ApplyMCVideoEffect(VideoEffectProcessorConfiguration configuration);

        MediaPlaybackItem DetachCurrentItem();
    }
}