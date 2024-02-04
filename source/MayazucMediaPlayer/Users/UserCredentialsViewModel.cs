using System.Collections.ObjectModel;
using System.Linq;
using Windows.Security.Credentials;

namespace MayazucMediaPlayer.Users
{
    public class UserCredentialsViewModel
    {
        const string openSubtitleServiceName = "Open subtitles";

        public ObservableCollection<UserLoginInformation> UserCredentials
        {
            get;
            private set;
        } = new ObservableCollection<UserLoginInformation>();

        public UserCredentialsViewModel() { }

        public void LoadAll()
        {
            PasswordVault vault = new PasswordVault();
            UserCredentials.Clear();
            UserCredentials.Add(AddOpenSubtitles(vault));
        }

        private UserLoginInformation AddOpenSubtitles(PasswordVault vault)
        {
            var allCredentials = vault.RetrieveAll();
            var credential = allCredentials.FirstOrDefault(x => x.Resource == openSubtitleServiceName);
            if (credential == null)
            {
                credential = new PasswordCredential();
                credential.Resource = openSubtitleServiceName;

            }

            var returnValue = new UserLoginInformation(credential, null);
            returnValue.ServiceLogo = "ms-appx:///Assets/OpenSubtitlesLogo.png";
            return returnValue;
        }
    }
}
