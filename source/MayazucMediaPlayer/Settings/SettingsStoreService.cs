using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.LocalCache;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace MayazucMediaPlayer.Settings
{
    public partial class SettingsStoreService
    {
        private AsyncLock lockObject = new AsyncLock();
        ApplicationSettingsContainer Containers { get; set; } = new ApplicationSettingsContainer();
        Timer autoSaveTimer = new Timer();

        public SettingsStoreService(string path = null)
        {
            LoadSettings(path);
            autoSaveTimer.Elapsed += AutoSaveTimer_Elapsed;
            autoSaveTimer.AutoReset = false;
            autoSaveTimer.Stop();
        }

        private void AutoSaveTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            SaveSettings();
        }

        public T GetValueOrDefault<T>(string container, string key, T defaultValue)
        {
            using (lockObject.Lock())
            {
                var localSettings = Containers;
                EnsureDefault(container, key, defaultValue);
                return (T)localSettings[container][key];
            }
        }

        public void SetValueOrDefault<T>(string container, string key, T defaultValue, T value)
        {
            using (lockObject.Lock())
            {
                var localSettings = Containers;
                EnsureDefault(container, key, defaultValue);
                if (!localSettings[container].ContainsKey(key))
                {
                    localSettings[container].Add(key, value);
                }
                else
                {
                    localSettings[container][key] = value;
                }

                StartAutoSaveTimer();
            }
        }

        private void StartAutoSaveTimer()
        {
            autoSaveTimer.Stop();
            autoSaveTimer.Interval = Random.Shared.Next(3000, 5000);
            autoSaveTimer.Start();
        }

        public bool SettingsExists(string container, string key)
        {
            var localSettings = Containers;
            if (localSettings.ContainsKey(container))
            {
                var containerInstance = localSettings[container];
                return containerInstance.ContainsKey(key);
            }
            else
            {
                return false;
            }
        }

        public void EnsureDefault<T>(string container, string key, T defaultValue)
        {
            if (!Containers.ContainsKey(container))
            {
                var valueContainer = new ApplicationSettingsContainerValue
                {
                    { key, defaultValue }
                };
                Containers.Add(container, valueContainer);
            }
            else
            {
                var valueContainer = Containers[container];
                valueContainer.TryAdd(key, defaultValue);
            }
        }

        public partial class ApplicationSettingsContainer : Dictionary<string, ApplicationSettingsContainerValue>
        {

        }

        public partial class ApplicationSettingsContainerValue : Dictionary<string, dynamic>
        {

        }

        public void SaveSettings(string path = null)
        {
            using (lockObject.Lock())
            {
                if (path == null)
                {
                    path = LocalFolders.GetDefaultSettingsFilePath().FullName;
                }

                var data = JsonConvert.SerializeObject(Containers);
                File.WriteAllText(path, data);
            }
        }

        private void LoadSettings(string path = null)
        {
            if (path == null)
            {
                path = LocalFolders.GetDefaultSettingsFilePath().FullName;
            }

            var data = File.ReadAllText(path);
            Containers = JsonExtensions.PreferInt32DeserializeObject<ApplicationSettingsContainer>(data);
            if (Containers == null)
            {
                Containers = new ApplicationSettingsContainer();
            }
        }
    }
}
