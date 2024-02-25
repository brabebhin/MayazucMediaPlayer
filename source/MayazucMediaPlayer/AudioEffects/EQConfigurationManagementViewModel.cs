using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Runtime;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MayazucMediaPlayer.AudioEffects
{
    public class EQConfigurationManagementViewModel : ServiceBase
    {
        public EqualizerService EQModels
        {
            get;
            private set;
        }

        BasePage _EqualizerPresetEditPage;
        public BasePage EqualizerPresetEditPage
        {
            get => _EqualizerPresetEditPage;
            set
            {
                var oldValue = _EqualizerPresetEditPage;
                _EqualizerPresetEditPage = value;
                NotifyPropertyChanged(nameof(EqualizerPresetEditPage));
                oldValue?.Dispose();
            }
        }

        public ObservableCollection<EqualizerConfiguration> Configurations
        {
            get
            {
                return EQModels.EqualizerConfigurations;
            }
        }


        public IRelayCommand<object> AddCommand
        {
            get; private set;
        }

        public IRelayCommand<object> DeleteButtonCommand
        {
            get;
            private set;
        }

        public IAsyncRelayCommand<object> EditEqualizerConfigurationPresetsCommand
        {
            get;
            private set;
        }

        public IServiceProvider ServiceProvider
        {
            get;
            private set;
        }

        public EQConfigurationManagementViewModel(EqualizerService eqm,
            DispatcherQueue dp,
            IServiceProvider serviceProvider) : base(dp)
        {
            EQModels = eqm;
            ServiceProvider = serviceProvider;

            AddCommand = new AsyncRelayCommand<object>(CreateNewConfiguration);
            DeleteButtonCommand = new RelayCommand<object>(DeleteSelection);
            EditEqualizerConfigurationPresetsCommand = new AsyncRelayCommand<object>(EditEqualizerConfigurationPresets);
        }

        private async Task EditEqualizerConfigurationPresets(object arg)
        {
            EqualizerConfiguration config = arg as EqualizerConfiguration;
            var editPage = await PageFactory.GetPage(typeof(AudioPresetManagement), config);
            await editPage.InitializeStateAsync(arg);
            EqualizerPresetEditPage = editPage;
        }

        private async Task CreateNewConfiguration(object? sender)
        {
            await CreateOrEditConfiguration();
        }

        public async Task CreateOrEditConfiguration()
        {
            EqualizerConfigurationWizard wizzard = null;
            EqualizerConfiguration config = null;

            wizzard = new EqualizerConfigurationWizard();
            await ContentDialogService.Instance.ShowAsync(wizzard);
            if (wizzard.Succeded)
            {
                var validation = wizzard.Validate();
                if (validation.Any())
                {
                    var strings = string.Empty;
                    foreach (var s in validation)
                    {
                        strings += s + Environment.NewLine;
                    }
                    PopupHelper.ShowInfoMessage(strings, "Invalid input");
                }
                else
                {
                    var configName = wizzard.ConfigName;
                    var bands = wizzard.GetFrequencyDefinitions();
                    config = new EqualizerConfiguration(bands, null, configName);
                }
            }
            if (config != null)
            {
                var player = ServiceProvider.GetService<IBackgroundPlayer>();
                await player.AddEqualizerConfiguration(config);
            }
        }

        private async void DeleteSelection(object args)
        {
            var selectedConfiguration = args as EqualizerConfiguration;
            var player = ServiceProvider.GetService<IBackgroundPlayer>();

            var deletionResult = await player.DeleteEqualizerConfiguration(selectedConfiguration);
            if (deletionResult != EqualizerConfigurationDeletionResult.Success)
            {
                var message = "Cannot delete the currently active configuration.Set another configuration to active, then try again";
                if (deletionResult != EqualizerConfigurationDeletionResult.FailedInUse)
                    message = "Could not delete configuration";
                MessageDialog diag = new MessageDialog(message);
                await diag.ShowAsync();
            }
        }

        private async Task RestoreDefault(object? sender)
        {
            await SetDefaultPreset(EqualizerConfiguration.GetDefault());
        }

        public async Task SetDefaultPreset(EqualizerConfiguration selected)
        {
            await MessageDialogService.Instance.ShowMessageDialogAsync("Are you sure you want to change your default equalizer setting?", "Are you sure?",
            new UICommand("Yes", async (s) =>
            {
                selected.SetDefault();
                EQModels.SavedPresets.Clear();

                foreach (var v in selected.Presets)
                {
                    EQModels.SavedPresets.Add(v);
                }

                await ContentDialogService.Instance.ShowAsync(new RestartApplicationDialog("Equalizer reset requires restart"));
            }),
            new UICommand("No"));
        }
    }
}
