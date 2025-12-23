using MayazucMediaPlayer.Services;
using System;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.VideoEffects
{
    public partial class ManagedVideoEffectProcessorConfiguration : ObservableObject
    {
        bool _masterSwitch;
        float _Brightness;
        float _Contrast;
        float _Saturation;
        float _Temperature;
        float _Tint;
        float _Sharpness;
        float _SharpnessThreshold;
        bool _grayscaleEnabled;
        bool _blurEnabled;

        public bool MasterSwitch
        {
            get => _masterSwitch;
            set
            {
                base.SetProperty(ref _masterSwitch, value, nameof(MasterSwitch));
                ConfigurationChanged(this, nameof(MasterSwitch));
            }
        }

        public bool GrayscaleEnabled
        {
            get => _grayscaleEnabled;
            set
            {
                base.SetProperty(ref _grayscaleEnabled, value, nameof(GrayscaleEnabled));
                ConfigurationChanged(this, nameof(GrayscaleEnabled));
            }
        }

        public bool BlurEnabled
        {
            get => _blurEnabled;
            set
            {
                base.SetProperty(ref _blurEnabled, value, nameof(BlurEnabled));
                ConfigurationChanged(this, nameof(BlurEnabled));
            }
        }

        public float Brightness
        {
            get => _Brightness;
            set
            {
                base.SetProperty(ref _Brightness, value, nameof(Brightness));
                ConfigurationChanged(this, nameof(Brightness));
            }
        }
        public float Contrast
        {
            get => _Contrast;
            set
            {
                base.SetProperty(ref _Contrast, value, nameof(Contrast));
                ConfigurationChanged(this, nameof(Contrast));
            }
        }
        public float Saturation
        {
            get => _Saturation;
            set
            {
                base.SetProperty(ref _Saturation, value, nameof(Saturation));
                ConfigurationChanged(this, nameof(Saturation));
            }
        }

        public float Temperature
        {
            get => _Temperature;
            set
            {
                base.SetProperty(ref _Temperature, value, nameof(Temperature));
                ConfigurationChanged(this, nameof(Temperature));
            }
        }

        public float Tint
        {
            get => _Tint;
            set
            {
                base.SetProperty(ref _Tint, value, nameof(Tint));
                ConfigurationChanged(this, nameof(Tint));
            }
        }
        public float Sharpness
        {
            get => _Sharpness;
            set
            {
                this.SetProperty(ref _Sharpness, value, nameof(Sharpness));
                ConfigurationChanged(this, nameof(Sharpness));
            }
        }

        public float SharpnessThreshold
        {
            get => _SharpnessThreshold;
            set
            {
                SetProperty(ref _SharpnessThreshold, value, nameof(SharpnessThreshold));
                ConfigurationChanged(this, nameof(Brightness));
            }
        }

        public event EventHandler<string> ConfigurationChanged;

        public void RemoveVideoEffect(MediaPlayer player)
        {
            player.RemoveAllEffects();
        }
    }
}
