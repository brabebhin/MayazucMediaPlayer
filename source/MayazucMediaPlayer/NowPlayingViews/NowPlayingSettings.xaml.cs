using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.NowPlayingViews
{
    public sealed partial class NowPlayingSettings : UserControl
    {
        public NowPlayingSettings()
        {
            InitializeComponent();
            var items = new List<SettingsItemGroup>();
            var templates = new SettingsTemplates();
            items.Add(ContextualSettings.GetSubtitleSettings());
            items.Add(ContextualSettings.GetAutoPlaySettings());
            items.Add(ContextualSettings.GetPlaybackControlSettings());
            var result = from act in items group act by act into grp select grp;
            cvsSettingsItems.Source = result;
            cvsSettingsItems.IsSourceGrouped = true;
            DataContext = this;
        }
    }
}
