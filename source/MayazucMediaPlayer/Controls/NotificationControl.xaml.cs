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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
