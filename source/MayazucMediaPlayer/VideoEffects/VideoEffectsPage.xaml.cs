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
        public VideoEffectsPageViewModel? DataService
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

        public async Task LoadState(ManagedVideoEffectProcessorConfiguration effectConfig)
        {
            DataService = new VideoEffectsPageViewModel(DispatcherQueue, effectConfig);
            DataContext = DataService;

            DataService.ProfileSliderValueChanged += Model_ProfileSliderValueChanged;
            await DataService.LoadSavedColorProfiles();
        }


        private async void ColorProfileSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (cbSavedProfiles.SelectedItem != null)
            {
                cbSavedProfiles.SelectionChanged -= ColorProfileSelectionChanged;
                DataService.ProfileSliderValueChanged -= Model_ProfileSliderValueChanged;
                await DataService.LoadSelecteProfileAsync((SavedColorProfile)cbSavedProfiles.SelectedItem);
                DataService.ProfileSliderValueChanged += Model_ProfileSliderValueChanged;
                cbSavedProfiles.SelectionChanged += ColorProfileSelectionChanged;
            }
        }

        private async void ShowManagementDialog(object? sender, RoutedEventArgs e)
        {
            VideoColorProfileManagementDialog diag = new VideoColorProfileManagementDialog(DataService);
            await ContentDialogService.Instance.ShowAsync(diag);

        }

        private void ResetDefaultProfile(object? sender, RoutedEventArgs e)
        {
            cbSavedProfiles.SelectionChanged -= ColorProfileSelectionChanged;
            DataService.ProfileSliderValueChanged -= Model_ProfileSliderValueChanged;
            cbSavedProfiles.SelectedItem = DataService.SavedColorProfiles.FirstOrDefault(x => x.IsDefault);
            DataService.ResetDefault.Execute(null);
            DataService.ProfileSliderValueChanged += Model_ProfileSliderValueChanged;
            cbSavedProfiles.SelectionChanged += ColorProfileSelectionChanged;
        }

        private void RestoreDefaultSliderValue(object sender, RoutedEventArgs e)
        {
            DataService.ResetDefault.Execute(sender.GetDataContextObject<VideoEffectSliderProperty>());
        }
    }
}
