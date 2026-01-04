using Microsoft.UI.Xaml;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.MediaCollections
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NetworkStreamsCollectionPage : BasePage
    {
        public override string Title => "Network streams";

        public NetworkStreamsCollectionUiService DataService { get; private set; }

        public NetworkStreamsCollectionPage()
        {
            this.InitializeComponent();
            DataService = new NetworkStreamsCollectionUiService(DispatcherQueue);
            DataContext = DataService;
        }

        protected override async Task OnInitializeStateAsync(object? parameter)
        {
            await DataService.LoadHistory();
            await base.OnInitializeStateAsync(parameter);
        }

        private async void PlayUrlHistory_click(object sender, RoutedEventArgs e)
        {
            await DataService.PlayUrl(sender.GetDataContextObject<NetworkStreamHistoryEntry>().Url);
        }
    }
}
