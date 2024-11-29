using System;

namespace MayazucMediaPlayer.MediaCollections
{
    public partial class NetworkStreamHistoryEntryCollection : ObservableHashSet<NetworkStreamHistoryEntry>
    {
        public override bool Add(NetworkStreamHistoryEntry item)
        {
            if (this.TryGetValue(item, out var entry))
            {
                entry.Time = DateTime.UtcNow;
                return true;
            }
            else
            {
                item.Time = DateTime.UtcNow;
                return base.Add(item);
            }
        }

        public override bool Remove(NetworkStreamHistoryEntry item)
        {
            return base.Remove(item);
        }

        public bool Add(string url)
        {
            return this.Add(new NetworkStreamHistoryEntry() { Url = url });
        }
    }
}
