using Microsoft.UI.Xaml;

namespace MayazucMediaPlayer.Settings
{
    public partial class ContentSettingsItem : SettingsItem
    {
        public IContentSettingsItem Content
        {
            get;
            set;
        }

        public override DataTemplate Template
        {
            get
            {
                return base.TemplatesDictionary.ContentSettingItem;
            }
        }

        public ContentSettingsItem(IContentSettingsItem content) : base(string.Empty)
        {
            Content = content;
        }

        protected override void RecheckValueInternal()
        {
            Content?.RecheckValue();
        }
    }
}
