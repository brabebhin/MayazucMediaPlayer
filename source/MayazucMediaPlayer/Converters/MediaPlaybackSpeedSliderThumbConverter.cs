using MayazucMediaPlayer.MediaPlayback;
using Microsoft.UI.Xaml.Data;
using System;

namespace MayazucMediaPlayer.Converters
{
    public partial class MediaPlaybackSpeedSliderThumbConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var iValue = (int)(double)value;
            return PlaybackSpeedService.SupportedPlaybackSpeeds[iValue].ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
