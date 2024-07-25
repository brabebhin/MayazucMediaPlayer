using MayazucMediaPlayer.Settings;

namespace MayazucMediaPlayer.MediaPlayback.PlayTo
{
    public class PlayToConfiguration
    {
        public string InstanceName
        {
            get
            {
                return SettingsService.Instance.NetworkInstanceName;
            }
        }


    }
}
