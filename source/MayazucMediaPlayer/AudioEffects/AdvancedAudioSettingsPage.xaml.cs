using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Settings;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.AudioEffects
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdvancedAudioSettingsPage : BaseUserControl, IContentSettingsItem
    {
        public AdvancedAudioSettingsViewModel DataService
        {
            get;
            private set;
        }

        public AdvancedAudioSettingsPage()
        {
            InitializeComponent();
            DataService = new AdvancedAudioSettingsViewModel(DispatcherQueue);
            DataContext = DataService;
            DispatcherQueue.TryEnqueue(async () =>
            {
                await DataService.LoadAudioDevicesAsync();
            });
        }

        public void RecheckValue()
        {
        }

        private void DataServiceSetBalanceToZeroCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DataService.SetBalanceToZeroCommand.Execute(null);
        }
    }
}
