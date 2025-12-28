using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.MediaCollections;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.Users;
using MovieCollection.OpenSubtitles;
using MovieCollection.OpenSubtitles.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles
{
    public partial class OpenSubtitlesRestAgent : IOpenSubtitlesAgent
    {
        private static readonly HttpClient httpClient = new HttpClient();

        readonly Lazy<OpenSubtitlesService> service;
        private bool disposedValue;

        private string ApiKey
        {
            get
            {
                return Environment.GetEnvironmentVariable("mayazuc.opensubtitles.apikey", EnvironmentVariableTarget.User);
            }
        }

        public void EnsureHasApiKey()
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new InvalidOperationException("API key must be supplied in environment variable \"mayazuc.opensubtitles.apikey\" for current user account.");
            }
        }

        private string? Token { get; set; }

        public bool LoggedIn { get; private set; }

        public OpenSubtitlesRestAgent()
        {
            service = new Lazy<OpenSubtitlesService>(() => new OpenSubtitlesService(httpClient, GetOptions()));
        }

        public async Task<FileInfo?> AutoDownloadSubtitleAsync(SubtitleRequest request)
        {
            try
            {
                EnsureHasApiKey();
                var found = await SearchSubtitlesAsync(request);
                var first = found.FirstOrDefault();
                if (first != null)
                {
                    return await DownloadSubtitleAsync(first);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private OpenSubtitlesOptions GetOptions()
        {
            return new OpenSubtitlesOptions
            {
                ApiKey = ApiKey,
                ProductInformation = new ProductHeaderValue("MayazucMediaPlayer", "10"),
            };
        }

        public async Task<FileInfo?> DownloadSubtitleAsync(IOSDBSubtitleInfo info)
        {
            EnsureHasApiKey();

            var impl = (OSDBSubtitleInfo2)info;

            var urlSubtitleDownloadRequest = new NewDownload()
            {
                FileId = impl.FileId
            };

            var downloadUrlResponse = await service.Value.GetSubtitleForDownloadAsync(urlSubtitleDownloadRequest);
            if (downloadUrlResponse.Remaining <= 0)
                throw new InvalidOperationException("Too many subtitle downloads. ");
            try
            {
                using var subtitleData = await httpClient.GetStreamAsync(downloadUrlResponse.Link);
                var folder = await LocalFolders.GetCachedSubtitlesFolder();
                var file = await folder.CreateFileAsync(info.SubFileName, CreationCollisionOption.ReplaceExisting);
                //using (var deflateStream = new GZipStream(subtitleData, CompressionMode.Decompress))
                {
                    using var stream = await file.OpenStreamForWriteAsync();
                    await subtitleData.CopyToAsync(stream);
                }

                return file.ToFileInfo();
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> LoginAsync()
        {
            try
            {
                EnsureHasApiKey();

                var credentials = CredentialsProvider.GetOpenSubtitles();
                string username = "";
                string password = "";

                if (credentials != null)
                {
                    username = credentials.UserName;
                    password = credentials.Password;
                }

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return false;
                }

                var login = await service.Value.LoginAsync(new NewLogin()
                {
                    Username = username,
                    Password = password,
                });

                Token = login.Token;
                return !string.IsNullOrWhiteSpace(Token);
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            EnsureHasApiKey();
            if (Token != null)
            {
                await service.Value.LogoutAsync(Token);
            }
        }

        public async Task<ReadOnlyCollection<IOSDBSubtitleInfo>> SearchSubtitlesAsync(SubtitleRequest request)
        {
            EnsureHasApiKey();

            List<IOSDBSubtitleInfo> returnValue = new List<IOSDBSubtitleInfo>();

            if (string.IsNullOrWhiteSpace(Token))
            {
                var suceeded = await LoginAsync();

                if (!suceeded)
                {
                    throw new Exception("Could not login to OpenSubtitles.org. Check username and password");
                }
            }

            //var hash = await OSDBExtensions.ComputeOSDBHash(file);
            var fileName = Path.GetFileName(request.FullMediaLocation);
            var searchOptions = new NewSubtitleSearch()
            {
                MovieHash = request.FileHash,
                Languages = new List<string>() { SettingsService.Instance.PreferredSubtitleLanguage.TwoLetterIsoCode },
                Query = fileName
            };

            var subsSearchResult = await service.Value.SearchSubtitlesAsync(searchOptions);
            if (subsSearchResult != null && subsSearchResult.TotalCount > 0 && subsSearchResult.Data != null)
            {
                var fileNameSplits = fileName.ToLowerInvariant().SplitByNonAlphaNumeric();

                foreach (var data in subsSearchResult.Data.Where(x => x.Attributes != null && x.Attributes.Files.Any()))
                {
                    var firstFileId = data.Attributes?.Files?.FirstOrDefault();
                    if (firstFileId != null)
                    {
                        var subFileName = firstFileId.FileName;
                        var fileId = firstFileId.FileId;

                        var sub = new OSDBSubtitleInfo2(languageName: data.Attributes?.Language ?? string.Empty, subFileName: subFileName ?? "unknown sub file name", subFormat: System.IO.Path.GetExtension(fileName), fileId: fileId);
                        var computedScope = sub.SubFileName?.CalculateStringSimilarityScores(fileNameSplits);
                        if (computedScope.HasValue)
                            sub.SimilarityScore = computedScope.Value;
                        returnValue.Add(sub);
                    }
                }
            }

            return returnValue.AsReadOnly();
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~OpenSubtitlesRestAgent()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
