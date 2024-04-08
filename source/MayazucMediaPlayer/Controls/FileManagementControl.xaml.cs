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
        public FileManagementControl()
        {
            InitializeComponent();
            DataTemplateSelectorInstance = new FileManagerDataTemplateSelector(Resources["MusicItemDataTemplate"] as DataTemplate, Resources["VideoPlaylistItemDataTemplate"] as DataTemplate);
        }

        public FileManagerDataTemplateSelector DataTemplateSelectorInstance { get; private set; }

        public FilePickerUiService DataModel
        {
            get; private set;
        }

        public void PerformCleanUp()
        {
            if (DataModel != null)
            {
                DataModel.GetSelectedItemsRequest -= DataModel_GetSelectedItemsRequest;

                DataModel.SetSelectedItems -= DataModel_SetSelectedItems;
            }
        }

        private async void DataModel_SetSelectedItems(object? sender, IEnumerable<ItemIndexRange> e)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                if (COntentPresenter.SelectedItems.Any())
                {
                    COntentPresenter.DeselectRange(new ItemIndexRange(0, (uint)DataModel.Items.Count));
                }
                if (e != null)
                {
                    foreach (var o in e)
                    {
                        COntentPresenter.SelectRange(o);
                    }
                }

            });
        }

        private void DataModel_GetSelectedItemsRequest(object? sender, List<object> e)
        {
            e.AddRange(COntentPresenter.SelectedItems);
        }

        private void DataModel_ClearSelectionRequest(object? sender, EventArgs e)
        {
            COntentPresenter.SelectedItems.Clear();
        }

        public Task LoadStateInternal(FilePickerUiService model)
        {
            if (DataModel == null)
            {
                DataModel = model;
                DataContext = DataModel;
                mcSearchBar.Filter = (x) => { return ((IMediaPlayerItemSourceProvder)x).Path; };

                DataModel.GetSelectedItemsRequest += DataModel_GetSelectedItemsRequest;

                DataModel.SetSelectedItems += DataModel_SetSelectedItems;
            }
            return Task.CompletedTask;
        }
        private void SelectionChangedForListView(object? sender, SelectionChangedEventArgs e)
        {
            if (COntentPresenter.SelectedItems.Count == 0 || COntentPresenter.SelectionMode == ListViewSelectionMode.None)
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
            if (DataModel != null)
                await DataModel.PlayFile(file);
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
