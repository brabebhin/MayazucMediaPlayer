using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services;
using System.IO;
using System.Threading.Tasks;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MayazucMediaPlayer.FileSystemViews
{
    /// <summary>
    /// Pass a MediaDataStorageUIWrapper as parameter
    /// </summary>
    public sealed partial class FileDetailsPage : BaseUserControl
    {
        readonly MediaPlaybackItemUIInformation initialData;
        readonly Task fileDetailsLoadTask;

        public FileDetailsPage(MediaPlaybackItemUIInformation data,
            PlaybackSequenceService playbackModels,
            PlaylistsService playlistsService)
        {
            InitializeComponent();
            initialData = data;
            fileDetailsLoadTask = DisplayControl.LoadStateInternal(data, playbackModels, playlistsService);
        }

        public async Task LoadState()
        {
            await fileDetailsLoadTask;
        }

        private static async Task<MediaPlaybackItemUIInformation> GetNavigationParameter(MediaPlayerItemSourceUIWrapper data)
        {
            var mds = data.MediaData;
            var builder = new FFmpegInteropItemBuilder(null);
            try
            {
                using (var ffmpeg = await builder.GetFFmpegInteropMssAsync(mds, true, 0))
                {
                    return MediaPlaybackItemUIInformation.Create(ffmpeg, mds);
                }
            }
            catch
            {
                //usually when file is not available.
                return MediaPlaybackItemUIInformation.Create(null, mds);
            }
        }

        public static async Task ShowForStorageFile(FileInfo file)
        {
            var itemDetails = await FileDetailsPage.GetNavigationParameter(file);

            await ShowInternalAsync(file.Name, itemDetails);
        }

        public static async Task ShowForMediaData(MediaPlayerItemSourceUIWrapper data)
        {
            var itemDetails = await FileDetailsPage.GetNavigationParameter(data);
            await ShowInternalAsync(data.MediaData.Title, itemDetails);
        }

        private static async Task ShowInternalAsync(string title, MediaPlaybackItemUIInformation itemDetails)
        {
            var wnd = new FileDetailsPage(itemDetails, AppState.Current.Services.GetService<PlaybackSequenceService>(), AppState.Current.Services.GetService<PlaylistsService>());
            await wnd.LoadState();
            await MainWindowingService.Instance.ShowContentDialog(wnd);
        }

        private static async Task<MediaPlaybackItemUIInformation> GetNavigationParameter(FileInfo file)
        {
            return await MediaPlaybackItemUIInformation.CreateAsync(file);
        }
    }
}
