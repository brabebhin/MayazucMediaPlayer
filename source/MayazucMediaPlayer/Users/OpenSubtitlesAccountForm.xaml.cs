using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Windows.Security.Credentials;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Users
{
    public sealed partial class OpenSubtitlesAccountForm : BaseUserControl, IContentSettingsItem
    {
        public const string openSubtitleServiceName = "Open subtitles";
        UserLoginInformation Model;

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

            Model = new UserLoginInformation(credential, SubtitlesAgent);
            DataContext = Model;
        }

        public void RecheckValue()
        {
        }
    }
}
