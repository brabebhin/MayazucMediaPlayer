using MayazucMediaPlayer.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.AudioEffects
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AudioEffectsPage : BaseUserControl
    {
        public AudioEffectsViewModel EffectsModel
        {
            get;
            private set;
        }

        public AudioEffectsPage()
        {
            InitializeComponent();
            EffectsModel = new AudioEffectsViewModel(DispatcherQueue);
            DataContext = EffectsModel;
        }

        private void AudioEffectCheckedChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            EffectsModel.SaveEffectsCommand.Execute(sender);
        }
    }
}
