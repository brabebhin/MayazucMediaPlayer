using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.VideoEffects
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoEffectsPage : BaseUserControl
    {
        public VideoEffectsPageViewModel? Model
        {
            get;
            private set;
        }

        public VideoEffectsPage()
        {
            InitializeComponent();
            cbSavedProfiles.SelectedIndex = 0;

        }

        private void Model_ProfileSliderValueChanged(object? sender, object e)
        {
            cbSavedProfiles.SelectedItem = null;
        }

        public async Task LoadState(VideoEffectProcessorConfiguration effectConfig)
        {
            Model = new VideoEffectsPageViewModel(DispatcherQueue, effectConfig);
            DataContext = Model;

            Model.ProfileSliderValueChanged += Model_ProfileSliderValueChanged;
            await Model.LoadSavedColorProfiles();
        }


        private async void ColorProfileSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (cbSavedProfiles.SelectedItem != null)
            {
                cbSavedProfiles.SelectionChanged -= ColorProfileSelectionChanged;
                Model.ProfileSliderValueChanged -= Model_ProfileSliderValueChanged;
                await Model.LoadSelecteProfileAsync((SavedColorProfile)cbSavedProfiles.SelectedItem);
                Model.ProfileSliderValueChanged += Model_ProfileSliderValueChanged;
                cbSavedProfiles.SelectionChanged += ColorProfileSelectionChanged;
            }
        }

        private async void ShowManagementDialog(object? sender, RoutedEventArgs e)
        {
            VideoColorProfileManagementDIalog diag = new VideoColorProfileManagementDIalog(Model);
            await ContentDialogService.Instance.ShowAsync(diag);

        }

        private void ResetDefaultProfile(object? sender, RoutedEventArgs e)
        {
            cbSavedProfiles.SelectionChanged -= ColorProfileSelectionChanged;
            Model.ProfileSliderValueChanged -= Model_ProfileSliderValueChanged;
            cbSavedProfiles.SelectedItem = Model.SavedColorProfiles.FirstOrDefault(x => x.IsDefault);
            Model.ResetDefault.Execute(null);
            Model.ProfileSliderValueChanged += Model_ProfileSliderValueChanged;
            cbSavedProfiles.SelectionChanged += ColorProfileSelectionChanged;
        }
    }
}
