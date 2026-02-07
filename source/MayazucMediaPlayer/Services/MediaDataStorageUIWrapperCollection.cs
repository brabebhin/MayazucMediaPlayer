using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.Services.MediaSources;
using System.Collections.Generic;
using System.Linq;

namespace MayazucMediaPlayer.Services
{
    public class MediaDataStorageUIWrapperCollection<T> : IMediaPlayerItemSourceProvderCollection<T> where T : MediaPlayerItemSourceUIWrapper
    {
        public MediaDataStorageUIWrapperCollection() : base()
        {
        }

        public MediaDataStorageUIWrapperCollection(IEnumerable<T> items) : base(items)
        {
        }

        public IReadOnlyCollection<IMediaPlayerItemSource> ToMediaDataList()
        {
            return this.Select(x => x.MediaData).ToList().AsReadOnly();
        }

        public int IndexOfMediaData(IMediaPlayerItemSource other)
        {
            if (other == null) return -1;

            for (int i = 0; i < Count; i++)
            {
                if (other.ID.Equals(this[i].MediaData.ID) || other.MediaPath == this[i].MediaData.MediaPath)
                    return i;
            }

            return -1;
        }

        public int IndexOfMediaData(MediaPlayerItemSourceUIWrapper other)
        {
            return IndexOfMediaData(other.MediaData);
        }
    }
}
