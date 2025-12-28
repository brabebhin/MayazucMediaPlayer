using Microsoft.UI.Xaml;

namespace MayazucMediaPlayer.Settings
{
    public partial class SettingsTemplates
    {
        public static readonly SettingsTemplates Instance = new SettingsTemplates();

        public SettingsTemplates()
        {
            InitializeComponent();
        }

        public DataTemplate ComboboxWithTextBoxandButton
        {
            get
            {
                return (DataTemplate)this["ComboboxWithTextBoxandButton"];
            }
        }

        public DataTemplate SystemHyperlinkButton
        {
            get
            {
                return (DataTemplate)this["SystemHyperlinkButton"];
            }
        }

        public DataTemplate ToggleSwitchWithImage
        {
            get
            {
                return (DataTemplate)this["ToggleSwitchWithImage"];
            }
        }

        public DataTemplate SimpleCheckBoxWithText
        {
            get
            {
                return (DataTemplate)this["SimpleCheckboxTemplate"];
            }
        }

        public DataTemplate PlainTextBlock
        {
            get
            {
                return (DataTemplate)this["PlainTextBlock"];
            }
        }

        public DataTemplate ComboBoxWithHeader
        {
            get { return (DataTemplate)this["ComboBoxWithHeader"]; }
        }

        public DataTemplate TimePickerSettingsItem
        {
            get { return (DataTemplate)this["TimePickerSettingsItem"]; }
        }

        public DataTemplate ButtonWithImageAndText
        {
            get { return (DataTemplate)this["ButtonWithImage"]; }
        }


        public DataTemplate ContentSettingItem
        {
            get { return (DataTemplate)this["GenericContentControl"]; }
        }
    }
}
