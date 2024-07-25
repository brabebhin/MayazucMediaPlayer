using MayazucMediaPlayer.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Settings
{
    public sealed partial class AdvancedTrackMetadataSettingsControl : BaseUserControl, IContentSettingsItem
    {
        public AdvancedTrackMetadataSettingsControl()
        {
            InitializeComponent();
            MetadataSourceCombobox.ItemsSource = new List<MetadataSourceOption>()
            {
                new MetadataSourceOption(true, "Extract from files"),
                new MetadataSourceOption(false, "Use folder hierarchy"),
            };

            var metadataOptions = SettingsService.Instance.MetadataOptionsUseDefault;

            SetComboboxSourceInitialState(metadataOptions);
            MetadataSourceCombobox.SelectionChanged += MetadataSourceCombobox_SelectionChanged;

            FolderHigerarchyTag.ItemsSource = new List<MediaMetadataTagSourceOption>()
            {
                new MediaMetadataTagSourceOption("Artist / Album / {Title} (Itunes style)", ()=>
                {
                    SettingsService.Instance.MetadataGenreIndex =0;
                    SettingsService.Instance.MetadataAlbumIndex =1;
                    SettingsService.Instance.MetadataArtistIndex =2;

                }),
                new MediaMetadataTagSourceOption("Artist / Genre / Album / {Title}", ()=>{

                    SettingsService.Instance.MetadataGenreIndex =2;
                    SettingsService.Instance.MetadataAlbumIndex =1;
                    SettingsService.Instance.MetadataArtistIndex =3;

                }),
                new MediaMetadataTagSourceOption("Artist / Album / Genre / {Title}", ()=>{

                    SettingsService.Instance.MetadataGenreIndex =1;
                    SettingsService.Instance.MetadataAlbumIndex =2;
                    SettingsService.Instance.MetadataArtistIndex =3;

                }),
                new MediaMetadataTagSourceOption("Album / Artist / {Title}", ()=>{

                    SettingsService.Instance.MetadataGenreIndex =0;
                    SettingsService.Instance.MetadataAlbumIndex =2;
                    SettingsService.Instance.MetadataArtistIndex =1;

                }),
                new MediaMetadataTagSourceOption("Genre / Artist / Album / {Title}", ()=>{

                    SettingsService.Instance.MetadataGenreIndex =3;
                    SettingsService.Instance.MetadataAlbumIndex =1;
                    SettingsService.Instance.MetadataArtistIndex =2;
                }),
                new MediaMetadataTagSourceOption("Genre / Album / Artist / {Title}", ()=>{

                    SettingsService.Instance.MetadataGenreIndex =3;
                    SettingsService.Instance.MetadataAlbumIndex =2;
                    SettingsService.Instance.MetadataArtistIndex =1;
                }),
            };

            FolderHigerarchyTag.SelectionChanged += FolderHierachyTag_SelectionChanged;
            FolderHigerarchyTag.SelectedIndex = SettingsService.Instance.FolderHierarchyMetadataIndex;
        }

        private void FolderHierachyTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = FolderHigerarchyTag.SelectedItem as MediaMetadataTagSourceOption;
            selection?.SetterCallback();
            SettingsService.Instance.FolderHierarchyMetadataIndex = FolderHigerarchyTag.SelectedIndex;
        }

        private void SetComboboxSourceInitialState(bool metadataOptions)
        {
            MetadataSourceCombobox.SelectedIndex = metadataOptions ? 0 : 1;
            VisualStateManager.GoToState(this, metadataOptions ? "ExtratTagsFromFiles" : "UseFolderStructure", false);
        }

        public void RecheckValue()
        {
            MetadataSourceCombobox.SelectionChanged -= MetadataSourceCombobox_SelectionChanged;

            var metadataOptions = SettingsService.Instance.MetadataOptionsUseDefault;
            SetComboboxSourceInitialState(metadataOptions);


            MetadataSourceCombobox.SelectionChanged += MetadataSourceCombobox_SelectionChanged;

        }

        private void MetadataSourceCombobox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SettingsService.Instance.MetadataOptionsUseDefault = (MetadataSourceCombobox.SelectedItem as MetadataSourceOption).ExtractFromFiles;
            CheckComboboxSourceState();
        }

        private void CheckComboboxSourceState()
        {
            var metadataOptions = SettingsService.Instance.MetadataOptionsUseDefault;
            VisualStateManager.GoToState(this, metadataOptions ? "ExtratTagsFromFiles" : "UseFolderStructure", false);
        }
    }

    public class MetadataSourceOption
    {
        public bool ExtractFromFiles { get; private set; }

        public string Description { get; private set; }

        public MetadataSourceOption(bool extractFromFiles, string description)
        {
            ExtractFromFiles = extractFromFiles;
            Description = description;
        }

        public override string ToString()
        {
            return Description;
        }
    }

    public class MediaMetadataTagSourceOption
    {
        public string Description { get; private set; }

        public Action SetterCallback { get; private set; }

        public override string ToString()
        {
            return Description;
        }

        public MediaMetadataTagSourceOption(string description, Action setterCallback)
        {
            Description = description;
            SetterCallback = setterCallback;
        }
    }
}
