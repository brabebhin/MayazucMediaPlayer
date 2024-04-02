using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaPlayback;
using System.Threading.Tasks;
using Windows.Media.Playback;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.NowPlayingViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlayingManagementPage : BasePage
    {
        public override string Title => "Queue";

        public NowPlayingManagementPage()
        {
            InitializeComponent();
        }

        protected override async Task OnInitializeStateAsync(object? parameter)
        {
            await NowPlayingInstance.LoadState(CurrentPlaybackItem);
        }

        protected override Task OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            return NowPlayingInstance.OnMediaOpened(sender, args);
        }
    }
}
