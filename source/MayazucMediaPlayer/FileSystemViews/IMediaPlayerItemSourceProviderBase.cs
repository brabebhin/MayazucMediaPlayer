using MayazucMediaPlayer.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace MayazucMediaPlayer.FileSystemViews
{
    public partial class IMediaPlayerItemSourceProviderBase : ObservableObject
    {
        int trackNumber;
        public int ExpectedPlaybackIndex
        {
            get => trackNumber;
            set
            {
                if (trackNumber == value) return;

                trackNumber = value;
                NotifyPropertyChanged(nameof(ExpectedPlaybackIndex));
                NotifyPropertyChanged(nameof(UIDisplayedIndex));
            }
        }

        public int UIDisplayedIndex
        {
            get => trackNumber + 1;
        }

        bool isInPlayback;

        public bool IsInPlayback
        {
            get => isInPlayback;
            set
            {
                if (isInPlayback != value)
                {
                    isInPlayback = value;
                    NotifyPropertyChanged(nameof(IsInPlayback));
                    NotifyPropertyChanged(nameof(BackgroundColor));
                }
            }
        }

        public SolidColorBrush BackgroundColor
        {
            get
            {
                if (IsInPlayback)
                {
                    return new SolidColorBrush(Colors.Coral);
                }
                return new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}
