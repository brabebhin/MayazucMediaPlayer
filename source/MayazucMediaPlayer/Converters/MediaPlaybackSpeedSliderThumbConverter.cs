using MayazucMediaPlayer.MediaPlayback;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
