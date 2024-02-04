using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace MayazucMediaPlayer
{
    public static class SupportedFileFormats
    {
        /// <summary>
        /// All supported file formats, audio, video, playlists
        /// </summary>
        static SupportedFileFormats()
        {
            AllSupportedFileFormats = AllSupportedFormats.ToImmutableHashSet();
            AllMusicAndPlaylistFormats = allMusicFormats.ToImmutableHashSet();
            AllAudioFormats = allCueableFormats.ToImmutableHashSet();
            AllWriteableFormats = writeableFormats.ToImmutableHashSet();
            SupportedVideoFiles = _SupportedVideoFiles.ToImmutableHashSet();
            SupportedStreamingUriSchemes = StreamUris.ToImmutableHashSet();
        }

        public static ImmutableHashSet<string> AllSupportedFileFormats
        {
            get; private set;
        }

        /// <summary>
        /// All supported audio formats, including playlist types
        /// </summary>
        public static ImmutableHashSet<string> AllMusicAndPlaylistFormats
        {
            get;
            private set;
        }

        /// <summary>
        /// All supported audio files.
        /// </summary>
        public static ImmutableHashSet<string> AllAudioFormats
        {
            get;
            private set;
        }

        /// <summary>
        /// All files which can be fed into the tag editor
        /// </summary>
        public static ImmutableHashSet<string> AllWriteableFormats
        {
            get;
            private set;
        }

        /// <summary>
        /// Supported uri schemes
        /// </summary>
        public static ImmutableHashSet<string> SupportedStreamingUriSchemes
        {
            get;
            private set;
        }

        static readonly string[] writeableFormats = new string[]
        {
            ".wma", ".mp3", ".ogg",
            ".flac", ".m4a", ".aac", ".adt", ".adts", ".ac3", ".ec3", ".wav", ".oga", ".wv",   ".mpc", ".tta",".m4a",
            ".m4r", ".m4b", ".m4p", ".3g2", ".asf", ".aif", ".aiff", ".afc", ".aifc",
            ".ape", ".alac", ".awb"
        };

        static readonly string[] allMusicFormats = new string[] { ".wma", ".mp3", ".ogg", ".opus",
            ".flac", ".m4a", ".aac", ".adt", ".adts", ".ac3", ".ec3", ".wav", ".oga", ".wv",   ".mpc", ".tta",".m4a",
        ".m4r", ".m4b", ".m4p", ".3g2", ".asf", ".aif", ".aiff", ".afc", ".aifc",
        ".ape",     ".alac", ".m3u", ".awb", ".wpl", ".zpl", ".m3u8"};

        static readonly string[] allCueableFormats = new string[] { ".wma", ".mp3", ".ogg", ".opus",
            ".flac", ".m4a", ".aac", ".adt", ".adts", ".ac3", ".ec3", ".wav", ".oga", ".wv",   ".mpc", ".tta",".m4a",
        ".m4r", ".m4b", ".m4p", ".3g2", ".asf", ".aif", ".aiff", ".afc", ".aifc",
        ".ape",     ".alac", ".awb" };

        static readonly string[] SystemSupportedMusicFormats = new string[] { ".wma", ".mp3", ".m4a", ".aac", ".adt", ".adts", ".ac3", ".ec3" };
        static readonly string[] _SupportedVideoFiles = new string[] { ".mp4", ".avi", ".wmv", ".h264", ".mkv", ".vob", ".h265", ".ts", ".glv", ".m4v", ".av1", ".webm" };

        static readonly string[] AllSupportedFormats = new string[] { ".wma", ".mp3", ".ogg", ".opus",
            ".flac", ".m4a", ".aac", ".adt", ".adts", ".ac3", ".ec3", ".wav", ".oga", ".wv",   ".mpc", ".tta",".m4a",
        ".m4r", ".m4b", ".m4p", ".3g2", ".asf", ".aif", ".aiff", ".afc", ".aifc",
        ".ape",     ".alac", ".m3u", ".awb", ".mp4", ".avi", ".wmv", ".h264", ".mkv", ".wpl", ".zpl", ".vob", ".h265", ".ts", ".glv", ".m4v", ".m3u8", ".av1", ".webm"  };

        static readonly string[] SubtitleFormats = new string[] { ".srt", ".sub", ".ttml", ".vtt" };

        static readonly string[] PlaylistFormats = new string[] { ".m3u", ".wpl", ".zpl", ".m3u8" };

        static readonly string[] PictureFormats = new string[] { ".png", ".jpg", ".jpeg", ".bmp" };

        static readonly string[] StreamUris = new string[] { "http", "rtsp", "rtmp" };

        public static ReadOnlyCollection<string> SupportedAlbumArtPictureFormats
        {
            get
            {
                return new ReadOnlyCollection<string>(PictureFormats);
            }
        }

        /// <summary>
        /// supported subtitle formats
        /// </summary>
        public static IEnumerable<string> SupportedSubtitleFormats
        {
            get
            {
                return SubtitleFormats.AsEnumerable();
            }
        }

        /// <summary>
        /// All supported video formats
        /// </summary>
        public static ImmutableHashSet<string> SupportedVideoFiles
        {
            get;
            private set;
        }

        public static bool IsSupportedMedia(string path)
        {
            return AllSupportedFileFormats.Contains(Path.GetExtension(path));
        }

        public static MediaFileType GetMediaFileType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            switch (ext)
            {
                case ".m3u": return MediaFileType.M3U;
                case ".cue": return MediaFileType.Cue;
                case ".srt":
                case ".sub":
                case ".ttml":
                case ".vtt": return MediaFileType.Subtitle;
                default: return MediaFileType.Media;
            }
        }

        public static bool IsVideoFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return SupportedVideoFiles.Contains(ext);
        }

        public static bool IsAudioFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return AllAudioFormats.Contains(ext);
        }

        public static bool IsPlaylistFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return PlaylistFormats.Contains(ext);
        }

        public static bool IsSupportedExtension(this IStorageItem item)
        {
            var pathLower = item.Path.ToLowerInvariant();
            return AllSupportedFormats.Any(x => pathLower.EndsWith(x));
        }
    }

    public enum SupportedFileTypesConfiguration
    {
        AllFiles,
        AllWriteableFiles,
        AllMusicFiles,
        CueableFormats
    }

    [Flags]
    public enum MediaFileType
    {
        Media,
        M3U,
        Cue,
        Subtitle

    }
}
