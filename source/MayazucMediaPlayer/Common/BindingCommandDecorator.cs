using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Common
{
    public interface BindingCommandDecorator<T> where T : class
    {
        T Commands { get; }
    }
}
