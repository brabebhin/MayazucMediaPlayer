using MayazucMediaPlayer.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace MayazucMediaPlayer.Playlists
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>   
    public sealed partial class PlaylistViewerPage : BasePage
    {
        public override string Title => "Playlists";

        private PlaylistDetailsPage playlistDetailsViewPage;

        private PlaylistDetailsPage PlaylistDetailsViewPage
        {
            get => playlistDetailsViewPage;
            set
            {
                if (playlistDetailsViewPage != null)
                {
                    PlaylistsManagerSplitView.Content = null;

                    playlistDetailsViewPage.Dispose();
                }

                playlistDetailsViewPage = value;

                if (playlistDetailsViewPage != null)
                {
                    PlaylistsManagerSplitView.Content = playlistDetailsViewPage;
                }
            }
        }

        PlaylistViewerModel model;
        public PlaylistViewerModel PlaylistViewModelInstance
        {
            get
            {
                return model;
            }
            set
            {
                if (model != null)
                    model.OnPlaylistItemDeleted -= model_OnPlaylistItemDeleted;
                model = value;
                if (model != null)
                    model.OnPlaylistItemDeleted += model_OnPlaylistItemDeleted;
            }
        }


        public PlaylistViewerPage()
        {
            InitializeComponent();
        }

        private void PlaylistDetailsViewPage_ExternalNavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            Frame.NotifyExternalNavigationRequest(sender, e);
        }

        private void PlaylistViewerPage_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            CheckSize(e.NewSize.Width);
        }

        private void CheckSize(double width)
        {
            if (width >= 1000)
            {
                VisualStateManager.GoToState(this, "DoubleViewWidth", false);
                //PlaylistsSplitViewButton.IsChecked = true;
            }
            else
            {
                VisualStateManager.GoToState(this, "SingleViewWidth", false);
            }
        }

        protected override void FreeMemory()
        {
            PlaylistViewModelInstance.GetSelectedItemsRequest -= Model_GetSelectedItemsRequest;
            PlaylistViewModelInstance.NavigationRequest -= Model_NavigationRequest;
            DisplayList.SelectionChanged -= DisplayList_SelectionChanged;

            PlaylistViewModelInstance = null;
            PlaylistDetailsViewPage?.Dispose();
            base.FreeMemory();
        }

        protected override Task OnInitializeStateAsync(object? parameter)
        {
            PlaylistViewModelInstance = ServiceProvider.GetService<PlaylistViewerModel>();
            DataContext = PlaylistViewModelInstance;
            PlaylistViewModelInstance.LoadState(parameter);

            SizeChanged += PlaylistViewerPage_SizeChanged;
            CheckSize(ActualSize.X);
            DisplayList.SelectionChanged += DisplayList_SelectionChanged;

            PlaylistViewModelInstance.GetSelectedItemsRequest += Model_GetSelectedItemsRequest;
            PlaylistViewModelInstance.NavigationRequest += Model_NavigationRequest;
            mcSearchBar.Filter = (x) => { return ((PlaylistItem)x).Title; };
            return Task.CompletedTask;
        }


        private void Model_NavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            (Frame as DependencyInjectionFrame).NotifyExternalNavigationRequest(this, e);
        }

        private void Model_GetSelectedItemsRequest(object? sender, List<PlaylistItem> e)
        {
            if (DisplayList.SelectionMode == ListViewSelectionMode.Multiple)
            {
                e.AddRange(DisplayList.SelectedItems.OfType<PlaylistItem>());
            }
        }


        void DisplayList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (PlaylistViewModelInstance.DisplayListSelectionMode == ListViewSelectionMode.Multiple)
            {
                if (DisplayList.SelectedItems.Count > 0 && DisplayList.SelectionMode == ListViewSelectionMode.Multiple)
                {
                    PlaylistViewModelInstance.ChangeButtonAvailabilityButtons(true);
                }
                else
                {
                    PlaylistViewModelInstance.ChangeButtonAvailabilityButtons(false);
                }
            }
        }


        private void model_OnPlaylistItemDeleted(object? sender, PlaylistItem e)
        {
            try
            {
                var detailsPage = PlaylistDetailsViewPage as PlaylistDetailsPage;
                if (detailsPage == null) return;

                var playlistItemDisplayed = detailsPage.PlaylistsDetailsService.CurrentPlaylistItem;

                if (playlistItemDisplayed == e)
                    PlaylistDetailsViewPage = null;
            }
            catch { }
        }

        private void GoToDetailsPage(object? sender, TappedRoutedEventArgs e)
        {
            var itm = (sender as FrameworkElement).DataContext as PlaylistItem;
            var newUC = new PlaylistDetailsPage(itm, ServiceProvider.GetService<PlaylistsDetailsService>());
            Grid.SetRowSpan(newUC, 2);
            Grid.SetColumn(newUC, 1);
            newUC.HorizontalAlignment = HorizontalAlignment.Stretch;
            newUC.VerticalAlignment = VerticalAlignment.Stretch;
            PlaylistDetailsViewPage = newUC;
        }
    }
}
