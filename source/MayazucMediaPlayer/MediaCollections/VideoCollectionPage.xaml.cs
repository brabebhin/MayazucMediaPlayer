using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.MediaCollections
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoCollectionPage : BasePage
    {
        readonly VideoCollectionUiService Model;
        readonly FilePickerUiService FileModel;
        public override string Title => "Video collection";

        public VideoCollectionPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
            Model = new VideoCollectionUiService(DispatcherQueue);
            FileModel = new FilePickerUiService(DispatcherQueue,
                base.ApplicationDataModels.PlaybackModel,
                ServiceProvider.GetService<PlaylistsService>());
        }

        protected override async Task OnInitializeStateAsync(object? parameter)
        {
            FileModel.NavigationRequest += DataModel_NavigationRequest;
            await RefreshVideoLibraryInternal();
        }

        protected override void FreeMemory()
        {
            FileModel.NavigationRequest -= DataModel_NavigationRequest;

            base.FreeMemory();
        }

        private void DataModel_NavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            Frame.NavigateAsync(e.PageType, e.Parameter);
        }

        private async Task RefreshVideoLibraryInternal()
        {
            fileManagementControl.ShowProgressRing = true;

            try
            {
                await Model.LoadVideoCollectionAsync();
                FileModel.ClearAllCommand.Execute(new object());
                await FileModel.HandleDragDropActivation(Model.VideoCollection);
                await fileManagementControl.LoadStateInternal(FileModel);
            }
            catch { }
            fileManagementControl.ShowProgressRing = false;
        }

        private async void AddFolderToVideoLibrary(object? sender, RoutedEventArgs e)
        {
            try
            {
                StorageLibrary lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
                var folder = await lib.RequestAddFolderAsync();
                if (folder != null)
                {
                    await FileModel.OpenFolderAsync(folder);
                }
            }
            catch { }
        }

        private async void RefreshVideoLibrary(object? sender, RoutedEventArgs e)
        {
            await RefreshVideoLibraryInternal();
        }
    }
}
