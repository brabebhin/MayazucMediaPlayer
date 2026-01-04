using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MayazucMediaPlayer.FileSystemViews
{
    public class IMediaPlayerItemSourceProvderCollection<T> : ObservableCollection<T> where T : IMediaPlayerItemSourceProvder
    {
        private ConcurrentDictionary<string, List<T>> pathMap = new ConcurrentDictionary<string, List<T>>();
        private string playingMediaPath = null;

        public IMediaPlayerItemSourceProvderCollection()
        {
        }

        public IMediaPlayerItemSourceProvderCollection(IEnumerable<T> collection) : base(collection)
        {
            SyncPathMap(collection);
        }

        public IMediaPlayerItemSourceProvderCollection(List<T> list) : base(list)
        {
            SyncPathMap(list);
        }

        private void SyncPathMap(IEnumerable<T> list)
        {
            foreach (T item in list)
            {
                SyncAddPathMap(item);
            }
        }

        public void NotifyPlayingMediaPath(string mediaPath)
        {
            if (playingMediaPath != null)
            {
                if (pathMap.TryGetValue(playingMediaPath, out var list))
                {
                    list.ForEach(x => x.IsInPlayback = false);
                }
            }

            playingMediaPath = mediaPath;

            if (playingMediaPath != null)
            {
                if (pathMap.TryGetValue(mediaPath, out var list))
                {
                    list.ForEach(x => x.IsInPlayback = true);
                }
            }
        }

        private void SyncAddPathMap(T item)
        {
            if (pathMap.TryGetValue(item.Path, out var value))
            {
                value.Add(item);
            }
            else
            {
                pathMap.TryAdd(item.Path, new List<T>() { item });
            }
        }

        private void SyncRemovePathMap(T item)
        {
            if (pathMap.TryGetValue(item.Path, out var value))
            {
                value.Remove(item);
            }
        }

        protected override sealed void SetItem(int index, T item)
        {
            var oldItem = this[index];
            base.SetItem(index, item);

            item.ExpectedPlaybackIndex = index;

            SyncAddPathMap(item);
            SyncRemovePathMap(oldItem);
        }

        protected override sealed void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            for (int i = index; i < Count; i++)
            {
                Items[i].ExpectedPlaybackIndex = i;
            }

            SyncAddPathMap(item);
        }

        protected override sealed void RemoveItem(int index)
        {
            var oldItem = Items[index];
            base.RemoveItem(index);
            for (int i = index; i < Count; i++)
            {
                Items[i].ExpectedPlaybackIndex = i;
            }

            SyncRemovePathMap(oldItem);
        }
    }
}
