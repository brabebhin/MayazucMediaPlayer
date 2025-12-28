using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.LocalCache;
using Nito.AsyncEx;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Timers;
using Windows.Storage;

namespace MayazucMediaPlayer.Settings
{
    public partial class SettingsStoreService
    {
        private static AsyncLockManager lockManager = new AsyncLockManager();
        private const string SettingsContainer = "settings";

        public static T GetValueOrDefault<T>(string key, T defaultValue)
        {
            try
            {
                using (lockManager.GetLock($"{SettingsContainer}.{key}").Lock())
                {
                    var localSettings = ApplicationData.Current.LocalSettings;
                    EnsureDefault(key, defaultValue);
                    return (T)localSettings.Containers[SettingsContainer].Values[key];
                }
            }
            catch
            {
                // usually a parser error
                EnsureDefault(key, defaultValue);
                return defaultValue;
            }
        }

        public static void SetValueOrDefault<T>(string key, T defaultValue, T value)
        {
            using (lockManager.GetLock($"{SettingsContainer}.{key}").Lock())
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                EnsureDefault(key, defaultValue);
                if (!localSettings.Containers[SettingsContainer].Values.ContainsKey(key))
                {
                    localSettings.Containers[SettingsContainer].Values.Add(key, value);
                }
                else
                {
                    localSettings.Containers[SettingsContainer].Values[key] = value;
                }
            }
        }

        private static bool SettingsExists(string key)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Containers.ContainsKey(SettingsContainer))
            {
                var containerInstance = localSettings.Containers[SettingsContainer];
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

        private static void EnsureDefault<T>(string key, T defaultValue)
        {
            ApplicationDataContainer containerValue = null;
            if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(SettingsContainer))
            {
                containerValue = ApplicationData.Current.LocalSettings.CreateContainer(SettingsContainer, ApplicationDataCreateDisposition.Always);
                containerValue.Values.Add(new KeyValuePair<string, object>(key, defaultValue));
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Containers[SettingsContainer].Values.Keys.Contains(key) == false)
                {
                    ApplicationData.Current.LocalSettings.Containers[SettingsContainer].Values.Add(new KeyValuePair<string, object>(key, defaultValue));
                }
            }
        }
    }
}

