using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MayazucMediaPlayer
{
    public class ObservableHashSet<T> : ObservableCollection<T>
    {
        HashSet<T> uniqueCheck = new HashSet<T>();

        public bool TryGetValue(T key, out T value)
        {
            return uniqueCheck.TryGetValue(key, out value);
        }

        public new virtual bool Add(T item)
        {
            var ok = uniqueCheck.Add(item);
            if (ok)
            {
                base.Add(item);
            }
            return ok;
        }

        public new virtual bool Remove(T item)
        {
            var ok = uniqueCheck.Remove(item);
            if (ok)
            {
                base.Remove(item);
            }
            return ok;
        }
    }
}
