using System;
using System.Collections;
using System.Linq;

namespace MayazucMediaPlayer.Services
{
    public partial class AdvancedCollectionView : ObservableObject
    {
        IList collection = new ArrayList();
        public IList ItemsSource
        {
            get
            {
                return collection;
            }
            set
            {
                if (value != collection)
                {
                    collection = value;
                    CurrentView = collection;
                    NotifyPropertyChanged(nameof(CurrentView));
                    NotifyPropertyChanged(nameof(Count));
                }
            }
        }

        public IEnumerable CurrentView
        {
            get;
            private set;
        } = new ArrayList();

        public void Filter(Func<object, bool> filter)
        {
            if (filter == null)
            {
                CurrentView = collection;
            }
            else
            {
                CurrentView = collection.Cast<object>().Where(x => filter(x)).ToList();
            }

            NotifyPropertyChanged(nameof(CurrentView));
        }

        public int Count
        {
            get
            {
                return collection.Count;
            }
        }
    }
}
