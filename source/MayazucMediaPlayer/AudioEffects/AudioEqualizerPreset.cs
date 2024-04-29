using MayazucMediaPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
namespace MayazucMediaPlayer.AudioEffects
{
    public class AudioEqualizerPreset : ObservableObject
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

        public double WindowWidth
        {
            get
            {
                var window = CoreWindow.GetForCurrentThread();
                if (window != null)
                {

                    return window.Bounds.Width;
                }
                else return 0;
            }
        }

        public bool IsEnabled { set; get; }

        public string PresetName
        {
            set
            {
                presetName = value;
                NotifyPropertyChanged("PresetName");
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
    }
}