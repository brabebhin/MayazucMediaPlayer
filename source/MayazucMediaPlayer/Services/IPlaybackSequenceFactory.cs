
namespace MayazucMediaPlayer.Services
{
    public interface IPlaybackSequenceProviderFactory
    {
        IPlaybackSequenceProvider GetPlaybackSequence(string filename);
    }
}