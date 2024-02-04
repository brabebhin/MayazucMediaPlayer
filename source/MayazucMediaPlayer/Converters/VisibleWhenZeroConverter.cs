using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MayazucMediaPlayer.Converters
{
    public class VisibleWhenZeroConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, string l)
        {
            var inverted = false;
            if (p != null)
            {
                inverted = !bool.Parse(p.ToString());
            }
            if (!inverted)
            {

                if (0 == System.Convert.ToInt32(v)) return Visibility.Visible;
                else return Visibility.Collapsed;
            }
            else
            {
                if (0 == System.Convert.ToInt32(v)) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        public object ConvertBack(object v, Type t, object p, string l) => null;
    }
}
