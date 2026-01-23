using FFmpegInteropX;
using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.MediaMetadata
{
    public partial class EmbeddedMetadata
    {
        public string Album { get; private set; } = string.Empty;

        public string Artist { get; private set; } = string.Empty;

        public string Genre { get; private set; } = string.Empty;

        public string Title { get; private set; } = string.Empty;

        public string SavedThumbnailFile { get; private set; } = AssetsPaths.PlaceholderAlbumArt;

        public bool HasSavedThumbnailFile()
        {
            return SavedThumbnailFile != AssetsPaths.PlaceholderAlbumArt;
        }

        public ReadOnlyDictionary<string, string> JoinedMetadata()
        {
            Dictionary<string, string> returnValue = new Dictionary<string, string>
            {
                { nameof(Album), Album },
                { nameof(Artist), Artist },
                { nameof(Genre), Genre },
                { nameof(Title), Title }
            };
            foreach (var additional in AdditionalMetadata)
            {
                returnValue.TryAdd(additional.Key, additional.Value);
            }

            return returnValue.AsReadOnly();
        }

        public ReadOnlyDictionary<string, string> AdditionalMetadata { get; private set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        public EmbeddedMetadata(string album, string artist, string genre, string title, ReadOnlyDictionary<string, string> additionalMetadata)
        {
            Album = album;
            Artist = artist;
            Genre = genre;
            Title = title;
            AdditionalMetadata = additionalMetadata;
        }

        public EmbeddedMetadata(string album, string artist, string genre, string title, string savedThumbnailFile, ReadOnlyDictionary<string, string> additionalMetadata)
        {
            SavedThumbnailFile = string.IsNullOrWhiteSpace(savedThumbnailFile) ? AssetsPaths.PlaceholderAlbumArt : savedThumbnailFile;
            Album = album;
            Artist = artist;
            Genre = genre;
            Title = title;
            AdditionalMetadata = additionalMetadata;
        }

        public EmbeddedMetadata(string album, string author, string genre, string title)
        {
            Artist = author;
            Album = album;
            Genre = genre;
            Title = title;
        }

        public EmbeddedMetadata(string performer, string title)
        {
            Artist = performer;
            Title = title;
        }

        public override bool Equals(object obj)
        {
            return obj is EmbeddedMetadata result &&
                   Album == result.Album &&
                   Artist == result.Artist &&
                   Genre == result.Genre &&
                   Title == result.Title;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Album, Artist, Genre, Title);
        }
    }

    public class EmbeddedMetadataSeed
    {
        public string Album { get; set; } = string.Empty;

        public string Artist { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string SavedThumbnailFile { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public Dictionary<string, string> AdditionalMetadata { get; set; } = new Dictionary<string, string>();

        public EmbeddedMetadata GetEmbeddedMetadata()
        {
            return new EmbeddedMetadata(album: Album, artist: Artist, genre: Genre, title: Title, savedThumbnailFile: SavedThumbnailFile, additionalMetadata: new ReadOnlyDictionary<string, string>(AdditionalMetadata));
        }

        public EmbeddedMetadataSeed() { }

        public EmbeddedMetadataSeed(EmbeddedMetadata metadata, string filePath)
        {
            Album = metadata.Album;
            Artist = metadata.Artist;
            Genre = metadata.Genre;
            Title = metadata.Title;

            SavedThumbnailFile = metadata.SavedThumbnailFile;
            AdditionalMetadata = metadata.AdditionalMetadata.ToDictionary(x => x.Key, x => x.Value);
            FilePath = filePath;
        }
    }

    public static class EmbeddedMetadataResolver
    {
        public static bool IsSupportedExtension(FileInfo info)
        {
            return SupportedFileFormats.MusicFormats.Contains(info.Extension);
        }

        public static async Task<FileInfo> GetMetadataDocumentForFile(string filePath)
        {
            var filename = Utilities.EncodePathWithExtension(filePath, ".json");
            var folder = await KnownLocations.MetadataDatabaseFolder();
            return new FileInfo(Path.Combine(folder.FullName, filename));
        }

        public static EmbeddedMetadata ReadMetadataDocumentForFile(FileInfo document, FileInfo sourceFile)
        {
            try
            {
                if (document.Exists)
                {
                    var documentData = JsonSerializer.Deserialize<EmbeddedMetadataSeed>(File.ReadAllText(document.FullName), MayazucJsonSerializerContext.Default.EmbeddedMetadataSeed); ;
                    var metadata = documentData.GetEmbeddedMetadata();
                    return metadata;
                }
            }
            catch
            {
            }
            return GetDefaultMetadataForFile(sourceFile.FullName);
        }

        public static EmbeddedMetadata GetDefaultMetadataForFile(string filePath)
        {
            return new EmbeddedMetadata(album: string.Empty, artist: string.Empty, genre: string.Empty, title: Path.GetFileNameWithoutExtension(filePath), additionalMetadata: new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()), savedThumbnailFile: AssetsPaths.PlaceholderAlbumArt);
        }

        public static EmbeddedMetadata GetDefaultMetadataForFileNotFound(string filePath)
        {
            return new EmbeddedMetadata("Source not available", "Source not available", "Source not available", Path.GetFileNameWithoutExtension(filePath), new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()));
        }

        public static async Task<EmbeddedMetadata> RetrieveEmbeddedMetadata(FFmpegMediaSource ffmpegMediaSource,
            FileInfo fileToGetThMetadataFrom)
        {
            var fallbackTitle = fileToGetThMetadataFrom.FullName;
            var albumIndex = SettingsService.Instance.MetadataAlbumIndex;
            var artistIndex = SettingsService.Instance.MetadataArtistIndex;
            var genreIndex = SettingsService.Instance.MetadataGenreIndex;
            var useDefault = SettingsService.Instance.MetadataOptionsUseDefault;

            List<string> pathFragments = fallbackTitle.Split(Path.DirectorySeparatorChar).Reverse().ToList();

            var title = ffmpegMediaSource.GetTitle();
            if (string.IsNullOrWhiteSpace(title))
                title = fallbackTitle;

            var Album = GetMetadataProperty(albumIndex, useDefault, pathFragments, () =>
            {
                return ffmpegMediaSource.GetAlbum();
            });

            var Artist = GetMetadataProperty(artistIndex, useDefault, pathFragments, () =>
            {
                return ffmpegMediaSource.GetArtist();
            });

            var Genre = GetMetadataProperty(genreIndex, useDefault, pathFragments, () =>
            {
                return ffmpegMediaSource.GetGenre();
            });

            var albumArtFile = Utilities.EncodePathWithoutExtension(fileToGetThMetadataFrom.FullName);

            var thumbnail = await GetFFmpegInteropAlbumCover(albumArtFile, ffmpegMediaSource);

            return new EmbeddedMetadata(album: Album, artist: Artist, genre: Genre, title: title, additionalMetadata: ffmpegMediaSource.GetMetadataDictionary(), savedThumbnailFile: thumbnail.FullName);
        }

        public static async Task<EmbeddedMetadata> ExtractMetadataAsync(FileInfo fileToGetThumbnailFrom)
        {
            var config = FFmpegInteropXExtensions.GetFFmpegUserConfigs();
            var mediaSource = await FFmpegInteropItemBuilder.CreateFFmpegInteropMediaSourceFromFileAsync(config, fileToGetThumbnailFrom);
            return await RetrieveEmbeddedMetadata(mediaSource, fileToGetThumbnailFrom);
        }

        private static async Task<FileInfo> GetFFmpegInteropAlbumCover(
            string albumartFileName,
            FFmpegMediaSource mediaSource)
        {
            var folderToSaveThumbnailIn = await KnownLocations.GetAlbumArtFolder();
            FileInfo retValue = new FileInfo(AssetsPaths.PlaceholderAlbumArt);
            if (!mediaSource.HasThumbnail) return retValue;

            var picture = mediaSource.ExtractThumbnail();
            if (picture == null)
                return retValue;
            var fileExtension = picture.Extension;
            if (string.IsNullOrWhiteSpace(fileExtension))
                return retValue;

            using var resultingAlbumArtFile = folderToSaveThumbnailIn.CreateFileStream(albumartFileName + fileExtension);

            try
            {
                using (var stream = resultingAlbumArtFile.FileStream)
                {
                    var buffer = picture.Buffer.ToArray();
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    retValue = resultingAlbumArtFile.FileInformation;
                }
            }
            catch { }


            return retValue;
        }

        static string GetMetadataProperty(int metadataFileIndex,
                  bool shouldUseDefault,
                  IList<string> filePathFragemnts,
                  Func<string> tagLibSource)
        {
            var tagLibSourceResult = tagLibSource();
            if (metadataFileIndex == 0 || shouldUseDefault)
            {
                if (tagLibSourceResult != null)
                {
                    return tagLibSourceResult;
                }
            }
            else
            {
                if (metadataFileIndex < filePathFragemnts.Count)
                {
                    return filePathFragemnts[metadataFileIndex];
                }
                else
                {
                    if (tagLibSourceResult != null)
                    {
                        return tagLibSourceResult;
                    }
                }
            }

            return string.Empty;
        }
    }
}

