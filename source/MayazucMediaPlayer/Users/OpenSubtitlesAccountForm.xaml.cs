using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;
using Microsoft.UI.Xaml;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Users
{
    public sealed partial class OpenSubtitlesAccountForm : BaseUserControl, IContentSettingsItem
    {
        public const string openSubtitleServiceName = "Open subtitles";
        UserLoginInformation LoginDataModel;

        public IOpenSubtitlesAgent SubtitlesAgent
        {
            get;
            set;
        }

        public OpenSubtitlesAccountForm()
        {
            InitializeComponent();
            Loaded += OpenSubtitlesAccountForm_Loaded;
            Unloaded += OpenSubtitlesAccountForm_Unloaded;
            pbApiKey.Text = SettingsService.Instance.OpenSubtitlesApiKey;
        }

        private void OpenSubtitlesAccountForm_Unloaded(object? sender, RoutedEventArgs e)
        {

        }

        private void OpenSubtitlesAccountForm_Loaded(object? sender, RoutedEventArgs e)
        {
            LoadStoredCredentials();
        }


        private void LoadStoredCredentials()
        {
            PasswordVault vault = new PasswordVault();
            var allCredentials = vault.RetrieveAll();
            var credential = allCredentials.FirstOrDefault(x => x.Resource == openSubtitleServiceName);
            if (credential == null)
            {
                credential = new PasswordCredential();
                credential.Resource = openSubtitleServiceName;
            }

            LoginDataModel = new UserLoginInformation(credential, SubtitlesAgent);
            DataContext = LoginDataModel;
        }

        public void RecheckValue()
        {
        }

        private async void SaveCredentials_click(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            SettingsService.Instance.OpenSubtitlesApiKey = pbApiKey.Text;
            LoginDataModel.SaveCommand.Execute(sender);
        }
    }
}
