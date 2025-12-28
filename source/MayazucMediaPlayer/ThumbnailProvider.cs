using MayazucMediaPlayer.LocalCache;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MayazucMediaPlayer
{
    public static class ThumbnailProvider
    {
        public static async Task<IReadOnlyCollection<string>> GetExpectedAlbumArtFileNames(FileInfo inputFile)
        {
            var folderToSaveThumbnailIn = await LocalFolders.GetAlbumArtFolder();

            var albumArtFile = Utilities.EncodePathWithoutExtension(inputFile.FullName);
            List<string> returnValue = new List<string>();

            foreach (var ext in SupportedFileFormats.SupportedAlbumArtPictureFormats)
            {
                returnValue.Add(Path.Combine(folderToSaveThumbnailIn.FullName, albumArtFile + ext));
            }

            return returnValue;
        }
    }
}
