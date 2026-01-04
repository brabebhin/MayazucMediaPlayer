using FFmpegInteropX;
using Microsoft.UI.Xaml.Media.Imaging;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace MayazucMediaPlayer
{
    public static class Utilities
    {
        /// <summary>
        /// generates a random file name
        /// </summary>
        /// <param name="extension">must also include the dot</param>
        /// <returns></returns>
        public static string EncodePathWithExtension(string basePath, string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException("extension cannot be null, empty or white space");
            }

            return $"{EncodePathWithoutExtension(basePath)}{extension}";
        }

        /// <summary>
        /// generates a random file name
        /// </summary>
        /// <param name="extension">must also include the dot</param>
        /// <returns></returns>
        public static string EncodePathWithoutExtension(string path)
        {
            var hashAlg = new SHA512Managed();
            var hash = hashAlg.ComputeHash(Encoding.UTF8.GetBytes(path));
            var safeRandomName = WebUtility.UrlEncode(Convert.ToBase64String(hash));
            return safeRandomName;
        }

        public static void ConfigureFilePicker(this FileOpenPicker filepicker, SupportedFileTypesConfiguration config)
        {
            filepicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            IEnumerable<string> extensions = null;
            switch (config)
            {
                case SupportedFileTypesConfiguration.AllFiles:
                default:
                    extensions = SupportedFileFormats.AllSupportedFileFormats;

                    break;
                case SupportedFileTypesConfiguration.AllMusicFiles:
                    extensions = SupportedFileFormats.AllMusicAndPlaylistFormats;

                    break;
                case SupportedFileTypesConfiguration.AllWriteableFiles:
                    extensions = SupportedFileFormats.WriteableFormats;

                    break;
                case SupportedFileTypesConfiguration.CueableFormats:
                    extensions = SupportedFileFormats.MusicFormats;
                    break;
            }

            foreach (string ext in extensions)
            {
                filepicker.FileTypeFilter.Add(ext);
            }
        }

        static readonly AsyncLock CopyThumbnailStreamAsyncLock = new AsyncLock();
        public static async Task<FileInfo> CopyThumbnailStreamAsync(MediaThumbnailData mediaThumbnail, string path)
        {
            using (await CopyThumbnailStreamAsyncLock.LockAsync())
            {
                var aaFolder = await LocalCache.KnownLocations.GetAlbumArtFolder();
                using var file = aaFolder.CreateFile($"{Utilities.EncodePathWithoutExtension(path)}{mediaThumbnail.Extension}");
                var buffer = mediaThumbnail.Buffer.ToArray();
                await file.FileStream.WriteAsync(buffer, 0, buffer.Length);
                return file.FileInformation;
            }
        }

        public static async Task<byte[]> ResizeImageAsync(IRandomAccessStream imageStream, int reqWidth, int reqHeight, int quality)
        {
            var decoder = await BitmapDecoder.CreateAsync(imageStream);
            if (decoder.PixelHeight > reqHeight || decoder.PixelWidth > reqWidth)
            {
                using (imageStream)
                {
                    using (var resizedStream = new InMemoryRandomAccessStream())
                    {

                        BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);
                        double widthRatio = (double)reqWidth / decoder.PixelWidth;
                        double heightRatio = (double)reqHeight / decoder.PixelHeight;

                        double scaleRatio = Math.Min(widthRatio, heightRatio);

                        if (reqWidth == 0)
                        {
                            scaleRatio = heightRatio;
                        }

                        if (reqHeight == 0)
                        {
                            scaleRatio = widthRatio;
                        }

                        uint aspectHeight = (uint)Math.Floor(decoder.PixelHeight * scaleRatio);
                        uint aspectWidth = (uint)Math.Floor(decoder.PixelWidth * scaleRatio);

                        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;

                        encoder.BitmapTransform.ScaledHeight = aspectHeight;
                        encoder.BitmapTransform.ScaledWidth = aspectWidth;

                        await encoder.FlushAsync();
                        resizedStream.Seek(0);
                        var outBuffer = new byte[resizedStream.Size];
                        uint x = await resizedStream.WriteAsync(outBuffer.AsBuffer());
                        return outBuffer;
                    }
                }
            }
            return null;
        }

        public static async Task<BitmapImage> BitmapImageFromByteArray(byte[] data)
        {
            var stream = new MemoryStream(data);
            BitmapImage img = new BitmapImage();
            await img.SetSourceAsync(stream.AsRandomAccessStream());
            return img;
        }

        public static string TaglibMimeToFileExtensions(string taglibMime)
        {
            if (taglibMime.Contains("png")) return ".png";
            if (taglibMime.Contains("jpg")) return ".jpg";
            if (taglibMime.Contains("jpeg")) return ".jpeg";
            if (taglibMime.Contains("bmp")) return ".bmp";

            return null;
        }

        public static bool TrueOrDefault(this bool? value)
        {
            if (value.HasValue) return value.Value;
            else return false;
        }

        /// <summary>
        /// Normalizes the value <paramref name="m"/> measured in the range rmin..rmax to the range tmin..tmax
        /// </summary>
        /// <param name="m">the value to normalize</param>
        /// <param name="rmin">the lower end of the values that m can take</param>
        /// <param name="rmax">the upper end of the values that m can take</param>
        /// <param name="tmin">the lower end of the new range to normalize to</param>
        /// <param name="tmax">the upper end of the new range to normalize to</param>
        /// <returns></returns>
        public static int NormalizeValue(int m, float rmin, float rmax, float tmin, float tmax)
        {
            var result = (tmax - tmin) * ((m - rmin) / (rmax - rmin)) + tmin;
            return (int)Math.Floor(result);
        }
    }
}
