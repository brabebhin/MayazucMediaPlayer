using System.Collections.Generic;

namespace MayazucMediaPlayer.Services
{
    public interface IFilterableCollection<T>
    {
        IEnumerable<T> Filter(string filterParam);
    }
}
