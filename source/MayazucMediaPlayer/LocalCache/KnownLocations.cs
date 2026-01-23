using MayazucMediaPlayer.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace MayazucMediaPlayer.LocalCache
{
    public static class KnownLocations
    {
        static readonly AsyncLockManager lockProvider = new AsyncLockManager();
        static readonly SimpleObjectCache resultsProvider = new SimpleObjectCache();

        public static async Task<DirectoryInfo> GetVideoColorProfilesFolder()
        {
            const string Key = "GetVideoColorProfilesFolder";
            using (lockProvider.GetLock(Key).Lock())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var videoColorProfileFolder = Directory.CreateDirectory(Path.Combine(ApplicationDataFolder.CurrentLocalFolderPath(), "videoColorProfiles"));
                    resultsProvider.TryAdd(Key, videoColorProfileFolder);
                }
                return resultsProvider.Get<DirectoryInfo>(Key);
            }
        }

        public static async Task<StorageFolder> GetSavedVideoFramesFolder()
        {
            const string Key = "GetSavedVideoFramesFolder";
            using (await lockProvider.GetLock(Key).LockAsync())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var value = resultsProvider.TryAdd(Key,
                        await KnownFolders.PicturesLibrary.CreateFolderAsync("Mayazuc video frames", CreationCollisionOption.OpenIfExists).AsTask());
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
                    var folder = new DirectoryInfo(Path.Combine(ApplicationDataFolder.CurrentLocalFolderPath(), "album art"));
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
                    var dbFolder = ApplicationDataFolder.CurrentLocalFolderPath();
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
                    var _equalizerPresetsFolderPath = Path.Combine(ApplicationDataFolder.CurrentLocalFolderPath(), "equalizerPresets");
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
                    var _equalizerConfigFolderPath = Path.Combine(ApplicationDataFolder.CurrentLocalFolderPath(), FolderNames.EqualizerConfigurations);
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
                    var folder = new DirectoryInfo(Path.Combine(ApplicationDataFolder.CurrentLocalFolderPath(), "meta"));
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
                    var file = new FileInfo(Path.Combine(ApplicationDataFolder.CurrentLocalFolderPath(), "keyboard.json"));
                    resultsProvider.TryAdd(Key, file);
                }
                return resultsProvider.Get<FileInfo>(Key);
            }
        }

        internal static async Task<FileInfo> GetInternetStreamsHistoryFile()
        {
            const string Key = "streamhistory";
            using (await lockProvider.GetLock(Key).LockAsync())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var file = new FileInfo(Path.Combine(ApplicationDataFolder.CurrentLocalFolderPath(), "streamhistory.json"));
                    if (!file.Exists) file.Create().Dispose();
                    resultsProvider.TryAdd(Key, file);
                }
                return resultsProvider.Get<FileInfo>(Key);
            }
        }

        internal static FileInfo GetDefaultSettingsFilePath()
        {
            const string Key = "GetDefaultSettingsFilePath";
            using (lockProvider.GetLock(Key).Lock())
            {
                if (!resultsProvider.HasKey(Key))
                {
                    var file = new FileInfo(Path.Combine(ApplicationDataFolder.CurrentLocalFolderPath(), "settings.json"));
                    if (!file.Exists) file.Create().Dispose();
                    resultsProvider.TryAdd(Key, file);
                }
                return resultsProvider.Get<FileInfo>(Key);
            }
        }
    }
}
