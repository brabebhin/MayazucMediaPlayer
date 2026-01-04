using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class MinimumSubtitleDurationControl : BaseUserControl, IContentSettingsItem
    {
        public MinimumSubtitleDurationControl()
        {
            InitializeComponent();
            IsEnabled = false;
        }

        public IMinimumSubtitleLengthAdapter Adapter
        {
            get;
            private set;
        }

        public void SetAdapter(IMinimumSubtitleLengthAdapter adapter)
        {
            Adapter = adapter;
            DataContext = Adapter;
            IsEnabled = Adapter != null;
        }

        public static MinimumSubtitleDurationControl CreateGlobal()
        {
            var returnValue = new MinimumSubtitleDurationControl();
            returnValue.SetAdapter(new GlobalMinimumSubtitleLengthAdapter());
            return returnValue;
        }

        public void RecheckValue()
        {
            Adapter?.RecheckValue();
        }

        private void DecreaseSlider(object? sender, RoutedEventArgs e)
        {
            var newValue = Adapter.MinimumDuration - SliderInstance.TickFrequency;
            if (newValue < 0)
            {
                newValue = 0;
            }
            Adapter.MinimumDuration = newValue;
        }

        private void AddSlider(object? sender, RoutedEventArgs e)
        {
            var newValue = Adapter.MinimumDuration + SliderInstance.TickFrequency;
            if (newValue > SliderInstance.Maximum)
            {
                newValue = SliderInstance.Maximum;
            }
            Adapter.MinimumDuration = newValue;
        }
    }
}
