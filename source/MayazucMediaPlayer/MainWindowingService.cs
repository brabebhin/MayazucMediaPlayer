using MayazucMediaPlayer.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer
{
    public class MainWindowingService
    {
        private MainWindow HostWindow
        {
            get;
            set;
        }

        public MainWindowingService(MainWindow HostWindow)
        {
            this.HostWindow = HostWindow;
        }

        public WindowId Id
        {
            get
            {
                return HostWindow.AppWindow.Id;
            }
        }

        public async Task RequestAlwaysOnTopOverlayMode(bool shouldOverlayOnTop)
        {
            await HostWindow.SetWindowOnTopOverlayMode(shouldOverlayOnTop);
        }

        public async Task RequestFullScreenMode(bool shouldFullScreen)
        {
            await HostWindow.GoToFullScreenMode(shouldFullScreen);
            MediaPlayerElementFullScreenModeChanged?.Invoke(this, shouldFullScreen);
        }

        public bool IsAlwaysOnTopWindowOverlayMode()
        {
            return HostWindow.IsInCompactOverlayMode();
        }

        public bool IsInFullScreenMode()
        {
            return HostWindow.IsInFullScreenMode();
        }

        public static MainWindowingService Instance { get; private set; }

        public static void InitializeInstanceAsync(MainWindow window)
        {
            Instance = new MainWindowingService(window);
        }

        public Task<ContentDialogServiceResult> ShowContentDialog(FrameworkElement element)
        {
            return HostWindow.ShowDialogAsync(element);
        }

        public event EventHandler<bool> MediaPlayerElementFullScreenModeChanged;
    }
}
