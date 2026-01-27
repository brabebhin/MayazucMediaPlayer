using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.Help;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MayazucMediaPlayer.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : BasePage
    {
        public override string Title => "Settings";

        public ObservableCollection<SettingsItemGroup> AllSettingsItems { get; private set; } = new ObservableCollection<SettingsItemGroup>();

        public SettingsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;

            List<SettingsItemGroup> groups = new List<SettingsItemGroup>
            {
                ContextualSettings.GetUISettings(),
                ContextualSettings.GetPlaybackControlSettings(),
                ContextualSettings.GetSubtitleSettings(),

                new SettingsItemGroup("\uED10", "Reset ") { new ContentSettingsItem(new ResetSettingsPage(allSettings)) },

                new SettingsItemGroup("\uE7F5", "Audio + Video", ContextualSettings.GetVideoSettings(), new SettingsItem[]{ new ContentSettingsItem(new WindowsAudioSettingsShortcutControl()) })
            };

            groups = groups.OrderBy(x => x.GroupName).ToList();
            groups.Add(new SettingsItemGroup("\uE7EF", "About") { new ContentSettingsItem(new AboutControl()) });
            AllSettingsItems.AddRange(groups);
            NavigationViewRoot.MenuItemsSource = AllSettingsItems;
            NavigationViewRoot.SelectedItem = AllSettingsItems.FirstOrDefault();
        }

        IEnumerable<SettingsItem> allSettings()
        {
            List<SettingsItem> items = new List<SettingsItem>();
            foreach (var g in AllSettingsItems)
            {
                items.AddRange(g);
            }

            return items;
        }


        protected override void FreeMemory()
        {
            //CacheManagementPageInstance.DisposePage();

            foreach (var s in allSettings())
            {
                s.Dispose();
            }
            base.FreeMemory();
        }

        private async void reset_defaultSettings(object? sender, RoutedEventArgs e)
        {
            ConfirmationDialog diag = new ConfirmationDialog("Reset settings", "This will reset most settings");
            await ContentDialogService.Instance.ShowAsync(diag);

            if (diag.Result)
            {
                SettingsService.Instance.ResetToDefaultValues();
                foreach (var s in allSettings())
                {
                    s.RecheckValue();
                }
            }
        }
    }
}
