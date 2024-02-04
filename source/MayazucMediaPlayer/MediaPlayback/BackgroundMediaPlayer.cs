﻿using CommunityToolkit.WinUI;
using FFmpegInteropX;
using MayazucMediaPlayer.AudioEffects;
using MayazucMediaPlayer.BackgroundServices;
using MayazucMediaPlayer.Helpers;
using MayazucMediaPlayer.MediaPlayback.MediaSequencer;
using MayazucMediaPlayer.MediaPlayback.PlayTo;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.VideoEffects;
using Microsoft.UI.Dispatching;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback
{
    public class BackgroundMediaPlayer : IBackgroundPlayer
    {
        private const int MediaPlayerAsyncOperationTimeoutInSeconds = 25;
        CommandDispatcher commandDispatcher = new CommandDispatcher();
        StopMusicOnTimerService stopMusicOnTimerService;
        public VideoEffectProcessorConfiguration VideoEffectsConfiguration { get; private set; } = new VideoEffectProcessorConfiguration();

        public EqualizerConfiguration CurrentEqualizerConfiguration
        {
            get;
            private set;
        }

        public PlaybackSequenceService PlaybackQueueService
        {
            get;
            private set;
        }

        public PlaylistsService PlaylistsService
        {
            get;
            private set;
        }

        public MayazucPlayToReciever PlayToRecieverInstance
        {
            get;
            private set;
        }

        readonly AsyncLock mediaOpenedLock = new AsyncLock();
        readonly AsyncLock playToReceiverLock = new AsyncLock();

        public MediaPlaybackItem? CurrentPlaybackItem
        {
            get
            {
                if (PlaybackListAdapter != null)
                {
                    return PlaybackListAdapter.CurrentPlaybackItem;
                }
                return null;
            }
        }

        AdaptiveEqualizationConfiguration savedAdaptiveValues;

        SystemMediaTransportControls controls;

        public event EventHandler<object> OnNullPlaybackItem;

        public DispatcherQueue DispatcherUiThread
        {
            get;
            private set;
        }

        IMediaPlayerItemSource currentPlaybackData;
        public IMediaPlayerItemSource CurrentPlaybackData
        {
            get
            {
                return currentPlaybackData;
            }
            private set
            {
                if (currentPlaybackData != null)
                {
                    currentPlaybackData.MetadataChanged -= CurrentPlaybackData_MetadataChanged;
                }
                currentPlaybackData = value;
                if (currentPlaybackData != null)
                {
                    currentPlaybackData.MetadataChanged += CurrentPlaybackData_MetadataChanged;
                }
            }
        }

        private async void CurrentPlaybackData_MetadataChanged(object? sender, EmbeddedMetadataResult e)
        {
            await commandDispatcher.EnqueueAsync(async () =>
            {
                if (sender == currentPlaybackData)
                    await HandleMetadataChanged(PlaybackListAdapter, CurrentPlaybackItem, CurrentPlaybackData);
            });
        }

        public event TypedEventHandler<MediaPlayer, MediaOpenedEventArgs> OnMediaOpened;
        public event EventHandler<PlaybackQueueTypeChangedArgs> PlaybackQueueTypeChanged;

        internal FFmpegInteropItemBuilder ItemBuilder
        {
            get;
            private set;
        }


        IMediaPlaybackListAdapter _playbackSource;
        private bool disposedValue;

        private IMediaPlaybackListAdapter PlaybackListAdapter
        {
            get => _playbackSource;
            set
            {
                if (value != _playbackSource)
                {
                    if (_playbackSource != null)
                    {
                        _playbackSource.CurrentPlaybackItemChanged -= PlaybackListAdapter_CurrentPlaybackItemChanged;
                        _playbackSource.Stop();
                        _playbackSource.Dispose();
                    }
                    _playbackSource = value;
                    CurrentPlayer.AutoPlay = false;
                    _playbackSource.CurrentPlaybackItemChanged += PlaybackListAdapter_CurrentPlaybackItemChanged;
                }
            }
        }

        public MediaPlayer CurrentPlayer { private set; get; }

        public FFmpegMediaSource FfmpegInteropInstance
        {
            get
            {
                if (CurrentPlaybackItem == null) return null;
                return CurrentPlaybackItem.GetExtradata().FFmpegMediaSource;
            }
        }

        public BusyFlag DataBusyFlag
        {
            get
            {
                if (PlaybackListAdapter != null)
                {
                    return PlaybackListAdapter.IsBusy;
                }
                return null;
            }
        }

        public EqualizerService EqualizerService
        {
            get;
            private set;
        }

        public BackgroundMediaPlayer(DispatcherQueue dispatcher,
            PlaybackSequenceService playbackModels,
            EqualizerService equalizerService,
            PlaylistsService playlistsService)
        {
            DispatcherUiThread = dispatcher;
            PlaybackQueueService = playbackModels;
            EqualizerService = equalizerService;
            PlaylistsService = playlistsService;
        }

        private void InitPlayer(IntPtr hwnd)
        {
            CurrentPlayer = new MediaPlayer();
            VideoEffectsConfiguration.ConfigurationChanged += VideoEffectsConfiguration_ConfigurationChanged;

            SettingsWrapper.RegisterSettingChangeCallback(nameof(SettingsWrapper.AutoPlayVideo),
                SettingsWrapper.RegisterSettingChangeCallback(nameof(SettingsWrapper.AutoPlayMusic), async (s, e) =>
                {
                    await AutoPlayChanged();
                }));

            SettingsWrapper.RegisterSettingChangeCallback(nameof(SettingsWrapper.PreventSubtitleOverlaps), (s, e) =>
            {
                commandDispatcher.EnqueueAsync(() =>
                {
                    try
                    {
                        var instance = FfmpegInteropInstance;
                        if (instance != null)
                            instance.Configuration.Subtitles.PreventModifiedSubtitleDurationOverlap = SettingsWrapper.PreventSubtitleOverlaps;
                    }
                    catch { }
                });
            });

            CurrentPlayer.MediaOpened += CurrentPlayer_MediaOpened;
            CurrentPlayer.PlaybackSession.PlaybackStateChanged += OnPlaybackStateChangedAsync;
            CurrentPlayer.IsLoopingEnabled = false;
            CurrentPlayer.AutoPlay = true;
            CurrentPlayer.IsMuted = false;
            CurrentPlayer.AudioBalance = SettingsWrapper.AudioBalance;
            CurrentPlayer.PlaybackSession.PlaybackRateChanged += PlaybackSession_PlaybackRateChanged;

            controls = SystemMediaTransportControlsInterop.GetForWindow(hwnd);

            controls.DisplayUpdater.Type = MediaPlaybackType.Music;
            controls.IsEnabled = true;
            controls.IsPlayEnabled = true;
            controls.IsPauseEnabled = true;
            controls.IsNextEnabled = true;
            controls.IsPreviousEnabled = true;
            controls.ButtonPressed += Controls_ButtonPressed;
            controls.PlaybackPositionChangeRequested += Controls_PlaybackPositionChangeRequested;

            if (SettingsWrapper.StopMusicOnTimerEnabled)
            {
                RestartStopMusicOnTimerService();
            }

            ItemBuilder = new FFmpegInteropItemBuilder(EqualizerService);
            PlaybackListAdapter = new MediaSequencerMediaPlaylistAdapter(PlaybackQueueService, ItemBuilder, CurrentPlayer, DispatcherUiThread, commandDispatcher, VideoEffectsConfiguration);

            if (SettingsWrapper.PlayToEnabled) _ = StartDlnaSink();

            SettingsWrapper.RegisterSettingChangeCallback(nameof(SettingsWrapper.PlayToEnabled), (s, e) =>
            {
                var value = (bool)s;
                if (value)
                    _ = StartDlnaSink();
                else _ = StopDlnaSink();
            });
        }

        private void PlaybackSession_PlaybackRateChanged(MediaPlaybackSession sender, object args)
        {
            commandDispatcher.EnqueueAsync(async () =>
            {
                var unnaturalRate = sender.PlaybackRate != 1.0;
                var currentItem = CurrentPlaybackItem;
                if (currentItem != null)
                {
                    bool hasVideo = currentItem.IsVideo();

                    if (hasVideo && unnaturalRate)
                    {
                        await ReloadCurrentPlaybackItem(currentItem);
                    }
                    else
                    {
                        await PlaybackListAdapter.ReloadNextItemAsync(CurrenItem: currentItem,
                            userAction: false,
                            changeIndex: true,
                            currentIndex: SettingsWrapper.PlaybackIndex);
                    }
                }

            });
        }

        private async void Controls_PlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        {
            await Seek(args.RequestedPlaybackPosition, true);
        }

        private async void Controls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await OnPlayRecieved();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await OnPauseRecieved();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    await OnNextRecieved();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    await OnPreviousRecieved();
                    break;
            }
        }

        private async void CurrentPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            await commandDispatcher.EnqueueAsync(async () =>
            {
                OnMediaOpened?.Invoke(sender, new MediaOpenedEventArgs(MediaOpenedEventReason.MediaPlayerObjectRequested, null));
                await PlaybackQueueService.HandleMediaOpened(sender, new MediaOpenedEventArgs(MediaOpenedEventReason.MediaPlayerObjectRequested, null));
            });
        }

        private void RestartStopMusicOnTimerService()
        {
            stopMusicOnTimerService = new StopMusicOnTimerService();
            _ = stopMusicOnTimerService.StartService(SettingsWrapper.StopMusicOnTimerPosition);
        }

        private void VideoEffectsConfiguration_ConfigurationChanged(object? sender, string e)
        {
            commandDispatcher.EnqueueAsync(async () =>
            {
                if (e == nameof(VideoEffectsConfiguration.MasterSwitch))
                {
                    var hasFilters = VideoEffectsConfiguration.MasterSwitch;
                    var currentItem = CurrentPlaybackItem;
                    if (currentItem != null)
                    {
                        bool hasVideo = currentItem.IsVideo();

                        if (hasVideo)
                        {
                            await ReloadCurrentPlaybackItem(currentItem);
                        }
                        else
                        {
                            await PlaybackListAdapter.ReloadNextItemAsync(CurrenItem: currentItem,
                                userAction: false,
                                changeIndex: true,
                                currentIndex: SettingsWrapper.PlaybackIndex);
                        }
                    }
                }
            });
        }

        private async Task ReloadCurrentPlaybackItem(MediaPlaybackItem item)
        {
            var initialState = CurrentPlayer.PlaybackSession.PlaybackState;
            CurrentPlayer.Pause();

            var resumeRequest = new ResumeRequest()
            {
                StartIndex = PlaybackQueueService.NowPlayingBackStore.IndexOfMediaData(CurrentPlaybackData),
                StartTime = CurrentPlayer.PlaybackSession.Position.Ticks,
                InitialItem = _playbackSource?.DetachCurrentItem()
            };

            await ResumeAsyncInternal(resumeRequest, autoPlay: initialState == MediaPlaybackState.Playing);
        }

        private async void CommandManager_PositionReceived(MediaPlaybackCommandManager sender, MediaPlaybackCommandManagerPositionReceivedEventArgs args)
        {
            using (var deferal = args.GetDeferral())
            {
                await Seek(args.Position, true);
            }
        }

        private void PlayToRecieverInstance_SourceReady(object? sender, PlayToRecieverSourceReadyEventArgs e)
        {
            _ = commandDispatcher.EnqueueAsync(async () =>
             {
                 CurrentPlayer.Pause();
                 if (!CurrentPlaybackDataIsDlna())
                 {
                     SettingsWrapper.ResumePositionDlnaIntrerupt = CurrentPlayer.PlaybackSession.Position.Ticks;
                     SettingsWrapper.PlaybackIndexDlnaIntrerupt = SettingsWrapper.PlaybackIndex;
                 }
                 PlaybackListAdapter = e.PlaybackSource;
                 try
                 {
                     //ShiftTracks: false, userAction: true, incrementIndex: true, autoPlayTrack: true
                     if (CurrentPlayer.Source == null)
                     {
                         if (!await PlaybackListAdapter.Start(SettingsWrapper.PlaybackIndex))
                         {
                             throw new InvalidOperationException();
                         }
                     }

                     PlaybackQueueTypeChanged?.Invoke(this, new PlaybackQueueTypeChangedArgs(QueueType.DLNA));
                     await PlaybackQueueService.NowPlayingBackStore.LoadSequenceAsync();// (NowPlayingManager.GetNowPlaying());
                 }
                 catch { }
             });
        }

        private void PlaybackListAdapter_CurrentPlaybackItemChanged(object? sender, MayazucCurrentMediaPlaybackItemChangedEventArgs e)
        {
            commandDispatcher.EnqueueAsync(async () =>
            {
                //whats up mon?
                using (await mediaOpenedLock.LockAsync())
                {
                    try
                    {
                        await HandleMediaOpened(CurrentPlayer, e);
                    }
                    catch { }
                }
            });
        }

        private void DestroySource()
        {
            PlaybackListAdapter.Stop();
            PlaybackListAdapter.Dispose();
            controls.DisplayUpdater.Thumbnail = null;
            controls.DisplayUpdater.Update();
        }

        private async Task ResetToLocalPlaybackAdapter()
        {
            PlaybackListAdapter = new MediaSequencerMediaPlaylistAdapter(PlaybackQueueService, ItemBuilder, CurrentPlayer, DispatcherUiThread, commandDispatcher, VideoEffectsConfiguration);
            PlaybackQueueTypeChanged?.Invoke(this, new PlaybackQueueTypeChangedArgs(QueueType.Local));
            await PlaybackQueueService.NowPlayingBackStore.LoadSequenceAsync();// (NowPlayingManager.GetNowPlaying());
        }

        public Task HandleStopMusicOnTimer()
        {
            return commandDispatcher.EnqueueAsync(() =>
            {
                stopMusicOnTimerService?.StopService();

                if (SettingsWrapper.StopMusicOnTimerEnabled)
                {
                    RestartStopMusicOnTimerService();
                }
            });
        }


        /// <summary>
        /// DONE
        /// Strategy: only to be used while seeking from external sources (like seekbar, jump previous/next)
        /// </summary>
        /// <param name="position"></param>
        /// <param name="userAction"></param>
        /// <returns></returns>
        public Task Seek(TimeSpan position, bool userAction)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                //await SeekInternal(position, userAction, CurrentPlaybackItem, false);

                if (CurrentPlaybackItem == null)
                {
                    return;
                }

                if (CanSeekToPosition(position, CurrentPlaybackItem))
                {
                    bool autoPlayAfterSeek = SettingsWrapper.StartPlaybackAfterSeek;
                    await AwaitForSeek(position);
                    if (autoPlayAfterSeek)
                    {
                        SettingsWrapper.PlayerResumePosition = 0;
                        CurrentPlayer.Play();
                    }
                }

            });
        }

        private async Task AwaitForSeek(TimeSpan position)
        {
            await DispatcherUiThread.EnqueueAsync(async () =>
            {
                await CurrentPlayer.SeekAsync(position, TimeSpan.FromSeconds(MediaPlayerAsyncOperationTimeoutInSeconds));
            });
        }

        public Task ResetFiltering(bool enabled)
        {
            return commandDispatcher.EnqueueAsync(() =>
            {
                SignalResetFilteringInternal(enabled, PlaybackListAdapter);
            });
        }

        private void SignalResetFilteringInternal(bool enabled, IMediaPlaybackListAdapter PlaybackListAdapter)
        {
            if (enabled)
            {
                SetFilters(PlaybackListAdapter);
            }
            else
            {
                PlaybackListAdapter?.RemoveFFmpegAudioEffects();
                PlaybackListAdapter?.RemoveFFmpegVideoEffects();
            }
        }

        /// <summary>
        /// Strategy: skip to given index
        /// </summary>
        /// <returns></returns>
        public Task SkipToIndex(int index)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                await SignalSkipToIndexInternal(true, index);
            });
        }

        /// <summary>
        /// Strategy: skip to given index
        /// 
        /// The index to skip is set by SettingsWrapper.PlaybackIndex. We might want to fix that
        /// </summary>
        /// <returns></returns>
        private async Task SignalSkipToIndexInternal(bool autoPlay, int index)
        {
            if (!await PlaybackListAdapter.BackstoreHasItems())
            {
                return;
            }

            SettingsWrapper.PlaybackIndex = index;
            SettingsWrapper.PlayerResumePosition = 0;

            try
            {
                if (await PlaybackListAdapter.BackstoreHasItems())
                {
                    var eventData = await WaitForItemChangedOrFail(async () =>
                    {
                        if (!await PlaybackListAdapter.Start(SettingsWrapper.PlaybackIndex))
                        {
                            throw new InvalidOperationException();
                        }
                    }, PlaybackListAdapter);

                    if (eventData != null && autoPlay)
                    {
                        CurrentPlayer.Play();
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// DONE
        /// Strategy: if there's already a track playing, and time is less than 5 seconds, skip previous
        /// Strategy: if there's already a track playing, and time is bigger than 5 seconds, seek to 0
        /// Strategy: skip to previous index if not playing.
        /// </summary>
        /// <returns></returns>
        public Task SkipPrevious()
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                await SignalSkipPreviousInternal();
            });
        }

        /// <summary>
        /// DONE
        /// Strategy: if there's already a track playing, and time is less than 5 seconds, skip previous
        /// Strategy: if there's already a track playing, and time is bigger than 5 seconds, seek to 0
        /// Strategy: skip to previous index if not playing.
        /// </summary>
        /// <returns></returns>
        private async Task SignalSkipPreviousInternal()
        {
            if (!await PlaybackListAdapter.BackstoreHasItems())
            {
                return;
            }

            SettingsWrapper.PlayerResumePosition = 0;

            var speedFactor = CurrentPlayer.PlaybackSession.PlaybackRate;
            if (speedFactor < 1)
            {
                speedFactor = 1;
            }

            if (await PlaybackListAdapter.CanGoBack() && CurrentPlayer.PlaybackSession.Position.Ticks < TimeSpan.FromSeconds(5 * speedFactor).Ticks)
            {
                var currentIndex = SettingsWrapper.PlaybackIndex;
                var skipIndex = currentIndex == 0 ? PlaybackQueueService.NowPlayingBackStore.Count - 1 : currentIndex - 1;
                await SignalSkipToIndexInternal(true, skipIndex);
            }
            else
            {
                CurrentPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            }

            CurrentPlayer.Play();
        }

        /// <summary>
        /// Strategy: destory source, recreate source, recreate current item, seek to position, play
        /// </summary>
        /// <returns></returns>
        public Task RestartCurrentItemAtCurrentPosition()
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                var RestartPosition = CurrentPlayer.PlaybackSession.Position.Ticks;
                DestroySource();

                await PlaybackQueueService.LoadNowPlaying();
                await ResetToLocalPlaybackAdapter();

                //ShiftTracks: false, userAction: true, incrementIndex: false, autoPlay: true)
                var eventData = await WaitForItemChangedOrFail(async () =>
                {
                    if (CurrentPlayer.Source == null)
                    {
                        if (!await PlaybackListAdapter.Start(SettingsWrapper.PlaybackIndex))
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }, PlaybackListAdapter);

                if (eventData != null)
                {
                    if (RestartPosition != 0)
                    {
                        var seekTimeSpan = TimeSpan.FromTicks(RestartPosition);
                        if (CanSeekToPosition(seekTimeSpan, CurrentPlaybackItem))
                        {
                            await AwaitForSeek(seekTimeSpan);
                        }
                        RestartPosition = 0;
                    }

                    CurrentPlayer.Play();
                }
            });
        }

        private async Task<MayazucCurrentMediaPlaybackItemChangedEventArgs> WaitForItemChangedOrFail(Func<Task> wrappedAction, IMediaPlaybackListAdapter playlist)
        {
            TaskCompletionSource<MayazucCurrentMediaPlaybackItemChangedEventArgs> itemChangedSignal = new TaskCompletionSource<MayazucCurrentMediaPlaybackItemChangedEventArgs>();

            EventHandler<MayazucCurrentMediaPlaybackItemChangedEventArgs> openedhandler = (s, e) =>
            {
                if (e.NewItem != null)
                {
                    itemChangedSignal.TrySetResult(e);
                }
            };

            EventHandler itemFailed = (s, e) =>
            {
                itemChangedSignal.TrySetResult(null);
            };

            playlist.CurrentPlaybackItemChanged += openedhandler;
            playlist.ItemCreationFailed += itemFailed;
            playlist.Disposing += itemFailed;
            try
            {
                await wrappedAction();
            }
            catch
            {
                itemChangedSignal.TrySetResult(null);
            }
            MayazucCurrentMediaPlaybackItemChangedEventArgs eventData = null;

            if (await PlaybackListAdapter.BackstoreHasItems())
            {
                var watchDogTask = Task.Delay(TimeSpan.FromSeconds(MediaPlayerAsyncOperationTimeoutInSeconds));
                var resultedTask = await Task.WhenAny(watchDogTask, itemChangedSignal.Task);
                if (resultedTask == itemChangedSignal.Task)
                    eventData = await itemChangedSignal.Task;
            }

            PlaybackListAdapter.CurrentPlaybackItemChanged -= openedhandler;
            playlist.ItemCreationFailed -= itemFailed;
            playlist.Disposing -= itemFailed;

            return eventData;
        }

        private async Task ReloadCurrentItem()
        {
            await PlaybackListAdapter.ReloadNextItemAsync(CurrenItem: CurrentPlaybackItem, userAction: true, changeIndex: false, currentIndex: SettingsWrapper.PlaybackIndex);
            await PlaybackListAdapter.MoveToNextItem(CurrentItem: CurrentPlaybackItem, userAction: true, changeIndex: false);
        }

        /// <summary>
        /// Strategy: if there's already a track playing, skip to the next one.
        /// Strategy: skip to next index if not playing.
        /// </summary>
        /// <returns></returns>
        public Task SkipNext()
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                await SignalSkipNextInternal();

            });
        }

        /// <summary>
        /// Strategy: if there's already a track playing, skip to the next one.
        /// Strategy: skip to next index if not playing.
        /// </summary>
        /// <returns></returns>
        private async Task SignalSkipNextInternal()
        {
            if (!await PlaybackListAdapter.BackstoreHasItems())
            {
                return;
            }

            CurrentPlayer.Pause();

            SettingsWrapper.PlayerResumePosition = 0;

            if (SettingsWrapper.RepeatMode == Constants.RepeatOne)
            {
                await PlaybackListAdapter.ReloadNextItemAsync(CurrenItem: CurrentPlaybackItem, userAction: true, changeIndex: true, currentIndex: SettingsWrapper.PlaybackIndex);
            }
            try
            {
                var eventData = await WaitForItemChangedOrFail(async () =>
                {
                    //true true true true               
                    if (CurrentPlayer.Source == null)
                    {
                        if (!await PlaybackListAdapter.Start(SettingsWrapper.PlaybackIndex))
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    else
                    {
                        if (!await PlaybackListAdapter.MoveToNextItem(CurrentItem: CurrentPlaybackItem, userAction: true, changeIndex: true))
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }, PlaybackListAdapter);

                if (eventData != null)
                    CurrentPlayer.Play();
            }
            catch { }
        }

        /// <summary>
        /// Play / pause. Optionally starts playback if player is inactive
        /// </summary>
        /// <param name="forceStart">Forces start of playback if player is not active</param>
        /// <returns></returns>
        public Task PlayPauseAsync(bool forceStart = true)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                await PlayPauseInternal(forceStart);
            });
        }

        private async Task PlayPauseInternal(bool forceStart)
        {
            if (CurrentPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                CurrentPlayer.Pause();
            }
            else if (CurrentPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
            {
                CurrentPlayer.Play();
                CurrentPlayer.AutoPlay = true;
            }
            else if (CurrentPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.None && CurrentPlayer.Source != null)
            {
                CurrentPlayer.Play();
                CurrentPlayer.AutoPlay = true;
            }
            else
            {
                if (forceStart)
                {
                    ResumeRequest request = new ResumeRequest();
                    request.StartTime = SettingsWrapper.PlayerResumePosition;
                    request.StartIndex = SettingsWrapper.PlaybackIndex;
                    await ResumeAsyncInternal(request, true);
                }
            }
        }

        public Task StopPlayback()
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                try
                {
                    SettingsWrapper.PlayerResumePosition = CurrentPlayer.PlaybackSession.Position.Ticks;
                    CurrentPlayer.Pause();
                    CurrentPlayer.Source = null;
                    DestroySource();
                    CurrentPlaybackItem?.Source?.Reset();
                    GC.Collect();
                    await DispatcherUiThread.EnqueueAsync(() =>
                    {
                        PlaybackQueueService.NowPlayingBackStore.ClearSequence();
                        PlaybackQueueService.NowPlayingBackStore.Clear();
                    });
                }
                catch { }
            });
        }

        bool CanSeekToPosition(TimeSpan seek, MediaPlaybackItem item)
        {
            if (CurrentPlayer.PlaybackSession != null && item != null)
            {
                var extraData = item.GetExtradata();
                if (extraData != null)
                {
                    return extraData.FFmpegMediaSource.Duration > seek;
                }
            }
            return false;
        }

        private async Task HandleMediaOpened(MediaPlayer sender, MayazucCurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (!SettingsWrapper.KeepPlaybackRateBetweenTracks && CurrentPlayer.PlaybackSession != null)
            {
                CurrentPlayer.PlaybackSession.PlaybackRate = 1;
            }
            var newItem = args.NewItem;
            if (newItem != null)
            {
                var extraProperties = newItem.GetExtradata();
                SettingsWrapper.PlaybackIndex = PlaybackQueueService.NowPlayingBackStore.IndexOfMediaData(extraProperties.MediaPlayerItemSource);
                var ffmpegInteropInstance = extraProperties.FFmpegMediaSource;
                if (ffmpegInteropInstance != null)
                {
                    ffmpegInteropInstance.PlaybackSession = CurrentPlayer.PlaybackSession;
                    ffmpegInteropInstance.Configuration.General.FastSeek = false;
                    HandleForcedSubtitles(newItem, ffmpegInteropInstance);
                }
                var currentPlaybackData = extraProperties.MediaPlayerItemSource;

                CurrentPlaybackData = currentPlaybackData;
                await HandleMetadataChanged(args.MediaPlaybackListAdapter, newItem, currentPlaybackData);

                SettingsWrapper.PlayerResumePosition = 0;

                var eventData = new NewMediaPlaybackItemOpenedEventArgs(null, newItem, extraProperties);
                var payload = new MediaOpenedEventArgs(MediaOpenedEventReason.MediaPlaybackListItemChanged, eventData);

                await PlaybackQueueService.HandleMediaOpened(sender, payload);

                OnMediaOpened?.Invoke(CurrentPlayer, payload);

            }
            else
            {
                OnNullPlaybackItem?.Invoke(this, null);
            }
        }

        private async Task HandleMetadataChanged(IMediaPlaybackListAdapter MediaPlaybackListAdapter, MediaPlaybackItem currentMediaPlaybackItem, IMediaPlayerItemSource currentPlaybackData)
        {
            await MediaPlaybackItemDisplayPropertiesHelper.SetPlaybackItemMediaProperties(currentMediaPlaybackItem, currentPlaybackData);
            RestoreAdaptiveEqualization();
            await SetAdaptiveEqualization(currentPlaybackData);

            SignalResetFilteringInternal(SettingsWrapper.EqualizerEnabled, MediaPlaybackListAdapter);
            controls.DisplayUpdater.CopyPropertiesFrom(currentMediaPlaybackItem);
            controls.DisplayUpdater.Update();
        }

        private void HandleForcedSubtitles(MediaPlaybackItem e, FFmpegMediaSource FfmpegInteropInstance)
        {
            if (SettingsWrapper.AutoloadForcedSubtitles)
            {
                var language = SettingsWrapper.PreferredSubtitleLanguage.LanguageName;
                for (int i = 0; i < FfmpegInteropInstance.SubtitleStreams.Count; i++)
                {
                    var subStream = FfmpegInteropInstance.SubtitleStreams[i];
                    if (subStream.IsForced)
                    {
                        if (MediaHelperExtensions.CheckSubtitlelanguage(subStream.SubtitleTrack, language))
                        {
                            e.TimedMetadataTracks.SetPresentationMode((uint)i, TimedMetadataTrackPresentationMode.PlatformPresented);
                        }
                    }
                }
            }
        }

        private void SetFilters(IMediaPlaybackListAdapter playbackListAdapter)
        {
            List<AvEffectDefinition> defs = new List<AvEffectDefinition>();
            
            //defs.Add(new AvEffectDefinition("sine", "frequency=8500:beep_factor=4:duration=5"));
            defs.AddRange(MediaHelperExtensions.GetEqualizerEffectDefinitions(EqualizerService.GetCurrentEqualizerConfig()));
            defs.AddRange(MediaHelperExtensions.GetAdditionalEffectsDefinitions());

            playbackListAdapter?.SetFFmpegAudioEffects(defs);
        }

        void GetCurrentEqualizerValues()
        {
            var presetName = string.Empty;
            var currentConfig = EqualizerService.GetCurrentEqualizerConfig();
            var configurationName = currentConfig.Name;
            var amplifications = currentConfig.Bands.Select(x => x.Amplification).ToList().AsReadOnly();
            savedAdaptiveValues = new AdaptiveEqualizationConfiguration(presetName, configurationName, amplifications);
        }

        private async Task SetAdaptiveEqualization(IMediaPlayerItemSource currentPlaybackData)
        {
            GetCurrentEqualizerValues();
            var useAdaptive = SettingsWrapper.AutomaticPresetManagement;
            if (useAdaptive)
            {
                var metadata = await currentPlaybackData.GetMetadataAsync();
                var currentConfig = EqualizerService.GetCurrentEqualizerConfig();
                StringComparer comparer = StringComparer.InvariantCultureIgnoreCase;
                var targetPreset = currentConfig.Presets.FirstOrDefault(x => comparer.Equals(x.MetadataAssociationValue, metadata.Genre));
                string presetName = "default";

                if (targetPreset == null)
                {
                    for (int i = 0; i < currentConfig.Bands.Count; i++)
                    {
                        currentConfig.Bands[i].Amplification = 0;
                    }
                }
                else
                {
                    presetName = targetPreset.PresetName;
                    for (int i = 0; i < currentConfig.Bands.Count; i++)
                    {
                        currentConfig.Bands[i].Amplification = targetPreset.FrequencyValues[i];
                    }
                }

                SettingsWrapper.SelectedEqualizerPreset = presetName;
            }
        }

        void RestoreAdaptiveEqualization()
        {
            var useAdaptive = SettingsWrapper.AutomaticPresetManagement;
            bool canRestore = true;
            var currentConfig = EqualizerService.GetCurrentEqualizerConfig();
            if (savedAdaptiveValues == null) return;
            canRestore = canRestore && currentConfig.Name == savedAdaptiveValues.EqualizerConfiguration;
            var currentPresetName = "default";

            if (useAdaptive && canRestore)
            {
                if (canRestore)
                {
                    currentPresetName = savedAdaptiveValues.PresetName;
                    for (int i = 0; i < savedAdaptiveValues.SavedAmplifications.Count; i++)
                    {
                        currentConfig.Bands[i].Amplification = savedAdaptiveValues.SavedAmplifications[i];
                    }
                }
                else
                {
                    for (int i = 0; i < currentConfig.Bands.Count; i++)
                    {
                        currentConfig.Bands[i].Amplification = 0;
                    }
                }

                SettingsWrapper.SelectedEqualizerPreset = currentPresetName;
            }
        }

        bool StopMusicOnTimerCall(bool userAction)
        {
            if (userAction == false)
            {
                if (SettingsWrapper.StopMusicOnTimerEnabled)
                {
                    var dtNow = DateTime.Now.TimeOfDay;
                    var stopPosition = SettingsWrapper.StopMusicOnTimerPosition;
                    if (dtNow > stopPosition && dtNow < stopPosition.Add(TimeSpan.FromSeconds(25)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task OnNextRecieved()
        {
            await commandDispatcher.EnqueueAsync(async () =>
            {
                await SignalSkipNextInternal();
            });
        }

        private async Task OnPauseRecieved()
        {
            await commandDispatcher.EnqueueAsync(() =>
            {
                CurrentPlayer.Pause();

            });
        }

        private async Task OnPlayRecieved()
        {
            await commandDispatcher.EnqueueAsync(async () =>
            {
                await PlayPauseInternal(true);
            });
        }

        private async Task OnPreviousRecieved()
        {
            await commandDispatcher.EnqueueAsync(async () =>
            {
                await SignalSkipPreviousInternal();
            });
        }

        async void OnPlaybackStateChangedAsync(MediaPlaybackSession sender, object args)
        {
            switch (CurrentPlayer.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Buffering:

                    controls.PlaybackStatus = MediaPlaybackStatus.Paused; break;

                case MediaPlaybackState.Paused:
                    controls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    await DispatcherUiThread.EnqueueAsync(() =>
                    {
                        ScreenSessionManager.Current.RequestScreenOff();
                    });
                    break;
                case MediaPlaybackState.Playing:
                    controls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    await DispatcherUiThread.EnqueueAsync(() =>
                    {
                        if (PlaysVideo())
                            ScreenSessionManager.Current.RequestKeepScreenOn();
                    });
                    break;
                case MediaPlaybackState.None:
                    controls.PlaybackStatus = MediaPlaybackStatus.Closed;

                    break;
                case MediaPlaybackState.Opening:
                    controls.PlaybackStatus = MediaPlaybackStatus.Changing;

                    break;
            }
        }

        private bool PlaysVideo()
        {
            var isVideo = CurrentPlaybackItem?.IsVideo();
            return isVideo.HasValue && isVideo.Value;
        }

        /// <summary>
        /// DONE: Strategy: reload next track in playback list
        /// </summary>
        /// <returns></returns>
        public async Task RepeatModeChanged()
        {
            await SignalReloadNextTrack(false, false);
        }

        /// <summary>
        /// DONE: Strategy: reload next track in playback list
        /// </summary>
        /// <returns></returns>
        public async Task ShuffleModeChanged()
        {
            await SignalReloadNextTrack(false, false);
        }

        /// <summary>
        /// DONE: Strategy: reload next track in playback list
        /// </summary>
        /// <returns></returns>
        public async Task AutoPlayChanged()
        {
            await SignalReloadNextTrack(true, true);
        }

        private Task SignalReloadNextTrack(bool reloadAutoPlay, bool userAction)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                if (!await PlaybackListAdapter.BackstoreHasItems())
                {
                    return;
                }

                await PlaybackListAdapter.ReloadNextItemAsync(CurrenItem: CurrentPlaybackItem, userAction: userAction, changeIndex: true, currentIndex: SettingsWrapper.PlaybackIndex);

            });
        }

        public async Task<TimeSpan> DelaySubtitles()
        {
            var result = await commandDispatcher.EnqueueAsync(() =>
            {
                if (CurrentPlaybackItem != null)
                {
                    var playbackItemExtradata = CurrentPlaybackItem.GetExtradata();
                    var currentDelay = playbackItemExtradata.CurrentSubtitleDelay;
                    TimeSpan nextDelay = currentDelay.Add(TimeSpan.FromMilliseconds(250));
                    playbackItemExtradata.CurrentSubtitleDelay = nextDelay;
                    return nextDelay;
                }
                return TimeSpan.Zero;
            });
            return (TimeSpan)result.Result;
        }

        public async Task<TimeSpan> QuickenSubtitles()
        {
            var result = await commandDispatcher.EnqueueAsync(() =>
            {
                if (CurrentPlaybackItem != null)
                {
                    var playbackItemExtradata = CurrentPlaybackItem.GetExtradata();
                    var currentDelay = playbackItemExtradata.CurrentSubtitleDelay;
                    TimeSpan nextDelay = currentDelay.Subtract(TimeSpan.FromMilliseconds(250));
                    playbackItemExtradata.CurrentSubtitleDelay = nextDelay;
                    return nextDelay;
                }
                return TimeSpan.Zero;
            });

            return (TimeSpan)result.Result;
        }

        public void ResumeDispatcher()
        {
            commandDispatcher = new CommandDispatcher();
        }

        /// <summary>
        /// Cross-concearn: start the play to reciever
        /// </summary>
        /// <returns></returns>
        public async Task StartDlnaSink()
        {
            using (await playToReceiverLock.LockAsync())
            {
                PlayToRecieverInstance = new MayazucPlayToReciever(DispatcherUiThread, CurrentPlayer, new PlayToConfiguration(), commandDispatcher, PlaybackQueueService);
                PlayToRecieverInstance.SourceReady += PlayToRecieverInstance_SourceReady;
                await PlayToRecieverInstance.InitializeAsync();
                await PlayToRecieverInstance.StartAsync();
            }
        }

        /// <summary>
        /// Stops the DLNA service
        /// </summary>
        /// <returns></returns>
        private async Task StopDlnaSink()
        {
            using (await playToReceiverLock.LockAsync())
            {
                if (PlayToRecieverInstance != null)
                {
                    await PlayToRecieverInstance.DisposeAsync();
                    PlayToRecieverInstance.SourceReady -= PlayToRecieverInstance_SourceReady;
                    PlayToRecieverInstance = null;
                    SettingsWrapper.PlayToEnabled = false;
                }
            }
        }

        /// <summary>
        /// Strategy: Stop the play to reciever, reload the saved playback queue. Do not start playing
        /// </summary>
        /// <returns></returns>
        public Task ForceDisconnectDlnaAsync()
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                await StopDlnaSink();
                //check if operation is valid
                if (CurrentPlaybackDataIsDlna())
                {
                    SettingsWrapper.PlayerResumePosition = SettingsWrapper.ResumePositionDlnaIntrerupt;
                    SettingsWrapper.PlaybackIndex = SettingsWrapper.PlaybackIndexDlnaIntrerupt;

                    var request = new ResumeRequest();
                    request.StartIndex = SettingsWrapper.PlaybackIndexDlnaIntrerupt;
                    request.StartTime = SettingsWrapper.ResumePositionDlnaIntrerupt;

                    DestroySource();
                    await PlaybackQueueService.LoadNowPlaying();
                    await ResetToLocalPlaybackAdapter();

                    if (!await PlaybackListAdapter.BackstoreHasItems())
                    {
                        return;
                    }

                    await ResumeAsyncInternal(request, true);
                }
            });
        }

        private bool CurrentPlaybackDataIsDlna()
        {
            return CurrentPlaybackData != null && CurrentPlaybackData.HasExternalSource;
        }

        /// <summary>
        /// The player will resume the current playback queue
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task ResumeAsync(ResumeRequest request)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                await ResumeAsyncInternal(request, true);
            });
        }

        public Task SetActiveSubtitleAsync(MediaPlaybackItem currentPlaybackItem, uint trackIndex, int? hashCode = null)
        {
            return commandDispatcher.EnqueueAsync(() =>
            {
                try
                {
                    if (currentPlaybackItem == null)
                        currentPlaybackItem = CurrentPlaybackItem;
                    if (hashCode != null)
                    {
                        if (hashCode != currentPlaybackItem.GetHashCode()) return;
                    }

                    if (currentPlaybackItem == null) return;

                    if (currentPlaybackItem.TimedMetadataTracks.Count > trackIndex)
                        currentPlaybackItem.TimedMetadataTracks.SetPresentationMode(trackIndex, TimedMetadataTrackPresentationMode.PlatformPresented);
                }
                catch { }
            });
        }

        private async Task ResumeAsyncInternal(ResumeRequest request, bool autoPlay)
        {
            CurrentPlayer.AutoPlay = false;
            if (!await PlaybackListAdapter.BackstoreHasItems())
            {
                return;
            }

            SettingsWrapper.PlaybackIndex = request.StartIndex;

            var eventData = await WaitForItemChangedOrFail(async () =>
            {
                if (!await PlaybackListAdapter.BackstoreHasItems())
                {
                    throw new InvalidOperationException();
                }
                if (CurrentPlayer.Source == null)
                {
                }
                if (request.InitialItem != null)
                {
                    if (!await PlaybackListAdapter.Start(initialItem: request.InitialItem))
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    if (!await PlaybackListAdapter.Start(SettingsWrapper.PlaybackIndex))
                    {
                        throw new InvalidOperationException();
                    }
                }

            }, PlaybackListAdapter);

            if (eventData != null)
            {
                if (request.StartTime != 0)
                {
                    var seekTimeSpan = TimeSpan.FromTicks(request.StartTime);
                    if (CanSeekToPosition(seekTimeSpan, CurrentPlaybackItem))
                    {
                        await AwaitForSeek(seekTimeSpan);
                    }
                }
                if (autoPlay)
                {
                    CurrentPlayer.AutoPlay = true;
                    CurrentPlayer.Play();
                }
            }
        }

        public Task SetDisableSubtitleAsync(MediaPlaybackItem currentPlaybackItem, uint trackIndex, int? hashCode = null)
        {
            return commandDispatcher.EnqueueAsync(() =>
            {
                try
                {
                    if (currentPlaybackItem == null)
                        currentPlaybackItem = CurrentPlaybackItem;
                    if (hashCode != null)
                    {
                        if (hashCode != currentPlaybackItem.GetHashCode()) return;
                    }

                    if (currentPlaybackItem == null) return;

                    if (currentPlaybackItem.TimedMetadataTracks.Count > trackIndex)
                        currentPlaybackItem.TimedMetadataTracks.SetPresentationMode(trackIndex, TimedMetadataTrackPresentationMode.Disabled);
                }
                catch { }
            });
        }

        public void InitializeAsync(IntPtr hwnd)
        {
            InitPlayer(hwnd);
            PlaybackQueueService.FillData();
            PlaylistsService.LoadPlaylistsAsync();
        }

        /// <summary>
        /// Remove items from playback queue.
        /// Strategy: if current item was removed, start playback from 0, reload next item
        /// Strategy: if current item was not removed, reload next item
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public Task RemoveItemsFromNowPlayingQueue(IEnumerable<MediaPlayerItemSourceUIWrapper> items)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                var currentTrack = CurrentPlaybackData;
                await PlaybackQueueService.RemoveItemsFromNowPlayingAsync(items);
                //removed current track, skip next
                if (items.Select(x => x.MediaData).Contains(currentTrack))
                {
                    await SignalSkipToIndexInternal(false, 0);
                }
                else
                {

                    //if we didn't remove the current track, handle this as if it was a shuffle
                    await HandleNowPlayingShuffle();
                }
            });
        }

        public Task RandomizeNowPlayingQueue()
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                var oldIndex = SettingsWrapper.PlaybackIndex;

                await DispatcherUiThread.EnqueueAsync(async () =>
                {
                    var newIndex = await PlaybackQueueService.RandomizeNowPlayingAsync(oldIndex);

                    SettingsWrapper.PlaybackIndex = newIndex;

                });

                await HandleNowPlayingShuffle();
            });
        }

        /// <summary>
        /// Strategy: if playback queue is empty => same as enqueue new list
        /// Strategy: if playback queue is not empty => Add items to playback queue, reload next item
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task AddToNowPlaying(IEnumerable<IMediaPlayerItemSource> FilesToAdd)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                await AddToNowPlayingInternal(FilesToAdd);
            });
        }

        /// <summary>
        /// Strategy: if playback queue is empty => same as enqueue new list
        /// Strategy: if playback queue is not empty => Add items to playback queue, reload next item
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task AddToNowPlayingInternal(IEnumerable<IMediaPlayerItemSource> FilesToAdd, int index = PlaybackSequenceService.AddToNowPlayingAtTheEnd)
        {
            //if playback queue is empty, handle as if it is enqueing a new playlist
            if (PlaybackQueueService.NowPlayingBackStore.Count == 0)
            {
                await StartPlaybackFromIndexAndPositionInternal(FilesToAdd, 0, 0);
            }
            else
            {
                // there is already a playback queue
                var toAdd = FilesToAdd.ToArray();
                await PlaybackQueueService.AddToNowPlayingAsync(toAdd, index);
                var signalReload = SettingsWrapper.PlaybackIndex == PlaybackQueueService.NowPlayingBackStore.Count - 1;

                if (signalReload)
                {
                    if (SettingsWrapper.RepeatMode != Constants.RepeatOne)
                    {
                        await PlaybackListAdapter.ReloadNextItemAsync(CurrenItem: CurrentPlaybackItem, userAction: true, changeIndex: true, currentIndex: SettingsWrapper.PlaybackIndex);
                    }
                }
            }
        }

        public Task EnqueueNext(IEnumerable<IMediaPlayerItemSource> FilesToAdd)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                var currentIndex = SettingsWrapper.PlaybackIndex;
                var playbackSize = PlaybackQueueService.NowPlayingBackStore.Count;
                int startIndex = PlaybackSequenceService.AddToNowPlayingAtTheEnd;
                if (currentIndex >= 0 || currentIndex < playbackSize - 1)
                    startIndex = currentIndex + 1;
                await AddToNowPlayingInternal(FilesToAdd, startIndex);
                await _playbackSource.ReloadNextItemAsync(CurrenItem: CurrentPlaybackItem, userAction: true, changeIndex: true, currentIndex: SettingsWrapper.PlaybackIndex);

                if (CurrentPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.None)
                    await SignalSkipNextInternal();
            });
        }

        /// <summary>
        /// Strategy: clear old queue, add new items to now playing, recreate source, start playback from begining
        /// </summary>
        public Task EnqueueNowPlayList(IEnumerable<IMediaPlayerItemSource> FilesToAdd)
        {
            return StartPlaybackFromIndexAndPosition(FilesToAdd, 0, 0);
        }

        private async Task EnqueueNewPlaylistCommon(IEnumerable<IMediaPlayerItemSource> FilesToAdd)
        {
            //add files to now playing in back model
            var toAdd = FilesToAdd.ToArray();

            SettingsWrapper.PlaybackIndex = 0;

            await PlaybackQueueService.EnqueueNewPlaylistAsync(toAdd.ToArray());
        }

        public Task MoveItemUpInPlaybackQueue(MediaPlayerItemSourceUIWrapper mds)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                var index = PlaybackQueueService.NowPlayingBackStore.IndexOf(mds);
                bool isPlaying = index == SettingsWrapper.PlaybackIndex;
                var count = PlaybackQueueService.NowPlayingBackStore.Count;
                var newIndex = index - 1;
                if (newIndex < 0)
                {
                    newIndex = PlaybackQueueService.NowPlayingBackStore.Count - 1;
                }
                //if (isPlaying)
                //{
                //    SettingsWrapper.PlaybackIndex = newIndex;
                //}

                await PlaybackQueueService.SwitchItemInPlaybackQueue(index, newIndex);

                await HandleNowPlayingShuffle();
            });
        }

        private async Task HandleNowPlayingShuffle()
        {
            if (!await PlaybackListAdapter.BackstoreHasItems())
            {
                return;
            }
            // Ensure the stored playback index is the playback index of the current media data
            SettingsWrapper.PlaybackIndex = PlaybackQueueService.NowPlayingBackStore.IndexOfMediaData(CurrentPlaybackData);

            await PlaybackListAdapter.ReloadNextItemAsync(CurrenItem: CurrentPlaybackItem, userAction: true, changeIndex: true, currentIndex: SettingsWrapper.PlaybackIndex);
        }
        public Task StartPlaybackFromIndexAndPosition(IEnumerable<IMediaPlayerItemSource> FilesToAdd, int index, long position)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                await StartPlaybackFromIndexAndPositionInternal(FilesToAdd, index, position);
            });
        }

        private async Task StartPlaybackFromIndexAndPositionInternal(IEnumerable<IMediaPlayerItemSource> FilesToAdd, int index, long position)
        {
            if (!FilesToAdd.Any())
            {
                return;
            }

            await EnqueueNewPlaylistCommon(FilesToAdd);
            //destory the current source, and create a new one
            DestroySource();
            await ResetToLocalPlaybackAdapter();
            //notify people that the playback type changed to local

            SettingsWrapper.PlayerResumePosition = position;
            SettingsWrapper.PlaybackIndex = index;
            SettingsWrapper.PlayerResumePath = FilesToAdd.ElementAt(index).MediaPath;
            ResumeRequest request = new ResumeRequest();
            request.StartIndex = index;
            request.StartTime = position;
            await ResumeAsyncInternal(request, true);
        }

        public Task SavePlaylistReorderAsync()
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                var t = CurrentPlaybackItem?.GetExtradata()?.MediaPlayerItemSource;

                var props = PlaybackQueueService.NowPlayingBackStore.Select(x => x.MediaData);
                await PlaybackQueueService.EnqueueNewPlaylistAsync(props);

                if (t != null)
                {
                    SettingsWrapper.PlaybackIndex = PlaybackQueueService.NowPlayingBackStore.IndexOfMediaData(t);
                }
                else
                {
                    //PlaybackQueueService.NowPlayingBackStore[SettingsWrapper.PlaybackIndex].MediaData.IsInPlayback = true;
                }

                await HandleNowPlayingShuffle();
            });
        }

        public Task MoveItemDownInPlaybackQueue(MediaPlayerItemSourceUIWrapper mds)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                var index = PlaybackQueueService.NowPlayingBackStore.IndexOf(mds);
                bool isPlaying = index == SettingsWrapper.PlaybackIndex;
                var count = PlaybackQueueService.NowPlayingBackStore.Count;
                var newIndex = (index + 1) % count;
                //if (isPlaying)
                //{
                //    SettingsWrapper.PlaybackIndex = newIndex;
                //}

                await PlaybackQueueService.SwitchItemInPlaybackQueue(index, newIndex);

                await HandleNowPlayingShuffle();
            });
        }

        public Task SetEqualizerConfiguration(EqualizerConfiguration configuration)
        {
            return commandDispatcher.EnqueueAsync(() =>
            {
                CurrentEqualizerConfiguration = configuration;
                configuration.SetDefault();
            });
        }

        public Task AddPresetToConfiguration(EqualizerConfiguration configuration, AudioEqualizerPreset preset)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                List<AudioEqualizerPreset> presets = new List<AudioEqualizerPreset>(configuration.Presets);
                presets.Add(preset);
                configuration.SaveToFileAsync();
                await DispatcherUiThread.EnqueueAsync(() =>
                {
                    configuration.SetPresets(presets);
                });
            });
        }

        public async Task<EqualizerConfiguration> GetCurrentEqualizerConfiguration()
        {
            var result = await commandDispatcher.EnqueueAsync(() =>
            {
                return EqualizerService.GetCurrentEqualizerConfig();
            });
            return (EqualizerConfiguration)result.Result;
        }

        public async Task AddEqualizerConfiguration(EqualizerConfiguration config)
        {
            await commandDispatcher.EnqueueAsync(async () =>
            {
                config.SaveToFileAsync();
                await DispatcherUiThread.EnqueueAsync(() =>
                {
                    EqualizerService.EqualizerConfigurations.Add(config);
                });
            });
        }

        public async Task<EqualizerConfigurationDeletionResult> DeleteEqualizerConfiguration(EqualizerConfiguration config)
        {
            var result = await commandDispatcher.EnqueueAsync(async () =>
            {
                if (EqualizerService.GetCurrentEqualizerConfig().CompareTo(config, false))
                    return EqualizerConfigurationDeletionResult.FailedInUse;
                if (!config.CanDelete) return EqualizerConfigurationDeletionResult.GeneralFailure;
                await DispatcherUiThread.EnqueueAsync(() =>
                {
                    EqualizerService.EqualizerConfigurations.Remove(config);
                    config.Delete();
                });
                return EqualizerConfigurationDeletionResult.Success;
            });
            return (EqualizerConfigurationDeletionResult)result.Result;
        }

        public Task DeletePresetFromConfiguration(EqualizerConfiguration configuration, AudioEqualizerPreset preset)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                await DispatcherUiThread.EnqueueAsync(() =>
                {
                    configuration.Presets.Remove(preset);

                    configuration.SaveToFileAsync();
                });
            });
        }

        public Task SkipToQueueItem(IMediaPlayerItemSource mediaData)
        {
            return commandDispatcher.EnqueueAsync(async () =>
            {
                var isContainedInPlaybackQueue = PlaybackQueueService.NowPlayingBackStore.Any(x => x.MediaData.MediaPath == mediaData.MediaPath);
                if (!isContainedInPlaybackQueue)
                {
                    await AddToNowPlayingInternal(new IMediaPlayerItemSource[] { mediaData });
                }

                var indexToSkip = PlaybackQueueService.NowPlayingBackStore.IndexOfMediaData(mediaData);
                await SignalSkipToIndexInternal(true, indexToSkip);

            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                PlaybackListAdapter.Dispose();
                commandDispatcher.SignalToCancel();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~BackgroundMediaPlayer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}