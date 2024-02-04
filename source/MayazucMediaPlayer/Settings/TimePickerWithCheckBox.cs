using Microsoft.UI.Xaml;
using System;

namespace MayazucMediaPlayer.Settings
{
    public sealed class TimePickerWithCheckBox : CheckBoxItem
    {
        readonly Action<object, string> settingPropertyChanged;

        public TimePickerWithCheckBox(string enabledPropertyName, string storedPropertyName) : base(enabledPropertyName)
        {
            TimePickerStorePropertyName = storedPropertyName;
            settingPropertyChanged = new Action<object, string>(SettingsWrapper_SettingChanged);
            SettingsWrapper.RegisterSettingChangeCallback(TimePickerStorePropertyName, settingPropertyChanged);
        }

        private void SettingsWrapper_SettingChanged(object? sender, string e)
        {
            if (e == TimePickerStorePropertyName) NotifyPropertyChanged(nameof(SelectedTime));
        }

        public string TimePickerDescription { get; set; }

        public override DataTemplate Template
        {
            get
            {
                return TemplatesDictionary.CheckBoxWithTimePicker;
            }
        }

        public string TimePickerStorePropertyName
        {
            get;
            private set;
        }

        public TimeSpan SelectedTime
        {
            get
            {
                return (TimeSpan)SettingsWrapper.GetProperty(TimePickerStorePropertyName);
            }
            set
            {
                SettingsWrapper.SetProperty(TimePickerStorePropertyName, (TimeSpan)value, this);
                NotifyPropertyChanged(nameof(SelectedTime));
            }
        }
    }
}
