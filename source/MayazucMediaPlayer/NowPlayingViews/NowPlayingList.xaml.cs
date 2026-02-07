using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.NowPlayingViews

{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlayingList : BasePage
    {
        public override string Title => "Now playing";

        readonly AsyncLock _AddToPlaylistLock = new AsyncLock();

        public NowPlayingUiService DataService
        {
            get;
            private set;
        }

        public NowPlayingList()
        {
            InitializeComponent();
            DataService = AppState.Current.Services.GetService<NowPlayingUiService>();
        }

        protected override async Task OnInitializeStateAsync(object? parameter)
        {
            DataContext = DataService;
            await NowPlayingFileManagementControl.LoadStateInternal(DataService);            
        }
    }
}
