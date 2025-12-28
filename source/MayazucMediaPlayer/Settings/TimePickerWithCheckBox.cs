using Microsoft.UI.Xaml;
using System;

namespace MayazucMediaPlayer.Settings
{
    public sealed partial class TimePickerSettingsItem : SettingsItem
    {
        public TimePickerSettingsItem(string propertyName, Action<object> setValueCallback, Func<object> getValueCallback) : base(propertyName, setValueCallback, getValueCallback)
        {
        }

        public string TimePickerDescription
        {
            get;
            set;
        }

        protected override void RecheckValueInternal()
        {

        }

        public override object DefaultValue { get => TimeSpan.Zero; set => base.DefaultValue = value; }


        public override DataTemplate Template
        {
            get
            {
                return TemplatesDictionary.TimePickerSettingsItem;
            }
        }

        public TimeSpan Value
        {
            get
            {
                return (TimeSpan)base.PropertyValue;
            }
            set
            {
                base.PropertyValue = value;
            }
        }
    }
}
