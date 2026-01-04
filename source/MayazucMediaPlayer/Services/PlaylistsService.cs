
using CommunityToolkit.WinUI;
using MayazucMediaPlayer.Playlists;
using MayazucMediaPlayer.Services.MediaSources;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    public partial class PlaylistsService : ServiceBase
    {
        public event EventHandler<string> PlaylistItemChanged;
        public event EventHandler<string> PlaylistItemDeleted;

        public ObservableCollection<PlaylistItem> Playlists
        {
            get; private set;
        } = new ObservableCollection<PlaylistItem>();


        public PlaylistsService(DispatcherQueue dispatcher) : base(dispatcher)
        {
        }

        public IEnumerable<PlaylistItem> LoadPlaylistsAsync()
        {
            var folder = LocalCache.KnownLocations.GetPlaylistsFolderAsync();
            var files = folder.EnumerateFiles().Where(x => SupportedFileFormats.IsPlaylistFile(x.FullName));
            Playlists.AddRange(LoadPlaylistsFromFiles(files));
            return Playlists;
        }

        public async Task<bool> RenamePlaylistAsync(PlaylistItem item, string result)
        {
            var oldPath = item.BackstorePath;
            var t = await item.RenameAsync(result);

            PlaylistItemChanged?.Invoke(item, oldPath);
            return t;
        }


        private static IEnumerable<PlaylistItem> LoadPlaylistsFromFiles(
            IEnumerable<FileInfo> files)
        {
            var playlists = new List<PlaylistItem>();
            foreach (var file in files)
            {
                playlists.Add(new PlaylistItem(file));
            }

            return playlists;
        }

        public async Task AddToPlaylist(PlaylistItem item, IEnumerable<IMediaPlayerItemSource> dataz)
        {
            await item.Add(dataz);
            PlaylistItemChanged?.Invoke(item, item.BackstorePath);
        }

        /// <summary>
        /// Adds a new playlist item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="initialItems"></param>
        public async Task<PlaylistItem> AddPlaylist(string itemName, IEnumerable<IMediaPlayerItemSource> initialItems)
        {
            var item = new PlaylistItem(PlaylistItem.GetDefaultPathForInternalPlaylistAsync(itemName));
            await item.SavePlaylistAsync(initialItems);
            await Dispatcher.EnqueueAsync(() =>
            {
                if (!Playlists.Any(x => x.BackstorePath == item.BackstorePath))
                {
                    Playlists.Add(item);
                }

                PlaylistItemChanged?.Invoke(item, item.BackstorePath);

            });

            return item;
        }

        public async Task<PlaylistItem> AddPlaylist(string itemName)
        {
            return await AddPlaylist(itemName, Enumerable.Empty<IMediaPlayerItemSource>());
        }

        public async Task RemovePlaylist(PlaylistItem item)
        {
            await Dispatcher.EnqueueAsync(async () =>
            {
                Playlists.Remove(item);
                await item.DeletePlaylistAsync();
                PlaylistItemDeleted?.Invoke(item, item.BackstorePath);
            });
        }
    }
}
