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

        public EQConfigurationManagementViewModel Model
        {
            get;
            set;
        }

        public EQConfigurationManagementPage()
        {
            InitializeComponent();

            Model = new EQConfigurationManagementViewModel(ServiceProvider.GetService<EqualizerService>(), DispatcherQueue, ServiceProvider);
            DataContext = Model;
            SizeChanged += EQConfigurationManagementPage_SizeChanged;
        }

        private void EQConfigurationManagementPage_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            masterDetailsRoot.OpenPaneLength = e.NewSize.Width / 2;
        }

        private void Model_GetSelectedItems(object? sender, List<object> e)
        {
            e.AddRange(ConfigurationsPresenter.SelectedItems);
        }

        private async void SomethingSelectedChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ConfigurationsPresenter.SelectedItem != null)
            {
                await Model.EditEqualizerConfigurationPresetsCommand.ExecuteAsync(ConfigurationsPresenter.SelectedItem as EqualizerConfiguration);
            }
        }

        private async void MakeSelectedDefault(object? sender, TappedRoutedEventArgs e)
        {
            await Model.SetDefaultPreset((sender as FrameworkElement).DataContext as EqualizerConfiguration);
        }

        public void RecheckValue()
        {

        }
    }
}
