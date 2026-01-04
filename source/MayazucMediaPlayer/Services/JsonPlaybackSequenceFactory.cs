using MayazucMediaPlayer.LocalCache;

namespace MayazucMediaPlayer.Services
{
    public class JsonPlaybackSequenceFactory : IPlaybackSequenceProviderFactory
    {
        public static string NowPlayingPlaybackSequenceStore
        {
            get
            {
                var finalPath = KnownLocations.GetNowPlayingJsonFile();
                return finalPath;
            }
        }

        public IPlaybackSequenceProvider GetPlaybackSequence(string filename)
        {
            return new JsonPlaybackSequenceManager(filename);
        }
    }
}
