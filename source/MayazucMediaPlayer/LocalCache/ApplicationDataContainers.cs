using Windows.Storage;

namespace MayazucMediaPlayer.LocalCache
{
    internal static class ApplicationDataContainers
    {
        public static ApplicationDataContainer AudioEffectsSlim
        {
            get
            {
                return ApplicationData.Current.LocalSettings.CreateContainer("CustomAudioEffectsSlim", ApplicationDataCreateDisposition.Always);
            }
        }

        public static ApplicationDataContainer EchoEffectsContainer
        {
            get
            {
                return AudioEffectsSlim.CreateContainer("aecho", ApplicationDataCreateDisposition.Always);
            }
        }

        public static ApplicationDataContainer ExtraStereoEffectsContainer
        {
            get
            {
                return AudioEffectsSlim.CreateContainer("extrastereo", ApplicationDataCreateDisposition.Always);
            }
        }
    }
}
