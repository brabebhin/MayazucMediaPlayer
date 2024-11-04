using MayazucMediaPlayer.Services;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MayazucMediaPlayer.FileSystemViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileFolderPickerPage : BasePage
    {
        public const string PlayCommandString = "Play";
        public const string AddToNowPlayingString = "AddToNowPlaying";
        public const string AddToExistingPlaylist = "AddToPlaylist";
        public const string PlayNextCommandString = "PlayNext";

        public override string Title => "Play files + folders";
        public FileManagementService DataModel
        {
            get; private set;
        }

        public FileFolderPickerPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void FreeMemory()
        {
            DataModel.NavigationRequest -= DataModel_NavigationRequest;
            DataModel = null;
            base.FreeMemory();
        }

        private void DataModel_NavigationRequest(object? sender, Navigation.NavigationRequestEventArgs e)
        {
            Frame.NavigateAsync(e.PageType, e.Parameter);
        }
        protected override async Task OnInitializeStateAsync(object? parameter)
        {
            if (DataModel == null)
            {
                DataModel = new FileManagementService(DispatcherQueue,
                    base.ApplicationDataModels.PlaybackModel,
                    ServiceProvider.GetService<PlaylistsService>());
                await fileManagementControl.LoadStateInternal(DataModel);
                DataContext = DataModel;

                DataModel.NavigationRequest += DataModel_NavigationRequest;
            }
        }
    }
}
