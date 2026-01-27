using MayazucMediaPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MayazucMediaPlayer.AudioEffects
{
    public partial class AudioEqualizerPreset : ObservableObject
    {
        string presetName;

        public string AmplificationsOverview
        {
            get
            {
                return string.Join(' ', FrequencyValues.Select(x => x.ToString()));
            }
        }

        public List<int> FrequencyValues
        {
            get;
            set;
        } = new List<int>();

        public bool IsEnabled { set; get; }

        public string PresetName
        {
            set
            {
                if (presetName != value)
                {
                    presetName = value;
                    NotifyPropertyChanged("PresetName");
                }
            }
            get
            {
                return presetName;
            }
        }

        public AudioEqualizerPreset()
        {
        }

        public AudioEqualizerPreset(string name, IEnumerable<int> _frequencyValues)
        {
            PresetName = name;
            FrequencyValues = new List<int>();

            SetAmplifications(_frequencyValues);
        }

        public void SetAmplifications(IEnumerable<int> _frequencyValues)
        {
            FrequencyValues.Clear();
            foreach (int x in _frequencyValues)
            {
                FrequencyValues.Add(x);
            }
        }



        public bool Equals(AudioEqualizerPreset other)
        {
            bool result = true;
            result = result & other.PresetName == PresetName;
            if (!result) return result;

            result = result & other.FrequencyValues.Count == FrequencyValues.Count;
            if (!result) return result;

            for (int i = 0; i < FrequencyValues.Count; i++)
            {
                result = result & other.FrequencyValues[i] == FrequencyValues[i];
                if (!result) return result;
            }

            return result;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FrequencyValues, PresetName);
        }

        public override string ToString()
        {
            return PresetName;
        }
    }
}