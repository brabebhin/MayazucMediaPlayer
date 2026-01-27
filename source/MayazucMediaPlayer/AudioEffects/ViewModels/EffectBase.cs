using MayazucMediaPlayer.Services;
using System;

namespace MayazucMediaPlayer.AudioEffects.ViewModels
{
    public partial class AudioEffect : ObservableObject
    {
        public string Type
        {
            get;
            private set;
        }

        /// <summary>
        /// a user defined string used to identify this effect. Must be unique
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        public string DisplayTitle
        {
            get;
            set;
        }

        /// <summary>
        /// true if effect is to cause any changes to audio pipeline
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _getEnabledCallback();
            }
            set
            {
                _setEnabledCallback(value);
                NotifyPropertyChanged(nameof(IsEnabled));
            }
        }

        private Func<bool> _getEnabledCallback;
        private Action<bool> _setEnabledCallback;

        public AudioEffect(string title, string type, Func<bool> getEnabledCallback, Action<bool> setEnabledCallback)
        {
            Type = type;
            Title = title;
            _getEnabledCallback = getEnabledCallback;
            _setEnabledCallback = setEnabledCallback;
        }
    }
}
