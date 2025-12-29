using MayazucMediaPlayer.LocalCache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Windows.Storage;

namespace MayazucMediaPlayer.Settings
{
    public partial class SettingsStoreService
    {
        private static AsyncLockManager lockManager = new AsyncLockManager();
        private const string SettingsKey = "settings";

        Dictionary<string, string> settings = new Dictionary<string, string>();

        public SettingsStoreService()
        {
            LoadSettings();
        }

        public void SaveSettings()
        {
            var json = JsonSerializer.Serialize(settings, MayazucJsonSerializerContext.Default.DictionaryStringString);
            File.WriteAllText(LocalFolders.GetDefaultSettingsFilePath().FullName, json);
        }

        public void LoadSettings()
        {
            try
            {
                var json = File.ReadAllText(LocalFolders.GetDefaultSettingsFilePath().FullName);
                settings = JsonSerializer.Deserialize(json, MayazucJsonSerializerContext.Default.DictionaryStringString);
            }
            catch
            {
                settings = new Dictionary<string, string>();
            }
        }

        public int IntGetValueOrDefault(string key, int defaultValue)
        {
            using (lockManager.GetLock($"{SettingsKey}.{key}").LockWithTimeout())
            {
                if (!settings.ContainsKey(key))
                {
                    settings.Add(key, defaultValue.ToString());
                }
                return int.Parse(settings[key]);
            }
        }

        public long LongGetValueOrDefault(string key, long defaultValue)
        {
            using (lockManager.GetLock($"{SettingsKey}.{key}").LockWithTimeout())
            {
                if (!settings.ContainsKey(key))
                {
                    settings.Add(key, defaultValue.ToString());
                }
                return long.Parse(settings[key]);
            }
        }

        public string StringGetValueOrDefault(string key, string defaultValue)
        {
            using (lockManager.GetLock($"{SettingsKey}.{key}").LockWithTimeout())
            {
                if (!settings.ContainsKey(key))
                {
                    settings.Add(key, defaultValue.ToString());
                }
                return settings[key];
            }
        }

        public TimeSpan TimeSpanGetValueOrDefault(string key, TimeSpan defaultValue)
        {
            using (lockManager.GetLock($"{SettingsKey}.{key}").LockWithTimeout())
            {
                if (!settings.ContainsKey(key))
                {
                    settings.Add(key, defaultValue.ToString());
                }
                return TimeSpan.Parse(settings[key]);
            }
        }

        public bool BoolGetValueOrDefault(string key, bool defaultValue)
        {
            using (lockManager.GetLock($"{SettingsKey}.{key}").LockWithTimeout())
            {
                if (!settings.ContainsKey(key))
                {
                    settings.Add(key, defaultValue.ToString());
                }
                return bool.Parse(settings[key]);
            }
        }

        public double DoubleGetValueOrDefault(string key, double defaultValue)
        {
            using (lockManager.GetLock($"{SettingsKey}.{key}").LockWithTimeout())
            {
                if (!settings.ContainsKey(key))
                {
                    settings.Add(key, defaultValue.ToString());
                }
                return double.Parse(settings[key]);
            }
        }

        public float DoubleGetValueOrDefault(string key, float defaultValue)
        {
            using (lockManager.GetLock($"{SettingsKey}.{key}").LockWithTimeout())
            {
                if (!settings.ContainsKey(key))
                {
                    settings.Add(key, defaultValue.ToString());
                }
                return float.Parse(settings[key]);
            }
        }

        public void SetValueOrDefault2(string key, object defaultValue, object value)
        {
            using (lockManager.GetLock($"{SettingsKey}.{key}").LockWithTimeout())
            {
                settings[key] = value.ToString();
            }
        }
    }
}

