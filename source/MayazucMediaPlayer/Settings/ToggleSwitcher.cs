using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

namespace MayazucMediaPlayer.Settings
{
    public class ToggleSwitcher : SettingsItem
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

        public ToggleSwitcher(string settingsWrapperPropertyName) : base(settingsWrapperPropertyName)
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
                toggleSwitchHeader = value;
                NotifyPropertyChanged(nameof(ToggleSwitchHeader));
            }
        }
    }
}
