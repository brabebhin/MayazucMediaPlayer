using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MayazucMediaPlayer.Settings
{
    public partial class ComboboxWithHeader : SettingsItem
    {
        public override DataTemplate Template
        {
            get
            {
                return TemplatesDictionary.ComboBoxWithHeader;
            }
        }

        public ObservableCollection<object> ComboboxStringList { get; set; } = new ObservableCollection<object>();

        public string ComboboxHeader { get; set; }

        string imagePath;

        public string ImagePath
        {
            get
            {
                return imagePath;
            }

            set
            {
                if (imagePath == value) return;

                imagePath = value;
                NotifyPropertyChanged(nameof(ImagePath));
            }
        }

        int selectedIndex = 0;
        public int SlectedIndex
        {
            get
            {
                selectedIndex = (int)base.PropertyValue;
                if (selectedIndex < 0)
                {
                    base.PropertyValue = DefaultValue;
                }

                return (int)base.PropertyValue;
            }
            set
            {
                if (value >= 0 && (int)PropertyValue != value)
                {
                    PropertyValue = value;
                    NotifyPropertyChanged(nameof(SlectedIndex));
                }
            }
        }

        protected override void RecheckValueInternal()
        {
            NotifyPropertyChanged(nameof(SlectedIndex));
        }

        public ComboboxWithHeader(string settingsWrapperPropertyName) : base(settingsWrapperPropertyName)
        {

        }


        public ComboboxWithHeader(string settingsWrapperPropertyName, params object[] comboboxItems) : this(settingsWrapperPropertyName)
        {
            foreach (var s in comboboxItems)
            {
                ComboboxStringList.Add(s.ToString());
            }
        }

        public ComboboxWithHeader(IEnumerable<object> comboboxItems, string settingsWrapperPropertyName) : base(settingsWrapperPropertyName)
        {
            foreach (var s in comboboxItems)
            {
                ComboboxStringList.Add(s);
            }
        }
    }
}
