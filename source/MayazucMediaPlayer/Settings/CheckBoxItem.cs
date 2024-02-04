using Microsoft.UI.Xaml;

namespace MayazucMediaPlayer.Settings
{
    public class CheckBoxItem : SettingsItem
    {
        public override DataTemplate Template
        {
            get
            {
                return TemplatesDictionary.SimpleCheckBoxWithText;
            }
        }


        public virtual bool IsChecked
        {
            get
            {
                return bool.Parse(PropertyValue.ToString());
            }

            set
            {
                PropertyValue = value;
                NotifyPropertyChanged(nameof(IsChecked));
            }
        }

        public CheckBoxItem(string settingsWrapperPropertyName) : base(settingsWrapperPropertyName)
        {
        }

        protected override void RecheckValueInternal()
        {
            NotifyPropertyChanged(nameof(IsChecked));
        }

        public string Description { get; set; } = "example description";
    }
}
