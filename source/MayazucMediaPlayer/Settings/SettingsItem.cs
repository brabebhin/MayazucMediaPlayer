using MayazucMediaPlayer.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MayazucMediaPlayer.Settings
{
    public abstract class SettingsItem : ObservableObject, IDisposable
    {
        protected SettingsTemplates TemplatesDictionary
        {
            get
            {
                return SettingsTemplates.Instance;
            }
        }

        private object propertyValue;

        /// <summary>
        /// The value of the settings
        /// </summary>
        public virtual object PropertyValue
        {
            get
            {
                return GetValueCallback();
            }
            set
            {
                if (propertyValue != null && propertyValue.Equals(value)) return;

                propertyValue = value;
                NotifyPropertyChanged(nameof(PropertyValue));
                SetValue();
            }
        }

        public virtual object DefaultValue
        {
            get;
            set;
        }

        /// <summary>
        /// The datatemplate to use with this object
        /// </summary>
        public virtual DataTemplate Template
        {
            get { return new DataTemplate(); }
        }


        public Action<object, SettingsItem> SettingsChangedCallback { get; set; }

        public Visibility ElementVisible
        {
            get
            {
                return elementVisible;
            }
            set
            {
                if (elementVisible == value) return;

                elementVisible = value;
                NotifyPropertyChanged(nameof(ElementVisible));
            }
        }


        string _settingsWrapperPropertyName;
        public string SettingsWrapperPropertyName
        {
            get
            {
                return _settingsWrapperPropertyName;
            }
            private set
            {
                if (_settingsWrapperPropertyName == value) return;
                _settingsWrapperPropertyName = value;
                NotifyPropertyChanged(nameof(SettingsChangedCallback));
                callback = SettingsService.Instance.RegisterSettingChangeCallback(_settingsWrapperPropertyName, (s, e) =>
                {
                    RecheckValue();
                });
            }
        }

        public Action<object> SetValueCallback
        {
            get;
            private set;
        }

        public Func<object> GetValueCallback
        {
            get;
            private set;
        }

        private void SetValue()
        {
            SetValueCallback(propertyValue);
        }

        Visibility elementVisible = Visibility.Visible;

        bool isEnabled = true;
        private Action<object, string> callback;
        private bool disposedValue;

        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (isEnabled == value) return;
                isEnabled = value;
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }

        public SettingsItem(string settingsWrapperPropertyName, Action<object> setValueCallback, Func<object> getValueCallback)
        {
            SettingsWrapperPropertyName = settingsWrapperPropertyName;
            SetValueCallback = setValueCallback;
            GetValueCallback = getValueCallback;
        }

        public void RecheckValue()
        {
            NotifyPropertyChanged(nameof(PropertyValue));
            RecheckValueInternal();
        }


        protected abstract void RecheckValueInternal();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                if (_settingsWrapperPropertyName != null && callback != null)
                    SettingsService.Instance.UnregisterSettingChangeCallback(_settingsWrapperPropertyName, callback);
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }
        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~SettingsItem()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }
        
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public partial class SettingsItemGroup : ObservableCollection<SettingsItem>, IGrouping<string, SettingsItem>
    {
        public string GroupImage
        {
            get;
            private set;
        }

        /// <summary>
        /// Used to group settings in jump view
        /// </summary>
        public virtual string GroupName
        {
            get;
            private set;
        }

        public string Key => GroupName;

        public SettingsItemGroup(string groupImage, string groupName) : base()
        {
            GroupImage = groupImage;
            GroupName = groupName;
        }

        public SettingsItemGroup(string groupImage, string groupName, IEnumerable<SettingsItem> secondaryItems) : this(groupImage, groupName)
        {
            GroupImage = groupImage;
            GroupName = groupName;
            SecondarySettings.AddRange(secondaryItems);
        }

        public SettingsItemGroup(string groupImage, string groupName, IEnumerable<SettingsItem> primaryItems, IEnumerable<SettingsItem> secondaryItems) : this(groupImage, groupName)
        {
            GroupImage = groupImage;
            GroupName = groupName;
            SecondarySettings.AddRange(secondaryItems);
            this.AddRange(primaryItems);
        }

        public ObservableCollection<SettingsItem> SecondarySettings
        {
            get; private set;
        } = new ObservableCollection<SettingsItem>();
    }
}
