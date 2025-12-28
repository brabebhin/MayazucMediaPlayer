using Microsoft.UI.Xaml;
using System;

namespace MayazucMediaPlayer.Settings
{
    public partial class CheckBoxItem : SettingsItem
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
                if (bool.Parse(PropertyValue.ToString()) == value) return;

                PropertyValue = value;
                NotifyPropertyChanged(nameof(IsChecked));
            }
        }

        public CheckBoxItem(string settingsWrapperPropertyName, Action<object> setValueCallback, Func<object> getValueCallback) : base(settingsWrapperPropertyName, setValueCallback, getValueCallback)
        {
        }

        protected override void RecheckValueInternal()
        {
            NotifyPropertyChanged(nameof(IsChecked));
        }

        public string Description { get; set; } = "example description";
    }
}
