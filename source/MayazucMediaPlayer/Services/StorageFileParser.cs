using MayazucMediaPlayer.Playlists;
using MayazucMediaPlayer.Services.MediaSources;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    public static class StorageFileParser
    {
        /// <summary>
        /// returns a list of music data storages returned from this file
        /// </summary>
        /// <param name="file">The target file</param>
        /// <returns>0 if no data is found. 1 if the file is audio, or more than 1 if file is m3u or cue</returns>
        public static async Task<IEnumerable<IMediaPlayerItemSource>> GetMediaPlayerItemSources(this FileInfo file)
        {
            MediaPlayerItemSourceList ReturnValue = new MediaPlayerItemSourceList();

            switch (file.Extension.ToLowerInvariant())
            {
                case ".m3u8":
                case ".m3u":
                case ".wpl":
                case ".zpl":
                    var playlist = new PlaylistItem(file);
                    ReturnValue.AddRange(await playlist.GetMediaDataSourcesAsync());
                    break;
                case ".cue":

                    //CueFile cue = new CueFile();
                    //ReturnValue.AddRange(await cue.OpenCueAsync(file));

                    break;

                default:

                    var mdss = IMediaPlayerItemSourceFactory.Get(file);
                    ReturnValue.Add(mdss);

                    break;
            }

            return ReturnValue;
        }
    }
}
