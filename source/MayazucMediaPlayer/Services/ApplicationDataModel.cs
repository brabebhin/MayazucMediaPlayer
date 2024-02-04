using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;

namespace MayazucMediaPlayer.Services
{
    public class ApplicationDataModel
    {
        public PlaybackSequenceService PlaybackModel
        {
            get;
            private set;
        }


        public IOpenSubtitlesAgent SubtitlesAgent
        {
            get;
            private set;
        }

        public ApplicationDataModel(PlaybackSequenceService models,
            IOpenSubtitlesAgent subsAgent)
        {
            PlaybackModel = models;
            SubtitlesAgent = subsAgent;
        }
    }
}
