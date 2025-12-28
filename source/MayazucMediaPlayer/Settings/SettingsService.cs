using MayazucMediaPlayer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.Networking.Connectivity;

namespace MayazucMediaPlayer.Settings
{
    public class SettingsService
    {
        static SettingsService instance;
        static object singletonLock = new object();

        public static SettingsService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (singletonLock)
                    {
                        if (instance == null)
                        {
                            instance = new SettingsService();
                        }
                    }
                }
                return instance;
            }
        }

        private const string DefaultEqualizerPresetValue = "default";

        public event EventHandler<string>? RepeatModeChanged;
        public event EventHandler<bool>? ShuffleModeChanged;

        readonly ConcurrentDictionary<string, List<Action<object, string>>> PropertyHandlerMap = new ConcurrentDictionary<string, List<Action<object, string>>>();

        public Action<object, string> RegisterSettingChangeCallback(string settingName, Action<object, string> callback)
        {
            if (!PropertyHandlerMap.ContainsKey(settingName))
            {
                PropertyHandlerMap.TryAdd(settingName, new List<Action<object, string>>());
            }

            var callbacks = PropertyHandlerMap[settingName];
            callbacks.Add(callback);
            return callback;
        }

        public void UnregisterSettingChangeCallback(string settingName, Action<object, string> callback)
        {
            var callbacks = PropertyHandlerMap[settingName];
            callbacks.Remove(callback);
        }

        private void RiseSettingChanged(object value, string settingName)
        {
            List<Action<object, string>> x;
            if (PropertyHandlerMap.TryGetValue(settingName, out x))
            {
                foreach (var c in x)
                {
                    c?.Invoke(value, settingName);
                }
            }
        }


        private SettingsService()
        {
            
        }

        internal void SaveSettings()
        {
            //tbd no-op
        }

        [SettingDefaultValue(false)]
        public bool AutoPlayMusic
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(AutoPlayMusic), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(AutoPlayMusic), false, value);
                RiseSettingChanged(value, nameof(AutoPlayMusic));
            }
        }

        [SettingDefaultValue(false)]
        public bool AutoPlayVideo
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(AutoPlayVideo), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(AutoPlayVideo), false, value);
                RiseSettingChanged(value, nameof(AutoPlayVideo));
            }
        }

        public string RepeatMode
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<string>(nameof(RepeatMode), Constants.RepeatAll);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<string>(nameof(RepeatMode), Constants.RepeatAll, value);
                RepeatModeChanged?.Invoke(null, value);
            }
        }

        public int PlaybackIndex
        {
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(PlaybackIndex), 0, value);
                RiseSettingChanged(value, nameof(PlaybackIndex));
            }
            get
            {
                return SettingsStoreService.GetValueOrDefault(nameof(PlaybackIndex), 0);
            }
        }

        public bool ShuffleMode
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(ShuffleMode), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(ShuffleMode), false, value);
                ShuffleModeChanged?.Invoke(null, value);
                //RiseSettingChanged(value, nameof(ShuffleMode));
            }
        }

        [SettingDefaultValue(0)]
        public double AudioBalance
        {
            get
            {
                var userOption = SettingsStoreService.GetValueOrDefault<double>(nameof(AudioBalance), 0);
                return userOption;
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<double>(nameof(AudioBalance), 0, value);
                RiseSettingChanged(value, nameof(AudioBalance));
            }
        }


        [SettingDefaultValue("")]
        public string NetworkInstanceName
        {
            get
            {
                var userOption = SettingsStoreService.GetValueOrDefault<string>(nameof(NetworkInstanceName), "");
                if (string.IsNullOrWhiteSpace(userOption))
                {
                    var hostNames = NetworkInformation.GetHostNames();
                    var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));
                    var computerName = localName.DisplayName.Replace(".local", "");
                    SettingsStoreService.SetValueOrDefault<string>(nameof(NetworkInstanceName), "", computerName);
                }
                return userOption;
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<string>(nameof(NetworkInstanceName), "", value);
                RiseSettingChanged(value, nameof(NetworkInstanceName));
            }
        }

        [SettingDefaultValue(false)]
        public bool AutoClearFilePicker
        {
            get
            {
                var userOption = SettingsStoreService.GetValueOrDefault<bool>(nameof(AutoClearFilePicker), false);
                return userOption;
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(AutoClearFilePicker), false, value);
                RiseSettingChanged(value, nameof(AutoClearFilePicker));
            }
        }


        [SettingDefaultValue(true)]
        public bool MetadataOptionsUseDefault
        {
            get
            {
                var userOption = SettingsStoreService.GetValueOrDefault<bool>(nameof(MetadataOptionsUseDefault), true);
                return userOption;
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(MetadataOptionsUseDefault), true, value);
                RiseSettingChanged(value, nameof(MetadataOptionsUseDefault));
            }
        }


        [SettingDefaultValue(0)]
        public int MetadataAlbumIndex
        {
            get
            {
                var userOption = SettingsStoreService.GetValueOrDefault<int>(nameof(MetadataAlbumIndex), 0);
                return userOption;
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(MetadataAlbumIndex), 0, value);
                RiseSettingChanged(value, nameof(MetadataAlbumIndex));
            }
        }

        [SettingDefaultValue(0)]
        public int MetadataArtistIndex
        {
            get
            {
                var userOption = SettingsStoreService.GetValueOrDefault<int>(nameof(MetadataArtistIndex), 0);
                return userOption;
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(MetadataArtistIndex), 0, value);
                RiseSettingChanged(value, nameof(MetadataArtistIndex));
            }
        }

        [SettingDefaultValue(0)]
        public int MetadataGenreIndex
        {
            get
            {
                var userOption = SettingsStoreService.GetValueOrDefault<int>(nameof(MetadataGenreIndex), 0);
                return userOption;
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(MetadataGenreIndex), 0, value);
                RiseSettingChanged(value, nameof(MetadataGenreIndex));
            }
        }

        public TimeSpan StopMusicOnTimerPosition
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<TimeSpan>(nameof(StopMusicOnTimerPosition), TimeSpan.Zero);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<TimeSpan>(nameof(StopMusicOnTimerPosition), TimeSpan.Zero, value);
                RiseSettingChanged(value, nameof(StopMusicOnTimerPosition));
                AppState.Current.MediaServiceConnector.PlayerInstance.HandleStopMusicOnTimer();
            }
        }

        [SettingDefaultValue(false)]
        public bool StopMusicOnTimerEnabled
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(StopMusicOnTimerEnabled), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(StopMusicOnTimerEnabled), false, value);
                RiseSettingChanged(value, nameof(StopMusicOnTimerEnabled));
                AppState.Current.MediaServiceConnector.PlayerInstance.HandleStopMusicOnTimer();
            }
        }

        [SettingDefaultValue(0)]
        public double PlayerResumePosition
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<double>(nameof(PlayerResumePosition), 0.0);

            }
            set
            {
                SettingsStoreService.SetValueOrDefault<double>(nameof(PlayerResumePosition), 0.0, value);
                RiseSettingChanged(value, nameof(PlayerResumePosition));
            }
        }


        [SettingDefaultValue(true)]
        public bool AutoDetectExternalSubtitle
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(AutoDetectExternalSubtitle), true);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(AutoDetectExternalSubtitle), true, value);
                RiseSettingChanged(value, nameof(AutoDetectExternalSubtitle));
            }
        }

        [SettingDefaultValue(-1)]
        public int PreferredSubtitleLanguageIndex
        {
            get
            {
                var index = SettingsStoreService.GetValueOrDefault<int>(nameof(PreferredSubtitleLanguageIndex), -1);
                if (index == -1)
                {
                    index = LanguageCodesService.GetDefaultLanguageIndex();
                    SettingsStoreService.SetValueOrDefault<int>(nameof(PreferredSubtitleLanguageIndex), -1, index);
                }

                return index;
            }
            set
            {
                var index = value;
                SettingsStoreService.SetValueOrDefault<int>(nameof(PreferredSubtitleLanguageIndex), -1, index);
                RiseSettingChanged(value, nameof(PreferredSubtitleLanguageIndex));
            }
        }

        public LanguageCode PreferredSubtitleLanguage
        {
            get
            {
                return LanguageCodesService.Codes[SettingsService.Instance.PreferredSubtitleLanguageIndex];
            }
        }

        [SettingDefaultValue(true)]
        public bool EqualizerEnabled
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(EqualizerEnabled), true);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(EqualizerEnabled), true, value);
                RiseSettingChanged(value, nameof(EqualizerEnabled));

            }
        }

        [SettingDefaultValue(false)]
        public bool VideoOutputAllowIyuv
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(VideoOutputAllowIyuv), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(VideoOutputAllowIyuv), false, value);
                RiseSettingChanged(value, nameof(VideoOutputAllowIyuv));

            }
        }

        [SettingDefaultValue(false)]
        public bool VideoOutputAllow10bit
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(VideoOutputAllow10bit), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(VideoOutputAllow10bit), false, value);
                RiseSettingChanged(value, nameof(VideoOutputAllow10bit));

            }
        }

        [SettingDefaultValue(false)]
        public bool VideoOutputAllowBgra8
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(VideoOutputAllowBgra8), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(VideoOutputAllowBgra8), false, value);
                RiseSettingChanged(value, nameof(VideoOutputAllowBgra8));

            }
        }

        [SettingDefaultValue(false)]
        public bool StartPlaybackAfterSeek
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(StartPlaybackAfterSeek), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(StartPlaybackAfterSeek), false, value);
                RiseSettingChanged(value, nameof(StartPlaybackAfterSeek));
            }
        }

        [SettingDefaultValue(2)]
        public int PlaybackTapGestureModeRaw
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<int>(nameof(PlaybackTapGestureModeRaw), 2);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(PlaybackTapGestureModeRaw), 2, value);
                RiseSettingChanged(value, nameof(PlaybackTapGestureModeRaw));
            }
        }

        [SettingDefaultValue(true)]
        public bool ShowConfirmationMessages
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(ShowConfirmationMessages), true);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(ShowConfirmationMessages), true, value);
                RiseSettingChanged(value, nameof(ShowConfirmationMessages));
            }
        }

        public PlaybackTapGestureMode PlaybackTapGestureMode
        {
            get
            {
                return (PlaybackTapGestureMode)PlaybackTapGestureModeRaw;
            }
        }

        [SettingDefaultValue(0)]
        public int DefaultUITheme
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<int>(nameof(DefaultUITheme), 0);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(DefaultUITheme), 0, value);
                RiseSettingChanged(value, nameof(DefaultUITheme));
            }
        }

        public string CurrentPlaylist
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<string>(nameof(CurrentPlaylist), string.Empty);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<string>(nameof(CurrentPlaylist), string.Empty, value);
                RiseSettingChanged(value, nameof(CurrentPlaylist));
            }
        }


        [SettingDefaultValue(true)]
        public bool AutoloadInternalSubtitle
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(AutoloadInternalSubtitle), true);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(AutoloadInternalSubtitle), true, value);
                RiseSettingChanged(value, nameof(AutoloadInternalSubtitle));
            }
        }

        [SettingDefaultValue(0)]
        public int FFmpegCharacterEncodingIndex
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<int>(nameof(FFmpegCharacterEncodingIndex), 0);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(FFmpegCharacterEncodingIndex), 0, value);
                RiseSettingChanged(value, nameof(FFmpegCharacterEncodingIndex));
            }
        }

        [SettingDefaultValue(true)]
        public bool KeepPlaybackRateBetweenTracks
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(KeepPlaybackRateBetweenTracks), true);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(KeepPlaybackRateBetweenTracks), true, value);
                RiseSettingChanged(value, nameof(KeepPlaybackRateBetweenTracks));
            }
        }

        [SettingDefaultValue(false)]
        public bool AutoloadForcedSubtitles
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(AutoloadForcedSubtitles), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(AutoloadForcedSubtitles), false, value);
                RiseSettingChanged(value, nameof(AutoloadForcedSubtitles));
            }
        }

        [SettingDefaultValue(0)]
        public int VideoDecoderMode
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<int>(nameof(VideoDecoderMode), 0);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(VideoDecoderMode), 0, value);
                RiseSettingChanged(value, nameof(VideoDecoderMode));
            }
        }

        public string PlayerResumePath
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<string>(nameof(PlayerResumePath), string.Empty);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<string>(nameof(PlayerResumePath), string.Empty, value);
                RiseSettingChanged(value, nameof(PlayerResumePath));
            }
        }


        [SettingDefaultValue(0)]
        public double MinimumSubtitleDuration
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<double>(nameof(MinimumSubtitleDuration), 0);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<double>(nameof(MinimumSubtitleDuration), 0, value);
                RiseSettingChanged(value, nameof(MinimumSubtitleDuration));
            }
        }

        [SettingDefaultValue(true)]
        public bool PreventSubtitleOverlaps
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(PreventSubtitleOverlaps), true);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(PreventSubtitleOverlaps), true, value);
                RiseSettingChanged(value, nameof(PreventSubtitleOverlaps));
            }
        }


        [SettingDefaultValue(false)]
        public bool PlayToEnabled
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(PlayToEnabled), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(PlayToEnabled), false, value);
                RiseSettingChanged(value, nameof(PlayToEnabled));
            }
        }

        public int PlaybackIndexDlnaIntrerupt
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<int>(nameof(PlaybackIndexDlnaIntrerupt), 0);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(PlaybackIndexDlnaIntrerupt), 0, value);
                RiseSettingChanged(value, nameof(PlaybackIndexDlnaIntrerupt));
            }
        }

        public double ResumePositionDlnaIntrerupt
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<double>(nameof(ResumePositionDlnaIntrerupt), 0);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<double>(nameof(ResumePositionDlnaIntrerupt), 0, value);
                RiseSettingChanged(value, nameof(ResumePositionDlnaIntrerupt));
            }
        }

        [SettingDefaultValue(true)]
        public bool StereoDownMix
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(StereoDownMix), true);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(StereoDownMix), true, value);
                RiseSettingChanged(value, nameof(StereoDownMix));
            }
        }


        [SettingDefaultValue("MC Media Center")]
        public string EqualizerConfiguration
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<string>(nameof(EqualizerConfiguration), "MC Media Center");
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<string>(nameof(EqualizerConfiguration), "MC Media Center", value);
                RiseSettingChanged(value, nameof(EqualizerConfiguration));
            }
        }

        [SettingDefaultValue(DefaultEqualizerPresetValue)]
        public string SelectedEqualizerPreset
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<string>(nameof(SelectedEqualizerPreset), "default");
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<string>(nameof(SelectedEqualizerPreset), "default", value);
                RiseSettingChanged(value, nameof(SelectedEqualizerPreset));
            }
        }

        [SettingDefaultValue(true)]
        public bool OnlyUseCacheInFilePicker
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<bool>(nameof(OnlyUseCacheInFilePicker), true);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<bool>(nameof(OnlyUseCacheInFilePicker), true, value);
                RiseSettingChanged(value, nameof(OnlyUseCacheInFilePicker));
            }
        }

        [SettingDefaultValue("folder.jpg;folder.png")]
        public string AlbumArtFolderCoverName
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<string>(nameof(AlbumArtFolderCoverName), "folder.jpg;folder.png");
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<string>(nameof(AlbumArtFolderCoverName), "folder.jpg;folder.png", value);
                RiseSettingChanged(value, nameof(AlbumArtFolderCoverName));
            }
        }

        [SettingDefaultValue(false)]
        public bool AutomaticPresetManagement
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault(nameof(AutomaticPresetManagement), false);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault(nameof(AutomaticPresetManagement), false, value);
                RiseSettingChanged(value, nameof(AutomaticPresetManagement));
            }
        }

        [SettingDefaultValue(0)]
        public int AlbumArtOptionIndex
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<int>(nameof(AlbumArtOptionIndex), 0);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(AlbumArtOptionIndex), 0, value);
                RiseSettingChanged(value, nameof(AlbumArtOptionIndex));
            }
        }

        [SettingDefaultValue(0)]
        public int FolderHierarchyMetadataIndex
        {
            get
            {
                return SettingsStoreService.GetValueOrDefault<int>(nameof(FolderHierarchyMetadataIndex), 0);
            }
            set
            {
                SettingsStoreService.SetValueOrDefault<int>(nameof(FolderHierarchyMetadataIndex), 0, value);
                RiseSettingChanged(value, nameof(FolderHierarchyMetadataIndex));
            }
        }
    }
}