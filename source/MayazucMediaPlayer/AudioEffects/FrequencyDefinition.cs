using MayazucMediaPlayer.Services;

namespace MayazucMediaPlayer.AudioEffects
{
    public class FrequencyDefinition : ObservableObject
    {
        int cutOff;

        public int CutOffFrequency
        {
            get
            {
                return cutOff;
            }
            set
            {
                cutOff = value;
                NotifyPropertyChanged(nameof(CutOffFrequency));
            }
        }

        public double Octave
        {
            get;
            set;
        }

        int amplification = 0;
        public int Amplification
        {
            get => amplification;
            set
            {
                amplification = value;
                NotifyPropertyChanged(nameof(Amplification));
            }
        }

        public FrequencyDefinition() { }

        public FrequencyDefinition(int freq)
        {
            CutOffFrequency = freq;
        }

        public FrequencyDefinition(FrequencyDefinition other)
        {
            Amplification = other.Amplification;
            CutOffFrequency = other.CutOffFrequency;
            Octave = other.Octave;
        }

        public bool Equals(FrequencyDefinition other)
        {
            if (CutOffFrequency.Equals(other.CutOffFrequency)) return true;
            else return false;
        }
    }
}
