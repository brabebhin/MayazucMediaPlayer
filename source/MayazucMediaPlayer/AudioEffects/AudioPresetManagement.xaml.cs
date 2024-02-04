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
        public AudioPresetManagementViewModel Model
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
            Model = new AudioPresetManagementViewModel(DispatcherQueue, eqConfiguration, ServiceProvider);
            DataContext = Model;
            return base.OnInitializeStateAsync(parameter);
        }

        protected override void FreeMemory()
        {
            base.FreeMemory();
        }

        private async void EditAmplifications_click(object? sender, RoutedEventArgs e)
        {
            await Model.EditAmplificationsCommand.ExecuteAsync(sender);
        }

        private async void DeletePreset_click(object? sender, RoutedEventArgs e)
        {
            await Model.DeletePresetCommand.ExecuteAsync(sender);
        }

        public void RecheckValue()
        {

        }
    }
}
