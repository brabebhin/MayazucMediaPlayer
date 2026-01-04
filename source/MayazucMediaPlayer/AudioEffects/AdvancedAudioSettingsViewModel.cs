using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.AudioEffects
{
    public sealed partial class AdvancedAudioSettingsViewModel : ServiceBase
    {
        public bool IsRunningOnDesktop
        {
            get
            {
                return true;// APIContractUtilities.IsRunningOnDesktop;
            }
        }


        public ObservableCollection<AudioDeviceWrapper> AudioDevices
        {
            get;
            private set;
        } = new ObservableCollection<AudioDeviceWrapper>();


        public double AudioBalanceValue
        {
            get
            {
                return SettingsService.Instance.AudioBalance;
            }
            set
            {
                if (SettingsService.Instance.AudioBalance == value) return;

                SettingsService.Instance.AudioBalance = value;
                NotifyPropertyChanged(nameof(AudioBalanceValue));
                AppState.Current.MediaServiceConnector.NotifyAudioBalanceChanged(value);
            }
        }

        public IRelayCommand SetBalanceToZeroCommand
        {
            get;
            private set;
        }

        public AdvancedAudioSettingsViewModel(DispatcherQueue disp) : base(disp)
        {
            SetBalanceToZeroCommand = new RelayCommand(() => { AudioBalanceValue = 0; });

        }

        public async Task LoadAudioDevicesAsync()
        {
            AudioDevices.Add(new AudioDeviceWrapper(null));

            await Task.CompletedTask;
        }
    }
}
