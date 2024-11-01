using FFmpegInteropX;
using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.MediaMetadata
{
    public partial class EmbeddedMetadataResult
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

        public EmbeddedMetadataResult(string album, string artist, string genre, string title, ReadOnlyDictionary<string, string> additionalMetadata)
        {
            Album = album;
            Artist = artist;
            Genre = genre;
            Title = title;
            AdditionalMetadata = additionalMetadata;
        }

        public EmbeddedMetadataResult(string album, string artist, string genre, string title, string savedThumbnailFile, ReadOnlyDictionary<string, string> additionalMetadata)
        {
            SavedThumbnailFile = string.IsNullOrWhiteSpace(savedThumbnailFile) ? AssetsPaths.PlaceholderAlbumArt : savedThumbnailFile;
            Album = album;
            Artist = artist;
            Genre = genre;
            Title = title;
            AdditionalMetadata = additionalMetadata;
        }

        public EmbeddedMetadataResult(string album, string author, string genre, string title)
        {
            Artist = author;
            Album = album;
            Genre = genre;
            Title = title;
        }

        public EmbeddedMetadataResult(string performer, string title)
        {
            Artist = performer;
            Title = title;
        }

        public override bool Equals(object obj)
        {
            return obj is EmbeddedMetadataResult result &&
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

    public class EmbeddedMetadataResultFile
    {
        public string Album { get; set; } = string.Empty;

        public string Artist { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string SavedThumbnailFile { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public Dictionary<string, string> AdditionalMetadata { get; set; } = new Dictionary<string, string>();

        public EmbeddedMetadataResult ToMetadataResult()
        {
            return new EmbeddedMetadataResult(album: Album, artist: Artist, genre: Genre, title: Title, savedThumbnailFile: SavedThumbnailFile, additionalMetadata: new ReadOnlyDictionary<string, string>(AdditionalMetadata));
        }

        public EmbeddedMetadataResultFile() { }

        public EmbeddedMetadataResultFile(EmbeddedMetadataResult metadata, string filePath)
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
            var filename = Utilities.GetRandomFileName(filePath, ".json");
            var folder = await LocalFolders.MetadataDatabaseFolder();
            return new FileInfo(Path.Combine(folder.FullName, filename));
        }

        public static EmbeddedMetadataResult ReadMetadataDocumentForFile(FileInfo document, FileInfo sourceFile)
        {
            try
            {
                if (document.Exists)
                {
                    var documentData = JsonConvert.DeserializeObject<EmbeddedMetadataResultFile>(File.ReadAllText(document.FullName));
                    var metadata = documentData.ToMetadataResult();
                    return metadata;
                }
            }
            catch
            {
            }
            return GetDefaultMetadataForFile(sourceFile.FullName);
        }

        public static EmbeddedMetadataResult GetDefaultMetadataForFile(string filePath)
        {
            return new EmbeddedMetadataResult(album: string.Empty, artist: string.Empty, genre: string.Empty, title: Path.GetFileNameWithoutExtension(filePath), additionalMetadata: new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()), savedThumbnailFile: AssetsPaths.PlaceholderAlbumArt);
        }

        public static EmbeddedMetadataResult GetDefaultMetadataForFileNotFound(string filePath)
        {
            return new EmbeddedMetadataResult("Source not available", "Source not available", "Source not available", Path.GetFileNameWithoutExtension(filePath), new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()));
        }

        public static async Task<EmbeddedMetadataResult> RetrieveEmbeddedMetadata(FFmpegMediaSource ffmpegMediaSource,
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

            var albumArtFile = Utilities.GetRandomFileNameWithoutExtension(fileToGetThMetadataFrom.FullName);

            var thumbnail = await GetFFmpegInteropAlbumCover(albumArtFile, ffmpegMediaSource);

            return new EmbeddedMetadataResult(album: Album, artist: Artist, genre: Genre, title: title, additionalMetadata: ffmpegMediaSource.GetMetadataDictionary(), savedThumbnailFile: thumbnail.FullName);
        }

        public static async Task<EmbeddedMetadataResult> ExtractMetadataAsync(FileInfo fileToGetThumbnailFrom)
        {
            var config = MediaHelperExtensions.GetFFmpegUserConfigs();
            var mediaSource = await FFmpegInteropItemBuilder.CreateFFmpegInteropMediaSourceFromFileAsync(config, fileToGetThumbnailFrom);
            return await RetrieveEmbeddedMetadata(mediaSource, fileToGetThumbnailFrom);
        }

        private static async Task<FileInfo> GetFFmpegInteropAlbumCover(
            string albumartFileName,
            FFmpegMediaSource mediaSource)
        {
            var folderToSaveThumbnailIn = await LocalFolders.GetAlbumArtFolder();
            FileInfo retValue = null;
            if (!mediaSource.HasThumbnail) return retValue;

            var picture = mediaSource.ExtractThumbnail();
            if (picture == null)
                return retValue;
            var fileExtension = picture.Extension;
            if (string.IsNullOrWhiteSpace(fileExtension))
                return retValue;

            using var resultingAlbumArtFile = folderToSaveThumbnailIn.CreateFile(albumartFileName + fileExtension);

            try
            {
                using (var stream = resultingAlbumArtFile.FileStream)
                {
                    var buffer = picture.Buffer.ToArray();
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    retValue = resultingAlbumArtFile.FileInformation;
                }
            }
            catch { retValue = null; }


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

