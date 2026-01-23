using MayazucMediaPlayer.MediaMetadata;
using Microsoft.Extensions.Caching.Memory;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MayazucMediaPlayer.BackgroundServices
{
    public static class FileMetadataLockManager
    {
        static readonly MemoryCache cacheLock = new MemoryCache(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromMinutes(5) });
        public static AsyncLock GetLock(string path)
        {
            return cacheLock.GetOrCreate<AsyncLock>(path, (cacheEntry) =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return new AsyncLock();
            })!;
        }
    }

    public partial class FileMetadataService : IDisposable
    {
        readonly AsyncCommandDispatcher executionQueue = new AsyncCommandDispatcher();

        public FileMetadataService()
        {
            
        }

        public void Dispose()
        {
            executionQueue.Dispose();
        }

        public async Task<EmbeddedMetadata> ProcessFileAsync(FileInfo info, bool highPriority)
        {
           var result = await executionQueue.EnqueueAsync(async () => {

                var asyncLock = FileMetadataLockManager.GetLock(info.FullName);
                using (await asyncLock.LockAsync())
                {
                    EmbeddedMetadata metadata = EmbeddedMetadataResolver.GetDefaultMetadataForFile(info.FullName);

                    try
                    {
                        var metadataFile = await EmbeddedMetadataResolver.GetMetadataDocumentForFile(info.FullName);
                        if (!metadataFile.Exists || metadataFile.CreationTimeUtc < info.LastWriteTimeUtc)
                        {
                            metadata = await EmbeddedMetadataResolver.ExtractMetadataAsync(info);
                            //process the file
                            var metadatadocumentFile = new EmbeddedMetadataSeed(metadata, info.FullName);
                            var json = System.Text.Json.JsonSerializer.Serialize(metadatadocumentFile);
                            await File.WriteAllTextAsync(metadataFile.FullName, json);
                        }
                        else
                        {
                            metadata = EmbeddedMetadataResolver.ReadMetadataDocumentForFile(metadataFile, info);
                        }
                    }
                    catch
                    {
                    }
                    return metadata;
                }

            });

            return (EmbeddedMetadata)result.Result!;
        }        
    }
}
