using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Common
{
    public static class EmptyCallbacks
    {
        public static Action<object> Action
        {
            get
            {
                return (value) => { };
            }
        }

        public static Func<object> Func
        {
            get
            {
                return () => { return new object(); };
            }
        }
    }
}
