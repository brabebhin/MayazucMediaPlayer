using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.NowPlayingViews
{
    public partial class NowPlayingCommandBarViewModel : ServiceBase
    {
        public NowPlayingCommandBarViewModel(DispatcherQueue dp) : base(dp)
        {
            CreatePlaybackCommands();
        }

        public IRelayCommand<object> PlayPauseCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> SkipNextCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> SkipPreviousCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> RepeatCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> ShuffleCommand
        {
            get;
            private set;
        }

        private void CreatePlaybackCommands()
        {
            PlayPauseCommand = new AsyncRelayCommand<object>(PlayPauSeButton);
            SkipNextCommand = new AsyncRelayCommand<object>(SkipNextButton);
            SkipPreviousCommand = new AsyncRelayCommand<object>(SkipPreviousButton);

            ShuffleCommand = new RelayCommand<object>(ToggleShuffle);
            RepeatCommand = new RelayCommand<object>(ToggleRepeat);
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

            await AppState.Current.MediaServiceConnector.PlayPauseAutoSwitch();
        }

        private void ChangeRepeatMode()
        {
            var isRepeating = SettingsService.Instance.RepeatMode;
            switch (isRepeating)
            {
                case Constants.RepeatOne:
                    SettingsService.Instance.RepeatMode = Constants.RepeatNone;

                    break;
                case Constants.RepeatNone:

                    SettingsService.Instance.RepeatMode = Constants.RepeatAll;

                    break;
                case Constants.RepeatAll:

                    SettingsService.Instance.RepeatMode = Constants.RepeatOne;

                    break;

                default:

                    SettingsService.Instance.RepeatMode = Constants.RepeatAll;

                    break;

            }
        }

        private void ChangeSHuffletMode()
        {
            var isShuffling = SettingsService.Instance.ShuffleMode;
            if (isShuffling)
            {
                SettingsService.Instance.ShuffleMode = false;
            }
            else
            {

                SettingsService.Instance.ShuffleMode = true;
            }
        }
    }
}
