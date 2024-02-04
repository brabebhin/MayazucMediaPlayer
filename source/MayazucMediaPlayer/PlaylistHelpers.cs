using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.Playlists;
using MayazucMediaPlayer.Services.MediaSources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MayazucMediaPlayer
{
    public static class PlaylistHelpers
    {
        public static async Task<AddToPlaylistResult> AddItemsToPlaylistAsync(IEnumerable<IMediaPlayerItemSource> files, IEnumerable<PlaylistItem> itemsToPickFrom)
        {
            try
            {
                PlaylistPicker picker = new PlaylistPicker();
                var playlist = await picker.PickPlaylistAsync(itemsToPickFrom);
                if (playlist != null)
                {
                    await playlist.Add(files);
                    return new AddToPlaylistResult(true, playlist);
                }
            }
            catch (Exception e)
            {
                return new AddToPlaylistResult(false, null, e);
            }
            return new AddToPlaylistResult(false, null);
        }
    }


    public class AddToPlaylistResult
    {
        public bool OK
        {
            get;
            private set;
        }

        public PlaylistItem Playlist
        {
            get;
            private set;
        }

        public Exception Error
        {
            get;
            private set;
        }

        public AddToPlaylistResult(bool oK, PlaylistItem playlist, Exception error) : this(oK, playlist)
        {
            Error = error;
        }

        public AddToPlaylistResult(bool oK, PlaylistItem playlist)
        {
            OK = oK;
            Playlist = playlist;
        }
    }
}
