using MayazucMediaPlayer.Controls;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Help
{
    public sealed partial class GeneralAboutControl : BaseUserControl
    {
        public GeneralAboutControl()
        {
            InitializeComponent();
            _ = licensesControl.InitializeStateAsync(null);
        }
    }
}
