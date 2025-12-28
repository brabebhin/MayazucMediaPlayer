using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;

namespace MayazucMediaPlayer.AudioEffects
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EQConfigurationManagementPage : BasePage, IContentSettingsItem
    {
        public override string Title => "Equalizer configurations";

        public EQConfigurationService DataService
        {
            get;
            private set;
        }

        public EQConfigurationManagementPage()
        {
            InitializeComponent();

            DataService = new EQConfigurationService(ServiceProvider.GetService<EqualizerService>(), DispatcherQueue, ServiceProvider);
            DataContext = DataService;
            SizeChanged += EQConfigurationManagementPage_SizeChanged;
        }

        private void EQConfigurationManagementPage_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            masterDetailsRoot.OpenPaneLength = e.NewSize.Width / 2;
        }

        private async void SomethingSelectedChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ConfigurationsPresenter.SelectedItem != null)
            {
                await DataService.EditEqualizerConfigurationPresetsCommand.ExecuteAsync((EqualizerConfiguration)ConfigurationsPresenter.SelectedItem);
            }
        }

        private async void MakeSelectedDefault(object? sender, TappedRoutedEventArgs e)
        {
            await DataService.SetDefaultPreset((sender.GetDataContextObject<EqualizerConfiguration>()));
        }

        public void RecheckValue()
        {
            // no-op
        }

        private async void DataServiceAddCommand(object sender, TappedRoutedEventArgs e)
        {
           await DataService.AddCommand.ExecuteAsync(sender.GetDataContextObject<EQConfigurationService>());
        }

        private async void EditEqualizerConfigurationPresetsCommand(object sender, TappedRoutedEventArgs e)
        {
            await DataService.EditEqualizerConfigurationPresetsCommand.ExecuteAsync(sender.GetDataContextObject<EqualizerConfiguration>());
        }

        private void DeleteButtonCommand(object sender, TappedRoutedEventArgs e)
        {
            DataService.DeleteButtonCommand.ExecuteAsync(sender.GetDataContextObject<EqualizerConfiguration>());
        }
    }
}
