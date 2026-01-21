using Microsoft.UI;
using Microsoft.UI.Windowing;
using System;
using WinRT.Interop;

namespace MayazucMediaPlayer
{
    public class WindowBase : WinUIEx.WindowEx
    {
        protected AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }
    }
}