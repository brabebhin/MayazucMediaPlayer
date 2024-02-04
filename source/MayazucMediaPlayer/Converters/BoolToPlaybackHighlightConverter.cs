using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace MayazucMediaPlayer.Converters
{
    public class BoolToPlaybackHighlightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (bool)value;
            if (b)
            {
                return new SolidColorBrush((Color)(Application.Current.Resources["PinkLogo"]));
            }
            else
            {
                return new SolidColorBrush((Color)(Application.Current.Resources["NavyBlueLogo"]));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
