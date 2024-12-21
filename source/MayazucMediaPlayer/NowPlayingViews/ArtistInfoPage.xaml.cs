using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaPlayback;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System.Threading.Tasks;
using Windows.Media.Playback;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.NowPlayingViews
{
    public sealed partial class ArtistInfoPage : BaseUserControl
    {
        readonly AsyncLock expanderMediaOpenedLock = new AsyncLock();

        public ArtistInfoPage()
        {
            InitializeComponent();
        }

        public async Task HandleMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            if (args.Reason != MediaOpenedEventReason.MediaPlayerObjectRequested)
            {
                await AdvancedFileDetailsControl.HandleMediaOpened(args.Data.PlaybackItem);
            }
        }

        public async Task LoadStateInternal(MediaPlaybackItem CurrentPlaybackItem)
        {
            //SetVisibilities((await Model.Models.CurrentMediaMetadata())?.MediaData);

            await AdvancedFileDetailsControl.HandleMediaOpened(CurrentPlaybackItem);
        }

        public void PerformCleanUp()
        {
            DataContext = null;
        }
    }
}
