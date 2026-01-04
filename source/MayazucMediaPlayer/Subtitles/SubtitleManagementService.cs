using MayazucMediaPlayer.LocalCache;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace MayazucMediaPlayer.Subtitles
{
    public static class SubtitleManagementService
    {
        static readonly AsyncLock subtitleClearCacheAsyncLock = new AsyncLock();
        public static async Task<int> ClearSubtitleCacheAsync()
        {
            using (await subtitleClearCacheAsyncLock.LockAsync())
            {
                var subsFolder = await KnownLocations.GetCachedSubtitlesFolder();
                var files = await subsFolder.GetFilesAsync();
                int deletedFiles = 0;

                foreach (var f in files)
                {
                    try
                    {
                        await f.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        deletedFiles++;
                    }
                    catch
                    {

                    }
                }

                return deletedFiles;
            }
        }
    }
}
