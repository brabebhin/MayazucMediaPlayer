using FFmpegInteropX;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MayazucMediaPlayer.MediaMetadata
{
    public static class FFmpegMetadataExtensions
    {
        public static string GetArtist(this FFmpegMediaSource ffmpegMediaSource)
        {
            string[] tagKeys = new string[] { "performer", "artist", "album_artist" };
            return ExtractMetadataInternal(ffmpegMediaSource, tagKeys);
        }

        public static string GetTitle(this FFmpegMediaSource ffmpegMediaSource)
        {
            string[] tagKeys = new string[] { "title" };
            return ExtractMetadataInternal(ffmpegMediaSource, tagKeys);
        }

        private static string ExtractMetadataInternal(FFmpegMediaSource ffmpegMediaSource, string[] albumTagKeys)
        {
            HashSet<string> resultsBuilder = new HashSet<string>();
            foreach (string key in albumTagKeys)
            {
                if (ffmpegMediaSource.MetadataTags.TryGetValue(key, out var tag))
                {
                    var joinedTag = string.Join(' ', tag);
                    resultsBuilder.Add(joinedTag);
                }
            }

            return string.Join(" - ", resultsBuilder);
        }

        public static string GetAlbum(this FFmpegMediaSource ffmpegMediaSource)
        {
            string[] tagKeys = new string[] { "album" };
            return ExtractMetadataInternal(ffmpegMediaSource, tagKeys);
        }

        public static string GetGenre(this FFmpegMediaSource ffmpegMediaSource)
        {
            string[] tagKeys = new string[] { "genre" };
            return ExtractMetadataInternal(ffmpegMediaSource, tagKeys);
        }

        public static ReadOnlyDictionary<string, string> GetMetadataDictionary(this FFmpegMediaSource fFmpegMediaSource)
        {
            return new ReadOnlyDictionary<string, string>(fFmpegMediaSource.MetadataTags.ToDictionary(x => x.Key, x => string.Join(" ", x.Value)));
        }

    }
}

