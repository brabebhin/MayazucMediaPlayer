using MayazucMediaPlayer.Common;
using Microsoft.UI.Xaml;

namespace MayazucMediaPlayer.Settings
{
    public partial class SystemSettingsHyperlink : SettingsItem
    {
        public SystemSettingsHyperlink() : base(string.Empty, EmptyCallbacks.Action, EmptyCallbacks.Func)
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
