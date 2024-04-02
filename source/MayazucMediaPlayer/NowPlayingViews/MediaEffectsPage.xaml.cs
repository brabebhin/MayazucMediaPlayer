using MayazucMediaPlayer.AudioEffects;
using MayazucMediaPlayer.MediaPlayback;
using MayazucNativeFramework;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.Media.Playback;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.NowPlayingViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MediaEffectsPage : BasePage
    {
        private readonly AudioEnhancementsPageViewModel AudioModel;
        private readonly VideoEffectProcessorConfiguration VideoEffectConfig;

        public override string Title => "Media Effects";

        public MediaEffectsPage()
        {
            InitializeComponent();
            AudioModel = ServiceProvider.GetService<AudioEnhancementsPageViewModel>();
            VideoEffectConfig = AppState.Current.MediaServiceConnector.VideoEffectsConfiguration;
            lsvPaneView.SelectedIndex = 0;
        }

        protected async override Task OnInitializeStateAsync(object? parameter)
        {
            AudioEffectsUI.LoadStateInternal(AudioModel);
            await VidioEffectsUI.LoadState(VideoEffectConfig);
        }

        protected override void FreeMemory()
        {
            AudioEffectsUI.PerformCleanUp();
        }

        protected override async Task OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            await AudioEffectsUI.HandleMediaOpened(sender, args);
        }

        private void ShowAudioVideoEffects(object sender, SelectionChangedEventArgs e)
        {
            if (lsvPaneView.SelectedIndex == 0)
            {
                AudioEffectsUI.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                VidioEffectsUI.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                AudioEffectsUI.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                VidioEffectsUI.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
        }
    }
}
