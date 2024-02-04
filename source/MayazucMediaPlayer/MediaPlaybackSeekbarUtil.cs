using System;

namespace MayazucMediaPlayer
{
    public class MediaPlaybackSeekbarUtil
    {
        public const int MaximumSliderValue = 100;

        public static double GetNormalizedValue(TimeSpan position, TimeSpan naturalDuration)
        {
            if (naturalDuration == TimeSpan.Zero) return 0;
            /*
             MaximumSliderValue ..... naturalDuration
             X........................position
             */
            var comptued = MaximumSliderValue * position.TotalSeconds / naturalDuration.TotalSeconds;
            return double.IsNaN(comptued) ? 0 : comptued;
        }

        public static TimeSpan GetDenormalizedValue(double normalized, TimeSpan naturalDuration)
        {
            /*
             MaximumSliderValue.... natural duration
             normalized ............X
             */
            if (naturalDuration == TimeSpan.Zero) return TimeSpan.Zero;


            var totalSeconds = normalized * naturalDuration.TotalSeconds / MaximumSliderValue;
            return double.IsNaN(totalSeconds) ? TimeSpan.Zero : TimeSpan.FromSeconds(totalSeconds);
        }
    }
}
