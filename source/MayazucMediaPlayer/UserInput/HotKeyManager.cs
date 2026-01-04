using MayazucMediaPlayer.LocalCache;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace MayazucMediaPlayer.UserInput
{
    public partial class HotKeyManager : IDisposable
    {
        private partial class HotKeyCollection : Dictionary<HotKeySettings, HotKeyId>
        {
            public void Add(MayazucHotKey item)
            {
                base.Add(item.Settings, item.Id);
            }

            public void AddRange(IEnumerable<MayazucHotKey> items)
            {
                foreach (var i in items)
                {
                    Add(i);
                }
            }
        }

        private class WinAPI
        {
            public delegate void HookProc(int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

            [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern bool UnhookWindowsHookEx(int idHook);

            [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern uint GetCurrentThreadId();

            [DllImport("user32.dll")]
            public static extern int MapVirtualKey(uint uCode, uint uMapType);

            [DllImport("user32.dll")]
            public static extern int ToUnicode(uint virtualKeyCode, uint scanCode,
           byte[] keyboardState,
           [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
    StringBuilder receivingBuffer,
           int bufferSize, uint flags);

            public const int WH_MIN = (-1);
            public const int WH_MSGFILTER = (-1);
            public const int WH_JOURNALRECORD = 0;
            public const int WH_JOURNALPLAYBACK = 1;
            public const int WH_KEYBOARD = 2;
            public const int WH_GETMESSAGE = 3;
            public const int WH_CALLWNDPROC = 4;
            public const int WH_CBT = 5;
            public const int WH_SYSMSGFILTER = 6;
            public const int WH_MOUSE = 7;
            public const int WH_HARDWARE = 8;
            public const int WH_DEBUG = 9;
            public const int WH_SHELL = 10;
            public const int WH_FOREGROUNDIDLE = 11;
            public const int WH_CALLWNDPROCRET = 12;
            public const int WH_KEYBOARD_LL = 13;
            public const int WH_MOUSE_LL = 14;
            public const int WH_MAX = 14;
            public const int WH_MINHOOK = WH_MIN;
            public const int WH_MAXHOOK = WH_MAX;

            public const int KF_EXTENDED = 0x0100;
            public const int KF_DLGMODE = 0x0800;
            public const int KF_MENUMODE = 0x1000;
            public const int KF_ALTDOWN = 0x2000;
            public const int KF_REPEAT = 0x4000;
            public const int KF_UP = 0x8000;

            internal static int HIWORD(IntPtr wParam)
            {
                return (int)((wParam.ToInt64() >> 16) & 0xffff);
            }

            internal static int LOWORD(IntPtr wParam)
            {
                return (int)(wParam.ToInt64() & 0xffff);
            }

        }

        private int m_hHook = 0;
        private WinAPI.HookProc? m_HookProcedure;

        readonly HotKeyCollection accelerators = new HotKeyCollection();
        public event EventHandler<HotKeyId>? AcceleratorInvoked;

        UIElement? host;

        public HotKeyManager()
        {
        }


        public async Task SaveAcceleratorsAsync()
        {
            var savedFile = await LocalFolders.GetKeyboardAcceleratorsFile();
        }

        public async Task RegisterAcceleratorsAsync(UIElement uiHost)
        {
            await GetAcceleratorsAsync();

            host = uiHost;
            m_HookProcedure = new WinAPI.HookProc(HookProcedure);
            m_hHook = WinAPI.SetWindowsHookEx(WinAPI.WH_KEYBOARD, m_HookProcedure, (IntPtr)0, (int)WinAPI.GetCurrentThreadId());
        }

        private void HookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return;
            var focusedElement = FocusManager.GetFocusedElement(host.XamlRoot);

            if (focusedElement == null || focusedElement is TextBox) return;

            var keyFlags = WinAPI.HIWORD(lParam);
            var isKeyReleased = (keyFlags & WinAPI.KF_UP) == WinAPI.KF_UP;

            if (!isKeyReleased) return;

            bool shift = (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down));
            bool ctrl = (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down));

            var virtualKey = GetCharFromKey((uint)wParam, shift, false);

            string modifier = "none";
            if (shift)
            {
                modifier = "shift";
            }
            else if (ctrl)
            {
                modifier = "control";
            }


            if (virtualKey == "") return;
            var key = new HotKeySettings(virtualKey, modifier);
            if (accelerators.TryGetValue(key, out var accelerator))
            {
                AcceleratorInvoked?.Invoke(this, accelerator);
            }

            Debug.WriteLine($"{isKeyReleased} {virtualKey}");
        }

        static string GetCharFromKey(uint key, bool shift, bool altGr)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
                keyboardState[(int)VirtualKey.Shift] = 0xff;
            if (altGr)
            {
                keyboardState[(int)VirtualKey.Control] = 0xff;
                keyboardState[(int)VirtualKey.Menu] = 0xff;
            }
            WinAPI.ToUnicode(key, 0, keyboardState, buf, 256, 0);

            if (buf.Length == 0) return "";
            return buf[0].ToString();
        }

        private async Task<HotKeyCollection> GetAcceleratorsAsync()
        {
            List<MayazucHotKey> accels = new List<MayazucHotKey>();
            var savedFile = await LocalFolders.GetKeyboardAcceleratorsFile();
            string json = string.Empty;
            if (savedFile.Exists)
                json = System.IO.File.ReadAllText(savedFile.FullName);
            if (!string.IsNullOrWhiteSpace(json))
            {

            }
            else
            {
                //load defaults
                accels.AddRange(GetDefaultAccelerators());
            }

            accelerators.AddRange(accels);
            return accelerators;
        }

        private ReadOnlyCollection<MayazucHotKey> GetDefaultAccelerators()
        {
            List<MayazucHotKey> defaults = new List<MayazucHotKey>();

            defaults.Add(new MayazucHotKey(new HotKeySettings("P", "none"), HotKeyId.PlayPause));
            defaults.Add(new MayazucHotKey(new HotKeySettings("B", "none"), HotKeyId.JumpBack));
            defaults.Add(new MayazucHotKey(new HotKeySettings("M", "none"), HotKeyId.JumpForward));
            defaults.Add(new MayazucHotKey(new HotKeySettings("M", "shift"), HotKeyId.SkipNext));
            defaults.Add(new MayazucHotKey(new HotKeySettings("B", "shift"), HotKeyId.SkipPrevious));
            defaults.Add(new MayazucHotKey(new HotKeySettings("\u001b", "none"), HotKeyId.ExitFullScreen)); //ESC
            defaults.Add(new MayazucHotKey(new HotKeySettings("s", "none"), HotKeyId.SaveVideoFrame));

            return defaults.AsReadOnly();
        }

        public void Dispose()
        {
            WinAPI.UnhookWindowsHookEx(WinAPI.WH_KEYBOARD);
        }
    }
}
