using FFmpegInteropX;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.MediaPlayback;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace MayazucMediaPlayer.Converters
{
    public sealed class ConverterLocator
    {
        public string TimespanToString(TimeSpan value) => (value).FormatTimespan();

        public string MediaPlayerPositionThumbToolTipValueConverter(double value)
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

        public string DoubleToStringThumbtipConverter(double value)
        {
            return (value).ToString("0.##");
        }

        public SolidColorBrush HighlightConverter(bool value)
        {
            if (value)
            {
                return new SolidColorBrush((Color)(Application.Current.Resources["PinkLogo"]));
            }
            else
            {
                return new SolidColorBrush((Color)(Application.Current.Resources["NavyBlueLogo"]));
            }
        }


        public Visibility VisibleWhenZeroConverter(int value, bool inverted)
        {
            if (!inverted)
            {
                if (0 == value) return Visibility.Visible;
                else return Visibility.Collapsed;
            }
            else
            {
                if (0 == value) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        public Visibility BooleanToVisibleConverter(bool value, bool direct)
        {
            if (value) return direct ? Visibility.Visible : Visibility.Collapsed;
            else return direct ? Visibility.Collapsed : Visibility.Visible;
        }

        public bool NegateBooleanConvreter(bool value) => !value;



        public Brush NowPlayingBrushConverter(bool value)
        {
            if (value)
            {
                return (Brush)App.CurrentInstance.Resources["NowPlayingHighlightTrueBrush"];
            }

            return (Brush)App.CurrentInstance.Resources["NowPlayingHighlightFalseBrush"];
        }

        public Visibility VisibilityNullConverter(object value)
        {
            if (value == null) return Visibility.Collapsed;
            if (value is string) return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

            return Visibility.Visible;
        }


        public bool BooleanCountConverter(int value)
        {
            if (value is int)
            {
                return (int)value != 0;
            }
            return false;
        }


        public bool BooleanCountConverter(IEnumerable<object> value)
        {
            if (value is IEnumerable<object>)
            {
                return (value as IEnumerable<object>).Count() != 0;
            }
            return false;
        }


        public string StreamInfoConverter(IStreamInfo streamInfo)
        {
            var result = $"{streamInfo.Name} {streamInfo.Language} {streamInfo.CodecName}";
            if (string.IsNullOrWhiteSpace(result))
            {
                return "default";
            }
            else return result;
        }


        public bool selectionModeToBoolean(ListViewSelectionMode value)
        {
            return value != ListViewSelectionMode.None;
        }


        public string MediaPlaybackSpeedSliderThumbConverter(double value)
        {
            var iValue = (int)value;
            return PlaybackSpeedService.SupportedPlaybackSpeeds[iValue].ToString();
        }
    }
}
