using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Notifications;
using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;


// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class OnlineSubtitlePicker : BaseDialog
    {
        readonly IOpenSubtitlesAgent agent;

        public ObservableCollection<IOSDBSubtitleInfo> AvailableSubtitles
        {
            get;
            private set;
        } = new ObservableCollection<IOSDBSubtitleInfo>();

        public IOSDBSubtitleInfo SelectedSubtitle
        {
            get;
            private set;
        }

        public FileInfo DownloadedSubtitleItem
        {
            get;
            private set;
        }

        readonly SubtitleRequest Request;

        public OnlineSubtitlePicker(SubtitleRequest request, IOpenSubtitlesAgent subtitlesAgent)
        {
            InitializeComponent();
            agent = subtitlesAgent;
            Loaded += OnlineSubtitlePicker_Loaded;
            agent = subtitlesAgent;
            DataContext = this;
            osdbForm.SubtitlesAgent = subtitlesAgent;
            Request = request;
        }

        private async void SelectSubtitle(object? sender, ItemClickEventArgs args)
        {
            try
            {
                lsvOnlineSubtitles.IsEnabled = false;
                LoadingBar.Visibility = Visibility.Visible;
                LoadingBar.IsIndeterminate = true;
                statusTextBlock.Text = "Downloading...";


                SelectedSubtitle = args.ClickedItem as IOSDBSubtitleInfo;
                if (SelectedSubtitle != null)
                {
                    DownloadedSubtitleItem = await agent.DownloadSubtitleAsync(SelectedSubtitle);

                }

                lsvOnlineSubtitles.IsEnabled = true;
                LoadingBar.Visibility = Visibility.Collapsed;
                LoadingBar.IsIndeterminate = false;
                statusTextBlock.Text = $"Downloading subtitle OK";

                Hide();
            }
            catch (Exception e)
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                LoadingBar.IsIndeterminate = false;
                statusTextBlock.Text = $"Downloading subtitle failed {e.Message}";
                MiscNotificationGenerator.ShowToast(e.Message, supressPopup: false);
            }
        }

        private async void OnlineSubtitlePicker_Loaded(object? sender, RoutedEventArgs e)
        {
            await GetSubtitileForFileAsync();
        }

        public async Task GetSubtitileForFileAsync()
        {
            try
            {
                LoadingBar.Visibility = Visibility.Visible;
                LoadingBar.IsIndeterminate = true;
                var subs = await agent.SearchSubtitlesAsync(Request);

                AvailableSubtitles.AddRange(subs);
                statusTextBlock.Text = "Search finish";
            }
            catch (Exception ex)
            {
                statusTextBlock.Text = $"Search failed: {ex.Message}";
                statusTextBlock.Foreground = new SolidColorBrush(Colors.Red);

            }
            finally
            {
                LoadingBar.IsIndeterminate = false;
                LoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowLogin(object? sender, RoutedEventArgs e)
        {
            if (ToggleShowLogin.IsChecked.Value)
            {
                osdbForm.Visibility = Visibility.Visible;
                usualContentGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                osdbForm.Visibility = Visibility.Collapsed;
                usualContentGrid.Visibility = Visibility.Visible;

            }
        }
    }
}
