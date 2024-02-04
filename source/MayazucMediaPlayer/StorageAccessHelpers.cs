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

        public static FileCreationResult CreateFile(this DirectoryInfo fodler, string fileName)
        {
            var path = Path.Combine(fodler.FullName, fileName);
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

        public static IEnumerable<FileInfo> GetFiles(this DirectoryInfo folder, IEnumerable<string> extensions)
        {
            return folder.EnumerateFiles().Where(x => extensions.Contains(x.Extension.ToLowerInvariant()));
        }
    }

    public static class KnownFoldersExtensions
    {
        public static async Task<IEnumerable<FileInfo>> GetFilesAsync(LibraryFolderId knownFolderId)
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
                LinkedList<FileInfo> files = new LinkedList<FileInfo>();

                foreach (var folder in folders)
                {
                    if (folder.Path == userFolder) userFolderProcessed = true;
                    var directory = folder.ToDirectoryInfo();
                    files.AddRangeLast(directory.EnumerateFiles("*.*", SearchOption.AllDirectories));
                }
                if (!userFolderProcessed)
                {
                    var userFolderInfo = new DirectoryInfo(userFolder);
                    files.AddRangeLast(userFolderInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly));
                }


                return files;
            });
        }
    }

    public enum LibraryFolderId
    {
        MusicFolder,
        VideosFolder,
    }

    public class FileCreationResult : IDisposable
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~FileCreationResult()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
