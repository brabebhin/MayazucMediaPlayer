using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.AudioEffects
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AudioPresetManagement : BasePage, IContentSettingsItem
    {
        public override string Title => "Equalizer preset management";
        public AudioPresetManagementViewModel DataService
        {
            get;
            private set;
        }

        public AudioPresetManagement()
        {
            InitializeComponent();
        }

        protected override Task OnInitializeStateAsync(object? parameter)
        {
            EqualizerConfiguration eqConfiguration = (EqualizerConfiguration)parameter;
            DataService = new AudioPresetManagementViewModel(DispatcherQueue, eqConfiguration, ServiceProvider);
            DataContext = DataService;
            return base.OnInitializeStateAsync(parameter);
        }

        protected override void FreeMemory()
        {
            base.FreeMemory();
        }

        private async void EditAmplifications_click(object? sender, RoutedEventArgs e)
        {
            await DataService.EditAmplificationsCommand.ExecuteAsync(sender.GetDataContextObject<AudioEqualizerPreset>());
        }

        private async void DeletePreset_click(object? sender, RoutedEventArgs e)
        {
            await DataService.DeletePresetCommand.ExecuteAsync(sender.GetDataContextObject<AudioEqualizerPreset>());
        }

        public void RecheckValue()
        {

        }

        private void DataServiceAddNewPresetCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DataService.AddNewPresetCommand.Execute(sender);
        }
    }
}
