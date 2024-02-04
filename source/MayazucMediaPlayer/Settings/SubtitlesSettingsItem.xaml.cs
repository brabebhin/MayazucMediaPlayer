using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Settings
{
    public sealed partial class SubtitlesSettingsItem : UserControl, IContentSettingsItem
    {
        public SubtitlesSettingsItem()
        {
            this.InitializeComponent();
        }

        public void RecheckValue()
        {
            foreach (var settingsItems in DefaultSettingsListView.Items.Cast<SettingsItem>())
            {
                settingsItems.RecheckValue();
            }
        }

        public object SubtitlesItemsSource
        {
            get => this.GetValue(SubtitlesItemsSourceProperty);
            set => SetValue(SubtitlesItemsSourceProperty, value);
        }

        public static DependencyProperty SubtitlesItemsSourceProperty = DependencyProperty.Register("SubtitlesItemsSource", typeof(object), typeof(SubtitlesSettingsItem), new PropertyMetadata(null, SettingsItemsSourcePropertyChanged));

        public object OpenSubtitlesItemsSource
        {
            get => this.GetValue(OpenSubtitlesItemsSourceProperty);
            set => SetValue(OpenSubtitlesItemsSourceProperty, value);
        }

        public static DependencyProperty OpenSubtitlesItemsSourceProperty = DependencyProperty.Register("OpenSubtitlesItemsSource", typeof(object), typeof(SubtitlesSettingsItem), new PropertyMetadata(null, SOpenSubtitlesettingsItemsSourcePropertyChanged));


        private static void SettingsItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as SubtitlesSettingsItem;
            obj.DefaultSettingsListView.ItemsSource = e.NewValue;
        }

        private static void SOpenSubtitlesettingsItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as SubtitlesSettingsItem;
            obj.OpenSubtitlesSettingsListView.ItemsSource = e.NewValue;
        }
    }
}
