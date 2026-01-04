using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Services;
using System;

namespace MayazucMediaPlayer.VideoEffects
{
    public partial class VideoEffectSliderProperty : ObservableObject
    {

        public event EventHandler<float> ValueChanged;

        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value) return;

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

        public double EffectPropertyMaximum
        {
            get
            {
                return _EffectPropertyMaximum;
            }
            set
            {
                if (_EffectPropertyMaximum == value) return;

                _EffectPropertyMaximum = (float)value;
                NotifyPropertyChanged(nameof(EffectPropertyMaximum));
            }
        }

        public double EffectPropertyValue
        {
            get
            {
                return _EffectPropertyValue;
            }
            set
            {
                if (_EffectPropertyValue == value) return;

                _EffectPropertyValue = (float)value;
                NotifyPropertyChanged(nameof(EffectPropertyValue));
                ValueChanged?.Invoke(this, (float)value);
            }
        }

        public double EffectPropertyMinimum
        {
            get
            {
                return _EffectPropertyMinimum;
            }
            set
            {
                if (_EffectPropertyMinimum == value) return;

                _EffectPropertyMinimum = (float)value;
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
