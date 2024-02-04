using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MayazucMediaPlayer.Settings
{
    public class ComboboxWithHeader<T> : SettingsItem
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
                if (value >= 0)
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


        public ComboboxWithHeader(string settingsWrapperPropertyName, params T[] comboboxItems) : this(settingsWrapperPropertyName)
        {
            foreach (T s in comboboxItems)
            {
                ComboboxStringList.Add(s.ToString());
            }
        }

        public ComboboxWithHeader(IEnumerable<T> comboboxItems, string settingsWrapperPropertyName) : base(settingsWrapperPropertyName)
        {
            foreach (T s in comboboxItems)
            {
                ComboboxStringList.Add(s);
            }
        }
    }
}
