using MayazucMediaPlayer.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Media.PlayTo;

namespace MayazucMediaPlayer.MediaPlayback.PlayTo
{
    public class MayazucPlayToReciever : IAsyncDisposable
    {
        public event EventHandler<PlayToRecieverSourceReadyEventArgs> SourceReady;

        readonly PlayToMediaPlaybackItemBuilder itemBuilder = new PlayToMediaPlaybackItemBuilder();
        bool IsReceiverStarted = false;
        bool IsSeeking = false;
        bool justLoadedMedia = false;
        bool IsPlayReceivedPreMediaLoaded = false;

        public PlaybackSequenceService PlaybackServiceInstance
        {
            get;
            private set;
        }

        public DispatcherQueue Dispatcher
        {
            get;
            private set;
        }

        public AsyncCommandDispatcher MediaCommandsDispatcher
        {
            get;
            private set;
        }

        public MediaPlayer CurrentPlayer
        {
            get;
            private set;
        }

        public PlayToConfiguration Configuration
        {
            get;
            private set;
        }

        private readonly ulong WindowId = 0;

        public MayazucPlayToReciever(DispatcherQueue dispatcher,
            MediaPlayer currentPlayer,
            PlayToConfiguration configuration,
            AsyncCommandDispatcher mediaCommandsDispatcher,
            PlaybackSequenceService playbackService,
            ulong windowId)
        {
            PlaybackServiceInstance = playbackService;
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            CurrentPlayer = currentPlayer ?? throw new ArgumentNullException(nameof(currentPlayer));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            MediaCommandsDispatcher = mediaCommandsDispatcher ?? throw new ArgumentNullException(nameof(MediaCommandsDispatcher));

            WindowId = windowId;
        }

        public PlayToReceiver Reciever
        {
            get;
            private set;
        }

        public async Task StartAsync()
        {
            await Dispatcher.EnqueueAsync(async () =>
            {
                try
                {
                    await Reciever.StartAsync();
                    IsReceiverStarted = true;
                }
                catch
                {
                    IsReceiverStarted = false;
                }
            });
        }

        public async ValueTask DisposeAsync()
        {
            await Dispatcher.EnqueueAsync(async () =>
            {
                try
                {
                    Reciever.NotifyPaused();
                    Reciever.NotifyStopped();
                    IsReceiverStarted = false;
                    await Reciever.StopAsync();
                    UnsubscribeToMediaPlayerEvents();
                }
                catch
                {
                    IsReceiverStarted = true;
                }
            });
        }

        public async Task InitializeAsync()
        {
            await Dispatcher.EnqueueAsync(() =>
            {
                Reciever = new PlayToReceiver();
                Reciever.SourceChangeRequested += receiver_SourceChangeRequested;

                Reciever.CurrentTimeChangeRequested += Reciever_CurrentTimeChangeRequested;
                Reciever.MuteChangeRequested += receiver_MuteChangeRequested;
                Reciever.PauseRequested += receiver_PauseRequested;
                Reciever.PlaybackRateChangeRequested += receiver_PlaybackRateChangeRequested;
                Reciever.PlayRequested += receiver_PlayRequested;
                Reciever.StopRequested += receiver_StopRequested;
                Reciever.TimeUpdateRequested += receiver_TimeUpdateRequested;
                Reciever.VolumeChangeRequested += receiver_VolumeChangeRequested;

                Reciever.FriendlyName = $"MCMediaCenter-{Configuration.InstanceName}";

                Reciever.SupportsAudio = true;
                Reciever.SupportsVideo = true;
                Reciever.SupportsImage = false;

                SubscribeToMediaPlayerEvents();
            });
        }

        private void SubscribeToMediaPlayerEvents()
        {
            CurrentPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            CurrentPlayer.MediaEnded += CurrentPlayer_MediaEnded;
            CurrentPlayer.MediaFailed += CurrentPlayer_MediaFailed;
            CurrentPlayer.MediaOpened += CurrentPlayer_MediaOpened;
            CurrentPlayer.MediaPlayerRateChanged += CurrentPlayer_MediaPlayerRateChanged;
            CurrentPlayer.SeekCompleted += CurrentPlayer_SeekCompleted;
            CurrentPlayer.VolumeChanged += CurrentPlayer_VolumeChanged;
        }


        private void UnsubscribeToMediaPlayerEvents()
        {
            CurrentPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;
            CurrentPlayer.MediaEnded -= CurrentPlayer_MediaEnded;
            CurrentPlayer.MediaFailed -= CurrentPlayer_MediaFailed;
            CurrentPlayer.MediaOpened -= CurrentPlayer_MediaOpened;
            CurrentPlayer.MediaPlayerRateChanged -= CurrentPlayer_MediaPlayerRateChanged;
            CurrentPlayer.SeekCompleted -= CurrentPlayer_SeekCompleted;
            CurrentPlayer.VolumeChanged -= CurrentPlayer_VolumeChanged;
        }

