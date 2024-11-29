using FFmpegInteropX;
using Microsoft.UI.Xaml.Data;
using System;

namespace MayazucMediaPlayer.Converters
{
    public partial class StreamInfoToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var streamInfo = value as IStreamInfo;
            var result = $"{streamInfo.Name} {streamInfo.Language} {streamInfo.CodecName}";
            if (string.IsNullOrWhiteSpace(result))
            {
                return "default";
            }
            else return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
