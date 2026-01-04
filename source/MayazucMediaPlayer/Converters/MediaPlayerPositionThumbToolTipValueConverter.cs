using Microsoft.UI.Xaml.Data;
using System;

namespace MayazucMediaPlayer.Converters
{
    public partial class MediaPlayerPositionThumbToolTipValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
            var doubleValue = (double)value;
            if (session != null)
            {
                var time = MediaPlaybackSeekbarUtil.GetDenormalizedValue(doubleValue, session.NaturalDuration);
                return MediaPlaybackSeekbarUtil.FormatTimespan(time);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