        private async void receiver_SourceChangeRequested(PlayToReceiver sender, SourceChangeRequestedEventArgs args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                if (args.Stream != null)
                {
                    UnsubscribeToMediaPlayerEvents();
                    IsPlayReceivedPreMediaLoaded = false;
                    justLoadedMedia = true;
                    var playbackList = new PlayToMediaPlaybackListAdapter(CurrentPlayer, itemBuilder, args, Reciever, PlaybackServiceInstance, WindowId);
                    playbackList.AttachingToMediaPlayer += PlaybackList_AttachingToMediaPlayer;
                    SourceReady.Invoke(this, new PlayToRecieverSourceReadyEventArgs(playbackList));
                }
            });
        }

        private void PlaybackList_AttachingToMediaPlayer(object? sender, EventArgs e)
        {
            SubscribeToMediaPlayerEvents();
        }

        private async void CurrentPlayer_VolumeChanged(MediaPlayer sender, object args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                if (IsReceiverStarted)
                {
                    Reciever.NotifyVolumeChange(sender.Volume, sender.IsMuted);
                }
            });
        }

        private async void CurrentPlayer_SeekCompleted(MediaPlayer sender, object args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                if (IsReceiverStarted)
                {
                    try
                    {
                        if (!IsSeeking)
                        {
                            Reciever.NotifySeeking();
                        }
                        Reciever.NotifySeeked();
                        IsSeeking = false;
                    }
                    catch
                    {
                    }
                }
            });
        }

        private async void CurrentPlayer_MediaPlayerRateChanged(MediaPlayer sender, MediaPlayerRateChangedEventArgs args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                if (IsReceiverStarted)
                {
                    Reciever.NotifyRateChange(sender.PlaybackSession.PlaybackRate);
                }
            });
        }

        private async void CurrentPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                if (IsReceiverStarted)
                {
                    Reciever.NotifyLoadedMetadata();
                    Reciever.NotifyDurationChange(CurrentPlayer.PlaybackSession.NaturalDuration);
                    if (IsPlayReceivedPreMediaLoaded == true)
                    {
                        CurrentPlayer.Play();
                    }
                }
            });
        }

        private async void CurrentPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                if (IsReceiverStarted)
                {
                    Reciever.NotifyError();
                }
            });
        }

        private async void CurrentPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                UnsubscribeToMediaPlayerEvents();
                if (IsReceiverStarted)
                {
                    Reciever.NotifyEnded();
                }
            });
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                if (IsReceiverStarted)
                {
                    switch (CurrentPlayer.PlaybackSession.PlaybackState)
                    {
                        case MediaPlaybackState.Playing:
                            Reciever.NotifyPlaying();
                            break;
                        case MediaPlaybackState.Paused:
                            if (justLoadedMedia)
                            {
                                Reciever.NotifyStopped();
                                justLoadedMedia = false;
                            }
                            else
                            {
                                Reciever.NotifyPaused();
                            }
                            break;
                        default:
                            break;
                    }
                }
            });
        }


        private async void receiver_VolumeChangeRequested(PlayToReceiver sender, VolumeChangeRequestedEventArgs args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                CurrentPlayer.Volume = args.Volume;
            });
        }

        private async void receiver_TimeUpdateRequested(PlayToReceiver sender, object args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(async () =>
            {
                await Dispatcher.EnqueueAsync(() =>
                {
                    if (CurrentPlayer.PlaybackSession.Position != null)
                        Reciever.NotifyTimeUpdate(CurrentPlayer.PlaybackSession.Position);
                });
            });
        }

        private async void receiver_StopRequested(PlayToReceiver sender, object args)
        {
            //stop
            await MediaCommandsDispatcher.EnqueueAsync(async () =>
            {
                CurrentPlayer.Pause();
                await Dispatcher.EnqueueAsync(() =>
                {
                    Reciever.NotifyStopped();
                });
            });
        }


        private async void receiver_PlayRequested(PlayToReceiver sender, object args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                CurrentPlayer.Play();
            });
        }

        private async void receiver_PlaybackRateChangeRequested(PlayToReceiver sender, PlaybackRateChangeRequestedEventArgs args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                CurrentPlayer.PlaybackSession.PlaybackRate = args.Rate;
            });
        }

        private async void receiver_MuteChangeRequested(PlayToReceiver sender, MuteChangeRequestedEventArgs args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                CurrentPlayer.IsMuted = args.Mute;
            });
        }

        private async void receiver_PauseRequested(PlayToReceiver sender, object args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(() =>
            {
                CurrentPlayer.Pause();
            });
        }

        private async void Reciever_CurrentTimeChangeRequested(PlayToReceiver sender, CurrentTimeChangeRequestedEventArgs args)
        {
            await MediaCommandsDispatcher.EnqueueAsync(async () =>
            {
                await Dispatcher.EnqueueAsync(() =>
                {
                    CurrentPlayer.PlaybackSession.Position = args.Time;
                    Reciever.NotifySeeking();
                    IsSeeking = true;
                });
            });
        }
    }
}
