using FFmpegInteropX;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.Services.MediaSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.MediaPlayback
{
    public class MediaPlaybackItemUIInformation
    {
        readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _mediaMetadataTags;
        readonly List<SubtitleStreamInfoWrapper> subtitleStreams = new List<SubtitleStreamInfoWrapper>();
        readonly List<VideoStreamInfoWrapper> videoStreams = new List<VideoStreamInfoWrapper>();
        readonly List<AudioStreamInfoWrapper> audioStreams = new List<AudioStreamInfoWrapper>();
        readonly List<ChapterStreamInfoWrapper> chapterStreams = new List<ChapterStreamInfoWrapper>();

        public IReadOnlyDictionary<string, IReadOnlyList<string>> MetadataTags
        {
            get
            {
                if (_mediaMetadataTags == null) return new ReadOnlyDictionary<string, IReadOnlyList<string>>(new Dictionary<string, IReadOnlyList<string>>());
                return _mediaMetadataTags;
            }
        }

        public IReadOnlyList<SubtitleStreamInfoWrapper> SubtitleStreams { get => subtitleStreams.AsReadOnly(); }
        public IReadOnlyList<VideoStreamInfoWrapper> VideoStreams { get => videoStreams.AsReadOnly(); }
        public IReadOnlyList<AudioStreamInfoWrapper> AudioStreams { get => audioStreams.AsReadOnly(); }
        public IReadOnlyList<ChapterStreamInfoWrapper> Chapters { get => chapterStreams.AsReadOnly(); }

        public IMediaPlayerItemSource MediaData
        {
            get;
            private set;
        }

        public TimeSpan Duration
        {
            get;
            private set;
        }

        private MediaPlaybackItemUIInformation(FFmpegMediaSource ffmpegData,
            IMediaPlayerItemSource mediaData)
        {
            if (ffmpegData != null)
            {
                Duration = ffmpegData.Duration;
                _mediaMetadataTags = ffmpegData != null ? ffmpegData.MetadataTags : null;
                if (ffmpegData != null)
                {
                    subtitleStreams.AddRange(ffmpegData.SubtitleStreams.Select(x => new SubtitleStreamInfoWrapper(x)));
                    videoStreams.AddRange(ffmpegData.VideoStreams.Select(x => new VideoStreamInfoWrapper(x)));
                    audioStreams.AddRange(ffmpegData.AudioStreams.Select(x => new AudioStreamInfoWrapper(x)));
                    chapterStreams.AddRange(ffmpegData.ChapterInfos.Select(x => new ChapterStreamInfoWrapper(x)));
                }
            }
            MediaData = mediaData;
        }

        public static MediaPlaybackItemUIInformation Create(FFmpegMediaSource ffmpegData, IMediaPlayerItemSource mediaData)
        {
            try
            {
                var returnValue = new MediaPlaybackItemUIInformation(ffmpegData, mediaData);
                return returnValue;
            }
            catch
            {

            }

            return null;
        }

        public static async Task<MediaPlaybackItemUIInformation> CreateAsync(FileInfo file)
        {
            var mds = IMediaPlayerItemSourceFactory.Get(new PickedFileItem(file));
            var builder = new FFmpegInteropItemBuilder(null);
            using (var ffmpeg = await builder.GetFFmpegInteropMssAsync(mds))
            {
                //await mds.FillMetadataAsync(ffmpeg);
                return MediaPlaybackItemUIInformation.Create(ffmpeg, mds);
            }
        }
    }


    public class SubtitleStreamInfoWrapper
    {
        public bool IsForced { get; }
        public bool IsExternal { get; }
        public bool IsDefault { get; }
        public long Bitrate { get; }
        public string CodecName { get; }
        public string Language { get; }
        public string Name { get; }

        public SubtitleStreamInfoWrapper(SubtitleStreamInfo info)
        {
            IsForced = info.IsForced;
            IsExternal = info.IsExternal;
            IsDefault = info.IsDefault;
            Bitrate = info.Bitrate;
            CodecName = info.CodecName;
            Language = info.Language;
            Name = info.Name;
        }
    }

    public class VideoStreamInfoWrapper
    {
        public DecoderEngine DecoderEngine { get; }
        public HardwareDecoderStatus HardwareDecoderStatus { get; }
        public int BitsPerSample { get; }
        public double DisplayAspectRatio { get; }
        public int PixelHeight { get; }
        public int PixelWidth { get; }
        public bool IsDefault { get; }
        public long Bitrate { get; }
        public string CodecName { get; }
        public string Language { get; }
        public string Name { get; }

        public VideoStreamInfoWrapper(VideoStreamInfo info)
        {
            DecoderEngine = info.DecoderEngine;
            HardwareDecoderStatus = info.HardwareDecoderStatus;
            BitsPerSample = info.BitsPerSample;
            DisplayAspectRatio = info.DisplayAspectRatio;
            PixelHeight = info.PixelHeight;
            PixelWidth = info.PixelWidth;
            IsDefault = info.IsDefault;
            Bitrate = info.Bitrate;
            CodecName = info.CodecName;
            Language = info.Language;
            Name = info.Name;
        }
    }

    public class AudioStreamInfoWrapper
    {
        public DecoderEngine DecoderEngine { get; }
        public int BitsPerSample { get; }
        public int SampleRate { get; }
        public int Channels { get; }
        public bool IsDefault { get; }
        public long Bitrate { get; }
        public string CodecName { get; }
        public string Language { get; }
        public string Name { get; }

        public AudioStreamInfoWrapper(AudioStreamInfo info)
        {
            DecoderEngine = info.DecoderEngine;
            BitsPerSample = info.BitsPerSample;
            SampleRate = info.SampleRate;
            Channels = info.Channels;
            IsDefault = info.IsDefault;
            Bitrate = info.Bitrate;
            CodecName = info.CodecName;
            Language = info.Language;
            Name = info.Name;
        }
    }

    public class ChapterStreamInfoWrapper
    {
        public string Title { get; private set; }
        public TimeSpan Duration { get; private set; }
        public TimeSpan StartTime { get; private set; }

        public ChapterStreamInfoWrapper(ChapterInfo info)
        {
            Title = info.Title;
            Duration = info.Duration;
            StartTime = info.StartTime;
        }
    }
}
