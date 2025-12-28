using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

namespace MayazucMediaPlayer.Settings
{
    public partial class ToggleSwitcher : SettingsItem
    {
        public EventHandler<bool> SwitchChanged;

        public override DataTemplate Template
        {
            get
            {
                return TemplatesDictionary.ToggleSwitchWithImage;
            }
        }

        public string OnText
        {
            get; set;
        }

        public string OffText
        {
            get; set;
        }

        string switchImageGlyph;

        public string SwitchImageGlyph
        {
            get
            {
                return switchImageGlyph;
            }
            set
            {
                if (switchImageGlyph == value) return;
                switchImageGlyph = value;
                NotifyPropertyChanged(nameof(SwitchImageGlyph));
            }
        }

        public bool IsOn
        {
            get
            {
                return (bool)PropertyValue;
            }
            set
            {
                if ((bool)PropertyValue.Equals(value)) return;

                PropertyValue = value;
                NotifyPropertyChanged(nameof(IsOn));
                SwitchChanged?.Invoke(this, (bool)PropertyValue);
            }
        }

        protected override void RecheckValueInternal()
        {
            NotifyPropertyChanged(nameof(IsOn));
        }

        string toggleSwitchHeader;

        public ToggleSwitcher(string settingsWrapperPropertyName, Action<object> setValueCallback, Func<object> getValueCallback) : base(settingsWrapperPropertyName, setValueCallback, getValueCallback)
        {
        }

        public List<SettingsItem> Children { get; set; } = new List<SettingsItem>();

        public string ToggleSwitchHeader
        {
            get
            {
                return toggleSwitchHeader;
            }

            set
            {
                if (toggleSwitchHeader == value) return;
                toggleSwitchHeader = value;
                NotifyPropertyChanged(nameof(ToggleSwitchHeader));
            }
        }
    }
}
