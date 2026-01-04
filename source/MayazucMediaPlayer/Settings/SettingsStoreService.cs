using MayazucMediaPlayer.LocalCache;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;

namespace MayazucMediaPlayer.Settings
{
    public partial class SettingsStoreService
    {
        ConcurrentDictionary<string, string> settings = new ConcurrentDictionary<string, string>();

        public SettingsStoreService()
        {
            LoadSettings();
        }

        public void SaveSettings()
        {
            var json = JsonSerializer.Serialize(settings, MayazucJsonSerializerContext.Default.ConcurrentDictionaryStringString);
            File.WriteAllText(LocalFolders.GetDefaultSettingsFilePath().FullName, json);
        }

        public void LoadSettings()
        {
            try
            {
                var json = File.ReadAllText(LocalFolders.GetDefaultSettingsFilePath().FullName);
                settings = JsonSerializer.Deserialize(json, MayazucJsonSerializerContext.Default.ConcurrentDictionaryStringString);
            }
            catch
            {
                settings = new ConcurrentDictionary<string, string>();
            }
        }

        public int IntGetValueOrDefault(string key, int defaultValue)
        {
            return int.Parse(settings.GetOrAdd(key, (k) => defaultValue.ToString()));
        }

        public long LongGetValueOrDefault(string key, long defaultValue)
        {
            return long.Parse(settings.GetOrAdd(key, (k) => defaultValue.ToString()));
        }

        public string StringGetValueOrDefault(string key, string defaultValue)
        {
            return settings.GetOrAdd(key, (k) => defaultValue.ToString());
        }

        public TimeSpan TimeSpanGetValueOrDefault(string key, TimeSpan defaultValue)
        {
            return TimeSpan.Parse(settings.GetOrAdd(key, (k) => defaultValue.ToString()));
        }

        public bool BoolGetValueOrDefault(string key, bool defaultValue)
        {
            return bool.Parse(settings.GetOrAdd(key, (k) => defaultValue.ToString()));
        }

        public double DoubleGetValueOrDefault(string key, double defaultValue)
        {
            return double.Parse(settings.GetOrAdd(key, (k) => defaultValue.ToString()));
        }

        public float FloatGetValueOrDefault(string key, float defaultValue)
        {
            return float.Parse(settings.GetOrAdd(key, (k) => defaultValue.ToString()));
        }

        public void SetValueOrDefault2(string key, object defaultValue, object value)
        {
            settings.AddOrUpdate(key, value.ToString(), (k, v) => value.ToString());
        }
    }
}

