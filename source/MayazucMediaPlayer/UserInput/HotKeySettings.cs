using System;

namespace MayazucMediaPlayer.UserInput
{
    public class HotKeySettings
    {
        public HotKeySettings() { }

        public HotKeySettings(string virtualKey, string modifier)
        {
            VirtualKey = virtualKey.ToUpperInvariant();
            Modifier = modifier.ToLowerInvariant();
        }

        public string? VirtualKey { get; set; }

        public string? Modifier { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is HotKeySettings settings &&
                   VirtualKey == settings.VirtualKey &&
                   Modifier == settings.Modifier;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VirtualKey, Modifier);
        }
    }
}
