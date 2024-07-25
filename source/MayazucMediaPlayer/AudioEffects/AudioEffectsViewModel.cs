using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.AudioEffects.ViewModels;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;

namespace MayazucMediaPlayer.AudioEffects
{
    public sealed class AudioEffectsViewModel : ServiceBase
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
                    x.DisableEffect();
                }
                await AppState.Current.MediaServiceConnector.NotifyResetFiltering(SettingsService.Instance.EqualizerEnabled);
            });
            SaveEffectsCommand = new AsyncRelayCommand(async () =>
            {
                foreach (var x in Effects)
                {
                    if (x.IsEnabled)
                    {
                        x.EnableEffect();
                    }
                    else
                    {
                        x.DisableEffect();
                    }
                }

                await AppState.Current.MediaServiceConnector.NotifyResetFiltering(SettingsService.Instance.EqualizerEnabled);

            });
        }


        private void LoadDefaultEffects()
        {
            Effects.Clear();
            Effects.Add(new AudioEffect("echoInstruments", EffectTypes.aecho)
            {
                DisplayTitle = "Instruments",
                GetSlimConfigurationString = () => { return "0.8:0.88:60:0.4"; }
            });
   
            Effects.Add(new AudioEffect("echoMountain", EffectTypes.aecho)
            {
                DisplayTitle = "Mountains",
                GetSlimConfigurationString = () => { return "0.8:0.9:1000:0.3"; }
            });

            Effects.Add(new AudioEffect("echoRobot", EffectTypes.aecho)
            {
                DisplayTitle = "Robotic",
                GetSlimConfigurationString = () => { return "0.8:0.88:6:0.4"; }
            });
        }
    }
}
