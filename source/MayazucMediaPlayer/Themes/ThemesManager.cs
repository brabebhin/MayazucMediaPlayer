using MayazucMediaPlayer.Settings;
using Microsoft.UI.Xaml;

namespace MayazucMediaPlayer.Themes
{
    internal static class ThemesManager
    {
        public static ApplicationTheme? GetRequestedTheme()
        {
            var themeIndex = SettingsService.Instance.DefaultUITheme;
            switch (themeIndex)
            {
                default:
                case 0: return null;
                case 1: return ApplicationTheme.Dark;
                case 2: return ApplicationTheme.Light;
            }
        }
    }
}
