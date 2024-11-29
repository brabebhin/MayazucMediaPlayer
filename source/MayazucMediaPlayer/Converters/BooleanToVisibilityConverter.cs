using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MayazucMediaPlayer.Converters
{
    public partial class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool direct = parameter != null ? bool.Parse((string)parameter) : true;
            if ((bool)value) return direct ? Visibility.Visible : Visibility.Collapsed;
            else return direct ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            bool direct = parameter != null ? bool.Parse((string)parameter) : true;

            if ((Visibility)value == Visibility.Collapsed) return direct ? false : true;
            else return direct ? true : false;
        }
    }

    public partial class BooleanToCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IEnumerable<object>)
            {
                return (value as IEnumerable<object>).Count() != 0;
            }
            else if (value is int)
            {
                return (int)value != 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
