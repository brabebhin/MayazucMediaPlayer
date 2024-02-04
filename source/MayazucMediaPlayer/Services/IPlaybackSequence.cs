using FluentResults;
using MayazucMediaPlayer.Services.MediaSources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    public interface IPlaybackSequence
    {
        Task AddToSequenceAsync(IEnumerable<IMediaPlayerItemSource> mediaDatas, int index);
        Task<Result<MediaPlayerItemSourceUIWrapper>> GetMediaDataItemAtIndex(int index);
        Task LoadSequenceAsync();
        void LoadSequence();
        int RandomizeMusicDataStorage(int observableIndex);
        Task RemoveItemsFromSequenceAsync(IEnumerable<MediaPlayerItemSourceUIWrapper> items);
        Task SetSequence(IEnumerable<IMediaPlayerItemSource> mediaDatas);
        void Switch(int source, int destination);
        void ClearSequence();
    }
}