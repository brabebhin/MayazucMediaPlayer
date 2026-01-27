using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.AudioEffects.ViewModels;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;

namespace MayazucMediaPlayer.AudioEffects
{
    public sealed partial class AudioEffectsViewModel : ServiceBase
    {
        public AsyncRelayCommand SaveEffectsCommand
        {
            get;
            private set;
        }

        public AsyncRelayCommand ResetEffectsCommand
        {
            get;
            private set;
        }

        public ObservableCollection<AudioEffect> Effects
        {
            get;
            private set;
        } = new ObservableCollection<AudioEffect>();


        public AudioEffectsViewModel(DispatcherQueue disp) : base(disp)
        {
            LoadDefaultEffects();
            ResetEffectsCommand = new AsyncRelayCommand(async () =>
            {
                //LoadDefaultEffects();
                foreach (var x in Effects)
                {
                    x.IsEnabled = false;
                }
                await AppState.Current.MediaServiceConnector.NotifyResetFiltering(SettingsService.Instance.EqualizerEnabled);
            });
            SaveEffectsCommand = new AsyncRelayCommand(async () =>
            {
                await AppState.Current.MediaServiceConnector.NotifyResetFiltering(SettingsService.Instance.EqualizerEnabled);
            });
        }


        private void LoadDefaultEffects()
        {
            Effects.Clear();
            Effects.Add(new AudioEffect("echoInstruments", EffectTypes.aecho, () => SettingsService.Instance.EchoInstrumentsEffectEnabled, (enabled) => SettingsService.Instance.EchoInstrumentsEffectEnabled = enabled)
            {
                DisplayTitle = "Instruments",
            });

            Effects.Add(new AudioEffect("echoMountain", EffectTypes.aecho, () => SettingsService.Instance.EchoMountainsEffectEnabled, (enabled) => SettingsService.Instance.EchoMountainsEffectEnabled = enabled)
            {
                DisplayTitle = "Mountains",
            });

            Effects.Add(new AudioEffect("echoRobot", EffectTypes.aecho, () => SettingsService.Instance.EchoRoboticVoiceEffectEnabled, (enabled) => SettingsService.Instance.EchoRoboticVoiceEffectEnabled = enabled)
            {
                DisplayTitle = "Robotic",
            });
        }
    }
}
