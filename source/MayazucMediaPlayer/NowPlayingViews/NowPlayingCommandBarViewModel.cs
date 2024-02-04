using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Dispatching;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.NowPlayingViews
{
    public class NowPlayingCommandBarViewModel : ServiceBase
    {
        public NowPlayingCommandBarViewModel(DispatcherQueue dp) : base(dp)
        {
            CreatePlaybackCommands();
        }

        public CommandBase PlayPauseCommand
        {
            get;
            private set;
        }

        public CommandBase SkipNextCommand
        {
            get;
            private set;
        }

        public CommandBase SkipPreviousCommand
        {
            get;
            private set;
        }

        public CommandBase RepeatCommand
        {
            get;
            private set;
        }

        public CommandBase ShuffleCommand
        {
            get;
            private set;
        }

        private void CreatePlaybackCommands()
        {
            PlayPauseCommand = new AsyncRelayCommand(PlayPauSeButton);
            SkipNextCommand = new AsyncRelayCommand(SkipNextButton);
            SkipPreviousCommand = new AsyncRelayCommand(SkipPreviousButton);

            ShuffleCommand = new RelayCommand(ToggleShuffle);
            RepeatCommand = new RelayCommand(ToggleRepeat);
        }

        private async Task SkipPreviousButton(object? sender)
        {
            await AppState.Current.MediaServiceConnector.SkipPrevious();
        }

        private async Task SkipNextButton(object? sender)
        {
            await AppState.Current.MediaServiceConnector.SkipNext();
        }

        private async Task PlayPauSeButton(object? sender)
        {
            await SkipPlayBar();
        }

        private void ToggleShuffle(object? sender)
        {
            ChangeSHuffletMode();
        }

        private void ToggleRepeat(object? sender)
        {
            ChangeRepeatMode();
        }

        internal static async Task SkipPlayBar()
        {
            var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
            var player = AppState.Current.MediaServiceConnector.CurrentPlayer;
            if (session.PlaybackState == MediaPlaybackState.Playing)
            {
                player?.Pause();
                return;
            }

            await AppState.Current.MediaServiceConnector.SendPlayPause();
        }

        private void ChangeRepeatMode()
        {
            var isRepeating = SettingsWrapper.RepeatMode;
            switch (isRepeating)
            {
                case Constants.RepeatOne:
                    SettingsWrapper.RepeatMode = Constants.RepeatNone;

                    break;
                case Constants.RepeatNone:

                    SettingsWrapper.RepeatMode = Constants.RepeatAll;

                    break;
                case Constants.RepeatAll:

                    SettingsWrapper.RepeatMode = Constants.RepeatOne;

                    break;

                default:

                    SettingsWrapper.RepeatMode = Constants.RepeatAll;

                    break;

            }
        }

        private void ChangeSHuffletMode()
        {
            var isShuffling = SettingsWrapper.ShuffleMode;
            if (isShuffling)
            {
                SettingsWrapper.ShuffleMode = false;
            }
            else
            {

                SettingsWrapper.ShuffleMode = true;
            }
        }
    }
}
