using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using PlaylistsNET.Content;
using PlaylistsNET.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Playlists
{
    public enum PlayListItemType
    {
        Playlist,
        External,
    }

    /// <summary>
    /// An abstraction over m3u8, pls, wpl and zpl playlists
    /// </summary>
    public partial class PlaylistItem : ObservableObject
    {
        public const string FileExtension = ".wpl";

        public string Title
        {
            get
            {
                return BackStore.Name;
            }
        }

        public string BackstorePath
        {
            get
            {
                return BackStore.FullName;
            }
        }

        public char GroupPath
        {
            get
            {
                var c = Title[0];
                if (char.IsLetter(c))
                {
                    return char.ToUpperInvariant(c);
                }
                else
                {
                    return '#';
                }
            }
        }

        private FileInfo BackStore
        {
            get;
            set;
        }

        public PlaylistItem(FileInfo file)
        {
            if (!file.Exists)
            {
                file.Create().Dispose();
            }

            BackStore = new FileInfo(file.FullName);
        }

        public async Task Add(IEnumerable<IMediaPlayerItemSource> files)
        {
            var existing = await GetMediaDataSourcesAsync();
            var result = new List<IMediaPlayerItemSource>(existing);
            result.AddRange(files);

            await SavePlaylistAsync(result);
        }

        public Task DeletePlaylistAsync()
        {
            if (BackStore.Exists)
                BackStore.Delete();
            return Task.CompletedTask;
        }

        public virtual async Task<string> GetCoverSource()
        {
            var mds = await GetMediaDataSourcesAsync();
            var first = mds.FirstOrDefault();
            if (first != null)
            {
                var metadata = await first.GetMetadataAsync();
                if (!metadata.HasSavedThumbnailFile())
                {
                    return AssetsPaths.PlaylistCoverPlaceholder;
                }
                else return metadata.SavedThumbnailFile;
            }
            return AssetsPaths.PlaylistCoverPlaceholder;
        }

        public Task<IList<IMediaPlayerItemSource>> GetMediaDataSourcesAsync()
        {
            var result = new List<IMediaPlayerItemSource>();

            using var stream = BackStore.OpenRead();
            var parser = PlaylistParserFactory.GetPlaylistParser(BackStore.Extension);
            IBasePlaylist playlist = parser.GetFromStream(stream);

            foreach (var item in playlist.GetTracksPaths())
            {
                var src = IMediaPlayerItemSourceFactory.Get(item);
                if (src != null)
                    result.Add(src);
            }

            return Task.FromResult<IList<IMediaPlayerItemSource>>(result);
        }

        public Task<bool> RenameAsync(string newName)
        {
            var newPath = Path.Combine(KnownLocations.GetPlaylistsFolderAsync().FullName, newName + FileExtension);
            BackStore.MoveTo(newPath);
            BackStore = new FileInfo(newPath);

            NotifyPropertyChanged(nameof(Title));
            NotifyPropertyChanged(nameof(BackstorePath));
            NotifyPropertyChanged(nameof(GroupPath));

            return Task.FromResult(true);
        }

        public async Task SavePlaylistAsync(IEnumerable<IMediaPlayerItemSource> datas)
        {
            IBasePlaylist playlist = null;
            switch (BackStore.Extension)
            {
                case ".wpl":
                    playlist = await SaveWpl(datas);
                    break;
                case ".zpl":
                    playlist = await SaveZpl(datas);
                    break;
                case ".m3u8":
                    playlist = await Savem3u8(datas, true);
                    break;
                case ".m3u":
                    playlist = await Savem3u8(datas, false);
                    break;
            }

            var text = PlaylistToTextHelper.ToText(playlist);
            await File.WriteAllTextAsync(BackstorePath, text);
        }

        private async Task<IBasePlaylist> SaveWpl(IEnumerable<IMediaPlayerItemSource> datas)
        {
            var wplContent = new WplPlaylist();
            wplContent.FileName = BackStore.FullName;
            wplContent.Title = Title;

            foreach (var data in datas)
            {
                var metadata = await data.GetMetadataAsync();

                wplContent.PlaylistEntries.Add(new WplPlaylistEntry()
                {
                    AlbumArtist = metadata.Artist,
                    AlbumTitle = metadata.Album,
                    Path = data.MediaPath,
                    TrackArtist = metadata.Artist,
                    TrackTitle = metadata.Title
                });
            }

            return wplContent;
        }

        private async Task<IBasePlaylist> SaveZpl(IEnumerable<IMediaPlayerItemSource> datas)
        {
            var zplContent = new ZplPlaylist();
            zplContent.FileName = BackStore.FullName;
            zplContent.Title = Title;
            foreach (var data in datas)
            {
                var metadata = await data.GetMetadataAsync();
                zplContent.PlaylistEntries.Add(new ZplPlaylistEntry()
                {
                    AlbumArtist = metadata.Artist,
                    AlbumTitle = metadata.Album,
                    Path = data.MediaPath,
                    TrackArtist = metadata.Artist,
                    TrackTitle = metadata.Title
                });
            }

            return zplContent;
        }

        private async Task<IBasePlaylist> Savem3u8(IEnumerable<IMediaPlayerItemSource> datas, bool extended)
        {
            var m3uContent = new M3uPlaylist();
            m3uContent.IsExtended = extended;
            m3uContent.FileName = BackStore.FullName;
            foreach (var data in datas)
            {
                var metadata = await data.GetMetadataAsync();

                m3uContent.PlaylistEntries.Add(new M3uPlaylistEntry()
                {
                    AlbumArtist = metadata.Artist,
                    Path = data.MediaPath,
                    Title = metadata.Title,
                    Album = metadata.Album,
                });
            }

            return m3uContent;
        }

        public static FileInfo GetDefaultPathForInternalPlaylistAsync(string playListName)
        {
            var directory = KnownLocations.GetPlaylistsFolderAsync();
            var finalPath = Path.Combine(directory.FullName, playListName + FileExtension);
            return new FileInfo(finalPath);
        }

        public async Task SaveEmptyPlaylistAsync()
        {
            await SavePlaylistAsync(new List<IMediaPlayerItemSource>());
        }

        public bool Writeable => true;

        public override string ToString()
        {
            return Title;
        }
    }
}
