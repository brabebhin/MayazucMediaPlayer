using System.Collections.Concurrent;

namespace MayazucMediaPlayer.Common
{
    public class SimpleObjectCache
    {
        private readonly ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();

        public T TryAdd<T>(string key, T value)
        {
            cache.TryAdd(key, value);
            return value;
        }

        public T Get<T>(string key)
        {
            return (T)cache[key];
        }

        public bool HasKey(string key)
        {
            return cache.ContainsKey(key);
        }

        public void Clear()
        {
            cache.Clear();
        }
    }
}
