using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.AudioEffects
{
    public class AudioPresetManagementViewModel : ServiceBase
    {
        public IServiceProvider ServiceProvider
        {
            get;
            private set;
        }

        public IAsyncRelayCommand AddNewPresetCommand
        {
            get; private set;
        }

        public IAsyncRelayCommand<AudioEqualizerPreset> EditAmplificationsCommand
        {
            get; private set;
        }

        public IAsyncRelayCommand<AudioEqualizerPreset> DeletePresetCommand
        {
            get; private set;
        }

        public ObservableCollection<AudioEqualizerPreset> SavedPresets
        {
            get;
            private set;
        }

        public EqualizerConfiguration TargetEqualizerConfiguration
        {
            get;
            private set;
        }

        public AudioPresetManagementViewModel(
            DispatcherQueue dp,
            EqualizerConfiguration eqConfiguration,
            IServiceProvider serviceProvider) : base(dp)
        {
            ServiceProvider = serviceProvider;
            TargetEqualizerConfiguration = eqConfiguration;
            AddNewPresetCommand = new AsyncRelayCommand(AddNewPreset_tapped);
            EditAmplificationsCommand = new AsyncRelayCommand<AudioEqualizerPreset>(EditAmplifications_click);
            DeletePresetCommand = new AsyncRelayCommand<AudioEqualizerPreset>(DeletePreset_click);
            SavedPresets = eqConfiguration.Presets;
        }

        private async Task AddNewPreset_tapped()
        {
            var preset = await AudioEffectsExtensions.CreateNewPresetAsync(false, TargetEqualizerConfiguration);
            if (preset.IsSuccess)
            {
                var playerInstance = ServiceProvider.GetService<IBackgroundPlayer>();
                await playerInstance.AddPresetToConfiguration(TargetEqualizerConfiguration, preset.Value);
            }
        }

        private async Task EditAmplifications_click(AudioEqualizerPreset? preset)
        {
            await preset.SetAmplificationsAsync(false, TargetEqualizerConfiguration);
        }

        private async Task DeletePreset_click(AudioEqualizerPreset? preset)
        {
            var playerInstance = ServiceProvider.GetService<IBackgroundPlayer>();
            await playerInstance.DeletePresetFromConfiguration(TargetEqualizerConfiguration, preset);
        }
    }
}
