using CommunityToolkit.WinUI;
using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.System.Threading;

namespace MayazucMediaPlayer.AudioEffects
{
    public sealed partial class AudioEnhancementsPage : BaseUserControl
    {
        const double equalizerResetDelay = 0.1;

        ThreadPoolTimer timer = null;
        readonly object objectLock = new object();

        bool changePreset = true;
        bool ObserveUserActionEvent = true;

        public AudioEnhancementsPageViewModel DataService { get; private set; }

        public AudioEnhancementsPage()
        {
            InitializeComponent();
        }
      
        private void CbSavedPresets_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (cbSavedPresets.SelectedIndex != -1)
            {
                AudioEqualizerPreset selectedPreset = cbSavedPresets.SelectedItem as AudioEqualizerPreset;
                SettingsService.Instance.SelectedEqualizerPreset = (cbSavedPresets.SelectedItem as AudioEqualizerPreset).PresetName;
                SetSelectedPreset(selectedPreset, false);
                changePreset = false;
            }
            else
            {
                SettingsService.Instance.SelectedEqualizerPreset = "custom";
            }
        }

        public async Task HandleMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            await SetSelectedPreset();
        }

        private async void handler(ThreadPoolTimer timer)
        {
            await AppState.Current.MediaServiceConnector.NotifyResetFiltering(SettingsService.Instance.EqualizerEnabled);
            if (!changePreset)
            {
                changePreset = true;
            }
            else
            {
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    cbSavedPresets.SelectedIndex = DataService.FindMatchingPresentIndex();
                });
            }
        }

        public async void LoadStateInternal(AudioEnhancementsPageViewModel m)
        {
            DataService = m;

            DataContext = DataService;

            KillSwitch.IsOn = SettingsService.Instance.EqualizerEnabled;
            KillSwitch.Toggled += KillSwitchToggle;

            DataService.LoadState();

            changePreset = false;
            await SetSelectedPreset();
            cbSavedPresets.SelectionChanged += CbSavedPresets_SelectionChanged;

            //AudioSliderHeight = lsvBands.ActualHeight;
        }

        private async Task SetSelectedPreset()
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                var presetName = SettingsService.Instance.SelectedEqualizerPreset;
                var saved = DataService.AvailablePresets.FirstOrDefault
                    (x => x.PresetName == presetName);
                if (saved != null)
                {
                    cbSavedPresets.SelectedItem = saved;
                    SetSelectedPreset(saved, true);
                }
            });
        }

        public void PerformCleanUp()
        {
            cbSavedPresets.SelectionChanged -= CbSavedPresets_SelectionChanged;
            KillSwitch.Toggled -= KillSwitchToggle;
            timer?.Cancel();
        }

        private void FrequencyChanged(object? sender, PropertyChangedEventArgs e)
        {
            lock (objectLock)
            {
                if (e.PropertyName == "Frequency")
                {

                    if (timer != null)
                    {
                        timer.Cancel();
                    }

                    if (ObserveUserActionEvent)
                    {
                        cbSavedPresets.SelectedIndex = -1;
                        SettingsService.Instance.SelectedEqualizerPreset = "custom";
                    }

                    timer = ThreadPoolTimer.CreateTimer(handler, TimeSpan.FromSeconds(equalizerResetDelay));
                }
            }
        }

        private async void KillSwitchToggle(object? sender, RoutedEventArgs e)
        {
            SettingsService.Instance.EqualizerEnabled = KillSwitch.IsOn;
            DataService.EnableContexts(KillSwitch.IsOn);
            if (!KillSwitch.IsOn)
            {
                DataService.ResetEqualizerSliders();
            }
            await AppState.Current.MediaServiceConnector.NotifyResetFiltering(KillSwitch.IsOn);

        }

        private void SetSelectedPreset(AudioEqualizerPreset selectedPreset, bool preventReset)
        {
            if (preventReset)
            {
                ObserveUserActionEvent = false;
            }

            for (int i = 0; i < selectedPreset.FrequencyValues.Count; i++)
            {
                DataService.FrequencyBands[i].FrequencyAmplification = selectedPreset.FrequencyValues[i];
            }

            if (preventReset)
            {
                ObserveUserActionEvent = true;
            }
        }

        private void PrepNotifyEqualizerReset(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (ObserveUserActionEvent)
            {
                timer?.Cancel();
                timer = ThreadPoolTimer.CreateTimer(handler, TimeSpan.FromSeconds(equalizerResetDelay));
            }
        }

        private void DataServiceSavePresetButtonCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DataService.SavePresetButtonCommand.Execute(sender);
        }

        private void DataServiceResetButtonCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DataService.ResetButtonCommand.Execute(sender);
        }
    }
}
