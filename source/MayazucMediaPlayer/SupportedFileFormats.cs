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
            MusicFormats = allMusicFormatsInternal.ToImmutableHashSet(StringComparer.CurrentCultureIgnoreCase);
            WriteableFormats = allWriteableFormatsInternal.ToImmutableHashSet(StringComparer.CurrentCultureIgnoreCase);
            AllVideoFormats = allVideoFormatsInternal.ToImmutableHashSet(StringComparer.CurrentCultureIgnoreCase);
            SupportedStreamingUriSchemes = allStreamingUriSchemeInternal.ToImmutableHashSet(StringComparer.CurrentCultureIgnoreCase);
       
            AllMusicAndPlaylistFormats = MusicFormats.Union(PlaylistFormats.ToImmutableHashSet(StringComparer.CurrentCultureIgnoreCase)).ToImmutableHashSet(StringComparer.CurrentCultureIgnoreCase);
            AllSupportedFileFormats = AllMusicAndPlaylistFormats.Union(AllVideoFormats).Union(WriteableFormats);
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
        public static ImmutableHashSet<string> MusicFormats
        {
            get;
            private set;
        }

        /// <summary>
        /// All files which can be fed into the tag editor
        /// </summary>
        public static ImmutableHashSet<string> WriteableFormats
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

        static readonly List<string> allWriteableFormatsInternal = new List<string>
        {
            ".wma", ".mp3", ".ogg",
            ".flac", ".m4a", ".aac", ".adt", ".adts", ".ac3", ".ec3", ".wav", ".oga", ".wv",   ".mpc", ".tta",".m4a",
            ".m4r", ".m4b", ".m4p", ".3g2", ".asf", ".aif", ".aiff", ".afc", ".aifc",
            ".ape", ".alac", ".awb"
        };


        static readonly List<string> allMusicFormatsInternal = new List<string> { ".wma", ".mp3", ".ogg", ".opus",
            ".flac", ".m4a", ".aac", ".adt", ".adts", ".ac3", ".ec3", ".wav", ".oga", ".wv",   ".mpc", ".tta",".m4a",
        ".m4r", ".m4b", ".m4p", ".3g2", ".asf", ".aif", ".aiff", ".afc", ".aifc", ".m2ts",
        ".ape",     ".alac", ".awb" };

        static readonly List<string> SystemSupportedMusicFormats = new List<string> { ".wma", ".mp3", ".m4a", ".aac", ".adt", ".adts", ".ac3", ".ec3" };
        static readonly List<string> allVideoFormatsInternal = new List<string> { ".mp4", ".avi", ".wmv", ".h264", ".mkv", ".vob", ".h265", ".ts", ".glv", ".m4v", ".av1", ".webm" };

        static readonly List<string> allSupportedFormatsInternal = new List<string> { ".wma", ".mp3", ".ogg", ".opus",
            ".flac", ".m4a", ".aac", ".adt", ".adts", ".ac3", ".ec3", ".wav", ".oga", ".wv",   ".mpc", ".tta",".m4a",
        ".m4r", ".m4b", ".m4p", ".3g2", ".asf", ".aif", ".aiff", ".afc", ".aifc",
        ".ape",     ".alac", ".m3u", ".awb", ".mp4", ".avi", ".wmv", ".h264", ".mkv", ".wpl", ".zpl", ".vob", ".h265", ".ts", ".glv", ".m4v", ".m3u8", ".av1", ".webm", ".m2ts"  };

        static readonly List<string> SubtitleFormats = new List<string> { ".srt", ".sub", ".ttml", ".vtt", ".ssa", ".ass" };

        static readonly List<string> PlaylistFormats = new List<string> { ".m3u", ".wpl", ".zpl", ".m3u8" };

        static readonly List<string> PictureFormats = new List<string> { ".png", ".jpg", ".jpeg", ".bmp" };

        static readonly List<string> allStreamingUriSchemeInternal = new List<string> { "http", "rtsp", "rtmp" };

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
        public static ImmutableHashSet<string> AllVideoFormats
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
            return AllVideoFormats.Contains(ext);
        }

        public static bool IsAudioFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return MusicFormats.Contains(ext);
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
            return allSupportedFormatsInternal.Any(x => pathLower.EndsWith(x));
        }

        public static bool IsSupportedStreamingProtocol(string  protocol)
        {
            if (string.IsNullOrWhiteSpace(protocol)) return false;
            return SupportedStreamingUriSchemes.Contains(protocol);
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
