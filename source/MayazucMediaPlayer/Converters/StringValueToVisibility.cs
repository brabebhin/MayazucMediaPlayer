using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MayazucMediaPlayer.Converters
{
    public class StringValueToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sValue = value.ToString();
            if (!string.IsNullOrWhiteSpace(sValue))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
