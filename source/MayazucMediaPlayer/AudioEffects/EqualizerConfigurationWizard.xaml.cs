using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;



namespace MayazucMediaPlayer.AudioEffects
{
    /// <summary>
    /// gets user input for creating a new equalizer configuration. Gives the configuration name and the band frequencies
    /// </summary>
    public sealed partial class EqualizerConfigurationWizard : ContentDialog
    {
        public bool Succeded
        {
            get;
            private set;
        }


        public EqualizerConfigurationWizard()
        {
            InitializeComponent();
            DataContext = this;
        }

        public EqualizerConfigurationWizard(EqualizerConfiguration conf)
        {
            InitializeComponent();
            DataContext = this;
            ConfigName = conf.Name;
            foreach (var v in conf.Bands)
            {
                Bands.Add(new FrequencyBandBuilder()
                {
                    CutOff = v.CutOffFrequency
                });
            }
        }

        public string ConfigName { get; set; }

        public ObservableCollection<FrequencyBandBuilder> Bands { get; set; } = new ObservableCollection<FrequencyBandBuilder>();

        private void RemoveBand(object? sender, RoutedEventArgs e)
        {
            var s = (sender as FrameworkElement).DataContext as FrequencyBandBuilder;
            Bands.Remove(s);
        }

        private void AddBand(object? sender, RoutedEventArgs e)
        {
            FrequencyBandBuilder def = new FrequencyBandBuilder();
            Bands.Add(def);
        }

        private void Save(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Succeded = true;
        }

        private void Cancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Succeded = false;
        }


        public IEnumerable<FrequencyDefinition> GetFrequencyDefinitions()
        {
            return (Bands.Select(x => x.GetFrequencyDefinition()).AsEnumerable().OrderBy(x => x.CutOffFrequency));
        }

        public IEnumerable<string> Validate()
        {
            List<string> errors = new List<string>();
            if (Bands.Any(x => x.CutOff == 0))
                errors.Add("Some frequency bands have invalid inputs");
            if (string.IsNullOrWhiteSpace(ConfigName) || ConfigName.ToLowerInvariant() == "MC Media Center")
                errors.Add("Invalid name");
            if (!Bands.Any())
                errors.Add("You need at least 1 band");

            return errors.AsEnumerable();

        }
    }
}
