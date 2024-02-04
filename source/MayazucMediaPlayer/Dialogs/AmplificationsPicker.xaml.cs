using MayazucMediaPlayer.AudioEffects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class AmplificationsPicker : BaseDialog
    {
        public AudioEqualizerPreset InitialPreset
        {
            get; set;
        }

        public AmplificationsPickerResult Results
        {
            get; private set;
        }

        public bool LoadCurrentAmps
        {
            get; set;
        }



        public ObservableCollection<FrequencyBandContext> FrequencyBands
        {
            get; private set;
        } = new ObservableCollection<FrequencyBandContext>();

        public EqualizerConfiguration EqualizerConfiguration
        {
            get;
            private set;
        }

        public AmplificationsPicker(EqualizerConfiguration configuration)
        {
            InitializeComponent();

            Loaded += AmplificationsPicker_Loaded;
            lsvBands.ItemsSource = FrequencyBands;
            EqualizerConfiguration = configuration;
        }

        private void AmplificationsPicker_Loaded(object? sender, RoutedEventArgs e)
        {
            LoadCurrentAmplifications();
            if (InitialPreset != null)
            {
                for (int i = 0; i < InitialPreset.FrequencyValues.Count; i++)
                {
                    FrequencyBands[i].FrequencyAmplification = InitialPreset.FrequencyValues[i];
                }
            }
        }


        protected override void OnPrimaryButtonClick()
        {
            Results = new AmplificationsPickerResult(FrequencyBands);
            Results.Succeded = true;
        }

        protected override void OnSecondaryButtonClick()
        {
            Results = new AmplificationsPickerResult(FrequencyBands);
            Results.Succeded = false;
        }

        private void resetEqualizer_tapped(object? sender, TappedRoutedEventArgs e)
        {
            foreach (var v in FrequencyBands)
            {
                v.FrequencyAmplification = 0;
            }
        }

        private void useCurrentAmplifications_tapped(object? sender, TappedRoutedEventArgs e)
        {
            LoadCurrentAmplifications();
        }

        private void LoadCurrentAmplifications()
        {
            FrequencyBands.Clear();
            var currentConfig = EqualizerConfiguration;
            foreach (var c in currentConfig.Bands.Select(x => new FrequencyDefinition(x)))
            {
                FrequencyBands.Add(new FrequencyBandContext(c));
            }
        }
    }

    public sealed class AmplificationsPickerResult
    {

        public ReadOnlyObservableCollection<FrequencyBandContext> AmplificationBands
        {
            get;
            private set;
        }

        public bool Succeded
        {
            get; set;
        }

        public AmplificationsPickerResult(IEnumerable<FrequencyBandContext> amps)
        {
            AmplificationBands = new ReadOnlyObservableCollection<FrequencyBandContext>(new ObservableCollection<FrequencyBandContext>(amps));
        }
    }
}