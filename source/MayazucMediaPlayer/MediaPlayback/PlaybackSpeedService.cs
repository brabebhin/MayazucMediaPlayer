using System.Collections.ObjectModel;

namespace MayazucMediaPlayer.MediaPlayback
{
    internal static class PlaybackSpeedService
    {
        public static ReadOnlyCollection<double> SupportedPlaybackSpeeds { get; private set; } = new ReadOnlyCollection<double>(new double[] { 0.25, 0.5, 1, 1.5, 2 });

        public const double DefaultPlaybackSpeed = 1.0d;
    }
}
