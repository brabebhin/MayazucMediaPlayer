using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.Subtitles;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResetSettingsPage : BaseUserControl, IContentSettingsItem
    {
        readonly AsyncLock albumArtClearLock = new AsyncLock();
        readonly AsyncLock subtitlesClearLock = new AsyncLock();
        readonly Func<IEnumerable<SettingsItem>> allSettingsCallback;

        public ResetSettingsPage(Func<IEnumerable<SettingsItem>> _allSettingsCallback)
        {
            InitializeComponent();
            allSettingsCallback = _allSettingsCallback;
        }

        public async void ClearAlbumArtCache(object? sender, RoutedEventArgs e)
        {
            using (await albumArtClearLock.LockAsync())
            {
                try
                {
                    (sender as Button).IsEnabled = false;
                    var files = (await LocalFolders.GetAlbumArtFolder()).EnumerateFiles();
                    foreach (var f in files)
                    {
                        f.Delete();
                    }

                    var metadataFiles = (await LocalFolders.MetadataDatabaseFolder()).EnumerateFiles();
                    foreach (var f in metadataFiles)
                    {
                        f.Delete();
                    }
                }
                finally
                {
                    (sender as Button).IsEnabled = true;

                }
            }
        }

        private async void ClearSubtitleCache(object? sender, RoutedEventArgs e)
        {
            using (await subtitlesClearLock.LockAsync())
            {
                try
                {
                    (sender as Button).IsEnabled = false;

                    await SubtitleManagementService.ClearSubtitleCacheAsync();
                }
                finally
                {
                    (sender as Button).IsEnabled = true;

                }
            }
        }

        public void RecheckValue()
        {
        }

        private async void ResetSettings(object? sender, RoutedEventArgs e)
        {
            ConfirmationDialog diag = new ConfirmationDialog("Reset settings", "This will reset most settings");
            await ContentDialogService.Instance.ShowAsync(diag);

            if (diag.Result)
            {
                SettingsDefaultValues.RestoreSettingsDefaults();
                foreach (var s in allSettingsCallback())
                {
                    s.RecheckValue();
                }
            }
        }
    }
}
