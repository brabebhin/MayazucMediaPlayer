using MayazucMediaPlayer.AudioEffects;
using Microsoft.UI.Xaml;
// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class MetadataAsociationDialog : BaseDialog
    {
        public AudioEqualizerPreset InitialPreset { get; set; }

        public MetadataAsociationDialogResult Results
        {
            get; set;
        }


        public MetadataAsociationDialog()
        {
            InitializeComponent();
            Results = new MetadataAsociationDialogResult();
            DataContext = Results;
            Loaded += MetadataAsociationDialog_Loaded;
        }

        private void MetadataAsociationDialog_Loaded(object? sender, RoutedEventArgs e)
        {
            if (InitialPreset != null)
            {
                Results.MetadataAsociation = InitialPreset.MetadataAsociation;
                Results.MetadataAsociationValue = InitialPreset.MetadataAssociationValue;
            }

        }

        protected override void OnPrimaryButtonClick()
        {
            Results.Succeded = true;
        }

        protected override void OnSecondaryButtonClick()
        {
            Results.Succeded = false;
        }
    }

    public class MetadataAsociationDialogResult
    {
        public int MetadataAsociation
        {
            get; set;
        }

        public string MetadataAsociationValue
        {
            get; set;
        }

        public bool Succeded
        {
            get; set;
        }
    }
}
