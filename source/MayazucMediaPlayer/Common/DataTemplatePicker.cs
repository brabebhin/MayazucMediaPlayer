using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MayazucMediaPlayer.Common
{
    public class DataTemplatePicker : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is SettingsItem)
            {
                return (item as SettingsItem).Template;
            }

            return base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is SettingsItem)
            {
                return (item as SettingsItem).Template;
            }
            return base.SelectTemplateCore(item, container);
        }
    }
}
