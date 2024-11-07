using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace MayazucMediaPlayer.Converters
{
    [Obsolete]
    public class StringHashToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return (Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush).Color;
            var name = (string)value;
            return GetRandomHashColor(name);
        }

        public static Color GetRandomHashColor(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return (Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush).Color;
            var hash = GetStringNumeric(name);
            Random r = new Random(hash);

            return Color.FromArgb(255, (byte)r.Next(0, 256), (byte)r.Next(0, 256), (byte)r.Next(0, 256));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public static int GetStringNumeric(string name)
        {
            int retValue = 0;
            for (int i = 0; i < name.Length; i++)
                retValue += System.Convert.ToInt32(name[i]);

            return Math.Abs(retValue);
        }
    }
}
