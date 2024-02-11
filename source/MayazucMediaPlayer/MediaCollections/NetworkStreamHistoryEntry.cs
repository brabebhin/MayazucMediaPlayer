using System;

namespace MayazucMediaPlayer.MediaCollections
{
    public class NetworkStreamHistoryEntry
    {
        public string Url { get; set; }

        public DateTimeOffset? Time { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is NetworkStreamHistoryEntry entry &&
                   Url == entry.Url;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Url);
        }
    }
}
