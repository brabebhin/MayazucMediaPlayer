using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MayazucMediaPlayer
{
    public static class StorageAccessHelpers
    {
        public static FileInfo ToFileInfo(this StorageFile file)
        {
            return new FileInfo(file.Path);
        }

        public static DirectoryInfo ToDirectoryInfo(this StorageFolder folder)
        {
            return new DirectoryInfo(folder.Path);
        }

        public static Task<IRandomAccessStream> OpenReadAsync(this FileInfo file)
        {
            return Task.FromResult(file.OpenRead().AsRandomAccessStream());
        }

        public static FileCreationResult CreateFileStream(this DirectoryInfo folder, string fileName)
        {
            var path = Path.Combine(folder.FullName, fileName);
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                return new FileCreationResult(file, file.Create());
            }
            else
            {
                return new FileCreationResult(file, file.Open(FileMode.Open));
            }
        }

        public static string GetFilePath(this DirectoryInfo folder, string fileName)
        {
            return Path.Combine(folder.FullName, fileName);
        }

        public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo folder, IEnumerable<string> extensions)
        {
            return folder.EnumerateFiles().Where(x => extensions.Contains(x.Extension.ToLowerInvariant()));
        }
    }

    public static class KnownFoldersExtensions
    {
        public static async Task<IEnumerable<FileInfo>> GetFilesAsync(LibraryFolderId knownFolderId, IEnumerable<string> extensions)
        {
            return await Task.Run(async () =>
            {
                var knownFolder = knownFolderId switch
                {
                    LibraryFolderId.MusicFolder => KnownFolders.MusicLibrary,
                    LibraryFolderId.VideosFolder => KnownFolders.VideosLibrary,
                };
                var userFolder = Environment.GetFolderPath(knownFolderId == LibraryFolderId.MusicFolder ? Environment.SpecialFolder.MyMusic : Environment.SpecialFolder.MyVideos);

                bool userFolderProcessed = false;

                var folders = await knownFolder.GetFoldersAsync();
                IEnumerable<FileInfo> files = Enumerable.Empty<FileInfo>();

                foreach (var folder in folders)
                {
                    if (folder.Path == userFolder) userFolderProcessed = true;
                    var directory = folder.ToDirectoryInfo();
                    files = files.Union(directory.EnumerateFiles("*.*", SearchOption.AllDirectories));
                }
                if (!userFolderProcessed)
                {
                    var userFolderInfo = new DirectoryInfo(userFolder);
                    files = files.Union(userFolderInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly));
                }


                return files.Where(x => extensions.Contains(x.Extension));
            });
        }
    }

    public enum LibraryFolderId
    {
        MusicFolder,
        VideosFolder,
    }

    public partial class FileCreationResult : IDisposable
    {
        private bool disposedValue;

        public FileInfo FileInformation { get; private set; }

        public Stream FileStream { get; private set; }

        public FileCreationResult(FileInfo fileInformation, Stream fileStream)
        {
            FileInformation = fileInformation;
            FileStream = fileStream;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                FileStream.Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
