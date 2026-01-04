using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewSavedFramesFolderControl : Page
    {
        public ViewSavedFramesFolderControl()
        {
            InitializeComponent();
        }

        private async void OpenSavedFramesFolder_click(object? sender, RoutedEventArgs e)
        {
            var folder = await LocalCache.KnownLocations.GetSavedVideoFramesFolder();
            await Launcher.LaunchFolderAsync(folder);
        }
    }
}
