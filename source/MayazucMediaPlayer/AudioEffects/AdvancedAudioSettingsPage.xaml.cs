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
        public AdvancedAudioSettingsViewModel Model
        {
            get;
            private set;
        }

        public AdvancedAudioSettingsPage()
        {
            InitializeComponent();
            Model = new AdvancedAudioSettingsViewModel(DispatcherQueue);
            DataContext = Model;
            DispatcherQueue.TryEnqueue(async () =>
            {
                await Model.LoadAudioDevicesAsync();
            });
        }

        public void RecheckValue()
        {
        }
    }
}
