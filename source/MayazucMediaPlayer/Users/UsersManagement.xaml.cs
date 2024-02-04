using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Users
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UsersManagement : BaseUserControl, IContentSettingsItem
    {
        public UsersManagement()
        {
            InitializeComponent();
            osdbForm.SubtitlesAgent = ServiceProvider.GetService<IOpenSubtitlesAgent>();
        }

        public void RecheckValue()
        {
        }
    }
}
