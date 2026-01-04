using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class SettingsItemsListView : ListView
    {
        public SettingsItemsListView()
        {
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.SelectionMode = ListViewSelectionMode.None;
            this.ItemTemplateSelector = (DataTemplateSelector)App.Current.Resources["SettingsItemsTemplateSelector"];
        }
    }
}
