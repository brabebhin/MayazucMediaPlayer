using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MayazucMediaPlayer.Converters
{
    public partial class GridLengthToNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var desiredGridLength = (GridLength)parameter;
            if (value == null) return new GridLength(0, GridUnitType.Pixel);
            if (value is string) return string.IsNullOrWhiteSpace(value as string) ? new GridLength(0, GridUnitType.Pixel) : desiredGridLength;

            return desiredGridLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
