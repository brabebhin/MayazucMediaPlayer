using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.AudioEffects
{
    public class AudioEnhancementsPageViewModel : ServiceBase
    {
        public EqualizerService EQModels
        {
            get;
            private set;
        }

        public bool AutomaticPresetManagement
        {
            get
            {
                return EQModels.AutomaticPresetManagement;
            }
            set
            {
                EQModels.AutomaticPresetManagement = value;
                NotifyPropertyChanged(nameof(AutomaticPresetManagement));
            }
        }

        public AudioEqualizerPresetCollection AvailablePresets
        {
            get;
            private set;
        }

        public ObservableCollection<FrequencyBandContext> FrequencyBands
        {
            get; private set;
        } = new ObservableCollection<FrequencyBandContext>();

        public CommandBase ResetButtonCommand
        {
            get; private set;
        }

        public CommandBase SavePresetButtonCommand
        {
            get; private set;
        }

        public CommandBase ManageEqConfigurationsButtonCommand
        {
            get; private set;
        }

        public IBackgroundPlayer PlayerService
        {
            get;
            private set;
        }

        public AudioEnhancementsPageViewModel(EqualizerService eqm,
            DispatcherQueue dp,
            IBackgroundPlayer playerService) : base(dp)
        {
            EQModels = eqm;
            ResetButtonCommand = new RelayCommand(ResetEffects);
            SavePresetButtonCommand = new AsyncRelayCommand(SavePreset);
            ManageEqConfigurationsButtonCommand = new RelayCommand(manageEqualizerConfigurations);
            PlayerService = playerService;
        }

        public void LoadState()
        {
            LoadFrequencyBands();

            AvailablePresets = EQModels.GetCurrentEqualizerConfig().Presets;
            NotifyPropertyChanged(nameof(AvailablePresets));
            var on = SettingsWrapper.EqualizerEnabled;
            EnableContexts(on);
        }

        public void EnableContexts(bool enabled)
        {
            foreach (var c in FrequencyBands)
            {
                c.IsEnabled = enabled;
            }
        }

        private void LoadFrequencyBands()
        {
            FrequencyBands.Clear();
            var currentConfig = EQModels.GetCurrentEqualizerConfig();
            foreach (var c in currentConfig.Bands)
                FrequencyBands.Add(new FrequencyBandContext(c));
        }

        private void ResetEffects(object? sender)
        {
            ResetEqualizerSliders();
            ResetAudioEffects();
        }

        private async Task SavePreset(object? sender)
        {
            var currentConfig = await PlayerService.GetCurrentEqualizerConfiguration();
            var preset = await AudioEffectsExtensions.CreateNewPresetAsync(true, currentConfig);
            if (preset.IsSuccess)
            {
                await PlayerService.AddPresetToConfiguration(currentConfig, preset.Value);
            }
        }

        private void manageEqualizerConfigurations(object? sender)
        {
            SubmitNavigationEvent(typeof(EQConfigurationManagementPage), EQModels);
        }

        public void ResetEqualizerSliders()
        {
            foreach (var slider in FrequencyBands)
            {
                slider.FrequencyAmplification = 0;
            }
        }

        public void ResetAudioEffects()
        {

        }

        internal int FindMatchingPresentIndex()
        {
            for (int i = 0; i < AvailablePresets.Count; i++)
            {
                if (MatchesCurrentBands(AvailablePresets[i])) return i;
            }

            return -1;
        }

        private bool MatchesCurrentBands(AudioEqualizerPreset x)
        {
            if (x.FrequencyValues.Count != FrequencyBands.Count) return false;
            for (int i = 0; i < FrequencyBands.Count; i++)
            {
                if (FrequencyBands[i].FrequencyAmplification != x.FrequencyValues[i])
                    return false;
            }

            return true;
        }
    }
}
