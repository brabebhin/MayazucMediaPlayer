using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.UserInput;
using System;

namespace MayazucMediaPlayer.VideoEffects
{
    public class VideoEffectSliderProperty : ObservableObject
    {

        public event EventHandler<float> ValueChanged;

        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                NotifyPropertyChanged(nameof(Enabled));
            }
        }


        public string EffectPropertyName
        {
            get;
            private set;
        }

        public float DefaultValue
        {
            get;
            private set;
        }

        public double Resolution
        {
            get;
            private set;
        } = 0.1;

        float _EffectPropertyValue, _EffectPropertyMinimum, _EffectPropertyMaximum;

        public float EffectPropertyMaximum
        {
            get
            {
                return _EffectPropertyMaximum;
            }
            set
            {
                _EffectPropertyMaximum = value;
                NotifyPropertyChanged(nameof(EffectPropertyMaximum));
            }
        }

        public float EffectPropertyValue
        {
            get
            {
                return _EffectPropertyValue;
            }
            set
            {
                _EffectPropertyValue = value;
                NotifyPropertyChanged(nameof(EffectPropertyValue));
                ValueChanged?.Invoke(this, value);
            }
        }

        public float EffectPropertyMinimum
        {
            get
            {
                return _EffectPropertyMinimum;
            }
            set
            {
                _EffectPropertyMinimum = value;
                NotifyPropertyChanged(nameof(EffectPropertyMinimum));
            }
        }

        public IRelayCommand RestoreDefaultValue
        {
            get;
            private set;
        }


        public VideoEffectSliderProperty(string effectPropertyName, double resolution, float effectPropertyMaximum, float effectPropertyValue, float effectPropertyMinimum, float defaultValue) : this(effectPropertyName)
        {
            Resolution = resolution;
            EffectPropertyMaximum = effectPropertyMaximum;
            EffectPropertyValue = effectPropertyValue;
            EffectPropertyMinimum = effectPropertyMinimum;
            DefaultValue = defaultValue;

        }

        public VideoEffectSliderProperty(string effectPropertyName)
        {
            EffectPropertyName = effectPropertyName;
            RestoreDefaultValue = new RelayCommand(() => { EffectPropertyValue = DefaultValue; });
        }
    }
}
