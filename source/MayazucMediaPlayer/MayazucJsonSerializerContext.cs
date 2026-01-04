using MayazucMediaPlayer.MediaCollections;
using MayazucMediaPlayer.MediaMetadata;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static MayazucMediaPlayer.AudioEffects.EqualizerConfiguration;

namespace MayazucMediaPlayer
{
    [JsonSerializable(typeof(EqualizerConfigurationSeed))]
    [JsonSerializable(typeof(NetworkStreamHistoryEntry))]
    [JsonSerializable(typeof(List<NetworkStreamHistoryEntry>))]
    [JsonSerializable(typeof(EmbeddedMetadataSeed))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(Dictionary<string, double>))]
    [JsonSerializable(typeof(ConcurrentDictionary<string, string>))]
    public partial class MayazucJsonSerializerContext : JsonSerializerContext
    {
    }
}
