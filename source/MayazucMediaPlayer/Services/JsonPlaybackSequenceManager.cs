using MayazucMediaPlayer.Services.MediaSources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MayazucMediaPlayer.Services
{
    public class JsonPlaybackSequenceManager : IPlaybackSequenceProvider
    {
        public event EventHandler<IEnumerable<IMediaPlayerItemSource>> NewPlaybackQueue;
        public event EventHandler<IEnumerable<IMediaPlayerItemSource>> PlaybackQueueChanged;

        private string BackStoreFile { get; set; }

        public JsonPlaybackSequenceManager(string filePath)
        {
            BackStoreFile = filePath;
            CreateNowPlayingDB(filePath);
        }

        public void AddToNowPlaying(IEnumerable<IMediaPlayerItemSource> ToAdd)
        {
            var existing = GetPlaybackQueue().ToList();
            existing.AddRange(ToAdd);
            CreateNewPlaybackQueue(existing);
            PlaybackQueueChanged?.Invoke(this, ToAdd);
        }

        public void ClearNowPlaying()
        {
            CreateNewPlaybackQueue(new List<IMediaPlayerItemSource>());
            NewPlaybackQueue?.Invoke(this, new List<IMediaPlayerItemSource>());
        }

        public void CreateNewPlaybackQueue(IEnumerable<IMediaPlayerItemSource> ToAdd)
        {
            var json = JsonSerializer.Serialize(ToAdd.Select(x => x.MediaPath).ToList(), MayazucJsonSerializerContext.Default.ListString);
            File.WriteAllText(BackStoreFile, json);
            NewPlaybackQueue?.Invoke(this, new List<IMediaPlayerItemSource>(ToAdd));
        }

        private void CreateNowPlayingDB(string filePath)
        {
            if (!File.Exists(filePath))
                File.Create(filePath).Dispose();
        }

        public IReadOnlyCollection<IMediaPlayerItemSource> GetPlaybackQueue()
        {
            try
            {
                var json = File.ReadAllText(BackStoreFile);
                var data = JsonSerializer.Deserialize(json, MayazucJsonSerializerContext.Default.ListString);
                if (data == null) return new List<IMediaPlayerItemSource>().AsReadOnly();
                return data.Select(x => IMediaPlayerItemSourceFactory.Get(x)).ToList().AsReadOnly();
            }
            catch
            {
                return new List<IMediaPlayerItemSource>().AsReadOnly();
            }
        }
    }
}
