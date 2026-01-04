using Microsoft.UI.Xaml.Data;
using System;

namespace MayazucMediaPlayer.Converters
{
    public partial class TimespanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((TimeSpan)value).FormatTimespan();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
