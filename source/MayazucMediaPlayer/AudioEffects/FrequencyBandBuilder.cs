﻿using MayazucMediaPlayer.Services;

namespace MayazucMediaPlayer.AudioEffects
{
    public class FrequencyBandBuilder : ObservableObject
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
                cutOff = value;
                NotifyPropertyChanged(nameof(CutOff));
            }
        }

        public FrequencyDefinition GetFrequencyDefinition()
        {
            return new FrequencyDefinition(CutOff);
        }
    }
}
