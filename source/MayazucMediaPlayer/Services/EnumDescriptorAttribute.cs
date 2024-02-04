using System;

namespace MayazucMediaPlayer.Services
{
    public class PropertyDescriptor : Attribute
    {
        public string Description
        {
            get;
            set;
        }
    }
}
