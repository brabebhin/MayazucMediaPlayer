using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.IFD.Tags;

namespace MayazucMediaPlayer.MediaPlayback
{
    internal static class PlaybackSpeedService
    {
        public static ReadOnlyCollection<double> SupportedPlaybackSpeeds { get; private set; } = new ReadOnlyCollection<double>(new double[] { 0.25, 0.5, 1, 1.5, 2 });

        public const double DefaultPlaybackSpeed = 1.0d;
    }
}
