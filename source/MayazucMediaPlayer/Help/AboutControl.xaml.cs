using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Settings;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Help
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutControl : BaseUserControl, IContentSettingsItem
    {
        public AboutControl()
        {
            InitializeComponent();

            DataContext = this;
        }

        public void RecheckValue()
        {
        }
    }
}
