using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace MayazucMediaPlayer.LocalCache
{
    public class AsyncLockManager
    {
        private readonly ConcurrentDictionary<string, AsyncLock> locks = new ConcurrentDictionary<string, AsyncLock>();

        public AsyncLock GetLock(string key)
        {
            return locks.GetOrAdd(key, (s) =>
            {
                return new AsyncLock();
            });
        }

        public void Clear()
        {
            locks.Clear();
        }
    }

    public class SimpleObjectCache
    {
        private readonly ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();

        public T TryAdd<T>(string key, T value)
        {
            cache.TryAdd(key, value);
            return value;
        }

        public T Get<T>(string key)
        {
            return (T)cache[key];
        }

        public bool HasKey(string key)
        {
            return cache.ContainsKey(key);
        }

        public void Clear()
        {
            cache.Clear();
        }
    }


    public static class LocalFolders
    {
        static readonly AsyncLockManager lockProvider = new AsyncLockManager();
        static readonly SimpleObjectCache resultsProvider = new SimpleObjectCache();

        public static async Task<StorageFolder> GetVideoColorProfilesFolder()
        {
            const string Key = "GetVideoColorProfilesFolder";
            using (await lockProvider.GetLock(Key).LockAsync())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var value = resultsProvider.TryAdd(Key,
                        await ApplicationData.Current.LocalFolder.CreateFolderAsync("VideoColorProfiles", CreationCollisionOption.OpenIfExists).AsTask());
                }
                return resultsProvider.Get<StorageFolder>(Key);
            }
        }

        public static async Task<StorageFolder> GetSavedVideoFramesFolder()
        {
            const string Key = "GetSavedVideoFramesFolder";
            using (await lockProvider.GetLock("Key").LockAsync())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var value = resultsProvider.TryAdd(Key,
                        await KnownFolders.PicturesLibrary.CreateFolderAsync("MC Video frames", CreationCollisionOption.OpenIfExists).AsTask());
                }
                return resultsProvider.Get<StorageFolder>(Key);
            }
        }


        public static async Task<StorageFolder> GetCachedSubtitlesFolder()
        {
            const string Key = "GetCachedSubtitlesFolder";
            using (await lockProvider.GetLock(Key).LockAsync())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var value = resultsProvider.TryAdd(Key,
                        await KnownFolders.VideosLibrary.CreateFolderAsync("subtitles", CreationCollisionOption.OpenIfExists).AsTask());
                }
                return resultsProvider.Get<StorageFolder>(Key);
            }
        }

        public static DirectoryInfo GetPlaylistsFolderAsync()
        {
            const string Key = "GetPlaylistsFolderAsync";
            using (lockProvider.GetLock(Key).Lock())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var directory = new DirectoryInfo(KnownFolders.Playlists.Path);
                    if (!directory.Exists) directory.Create();
                    var value = resultsProvider.TryAdd(Key,
                       directory);
                }
                return resultsProvider.Get<DirectoryInfo>(Key);
            }
        }

        public static async Task<DirectoryInfo> GetAlbumArtFolder()
        {
            const string Key = "GetAlbumArtFolder";
            using (await lockProvider.GetLock(Key).LockAsync())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var folder = new DirectoryInfo(Path.Combine(ApplicationData.Current.LocalFolder.Path, "album art"));
                    if (!folder.Exists) folder.Create();
                    resultsProvider.TryAdd(Key, folder);
                }
                return resultsProvider.Get<DirectoryInfo>(Key);
            }
        }

        public static string GetNowPlayingJsonFile()
        {
            const string Key = "GetNowPlayingJsonFile";
            using (lockProvider.GetLock(Key).Lock())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var dbFolder = ApplicationData.Current.LocalFolder.Path;
                    var filePath = "nowplaying.json";
                    var finalPath = Path.Combine(dbFolder, filePath);
                    FileInfo info = new FileInfo(finalPath);
                    if (!info.Exists) info.Create().Dispose();
                    resultsProvider.TryAdd(Key, finalPath);
                }
                return resultsProvider.Get<string>(Key);
            }
        }

        internal static async Task<string> GetEqualizerPresetsFolderAsync()
        {
            const string Key = "GetNowPlayingJsonFile";
            using (await lockProvider.GetLock(Key).LockAsync())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var _equalizerPresetsFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "equalizerPresets");
                    if (!Directory.Exists(_equalizerPresetsFolderPath))
                        Directory.CreateDirectory(_equalizerPresetsFolderPath);
                    resultsProvider.TryAdd(Key, _equalizerPresetsFolderPath);
                }
                return resultsProvider.Get<string>(Key);
            }
        }

        internal static string EqualizerConfigurationsPayloadFolderPath()
        {
            const string Key = "EqualizerConfigurationsPayloadFolderPath";
            using (lockProvider.GetLock(Key).Lock())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var _equalizerConfigFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, FolderNames.EqualizerConfigurations);
                    resultsProvider.TryAdd(Key, _equalizerConfigFolderPath);
                }
                return resultsProvider.Get<string>(Key);
            }
        }

        public static async Task<DirectoryInfo> MetadataDatabaseFolder()
        {
            const string Key = "MetadataDatabaseFolder";
            using (await lockProvider.GetLock(Key).LockAsync())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var folder = new DirectoryInfo(Path.Combine(ApplicationData.Current.LocalFolder.Path, "meta"));
                    if (!folder.Exists) folder.Create();
                    resultsProvider.TryAdd(Key, folder);
                }
                return resultsProvider.Get<DirectoryInfo>(Key);
            }
        }

        internal static async Task<FileInfo> GetKeyboardAcceleratorsFile()
        {
            const string Key = "KeyboardAcceleratorsFile";
            using (await lockProvider.GetLock(Key).LockAsync())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var file = new FileInfo(Path.Combine(ApplicationData.Current.LocalFolder.Path, "keyboard.json"));
                    resultsProvider.TryAdd(Key, file);
                }
                return resultsProvider.Get<FileInfo>(Key);
            }
        }

        internal static async Task<FileInfo> GetInternetStreamsFavoritesFile()
        {
            const string Key = "DirbleFavStations";
            using (await lockProvider.GetLock(Key).LockAsync())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var file = new FileInfo(Path.Combine(ApplicationData.Current.LocalFolder.Path, "DirbleFavStations.json"));
                    if (!file.Exists) file.Create().Dispose();
                    resultsProvider.TryAdd(Key, file);
                }
                return resultsProvider.Get<FileInfo>(Key);
            }
        }
    }
}
