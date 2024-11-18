using CommunityToolkit.WinUI;
using MayazucMediaPlayer.FileSystemViews;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class FileManagementControl : BaseUserControl
    {
        public event EventHandler RefreshRequested;

        public FileManagementControl()
        {
            InitializeComponent();
            DataTemplateSelectorInstance = new FileManagerDataTemplateSelector(Resources["MusicItemDataTemplate"] as DataTemplate, Resources["VideoPlaylistItemDataTemplate"] as DataTemplate);
        }

        public FileManagerDataTemplateSelector DataTemplateSelectorInstance { get; private set; }

        public IPlaybackItemManagementUIService DataService
        {
            get; private set;
        }

        public void PerformCleanUp()
        {
            if (DataService != null)
            {
                DataService.GetSelectedItemsRequest -= DataModel_GetSelectedItemsRequest;

                DataService.SetSelectedItems -= DataModel_SetSelectedItems;
            }
        }

        private async void DataModel_SetSelectedItems(object? sender, IEnumerable<ItemIndexRange> e)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                if (ContentPresenter.SelectedItems.Any())
                {
                    ContentPresenter.DeselectRange(new ItemIndexRange(0, (uint)DataService.ItemsCount));
                }
                if (e != null)
                {
                    foreach (var o in e)
                    {
                        ContentPresenter.SelectRange(o);
                    }
                }

            });
        }

        private void DataModel_GetSelectedItemsRequest(object? sender, List<object> e)
        {
            e.AddRange(ContentPresenter.SelectedItems);
        }

        private void DataModel_ClearSelectionRequest(object? sender, EventArgs e)
        {
            ContentPresenter.SelectedItems.Clear();
        }

        public Task LoadStateInternal(IPlaybackItemManagementUIService model)
        {
            if (DataService == null)
            {
                DataService = model;
                DataContext = DataService;
                mcSearchBar.Filter = (x) => { return ((IMediaPlayerItemSourceProvder)x).Path; };

                DataService.GetSelectedItemsRequest += DataModel_GetSelectedItemsRequest;

                DataService.SetSelectedItems += DataModel_SetSelectedItems;
            }
            return Task.CompletedTask;
        }

        private void SelectionChangedForListView(object? sender, SelectionChangedEventArgs e)
        {
            if (ContentPresenter.SelectedItems.Count == 0 || ContentPresenter.SelectionMode == ListViewSelectionMode.None)
            {
                btnSaveAsPlaylistSelected.IsEnabled = btnRemoveSelected.IsEnabled = btnAddToExistingPlaylist.IsEnabled = false;
            }
            else
            {
                btnSaveAsPlaylistSelected.IsEnabled = btnRemoveSelected.IsEnabled = btnAddToExistingPlaylist.IsEnabled = true;
            }
        }

        private async void PlayFile(object? sender, ItemClickEventArgs e)
        {
            var file = e.ClickedItem as IMediaPlayerItemSourceProvder;
            if (DataService != null)
                await DataService.PlaySingleFileCommand.ExecuteAsync(file);
        }

        public string PlaceHolderText
        {
            get => (string)GetValue(PlaceHolderTextProperty);
            set
            {
                SetValue(PlaceHolderTextProperty, value);
            }
        }

        public bool ShowProgressRing
        {
            get => (bool)GetValue(ProgressBarVisibilityProperty);
            set
            {
                SetValue(ProgressBarVisibilityProperty, value);
                if (value)
                {
                    progressBar.Visibility = Visibility.Visible;
                }
                else
                {
                    progressBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        public static DependencyProperty PlaceHolderTextProperty = DependencyProperty.Register(nameof(PlaceHolderText), typeof(string), typeof(FileManagementControl), new PropertyMetadata("", PlaceHolderTextChanged));

        private static void PlaceHolderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (FileManagementControl)d;
            obj.PlaceHolderText = (string)e.NewValue;
        }

        public static DependencyProperty ProgressBarVisibilityProperty = DependencyProperty.Register(nameof(ShowProgressRing), typeof(bool), typeof(FileManagementControl), new PropertyMetadata(false, ProgressBarVisibilityChanged));

        private static void ProgressBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (FileManagementControl)d;
            obj.ShowProgressRing = (bool)e.NewValue;
        }

        public static DependencyProperty RefreshButtonVisibleProperty = DependencyProperty.Register(nameof(RefreshButtonVisible), typeof(Visibility), typeof(FileManagementControl), new PropertyMetadata((Visibility)Visibility.Collapsed, OnRefreshButtonVisibilityChanged));

        private static void OnRefreshButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (FileManagementControl)d;
            obj.RefreshButtonVisible = (Visibility)e.NewValue;
        }

        public Visibility RefreshButtonVisible
        {
            get => btnViewRefresh.Visibility;
            set => btnViewRefresh.Visibility = value;
        }

        private void OnRefresh_click(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            RefreshRequested?.Invoke(this, new EventArgs());
        }

        private async void SingleItemPlayFileCommand(object sender, RoutedEventArgs e)
        {
            await DataService.PlaySingleFileCommand.ExecuteAsync(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private async void SingleItemEnqueueFileCommand(object sender, RoutedEventArgs e)
        {
            await DataService.EnqueueSingleFileCommand.ExecuteAsync(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private async void SingleItemPlayNextSingleFileCommand(object sender, RoutedEventArgs e)
        {
            await DataService.PlayNextSingleFileCommand.ExecuteAsync(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private async void SingleItemPlayStartingFromFileCommand(object sender, RoutedEventArgs e)
        {
            await DataService.PlayStartingFromFileCommand.ExecuteAsync(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private async void SingleItemAddFileToPlaylistCommand(object sender, RoutedEventArgs e)
        {
            await DataService.AddSingleFileToPlaylistCommand.ExecuteAsync(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private void SingleItemRemoveSlidedItem(object sender, RoutedEventArgs e)
        {
            DataService.RemoveSlidedItem.Execute(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private async void SingleItemGoToPropertiesCommand(object sender, RoutedEventArgs e)
        {
            await DataService.GoToSingleItemPropertiesCommand.ExecuteAsync(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private async void SingleItemCopyFileToFolder(object sender, RoutedEventArgs e)
        {
            await DataService.CopySingleFileToFolder.ExecuteAsync(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private async void SingleItemCopyFileToClipboard(object sender, RoutedEventArgs e)
        {
            await DataService.CopyFileToClipboard.ExecuteAsync(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private void SingleItemCopyFilePath(object sender, RoutedEventArgs e)
        {
            DataService.SingleItemCopyFilePath.Execute(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private void SingleItemCopyFileName(object sender, RoutedEventArgs e)
        {
            DataService.SingleItemCopyFileName.Execute(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private void SingleItemCopyAlbum(object sender, RoutedEventArgs e)
        {
            DataService.SingleItemCopyFileName.Execute(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private void SingleItemCopyArtist(object sender, RoutedEventArgs e)
        {
            DataService.SingleItemCopyArtist.Execute(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private void SingleItemCopyGenre(object sender, RoutedEventArgs e)
        {
            DataService.SingleItemCopyGenre.Execute(sender.GetDataContextObject<IMediaPlayerItemSourceProvder>());
        }

        private async void FullCollectionPlayCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.PlayCommand.ExecuteAsync(sender);
        }

        private async void FullCollectionAddToNowPlayingCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.AddToNowPlayingCommand.ExecuteAsync(sender);
        }

        private void FullCollectionEnableSelection(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DataService.EnableSelection.Execute(sender);
        }

        private async void FullCollectionSaveAsPlaylistCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.SaveAsPlaylistCommand.ExecuteAsync(sender);
        }

        private async void FullCollectionClearAllCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.ClearAllCommand.ExecuteAsync(sender);
        }

        private async void FullCollectionAddToExistingPlaylistCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.AddToExistingPlaylistCommand.ExecuteAsync(sender);
        }

        private void FullCollectionSelectAllCommandOnlyAudio(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DataService.SelectAllCommandOnlyAudio.Execute(sender);
        }

        private void FullCollectionSelectAllCommandOnlyVideo(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DataService.SelectAllCommandOnlyVideo.Execute(sender);
        }

        private void FullCollectionSelectAllCommandSelected(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DataService.SelectAllCommandOnlyVideo.Execute(sender);
        }

        private void FullCollectionUnselectAllCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DataService.UnselectAllCommand.Execute(sender);
        }

        private async void FullCollectionSaveAsPlaylistCommandOnlyMusic(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.SaveAsPlaylistCommandOnlyMusic.ExecuteAsync(sender);
        }

        private async void FullCollectionSaveAsPlaylistCommandOnlyVideo(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.SaveAsPlaylistCommandOnlyVideo.ExecuteAsync(sender);
        }

        private async void FullCollectionSaveAsPlaylistCommandOnlySelected(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.SaveAsPlaylistCommandOnlySelected.ExecuteAsync(sender);
        }

        private async void FullCollectionSaveAsPlaylistCommandOnlyunselected(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.SaveAsPlaylistCommandOnlyunselected.ExecuteAsync(sender);
        }

        private async void FullCollectionRemoveSelectedCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.RemoveSelectedCommand.ExecuteAsync(sender);

        }

        private async void FullCollectionRemoveOnlyMusicCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.RemoveOnlyMusicCommand.ExecuteAsync(sender);
        }

        private async void FullCollectionRemoveOnlyVideoCommand(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.RemoveOnlyVideoCommand.ExecuteAsync(sender);
        }

        private async void FullCollectionAddToExistingPlaylistCommandOnlyVideo(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.AddToExistingPlaylistCommand.ExecuteAsync(sender);
        }

        private async void FullCollectionAddToExistingPlaylistCommandOnlyMusic(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.AddToExistingPlaylistCommandOnlyMusic.ExecuteAsync(sender);

        }

        private async void FullCollectionAddToExistingPlaylistCommandOnlySelected(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.AddToExistingPlaylistCommandOnlySelected.ExecuteAsync(sender);

        }

        private async void FullCollectionAddToExistingPlaylistCommandOnlyUnselected(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await DataService.AddToExistingPlaylistCommandOnlyUnselected.ExecuteAsync(sender);
        }
    }

    public class FileManagerDataTemplateSelector : DataTemplateSelector
    {
        readonly DataTemplate MusicTemplate;
        readonly DataTemplate VideoPlaylistTemplate;

        public FileManagerDataTemplateSelector(DataTemplate musicTemplate, DataTemplate videoPlaylistTemplate)
        {
            MusicTemplate = musicTemplate;
            VideoPlaylistTemplate = videoPlaylistTemplate;
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var pickedItem = item as IMediaPlayerItemSourceProvder;
            if (pickedItem is { SupportsMetadata: true }) return MusicTemplate;
            return VideoPlaylistTemplate;
        }
    }
}
