using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;

namespace MayazucMediaPlayer.Playlists
{
    public static class RandomPlaylist
    {
        public static async Task<IReadOnlyList<StorageFile>> GetRandomPlaylist()
        {
            return await Task.Run<IReadOnlyList<StorageFile>>(async () =>
            {
                List<StorageFolder> musicFolders = new List<StorageFolder>();
                musicFolders.AddRange(await KnownFolders.MusicLibrary.GetFoldersAsync());

                List<StorageFile> playlistFiles = new List<StorageFile>();

                Random r = new Random();
                while (playlistFiles.Count < PlaylistSize && musicFolders.Count > 0)
                {
                    var chosenFolder = musicFolders[r.Next(0, musicFolders.Count)];
                    IList<StorageFile> files = (await chosenFolder.GetFilesAsync(CommonFileQuery.OrderByName)).ToList();
                    files = files.Where(x => SupportedFileFormats.AllAudioFormats.Contains(x.FileType.ToLowerInvariant())).ToList();
                    HashSet<int> indices = new HashSet<int>();

                    var filesCount = PlaylistSize - playlistFiles.Count;
                    if (files.Count < filesCount)
                    {
                        playlistFiles.AddRange(files);
                    }
                    else
                    {
                        while (indices.Count < filesCount)
                        {
                            indices.Add(r.Next(0, files.Count));
                        }
                        foreach (int i in indices)
                        {
                            playlistFiles.Add(files[i]);
                        }
                    }
                }
                return playlistFiles.ToList().AsReadOnly();
            });
        }

        private const int PlaylistSize = 50;
    }
}
