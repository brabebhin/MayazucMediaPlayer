using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class NotificationControl : UserControl
    {
        public NotificationControl()
        {
            this.InitializeComponent();
        }

        public async Task ShowNotificationAsync(string title, string message, TimeSpan duration)
        {
            notificationRoot.Title = title;
            notificationRoot.Message = message;
            notificationRoot.IsOpen = true;
            await Task.Delay(duration);
            notificationRoot.IsOpen = false;
        }
    }
}
