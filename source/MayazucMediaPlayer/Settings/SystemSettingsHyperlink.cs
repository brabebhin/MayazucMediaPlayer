using Microsoft.UI.Xaml;

namespace MayazucMediaPlayer.Settings
{
    public class SystemSettingsHyperlink : SettingsItem
    {
        public SystemSettingsHyperlink() : base(string.Empty)
        {
        }

        public string SystemSettingsLink
        {
            get; set;
        }

        public string Description
        {
            get;
            set;
        }

        public override DataTemplate Template => TemplatesDictionary.SystemHyperlinkButton;

        protected override void RecheckValueInternal()
        {
            //no-op
        }
    }
}
