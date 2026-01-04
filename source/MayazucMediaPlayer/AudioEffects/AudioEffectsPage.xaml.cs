using MayazucMediaPlayer.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.AudioEffects
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AudioEffectsPage : BaseUserControl
    {
        public AudioEffectsViewModel DataService
        {
            get;
            private set;
        }

        public AudioEffectsPage()
        {
            InitializeComponent();
            DataService = new AudioEffectsViewModel(DispatcherQueue);
            DataContext = DataService;
        }

        private void AudioEffectCheckedChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            DataService.SaveEffectsCommand.Execute(sender);
        }

        private void DataServiceResetEffectsCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DataService.ResetEffectsCommand.Execute(sender);
        }

        private async void SaveEffectsCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.SaveEffectsCommand.ExecuteAsync(sender);
        }
    }
}
