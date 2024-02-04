namespace MayazucMediaPlayer.Navigation
{
    public sealed class CommandNavigationData
    {
        public string CommandName
        {
            get;
            private set;
        }


        public object NavigationParameter
        {
            get;
            private set;
        }

        public bool Handeled { get; set; }


        public CommandNavigationData(string cmd, object param)
        {
            CommandName = cmd;
            NavigationParameter = param;
        }
    }
}
