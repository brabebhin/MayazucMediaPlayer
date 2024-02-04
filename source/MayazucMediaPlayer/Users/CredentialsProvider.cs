using System.Linq;
using Windows.Security.Credentials;

namespace MayazucMediaPlayer.Users
{
    public static class CredentialsProvider
    {
        public static PasswordCredential GetOpenSubtitles()
        {
            try
            {
                PasswordVault vault = new PasswordVault();
                var credentials = vault.RetrieveAll().Where(x => x.Resource == OpenSubtitlesAccountForm.openSubtitleServiceName);
                if (credentials.Any())
                {
                    var credential = credentials.FirstOrDefault();
                    credential.RetrievePassword();
                    return credential;
                }
            }
            catch { }
            return null;
        }
    }
}
