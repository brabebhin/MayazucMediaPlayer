using FFmpegInteropX;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MayazucMediaPlayer.MediaPlayback
{
    public static class FFmpegVideoModeInterop
    {
        static readonly Dictionary<int, VideoDecoderMode> map = new Dictionary<int, VideoDecoderMode>();
        public static IReadOnlyDictionary<int, VideoDecoderMode> DecoderModeMap
        {
            get;
            private set;
        }

        public static IEnumerable<string> DecoderModeNames
        {
            get;
            private set;
        }

        static FFmpegVideoModeInterop()
        {
            map.Add(0, VideoDecoderMode.Automatic);
            map.Add(1, VideoDecoderMode.ForceFFmpegSoftwareDecoder);
            map.Add(2, VideoDecoderMode.ForceSystemDecoder);

            DecoderModeMap = new ReadOnlyDictionary<int, VideoDecoderMode>(map);
            DecoderModeNames = new string[] { "Autodetect GPU / software decoder", "Force ffmpeg software decoder", "Force system decoders" }.AsEnumerable();
        }
    }
}
