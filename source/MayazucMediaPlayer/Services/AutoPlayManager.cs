using FluentResults;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using Microsoft.WindowsAPICodePack.Shell;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    public class AutoPlayManager : IAutoPlayManager
    {
        public PlaybackSequenceService PlaybackServiceInstance
        {
            get;
            private set;
        }

        public AutoPlayManager(PlaybackSequenceService playbackServiceInstance)
        {
            PlaybackServiceInstance = playbackServiceInstance;
        }

        public DirectoryInfo CurrentAutoPlayFolder
        {
            get;
            private set;
        }

        private AutoPlayStorageFileQueryResultQueue? AutoPlaybackQueue
        {
            get;
            set;
        }

        public bool IsAutoPlayAvailable
        {
            get
            {
                if (SettingsService.Instance.AutoPlayMusic || SettingsService.Instance.AutoPlayVideo)
                {
                    return CurrentAutoPlayFolder != null;
                }
                else
                {
                    return false;
                }
            }
        }

        public FileInfo LoadedFile
        {
            get;
            private set;
        }

        readonly AsyncLock _operationLock = new AsyncLock();

        public async Task LoadAutoPlayAsync(FileInfo file)
        {
            using (await _operationLock.LockAsync())
            {
                try
                {
                    if (file == null)
                    {
                        return;
                    }

                    LoadedFile = file;
                    var parentFolder = file.Directory;
                    bool aMusic = SettingsService.Instance.AutoPlayMusic, aVideo = SettingsService.Instance.AutoPlayVideo;
                    if (parentFolder != null)
                    {
                        //create 2 file queries, one for the files in the auto play queue
                        //one containing all the files in the folder
                        HashSet<string> autoPlayViewQuery = new HashSet<string>();
                        HashSet<string> flatFolderViewQuery = new HashSet<string>();

                        foreach (var f in SupportedFileFormats.MusicFormats)
                        {
                            if (aMusic)
                            {
                                autoPlayViewQuery.Add(f);
                            }
                            flatFolderViewQuery.Add(f);
                        }

                        foreach (var f in SupportedFileFormats.AllVideoFormats)
                        {
                            if (aVideo)
                            {
                                autoPlayViewQuery.Add(f);
                            }
                            flatFolderViewQuery.Add(f);
                        }

                        var autoPlayQueryResult = parentFolder.EnumerateFiles(autoPlayViewQuery);
                        var flatFolderQueryResult = parentFolder.EnumerateFiles(flatFolderViewQuery);
                       
                        //var files = await result.GetFilesAsync(3, 1);
                        //var fileIndex = await result.FindStartIndexAsync(files.FirstOrDefault());

                        AutoPlaybackQueue = await AutoPlayStorageFileQueryResultQueue.Create(autoPlayQueryResult.ToImmutableList(), flatFolderQueryResult.ToImmutableList(), aVideo, aMusic);
                    }
                    CurrentAutoPlayFolder = parentFolder;

                }
                catch
                {
                    CurrentAutoPlayFolder = null;
                    AutoPlaybackQueue = null;
                }
            }
        }

        public async Task<Result<IMediaPlayerItemSource>> GetNextFile(FileInfo file)
        {
            if (AutoPlaybackQueue != null && CurrentAutoPlayFolder != null)
            {
                //first, find the index of the file in the flat folder view
                var currentFileIndex = await AutoPlaybackQueue.GetFolderViewIndexOfFile(file);
                //if the file is not in the query, return null
                if (currentFileIndex == -1) return Result.Fail("current file index == -1");
                //compute the next index
                var nextIndex = currentFileIndex + 1;
                //if the new index is too big, return null;
                if (nextIndex == int.MaxValue) return Result.Fail("new index exceeded bounds");
                //get the next file in the folder
                var nextFile = await AutoPlaybackQueue.GetFolderViewFileAtIndex(nextIndex);
                //if next file not found, return null
                if (nextFile == null) return Result.Fail("Next file not found");
                //finally, look for the file index in the auto play query
                var indexInAutoPlayQueue = await AutoPlaybackQueue.GetAutoPlayIndexOfFile(nextFile);
                //if the file is not found in the auto play query, return null
                if (indexInAutoPlayQueue == -1) return Result.Fail("Sanity check failed");
                //finally, everything is well.
                return Result.Ok((await nextFile.GetMediaPlayerItemSources()).FirstOrDefault());
            }

            return await Task.FromResult(Result.Fail("Not enough data"));
        }

        public async Task<bool> AddNextFileToNowPlaying(FileInfo file)
        {
            using (await _operationLock.LockAsync())
            {
                var mds = await GetNextFile(file);
                if (mds.IsSuccess)
                {
                    try
                    {
                        await PlaybackServiceInstance.AddToNowPlayingAsync(new IMediaPlayerItemSource[] { mds.Value }, PlaybackSequenceService.AddToNowPlayingAtTheEnd);
                    }
                    catch { return false; }
                }

                return mds.IsSuccess;
            }
        }

        public void Reset()
        {
            using (_operationLock.Lock())
            {
                LoadedFile = null;
                CurrentAutoPlayFolder = null;
                AutoPlaybackQueue = null;
            }
        }

        public Task<bool> IsPathLoaded(string path, bool autoMusic, bool autoVideo)
        {
            var directoryPath = Path.GetDirectoryName(path);
            var stringComp = StringComparer.OrdinalIgnoreCase;
            if (CurrentAutoPlayFolder == null) return Task.FromResult(false);
            if (AutoPlaybackQueue == null) return Task.FromResult(false);
            if (autoMusic != AutoPlaybackQueue.AutoPlayMusic || autoVideo != AutoPlaybackQueue.AutoPlayVideo)
                return Task.FromResult(false);
            return Task.FromResult(stringComp.Equals(directoryPath, CurrentAutoPlayFolder.FullName));
        }

        public Task<int> CountAsync()
        {
            if (AutoPlaybackQueue != null)
                return Task.FromResult(AutoPlaybackQueue.AutoPlayCount);
            else return Task.FromResult(0);
        }

        private class AutoPlayStorageFileQueryResultQueue
        {
            public int AutoPlayCount
            {
                get;
                private set;
            }

            private IEnumerable<FileInfo> AutoPlayView
            {
                get;
                set;
            }

            private IEnumerable<FileInfo> FolderView
            {
                get;
                set;
            }

            public bool AutoPlayMusic
            {
                get;
                private set;
            }

            public bool AutoPlayVideo
            {
                get;
                private set;
            }

            public Task<int> GetAutoPlayIndexOfFile(FileInfo file)
            {
                return Task.FromResult(AutoPlayView.IndexOf(file, new FileInfoPathComparer()));
            }

            public Task<FileInfo> GetAutoPlayFileAtIndex(int index)
            {
                return Task.FromResult(AutoPlayView.ElementAtOrDefault(index));
            }

            public Task<int> GetFolderViewIndexOfFile(FileInfo file)
            {
                return Task.FromResult(FolderView.IndexOf(file, new FileInfoPathComparer()));
            }

            public Task<FileInfo> GetFolderViewFileAtIndex(int index)
            {
                return Task.FromResult(FolderView.ElementAtOrDefault(index));
            }

            private AutoPlayStorageFileQueryResultQueue(int count,
                IEnumerable<FileInfo> queryResult,
                IEnumerable<FileInfo> folderView,
                bool autoPlayMusic,
                bool autoPlayVideo)
            {
                FolderView = folderView;
                AutoPlayCount = count;
                AutoPlayView = queryResult ?? throw new ArgumentNullException(nameof(queryResult));
                AutoPlayMusic = autoPlayMusic;
                AutoPlayVideo = autoPlayVideo;
            }

            public static Task<AutoPlayStorageFileQueryResultQueue> Create(IEnumerable<FileInfo> result, IEnumerable<FileInfo> flatFolderQueryResult, bool aVideo, bool aMusic)
            {
                return Task.FromResult(new AutoPlayStorageFileQueryResultQueue(result.Count(), result, flatFolderQueryResult, aMusic, aVideo));
            }
        }
    }
}
