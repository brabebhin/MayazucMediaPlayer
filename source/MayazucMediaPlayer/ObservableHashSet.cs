using System.Collections.Generic;
using System.Collections.Specialized;

namespace MayazucMediaPlayer
{
    public class ObservableHashSet<T> : HashSet<T>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public new bool Add(T item)
        {
            var ok = base.Add(item);
            if (ok)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
            return ok;
        }

        public new bool Remove(T item)
        {
            var ok = base.Remove(item);
            if (ok)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }
            return ok;
        }
    }
}
