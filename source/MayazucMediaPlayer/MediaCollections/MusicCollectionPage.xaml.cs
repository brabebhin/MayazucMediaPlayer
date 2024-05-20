using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using WinRT;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.MediaCollections
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MusicCollectionPage : BasePage
    {
        public override string Title => "Music collection";

        readonly MusicCollectionUiService Model;
        readonly FilePickerUiService FileModel;

        public MusicCollectionPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;

            Model = new MusicCollectionUiService(DispatcherQueue);
            FileModel = new FilePickerUiService(DispatcherQueue,
                base.ApplicationDataModels.PlaybackModel,
                ServiceProvider.GetService<PlaylistsService>());

        }

        protected override async Task OnInitializeStateAsync(object? parameter)
        {
            FileModel.NavigationRequest += DataModel_NavigationRequest;
            await RefreshMusicLibraryInternal();
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

        private async Task RefreshMusicLibraryInternal()
        {
            fileManagementControl.ShowProgressRing = true;

            try
            {
                await Model.LoadMusicCollectionAsync();
                FileModel.ClearAllCommand.Execute(null);
                await FileModel.HandleDragDropActivation(Model.MusicCollection);
                await fileManagementControl.LoadStateInternal(FileModel);
            }
            catch { }
            fileManagementControl.ShowProgressRing = false;
        }

        private async void AddFolderToMusicLibrary(object? sender, RoutedEventArgs e)
        {
            try
            {
                StorageLibrary lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);  
                
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
            await RefreshMusicLibraryInternal();
        }
    }
}
