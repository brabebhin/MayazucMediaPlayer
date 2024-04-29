using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using System.Collections.ObjectModel;
using System.Linq;

namespace MayazucMediaPlayer.AudioEffects
{
    public sealed class EqualizerService : ObservableObject
    {
        public ObservableCollection<AudioEqualizerPreset> SavedPresets
        {
            get;
            set;
        } = new ObservableCollection<AudioEqualizerPreset>();

        public ObservableCollection<EqualizerConfiguration> EqualizerConfigurations
        {
            get;
            private set;
        } = new ObservableCollection<EqualizerConfiguration>();

        public EqualizerService()
        {
            GetAvailableEqualizerConfigurations();
        }

        public void SaveEqualizationPayloadChanges()
        {
            DefaultPayload.SaveConfigurationAsync2();
        }

        private void GetAvailableEqualizerConfigurations()
        {
            DefaultPayload = EqualizerConfigurationsPayload.GetDefaultConfiguration();

            ///it used to be possible to edit this configuration, so check it out
            bool fixNecessary = false;
            try
            {
                foreach (var eqConfig in DefaultPayload.EQConfigurations)
                {
                    if (eqConfig.Name == EqualizerConfiguration.DefaultConfigurationName)
                    {
                        if (!eqConfig.CompareTo(EqualizerConfiguration.GetDefault(), false))
                        {
                            ///it used to be possible to edit this configuration, so check it out
                            EqualizerConfigurations.Add(new EqualizerConfiguration(eqConfig.Bands, eqConfig.Presets, $"{EqualizerConfiguration.DefaultConfigurationName} edited"));
                            EqualizerConfigurations.Add(EqualizerConfiguration.GetDefault());
                            fixNecessary = true;
                        }
                        else
                        {
                            EqualizerConfigurations.Add(eqConfig);
                        }
                    }
                    else
                    {
                        EqualizerConfigurations.Add(eqConfig);
                    }
                }
            }
            catch { }
            if (fixNecessary)
            {
                DefaultPayload.SaveConfigurationAsync2();
            }
        }

        public EqualizerConfiguration GetCurrentEqualizerConfig()
        {
            var currentConfigurationName = SettingsWrapper.EqualizerConfiguration;
            var target = EqualizerConfigurations.FirstOrDefault(x => x.Name == currentConfigurationName);
            if (target == null)
            {
                return DefaultPayload.EQConfigurations.FirstOrDefault();
            }
            return target;
        }

        public string CurrentPresetName
        {
            get
            {
                return SettingsWrapper.SelectedEqualizerPreset;
            }
            set
            {
                SettingsWrapper.SelectedEqualizerPreset = value;
                NotifyPropertyChanged(nameof(CurrentPresetName));
            }
        }

        public bool AutomaticPresetManagement
        {
            get
            {
                return SettingsWrapper.AutomaticPresetManagement;
            }
            set
            {
                SettingsWrapper.AutomaticPresetManagement = value;
                NotifyPropertyChanged(nameof(AutomaticPresetManagement));
            }
        }

        public EqualizerConfigurationsPayload DefaultPayload { get; private set; }

    }
}
