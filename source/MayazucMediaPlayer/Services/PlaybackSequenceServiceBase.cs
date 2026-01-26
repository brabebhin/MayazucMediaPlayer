using Microsoft.UI.Dispatching;

namespace MayazucMediaPlayer.Services
{
    public abstract class PlaybackSequenceServiceBase : ServiceBase
    {
        public PlaybackSequenceService PlaybackServiceInstance
        {
            get;
            protected set;
        }

        public PlaybackSequenceServiceBase(DispatcherQueue dispatcherQueue) : base(dispatcherQueue)
        {

        }

        public PlaybackSequenceServiceBase(DispatcherQueue dispatcherQueue, PlaybackSequenceService playbackSequenceService) : base(dispatcherQueue)
        {
            PlaybackServiceInstance = playbackSequenceService;
        }
    }
}
