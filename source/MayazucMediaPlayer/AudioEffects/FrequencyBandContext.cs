using MayazucMediaPlayer.Services;
using System;
using System.Globalization;
using System.Linq;

namespace MayazucMediaPlayer.AudioEffects
{
    public class FrequencyBandContext : ObservableObject
    {
        private bool _enabled = true;

        public bool IsEnabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    NotifyPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        public int FrequencyAmplification
        {
            get
            {
                return wrappedOject.Amplification;
            }
            set
            {
                if (wrappedOject.Amplification != value)
                {
                    wrappedOject.Amplification = value;
                    NotifyPropertyChanged(nameof(FrequencyAmplification));
                }
            }
        }

        public int FrequencyDisplay
        {
            get;
            private set;
        }

        public double Octave
        {
            get;
            private set;
        }

        public double SliderHeight
        {
            get;
            private set;
        } = 250;

        public void SetSliderHeight(double value)
        {
            if (SliderHeight != value)
            {
                SliderHeight = value;
                NotifyPropertyChanged($"{nameof(SliderHeight)}");
            }
        }

        readonly FrequencyDefinition wrappedOject;

        public FrequencyBandContext(FrequencyDefinition context)
        {
            FrequencyDisplay = context.CutOffFrequency;
            Octave = context.Octave;
            wrappedOject = context;
        }

        public string GetConfigurationString()
        {
            return string.Format(CultureInfo.InvariantCulture, ContainerKeys.EqualizerConfigurationStringFormat, FrequencyDisplay, Octave, FrequencyAmplification);
        }

        public void RefreshFromContext(string context)
        {
            var ctxStringSplits = context.Split(':');
            var freq = ctxStringSplits.FirstOrDefault(x => x.StartsWith("f="));
            FrequencyDisplay = int.Parse(freq.Replace("f=", ""));

            var octaves = ctxStringSplits.FirstOrDefault(x => x.StartsWith("width="));
            Octave = double.Parse(octaves.Replace("width=", ""));

            var gainstr = ctxStringSplits.FirstOrDefault(x => x.StartsWith("g="));
            FrequencyAmplification = int.Parse(gainstr.Replace("g=", ""));
        }
    }
}
