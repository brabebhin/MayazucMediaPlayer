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
            SettingsService.Instance.RegisterSettingChangeCallback(TimePickerStorePropertyName, settingPropertyChanged);
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
                return (TimeSpan)SettingsService.Instance.GetProperty(TimePickerStorePropertyName);
            }
            set
            {
                if ((TimeSpan)SettingsService.Instance.GetProperty(TimePickerStorePropertyName) == (TimeSpan)value) return;
                SettingsService.Instance.SetProperty(TimePickerStorePropertyName, (TimeSpan)value, this);
                NotifyPropertyChanged(nameof(SelectedTime));
            }
        }
    }
}
