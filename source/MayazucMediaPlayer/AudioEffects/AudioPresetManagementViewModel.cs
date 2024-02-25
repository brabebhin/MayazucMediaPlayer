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

        public IRelayCommand<object> AddNewPresetCommand
        {
            get; private set;
        }

        public IAsyncRelayCommand<object> EditAmplificationsCommand
        {
            get; private set;
        }

        public IAsyncRelayCommand<object> DeletePresetCommand
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
            AddNewPresetCommand = new AsyncRelayCommand<object>(AddNewPreset_tapped);
            EditAmplificationsCommand = new AsyncRelayCommand<object>(EditAmplifications_click);
            DeletePresetCommand = new AsyncRelayCommand<object>(DeletePreset_click);
            SavedPresets = eqConfiguration.Presets;
        }

        private async Task AddNewPreset_tapped(object? sender)
        {
            var preset = await AudioEffectsExtensions.CreateNewPresetAsync(false, TargetEqualizerConfiguration);
            if (preset.IsSuccess)
            {
                var playerInstance = ServiceProvider.GetService<IBackgroundPlayer>();
                await playerInstance.AddPresetToConfiguration(TargetEqualizerConfiguration, preset.Value);
            }
        }

        private async Task EditAmplifications_click(object? sender)
        {
            var preset = (sender as FrameworkElement).DataContext as AudioEqualizerPreset;
            await preset.SetAmplificationsAsync(false, TargetEqualizerConfiguration);
        }

        private async Task DeletePreset_click(object? sender)
        {
            var preset = (sender as FrameworkElement).DataContext as AudioEqualizerPreset;
            var playerInstance = ServiceProvider.GetService<IBackgroundPlayer>();
            await playerInstance.DeletePresetFromConfiguration(TargetEqualizerConfiguration, preset);
        }
    }
}
