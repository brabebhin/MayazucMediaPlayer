using MayazucMediaPlayer.MediaMetadata;
using Microsoft.Extensions.Caching.Memory;
using Nito.AsyncEx;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MayazucMediaPlayer
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

    public class PriorityBufferBlock<T>
    {
        readonly BufferBlock<T> lowPriority = new BufferBlock<T>();
        readonly BufferBlock<T> highPriority = new BufferBlock<T>();

        public async Task<PriorityBlockResult<T>> OutputAvailableAsync(CancellationToken token)
        {
            var hpAvailable = highPriority.OutputAvailableAsync(token);
            var lpAvailable = lowPriority.OutputAvailableAsync(token);
            var outputAvailableTask = (await Task.WhenAny(hpAvailable, lpAvailable));
            if (outputAvailableTask == hpAvailable)
            {
                return new PriorityBlockResult<T>(true, highPriority.Receive());
            }
            if (outputAvailableTask == lpAvailable)
            {
                return new PriorityBlockResult<T>(true, lowPriority.Receive());
            }

            return new PriorityBlockResult<T>(false, default(T));
        }

        public void Post(T item, bool isHighPriority)
        {
            if (isHighPriority) highPriority.Post(item);
            else lowPriority.Post(item);
        }

        public long Count()
        {
            return lowPriority.Count + highPriority.Count;
        }
    }

    public class PriorityBlockResult<T>
    {
        public bool OutputAvailable { get; private set; }

        public T Result { get; private set; }

        public PriorityBlockResult(bool outputAvailable, T result)
        {
            OutputAvailable = outputAvailable;
            Result = result;
        }
    }

    public class FileMetadataService : IAsyncDisposable
    {
        readonly PriorityBufferBlock<FileInfoProcessingJob> backlog = new PriorityBufferBlock<FileInfoProcessingJob>();
        readonly CancellationTokenSource cancelSignal = new CancellationTokenSource();
        readonly Task ProcessingTask;
        public FileMetadataService()
        {
            ProcessingTask = Task.Run(async () =>
            {
                while (!cancelSignal.IsCancellationRequested)
                {
                    var outputAvailableResult = await backlog.OutputAvailableAsync(cancelSignal.Token);
                    if (outputAvailableResult.OutputAvailable)
                    {
                        var fileToProcess = outputAvailableResult.Result;
                        var asyncLock = FileMetadataLockManager.GetLock(fileToProcess.File.FullName);
                        using (await asyncLock.LockAsync())
                        {
                            EmbeddedMetadataResult metadata = EmbeddedMetadataResolver.GetDefaultMetadataForFile(fileToProcess.File.FullName);

                            try
                            {
                                var metadataFile = await EmbeddedMetadataResolver.GetMetadataDocumentForFile(fileToProcess.File.FullName);
                                if (!metadataFile.Exists || metadataFile.CreationTimeUtc < fileToProcess.File.LastWriteTimeUtc)
                                {
                                    metadata = await EmbeddedMetadataResolver.ExtractMetadataAsync(fileToProcess.File);
                                    //process the file
                                    var metadatadocumentFile = new EmbeddedMetadataResultFile(metadata, fileToProcess.File.FullName);
                                    var json = System.Text.Json.JsonSerializer.Serialize(metadatadocumentFile);
                                    await File.WriteAllTextAsync(metadataFile.FullName, json);
                                }
                                else
                                {
                                    metadata = EmbeddedMetadataResolver.ReadMetadataDocumentForFile(metadataFile, fileToProcess.File);
                                }

                                fileToProcess.SetResult(metadata);
                            }
                            catch
                            {
                                fileToProcess.SetResult(metadata);
                            }
                        }
                    }
                }
            });
        }

        public async ValueTask DisposeAsync()
        {
            cancelSignal.Cancel();
            await ProcessingTask;
        }

        public async Task<EmbeddedMetadataResult> ProcessFileAsync(FileInfo info, bool highPriority)
        {
            var job = new FileInfoProcessingJob(info);
            backlog.Post(job, highPriority);
            return await job.AsyncTask;
        }

        private class FileInfoProcessingJob
        {
            readonly TaskCompletionSource<EmbeddedMetadataResult> taskCompletionSource = new TaskCompletionSource<EmbeddedMetadataResult>();

            public FileInfo File { get; }

            public Task<EmbeddedMetadataResult> AsyncTask
            {
                get { return taskCompletionSource.Task; }
            }

            public void SetResult(EmbeddedMetadataResult result)
            {
                taskCompletionSource.SetResult(result);
            }

            public FileInfoProcessingJob(FileInfo file)
            {
                File = file;
            }
        }
    }
}
