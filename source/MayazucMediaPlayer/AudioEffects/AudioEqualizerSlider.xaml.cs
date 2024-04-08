using MayazucMediaPlayer.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.AudioEffects
{
    public sealed partial class AudioEqualizerSlider : BaseUserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public bool DirectBinding
        {
            get; set;
        }

        public string? FasTag
        {
            get;
            set;
        }

        string displayFreq = "0";

        public string DisplayFrequency
        {
            get
            {
                return displayFreq;
            }

            set
            {
                displayFreq = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("DisplayFrequency"));
            }
        }

        public AudioEqualizerSlider() : base()
        {
            DirectBinding = true;
            InitializeComponent();
            Loaded += AudioEqualizerSlider_Loaded;

        }

        private void AudioEqualizerSlider_Loaded(object? sender, RoutedEventArgs e)
        {
            DataContext = this;
        }
    }
}
