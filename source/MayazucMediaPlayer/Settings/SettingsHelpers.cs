using Nito.AsyncEx;
using System.Collections.Generic;
using Windows.Storage;

namespace MayazucMediaPlayer.Settings
{
    public static class SettingsHelpers
    {
        public static AsyncLock lockObject = new AsyncLock();

        public static T GetValueOrDefault<T>(string container, string key, T defaultValue)
        {
            using (lockObject.Lock())
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                EnsureDefault(container, key, defaultValue);
                return (T)localSettings.Containers[container].Values[key];
            }
        }

        public static void SetValueOrDefault<T>(string container, string key, T defaultValue, T value)
        {
            using (lockObject.Lock())
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                EnsureDefault(container, key, defaultValue);
                if (!localSettings.Containers[container].Values.ContainsKey(key))
                {
                    localSettings.Containers[container].Values.Add(key, value);
                }
                else
                {
                    localSettings.Containers[container].Values[key] = value;
                }
            }
        }

        public static bool SettingsExists(string container, string key)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Containers.ContainsKey(container))
            {
                var containerInstance = localSettings.Containers[container];
                if (containerInstance.Values.ContainsKey(key))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static void EnsureDefault<T>(string container, string key, T defaultValue)
        {

            ApplicationDataContainer containerValue = null;
            if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(container))
            {
                containerValue = ApplicationData.Current.LocalSettings.CreateContainer(container, ApplicationDataCreateDisposition.Always);
                containerValue.Values.Add(new KeyValuePair<string, object>(key, defaultValue));
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Containers[container].Values.Keys.Contains(key) == false)
                {
                    ApplicationData.Current.LocalSettings.Containers[container].Values.Add(new KeyValuePair<string, object>(key, defaultValue));
                }
            }
        }
    }
}
