using System;
using System.Collections.ObjectModel;

namespace MayazucMediaPlayer.MediaPlayback
{
    public class AdaptiveEqualizationConfiguration
    {
        public string PresetName
        {
            get;
            private set;
        }

        public string EqualizerConfiguration
        {
            get;
            private set;
        }

        public ReadOnlyCollection<int> SavedAmplifications
        {
            get;
            private set;
        }

        public AdaptiveEqualizationConfiguration(string presetName, string equalizerConfiguration, ReadOnlyCollection<int> savedAmplifications)
        {
            PresetName = presetName ?? throw new ArgumentNullException(nameof(presetName));
            EqualizerConfiguration = equalizerConfiguration ?? throw new ArgumentNullException(nameof(equalizerConfiguration));
            SavedAmplifications = savedAmplifications ?? throw new ArgumentNullException(nameof(savedAmplifications));
        }
    }
}
