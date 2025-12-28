using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.MediaCollections
{
    public partial class NetworkStreamsCollectionUiService : ServiceBase
    {
        private const int HistorySize = 100;

        public AsyncRelayCommand<string> PlayUrlCommand { get; private set; }

        public AsyncRelayCommand ClearHistoryCommand { get; private set; }

        string _inputStreamUrl;
        public string InputStreamUrl
        {
            get
            {
                return _inputStreamUrl;
            }
            set
            {
                base.SetProperty(ref _inputStreamUrl, value);
                HasValidUrl = ValidateUrl(_inputStreamUrl, out var _);
            }
        }

        bool _HasValidUrl;
        public bool HasValidUrl
        {
            get
            {
                return _HasValidUrl;
            }
            set
            {
                base.SetProperty(ref _HasValidUrl, value, nameof(HasValidUrl));
            }
        }

        public NetworkStreamsCollectionUiService(DispatcherQueue dispatcher) : base(dispatcher)
        {
            PlayUrlCommand = new AsyncRelayCommand<string>(PlayUrl);
            ClearHistoryCommand = new AsyncRelayCommand(ClearHistory);
        }

        private async Task ClearHistory()
        {
            NetworkStreamsHistory.Clear();
            await SaveHistory();
        }

        public NetworkStreamHistoryEntryCollection NetworkStreamsHistory { get; private set; } = new NetworkStreamHistoryEntryCollection();

        public async Task LoadHistory()
        {
            NetworkStreamsHistory.Clear();
            var historyFile = await LocalCache.LocalFolders.GetInternetStreamsHistoryFile();
            var json = await File.ReadAllTextAsync(historyFile.FullName);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var entries = JsonSerializer.Deserialize<List<NetworkStreamHistoryEntry>>(json, MayazucJsonSerializerContext.Default.ListNetworkStreamHistoryEntry);
                if (entries != null)
                {
                    foreach (var entry in entries)
                    {
                        NetworkStreamsHistory.Add(entry);
                    }
                }
            }
        }

        public async Task SaveHistory()
        {
            var historyFile = await LocalCache.LocalFolders.GetInternetStreamsHistoryFile();

            var json = JsonSerializer.Serialize(NetworkStreamsHistory.OrderByDescending(x => x.Time).Take(HistorySize).ToList(), MayazucJsonSerializerContext.Default.ListNetworkStreamHistoryEntry);
            await File.WriteAllTextAsync(historyFile.FullName, json);
        }

        public async Task PlayUrl(object param)
        {
            var url = (string)param;
            if (ValidateUrl(url, out var result))
            {
                var mediaData = IMediaPlayerItemSourceFactory.Get(result);
                await AppState.Current.MediaServiceConnector.StartPlaybackFromBeginning(mediaData);
                NetworkStreamsHistory.Add(url);
                await SaveHistory();
            }
        }

        public bool ValidateUrl(string url, out Uri result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                result = uri;
                return SupportedFileFormats.IsSupportedStreamingProtocol(uri.Scheme);
            }

            return false;
        }
    }
}
