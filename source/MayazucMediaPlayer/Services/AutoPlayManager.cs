using FluentResults;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    public class AutoPlayManager(PlaybackSequenceService playbackServiceInstance) : IAutoPlayManager
    {
        public PlaybackSequenceService PlaybackServiceInstance
        {
            get;
            private set;
        } = playbackServiceInstance;

        private DirectoryInfo _currentAutoPlayFolder;

        private AutoPlayStorageFileQueryResultQueue? _autoPlaybackQueue;

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

                        _autoPlaybackQueue = await AutoPlayStorageFileQueryResultQueue.Create(autoPlayQueryResult.ToImmutableList(), flatFolderQueryResult.ToImmutableList(), aVideo, aMusic);
                    }
                    _currentAutoPlayFolder = parentFolder;

                }
                catch
                {
                    _currentAutoPlayFolder = null;
                    _autoPlaybackQueue = null;
                }
            }
        }

        public async Task<Result<IMediaPlayerItemSource>> GetNextFile(FileInfo file)
        {
            if (_autoPlaybackQueue != null && _currentAutoPlayFolder != null)
            {
                //first, find the index of the file in the flat folder view
                var currentFileIndex = await _autoPlaybackQueue.GetFolderViewIndexOfFile(file);
                //if the file is not in the query, return null
                if (currentFileIndex == -1) return Result.Fail("current file index == -1");
                //compute the next index
                var nextIndex = currentFileIndex + 1;
                //if the new index is too big, return null;
                if (nextIndex == int.MaxValue) return Result.Fail("new index exceeded bounds");
                //get the next file in the folder
                var nextFile = await _autoPlaybackQueue.GetFolderViewFileAtIndex(nextIndex);
                //if next file not found, return null
                if (nextFile == null) return Result.Fail("Next file not found");
                //finally, look for the file index in the auto play query
                var indexInAutoPlayQueue = await _autoPlaybackQueue.GetAutoPlayIndexOfFile(nextFile);
                //if the file is not found in the auto play query, return null
                if (indexInAutoPlayQueue == -1) return Result.Fail("Sanity check failed");
                //finally, everything is well.
                return Result.Ok((await nextFile.GetMediaPlayerItemSources()).FirstOrDefault());
            }

            return await Task.FromResult(Result.Fail("Not enough data"));
        }

        public Task<int> CountAsync()
        {
            if (_autoPlaybackQueue != null)
                return Task.FromResult(_autoPlaybackQueue.AutoPlayCount);
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
