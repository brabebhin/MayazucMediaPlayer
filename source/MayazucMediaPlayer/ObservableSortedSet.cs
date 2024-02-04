using System.Collections.Generic;
using System.Collections.Specialized;

namespace MayazucMediaPlayer
{
    public class ObservableSortedSet<T> : SortedSet<T>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public new bool Add(T item)
        {
            var result = base.Add(item);

            if (result)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }

            return result;
        }

        public new bool Remove(T item)
        {
            var result = base.Remove(item);

            if (result)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }

            return result;
        }
    }
}
