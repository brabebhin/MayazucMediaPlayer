using Windows.Media.Playback;

namespace MayazucMediaPlayer
{
    public abstract class Constants
    {
        public const string RepeatAll = "all";

        public const string RepeatOne = "one";

        public const string RepeatNone = "none";
        public const long JumpAheadSeconds = 10;
        public const long JumpBackSeconds = 5;

        public const TimedMetadataTrackPresentationMode DefaultSubtitlePresentationMode = TimedMetadataTrackPresentationMode.PlatformPresented;
    }
}
