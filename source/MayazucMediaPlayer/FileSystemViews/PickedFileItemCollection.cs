using MayazucMediaPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MayazucMediaPlayer.FileSystemViews
{
    public class IMediaPlayerItemSourceProvderCollection<T> : ObservableCollection<T>, IFilterableCollection<T> where T : IMediaPlayerItemSourceProvder
    {
        public IMediaPlayerItemSourceProvderCollection()
        {
        }

        public IMediaPlayerItemSourceProvderCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public IMediaPlayerItemSourceProvderCollection(List<T> list) : base(list)
        {
        }

        public virtual IEnumerable<T> Filter(string filterParam)
        {
            if (string.IsNullOrWhiteSpace(filterParam))
            {
                return this;
            }
            else
            {
                return new ObservableCollection<T>(this.Where(x => x.Path.IndexOf(filterParam, StringComparison.CurrentCultureIgnoreCase) >= 0));
            }
        }
    }
}
