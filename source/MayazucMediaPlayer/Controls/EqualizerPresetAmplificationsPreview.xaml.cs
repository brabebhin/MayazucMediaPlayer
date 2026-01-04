using MayazucMediaPlayer.AudioEffects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class EqualizerPresetAmplificationsPreview : UserControl
    {
        AudioEqualizerPreset preset = null;
        public AudioEqualizerPreset EqualizerPreset
        {
            get
            {
                return preset;
            }
            set
            {
                preset = value;
                SetPresetUI(value);
            }
        }

        public Visibility HeaderVisibility
        {
            get
            {
                return HeaderGrid.Visibility;
            }
            set
            {
                HeaderGrid.Visibility = value;
            }
        }

        public EqualizerPresetAmplificationsPreview()
        {
            this.InitializeComponent();
        }


        public static DependencyProperty HeaderVisibilityProperty = DependencyProperty.Register(nameof(HeaderVisibility), typeof(Visibility), typeof(EqualizerPresetAmplificationsPreview), new PropertyMetadata(Visibility.Visible, OnHeaderVisibilityChanged));

        private static void OnHeaderVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisObject = d as EqualizerPresetAmplificationsPreview;
            thisObject.HeaderVisibility = (Visibility)e.NewValue;
        }

        public static DependencyProperty EqualizerPresetProperty = DependencyProperty.Register(nameof(EqualizerPreset), typeof(AudioEqualizerPreset), typeof(EqualizerPresetAmplificationsPreview), new PropertyMetadata(null, OnPresetChanged));
        private static void OnPresetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var preset = e.NewValue as AudioEqualizerPreset;
            var thisObject = d as EqualizerPresetAmplificationsPreview;
            thisObject.EqualizerPreset = preset;
        }

        private void SetPresetUI(AudioEqualizerPreset preset)
        {
            PreviewGrid.ColumnDefinitions.Clear();
            PreviewGrid.Children.Clear();
            HeaderGrid.Children.Clear();
            HeaderGrid.ColumnDefinitions.Clear();
            if (preset != null)
            {
                var amplifications = preset.FrequencyValues;

                for (int i = 0; i < amplifications.Count; i++)
                {
                    PreviewGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    Border valueUI = new Border();
                    valueUI.Background = new SolidColorBrush(Microsoft.UI.Colors.BurlyWood);
                    Grid.SetColumn(valueUI, i);
                    Grid.SetRow(valueUI, NormalizeDisplayValue(amplifications[i]));
                    PreviewGrid.Children.Add(valueUI);

                    HeaderGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    TextBlock header = new TextBlock()
                    {
                        Text = amplifications[i].ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(header, i);
                    HeaderGrid.Children.Add(header);
                }
            }
        }

        /// <summary>
        /// Equalizer amplifications take values in the range of -12 ... +12
        /// The UI rows take values in the range of               24 ...   0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int NormalizeDisplayValue(int value)
        {
            var rmin = -12.0f;
            var rmax = 12.0f;

            var tmin = 24.0f;
            var tmax = 0.0f;
            return Utilities.NormalizeValue(value, rmin, rmax, tmin, tmax);
        }
    }
}
