using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Common;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Linq;

namespace MayazucMediaPlayer.Settings
{
    /// <summary>
    /// only used for metadata options
    /// </summary>
    public sealed partial class ComboBoxWithTextBlockAndButton : SettingsButton
    {
        public override DataTemplate Template
        {
            get
            {
                return TemplatesDictionary.ComboboxWithTextBoxandButton;
            }
        }

        public ObservableCollection<string> ComboboxStringList { get; set; } = new ObservableCollection<string>();

        public string ImageComboBoxPath
        {
            get
            {
                return imageComboBoxPath;
            }

            set
            {
                if (imageComboBoxPath == value) return;

                imageComboBoxPath = value;
                NotifyPropertyChanged(nameof(ImageComboBoxPath));
            }
        }

        string imageComboBoxPath;

        public string ComboboxHeader { get; set; }


        public string folderCoverNames
        {
            get; private set;
        }

        public string FolderCoverNames
        {
            get
            {
                return SettingsService.Instance.AlbumArtFolderCoverName;
            }
            set
            {
                SettingsService.Instance.AlbumArtFolderCoverName = value;
            }
        }

        bool textBoxEnabled;

        public bool TextBoxEnabled
        {
            get
            {
                return (int)PropertyValue == 2;
            }
            set
            {
                if (textBoxEnabled == value) return;

                textBoxEnabled = value;
                NotifyPropertyChanged(nameof(TextBoxEnabled));
            }
        }



        string imageTextBoxPath;

        public string ImageTextBoxPath
        {
            get
            {
                return imageTextBoxPath;
            }
            set
            {
                if (imageTextBoxPath == value) return;

                imageTextBoxPath = value;
                NotifyPropertyChanged(nameof(ImageTextBoxPath));
            }
        }


        string textBoxHeader;

        public string TextBoxHeader
        {
            get
            {
                return textBoxHeader;
            }
            set
            {
                if (textBoxHeader == value) return;

                textBoxHeader = value;
                NotifyPropertyChanged(nameof(TextBoxHeader));
            }
        }

        public int SelectedCBIndex
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
                    TextBoxEnabled = value == 2;
                    NotifyPropertyChanged(nameof(SelectedCBIndex));
                }
            }
        }

        public ComboBoxWithTextBlockAndButton(string settingsWrapperPropertyName, params string[] comboBoxItems) : base(settingsWrapperPropertyName)
        {
            foreach (string s in comboBoxItems)
            {
                ComboboxStringList.Add(s);
            }
        }

        private int selectedIndex;

        public override RelayCommand<object> Command
        {
            get; set;
        } = new RelayCommand<object>(SaveCoverClick);


        private static void SaveCoverClick(object? sender)
        {
            var s = sender as ComboBoxWithTextBlockAndButton;
            var splits = s.folderCoverNames.Split(';');
            var ilegalchar = System.IO.Path.GetInvalidFileNameChars();

            foreach (char c in ilegalchar)
            {
                if (splits.Any(x => x.Contains(c)))
                {
                    PopupHelper.ShowInfoMessage("File names contain invalid characters.", "Error");
                    return;
                }
            }

            SettingsService.Instance.AlbumArtFolderCoverName = s.folderCoverNames;
            PopupHelper.ShowInfoMessage("Settings saved successfully.", "^_^");

        }

    }
}
