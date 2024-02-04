using System;
using System.Collections.Generic;

namespace MayazucMediaPlayer.UserInput
{
    public class MayazucHotKey
    {
        public HotKeyId Id { get; private set; }

        public HotKeySettings Settings { get; private set; }

        public MayazucHotKey(HotKeySettings settings, HotKeyId id)
        {
            Id = id;
            Settings = settings;
        }

        public override bool Equals(object? obj)
        {
            return obj is MayazucHotKey accelerator &&
                   Id == accelerator.Id &&
                   EqualityComparer<HotKeySettings>.Default.Equals(Settings, accelerator.Settings);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Settings);
        }
    }
}
