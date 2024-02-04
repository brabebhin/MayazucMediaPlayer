namespace MayazucMediaPlayer.MediaPlayback
{
    public class AvEffectDefinition
    {
        public AvEffectDefinition(string _filterName, string _configString)
        {
            Configuration = _configString;
            FilterName = _filterName;
        }

        public string FilterName { get; }

        public string Configuration { get; }
    }
}