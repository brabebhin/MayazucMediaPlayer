using MayazucMediaPlayer.Services;

namespace MayazucMediaPlayer.AudioEffects
{
    public partial class FrequencyBandBuilder : ObservableObject
    {
        public FrequencyBandBuilder()
        {

        }

        int cutOff = 0;

        public int CutOff
        {
            get
            {
                return cutOff;
            }

            set
            {
                if (cutOff != value)
                {
                    cutOff = value;
                    NotifyPropertyChanged(nameof(CutOff));
                }
            }
        }

        public FrequencyDefinition GetFrequencyDefinition()
        {
            return new FrequencyDefinition(CutOff);
        }
    }
}
