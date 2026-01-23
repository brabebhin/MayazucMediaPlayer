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

    public partial class PriorityBufferBlock<T> : IDisposable
    {
        readonly PriorityQueue<T, int> priorityQueue = new System.Collections.Generic.PriorityQueue<T, int>();
        private ManualResetEventSlim queueHasItems = new ManualResetEventSlim(false);

        public PriorityBlockResult<T> OutputAvailable(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (priorityQueue.Count == 0)
                    {
                        queueHasItems.Reset();
                    }
                    queueHasItems.Wait(token);
                    var hadItem = priorityQueue.TryDequeue(out var lowestPriority, out var priority);
                    return new PriorityBlockResult<T>(hadItem, lowestPriority);
                }
                catch
                {

                }
            }
            return new PriorityBlockResult<T>(false, default(T));
        }

        public void Post(T item, bool isHighPriority)
        {
            priorityQueue.Enqueue(item, isHighPriority ? 0 : 1);
            queueHasItems.Set();
        }

        public void Dispose()
        {
            priorityQueue.Clear();
            queueHasItems.Set();
            queueHasItems.Dispose();
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

    public partial class FileMetadataService : IDisposable
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
                    var outputAvailableResult = backlog.OutputAvailable(cancelSignal.Token);
                    if (outputAvailableResult.OutputAvailable)
                    {
                        var fileToProcess = outputAvailableResult.Result;
                        var asyncLock = FileMetadataLockManager.GetLock(fileToProcess.File.FullName);
                        using (await asyncLock.LockAsync())
                        {
                            EmbeddedMetadata metadata = EmbeddedMetadataResolver.GetDefaultMetadataForFile(fileToProcess.File.FullName);

                            try
                            {
                                var metadataFile = await EmbeddedMetadataResolver.GetMetadataDocumentForFile(fileToProcess.File.FullName);
                                if (!metadataFile.Exists || metadataFile.CreationTimeUtc < fileToProcess.File.LastWriteTimeUtc)
                                {
                                    metadata = await EmbeddedMetadataResolver.ExtractMetadataAsync(fileToProcess.File);
                                    //process the file
                                    var metadatadocumentFile = new EmbeddedMetadataSeed(metadata, fileToProcess.File.FullName);
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

        public void Dispose()
        {
            backlog?.Dispose();
            cancelSignal.Cancel();
            while (!ProcessingTask.IsCompleted)
            {
                Thread.Yield();
            }
            cancelSignal.Dispose();
        }

        public async Task<EmbeddedMetadata> ProcessFileAsync(FileInfo info, bool highPriority)
        {
            var job = new FileInfoProcessingJob(info);
            backlog.Post(job, highPriority);
            return await job.AsyncTask;
        }

        private class FileInfoProcessingJob
        {
            readonly TaskCompletionSource<EmbeddedMetadata> taskCompletionSource = new TaskCompletionSource<EmbeddedMetadata>();

            public FileInfo File { get; }

            public Task<EmbeddedMetadata> AsyncTask
            {
                get { return taskCompletionSource.Task; }
            }

            public void SetResult(EmbeddedMetadata result)
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
