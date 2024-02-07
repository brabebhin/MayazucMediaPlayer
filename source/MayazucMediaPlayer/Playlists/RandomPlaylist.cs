using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace MayazucMediaPlayer.Playlists
{
    public static class RandomPlaylist
    {
        public static async Task<IReadOnlyList<FileInfo>> GetRandomPlaylist()
        {
            return await Task.Run<IReadOnlyList<FileInfo>>(async () =>
            {
                Random r = new Random();

                var files = await KnownFoldersExtensions.GetFilesAsync(LibraryFolderId.MusicFolder, SupportedFileFormats.MusicFormats);
                return files.ToList().Randomize().Take(PlaylistSize).ToList();
            });
        }

        private const int PlaylistSize = 50;
    }
}
