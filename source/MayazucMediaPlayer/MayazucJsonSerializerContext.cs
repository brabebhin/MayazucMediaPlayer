using MayazucMediaPlayer.MediaCollections;
using MayazucMediaPlayer.MediaMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static MayazucMediaPlayer.AudioEffects.EqualizerConfiguration;
using static MayazucMediaPlayer.Settings.SettingsStoreService;

namespace MayazucMediaPlayer
{
    [JsonSerializable(typeof(EqualizerConfigurationSeed))]
    [JsonSerializable(typeof(NetworkStreamHistoryEntry))]
    [JsonSerializable(typeof(List<NetworkStreamHistoryEntry>))]
    [JsonSerializable(typeof(EmbeddedMetadataSeed))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(Dictionary<string, double>))]
    public partial class MayazucJsonSerializerContext : JsonSerializerContext
    {
    }
}
