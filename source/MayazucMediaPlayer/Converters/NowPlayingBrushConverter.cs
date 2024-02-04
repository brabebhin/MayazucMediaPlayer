using Microsoft.UI.Xaml.Data;
using System;

namespace MayazucMediaPlayer.Converters
{
    public class NowPlayingBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                var v = (bool)value;
                if (v)
                {
                    return App.CurrentInstance.Resources["NowPlayingHighlightTrueBrush"];
                }
            }

            return App.CurrentInstance.Resources["NowPlayingHighlightFalseBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
