using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayazucMediaPlayer
{
    /// <summary>
    /// An observable collection that will update itself based on the changes made to the source collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class SynchronizedObservableCollection<TContents, TSource> : ObservableCollection<TContents>, IDisposable
    {
        private bool disposedValue;
        readonly ObservableCollection<TSource> source;
        readonly Func<TSource, TContents> converter;

        public static void AddRange<T>(ObservableCollection<T> target, IEnumerable<T> itemsToAdd)
        {
            foreach (var t in itemsToAdd)
            {
                target.Add(t);
            }
        }

        public SynchronizedObservableCollection(ObservableCollection<TSource> source, Func<TSource, TContents> converter)
        {
            this.converter = converter;
            this.source = source;
            AddRange(this, source.Select(converter));
            this.source.CollectionChanged += Source_CollectionChanged;
            (this.source as INotifyPropertyChanged).PropertyChanged += SynchronizedObservableCollection_PropertyChanged;
        }

        private void SynchronizedObservableCollection_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        private void Source_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var newItem = converter((TSource)e.NewItems[i]!);
                        this.Insert(i + e.NewStartingIndex, newItem);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        this.RemoveAt(i + e.OldStartingIndex);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        var newItem = converter((TSource)e.NewItems[i]!);
                        this[i + e.OldStartingIndex] = newItem;
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        this.Move(i + e.OldStartingIndex, i + e.NewStartingIndex);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    {
                        for (int i = 0; i < source.Count; i++)
                        {
                            this[i] = converter(source[i]);
                        }
                    }
                    break;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    source.CollectionChanged -= Source_CollectionChanged;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SynchronizedObservableCollection()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private class INotifyCollectionChangedSuspendable : IDisposable
        {
            readonly ObservableCollection<TSource> source;
            readonly NotifyCollectionChangedEventHandler suspendableHandler;

            public INotifyCollectionChangedSuspendable(ObservableCollection<TSource> source, NotifyCollectionChangedEventHandler suspendableHandler)
            {
                this.source = source;
                this.suspendableHandler = suspendableHandler;
                this.source.CollectionChanged -= suspendableHandler;
            }

            public void Dispose()
            {
                this.source.CollectionChanged += suspendableHandler;
            }
        }
    }
}
