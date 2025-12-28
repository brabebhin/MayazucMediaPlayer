using Nito.AsyncEx;
using System.Collections.Concurrent;

namespace MayazucMediaPlayer.LocalCache
{
    public class AsyncLockManager
    {
        private readonly ConcurrentDictionary<string, AsyncLock> locks = new ConcurrentDictionary<string, AsyncLock>();

        public AsyncLock GetLock(string key)
        {
            return locks.GetOrAdd(key, (s) =>
            {
                return new AsyncLock();
            });
        }

        public void Clear()
        {
            locks.Clear();
        }
    }
}
