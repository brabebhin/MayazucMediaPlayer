using MayazucMediaPlayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MayazucMediaPlayer.FileSystemViews
{
    public class IMediaPlayerItemSourceProvderCollection : ObservableCollection<IMediaPlayerItemSourceProvder>, IFilterableCollection<IMediaPlayerItemSourceProvder>
    {
        public IEnumerable<IMediaPlayerItemSourceProvder> Filter(string filterParam)
        {
            if (string.IsNullOrWhiteSpace(filterParam))
            {
                return this;
            }
            else
            {
                return new ObservableCollection<IMediaPlayerItemSourceProvder>(this.Where(x => x.Path.IndexOf(filterParam, StringComparison.CurrentCultureIgnoreCase) >= 0));
            }
        }
    }
}
