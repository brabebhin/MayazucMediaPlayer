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

            var metadataOptions = SettingsWrapper.MetadataOptionsUseDefault;

            SetComboboxSourceInitialState(metadataOptions);
            MetadataSourceCombobox.SelectionChanged += MetadataSourceCombobox_SelectionChanged;

            FolderHigerarchyTag.ItemsSource = new List<MediaMetadataTagSourceOption>()
            {
                new MediaMetadataTagSourceOption("Artist / Album / {Title} (Itunes style)", ()=>
                {
                    SettingsWrapper.MetadataGenreIndex =0;
                    SettingsWrapper.MetadataAlbumIndex =1;
                    SettingsWrapper.MetadataArtistIndex =2;

                }),
                new MediaMetadataTagSourceOption("Artist / Genre / Album / {Title}", ()=>{

                    SettingsWrapper.MetadataGenreIndex =2;
                    SettingsWrapper.MetadataAlbumIndex =1;
                    SettingsWrapper.MetadataArtistIndex =3;

                }),
                new MediaMetadataTagSourceOption("Artist / Album / Genre / {Title}", ()=>{

                    SettingsWrapper.MetadataGenreIndex =1;
                    SettingsWrapper.MetadataAlbumIndex =2;
                    SettingsWrapper.MetadataArtistIndex =3;

                }),
                new MediaMetadataTagSourceOption("Album / Artist / {Title}", ()=>{

                    SettingsWrapper.MetadataGenreIndex =0;
                    SettingsWrapper.MetadataAlbumIndex =2;
                    SettingsWrapper.MetadataArtistIndex =1;

                }),
                new MediaMetadataTagSourceOption("Genre / Artist / Album / {Title}", ()=>{

                    SettingsWrapper.MetadataGenreIndex =3;
                    SettingsWrapper.MetadataAlbumIndex =1;
                    SettingsWrapper.MetadataArtistIndex =2;
                }),
                new MediaMetadataTagSourceOption("Genre / Album / Artist / {Title}", ()=>{

                    SettingsWrapper.MetadataGenreIndex =3;
                    SettingsWrapper.MetadataAlbumIndex =2;
                    SettingsWrapper.MetadataArtistIndex =1;
                }),
            };

            FolderHigerarchyTag.SelectionChanged += FolderHierachyTag_SelectionChanged;
            FolderHigerarchyTag.SelectedIndex = SettingsWrapper.FolderHierarchyMetadataIndex;
        }

        private void FolderHierachyTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = FolderHigerarchyTag.SelectedItem as MediaMetadataTagSourceOption;
            selection?.SetterCallback();
            SettingsWrapper.FolderHierarchyMetadataIndex = FolderHigerarchyTag.SelectedIndex;
        }

        private void SetComboboxSourceInitialState(bool metadataOptions)
        {
            MetadataSourceCombobox.SelectedIndex = metadataOptions ? 0 : 1;
            VisualStateManager.GoToState(this, metadataOptions ? "ExtratTagsFromFiles" : "UseFolderStructure", false);
        }

        public void RecheckValue()
        {
            MetadataSourceCombobox.SelectionChanged -= MetadataSourceCombobox_SelectionChanged;

            var metadataOptions = SettingsWrapper.MetadataOptionsUseDefault;
            SetComboboxSourceInitialState(metadataOptions);


            MetadataSourceCombobox.SelectionChanged += MetadataSourceCombobox_SelectionChanged;

        }

        private void MetadataSourceCombobox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SettingsWrapper.MetadataOptionsUseDefault = (MetadataSourceCombobox.SelectedItem as MetadataSourceOption).ExtractFromFiles;
            CheckComboboxSourceState();
        }

        private void CheckComboboxSourceState()
        {
            var metadataOptions = SettingsWrapper.MetadataOptionsUseDefault;
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
