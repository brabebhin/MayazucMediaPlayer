using CommunityToolkit.WinUI;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaPlayback;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.Media.Playback;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.NowPlayingViews
{
    public sealed partial class NowPlayingCurrentMediaInfoControl : UserControl
    {
        public NowPlayingList NowPlayingInstance
        {
            get
            {
                return FullSizeNowPlayingControl;
            }
        }

        public NowPlayingCurrentMediaInfoControl()
        {
            InitializeComponent();
            CheckSize(ActualSize.X);
            SizeChanged += NowPlayingCurrentMediaInfoControl_SizeChanged;
        }

        private void NowPlayingCurrentMediaInfoControl_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
        {
            CheckSize((float)e.NewSize.Width);
        }

        private void CheckSize(float width)
        {
            RootSplitView.OpenPaneLength = width / 2;
        }

        public async Task LoadState(MediaPlaybackItem CurrentPlaybackItem)
        {
            await NowPlayingInstance.InitializeStateAsync(null);
            await CurrentMediaInfoPage.LoadStateInternal(CurrentPlaybackItem);
            if (CurrentPlaybackItem != null)
            {
                CurrentMediaInfoPage.Visibility = Visibility.Visible;
            }
        }

        internal async Task OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            await CurrentMediaInfoPage.HandleMediaOpened(sender, args);
            await DispatcherQueue.EnqueueAsync(() =>
            {
                CurrentMediaInfoPage.Visibility = Visibility.Visible;
            });
        }
    }
}
