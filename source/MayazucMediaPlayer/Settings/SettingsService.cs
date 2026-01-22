using MayazucMediaPlayer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Windows.Networking.Connectivity;

namespace MayazucMediaPlayer.Settings
{
    public class SettingsService
    {
        static SettingsService? instance;
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

        private SettingsStoreService storageService = new SettingsStoreService();
        private const string DefaultEqualizerPresetValue = "default";
        private Timer autoSaveTimer = new Timer();

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
            autoSaveTimer.Stop();

            autoSaveTimer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
            autoSaveTimer.AutoReset = false;
            autoSaveTimer.Start();

            List<Action<object, string>> x;
            if (PropertyHandlerMap.TryGetValue(settingName, out x))
            {
                foreach (var c in x)
                {
                    c?.Invoke(value, settingName);
                }
            }
        }

        private void AutoSaveSettingsElapsed(object? sender, ElapsedEventArgs e)
        {
            storageService.SaveSettings();
        }

        private SettingsService()
        {
            autoSaveTimer.Elapsed += AutoSaveSettingsElapsed;
        }

        internal void SaveSettings()
        {
            storageService.SaveSettings();
        }

        [SettingDefaultValue(false)]
        public bool AutoPlayMusic
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutoPlayMusic), false);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutoPlayMusic), false, value);
                RiseSettingChanged(value, nameof(AutoPlayMusic));
            }
        }

        [SettingDefaultValue(false)]
        public bool AutoPlayVideo
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutoPlayVideo), false);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutoPlayVideo), false, value);
                RiseSettingChanged(value, nameof(AutoPlayVideo));
            }
        }

        public string RepeatMode
        {
            get
            {
                return storageService.StringGetValueOrDefault(nameof(RepeatMode), Constants.RepeatAll);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(RepeatMode), Constants.RepeatAll, value);
                RepeatModeChanged?.Invoke(null, value);
            }
        }

        public int PlaybackIndex
        {
            set
            {
                storageService.SetValueOrDefault2(nameof(PlaybackIndex), 0, value);
                RiseSettingChanged(value, nameof(PlaybackIndex));
            }
            get
            {
                return storageService.IntGetValueOrDefault(nameof(PlaybackIndex), 0);
            }
        }

        public bool ShuffleMode
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(ShuffleMode), false);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(ShuffleMode), false, value);
                ShuffleModeChanged?.Invoke(null, value);
                //RiseSettingChanged(value, nameof(ShuffleMode));
            }
        }

        [SettingDefaultValue(0)]
        public double AudioBalance
        {
            get
            {
                var userOption = storageService.DoubleGetValueOrDefault(nameof(AudioBalance), 0);
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AudioBalance), 0, value);
                RiseSettingChanged(value, nameof(AudioBalance));
            }
        }


        [SettingDefaultValue("")]
        public string NetworkInstanceName
        {
            get
            {
                var userOption = storageService.StringGetValueOrDefault(nameof(NetworkInstanceName), "");
                if (string.IsNullOrWhiteSpace(userOption))
                {
                    var hostNames = NetworkInformation.GetHostNames();
                    var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));
                    var computerName = localName.DisplayName.Replace(".local", "");
                    storageService.SetValueOrDefault2(nameof(NetworkInstanceName), "", computerName);
                }
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(NetworkInstanceName), "", value);
                RiseSettingChanged(value, nameof(NetworkInstanceName));
            }
        }

        [SettingDefaultValue(true)]
        public bool MetadataOptionsUseDefault
        {
            get
            {
                var userOption = storageService.BoolGetValueOrDefault(nameof(MetadataOptionsUseDefault), true);
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(MetadataOptionsUseDefault), true, value);
                RiseSettingChanged(value, nameof(MetadataOptionsUseDefault));
            }
        }


        [SettingDefaultValue(0)]
        public int MetadataAlbumIndex
        {
            get
            {
                var userOption = storageService.IntGetValueOrDefault(nameof(MetadataAlbumIndex), 0);
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(MetadataAlbumIndex), 0, value);
                RiseSettingChanged(value, nameof(MetadataAlbumIndex));
            }
        }

        [SettingDefaultValue(0)]
        public int MetadataArtistIndex
        {
            get
            {
                var userOption = storageService.IntGetValueOrDefault(nameof(MetadataArtistIndex), 0);
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(MetadataArtistIndex), 0, value);
                RiseSettingChanged(value, nameof(MetadataArtistIndex));
            }
        }

        [SettingDefaultValue(0)]
        public int MetadataGenreIndex
        {
            get
            {
                var userOption = storageService.IntGetValueOrDefault(nameof(MetadataGenreIndex), 0);
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(MetadataGenreIndex), 0, value);
                RiseSettingChanged(value, nameof(MetadataGenreIndex));
            }
        }

        public TimeSpan StopMusicOnTimerPosition
        {
            get
            {
                return storageService.TimeSpanGetValueOrDefault(nameof(StopMusicOnTimerPosition), TimeSpan.Zero);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(StopMusicOnTimerPosition), TimeSpan.Zero, value);
                RiseSettingChanged(value, nameof(StopMusicOnTimerPosition));
                AppState.Current.MediaServiceConnector.PlayerInstance.HandleStopMusicOnTimer();
            }
        }

        [SettingDefaultValue(false)]
        public bool StopMusicOnTimerEnabled
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(StopMusicOnTimerEnabled), false);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(StopMusicOnTimerEnabled), false, value);
                RiseSettingChanged(value, nameof(StopMusicOnTimerEnabled));
                AppState.Current.MediaServiceConnector.PlayerInstance.HandleStopMusicOnTimer();
            }
        }

        [SettingDefaultValue(0)]
        public double PlayerResumePosition
        {
            get
            {
                return storageService.DoubleGetValueOrDefault(nameof(PlayerResumePosition), 0.0);

            }
            set
            {
                storageService.SetValueOrDefault2(nameof(PlayerResumePosition), 0.0, value);
                RiseSettingChanged(value, nameof(PlayerResumePosition));
            }
        }


        [SettingDefaultValue(true)]
        public bool AutoDetectExternalSubtitle
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutoDetectExternalSubtitle), true);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutoDetectExternalSubtitle), true, value);
                RiseSettingChanged(value, nameof(AutoDetectExternalSubtitle));
            }
        }

        [SettingDefaultValue(-1)]
        public int PreferredSubtitleLanguageIndex
        {
            get
            {
                var index = storageService.IntGetValueOrDefault(nameof(PreferredSubtitleLanguageIndex), -1);
                if (index == -1)
                {
                    index = LanguageCodesService.GetDefaultLanguageIndex();
                    storageService.SetValueOrDefault2(nameof(PreferredSubtitleLanguageIndex), -1, index);
                }

                return index;
            }
            set
            {
                var index = value;
                storageService.SetValueOrDefault2(nameof(PreferredSubtitleLanguageIndex), -1, index);
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
                return storageService.BoolGetValueOrDefault(nameof(EqualizerEnabled), true);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(EqualizerEnabled), true, value);
                RiseSettingChanged(value, nameof(EqualizerEnabled));

            }
        }


        [SettingDefaultValue(0)]
        public int DefaultUITheme
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(DefaultUITheme), 0);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(DefaultUITheme), 0, value);
                RiseSettingChanged(value, nameof(DefaultUITheme));
            }
        }


        [SettingDefaultValue(true)]
        public bool AutoloadInternalSubtitle
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutoloadInternalSubtitle), true);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutoloadInternalSubtitle), true, value);
                RiseSettingChanged(value, nameof(AutoloadInternalSubtitle));
            }
        }

        [SettingDefaultValue(0)]
        public int FFmpegCharacterEncodingIndex
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(FFmpegCharacterEncodingIndex), 0);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(FFmpegCharacterEncodingIndex), 0, value);
                RiseSettingChanged(value, nameof(FFmpegCharacterEncodingIndex));
            }
        }


        [SettingDefaultValue(false)]
        public bool AutoloadForcedSubtitles
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutoloadForcedSubtitles), false);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutoloadForcedSubtitles), false, value);
                RiseSettingChanged(value, nameof(AutoloadForcedSubtitles));
            }
        }

        [SettingDefaultValue(0)]
        public int VideoDecoderMode
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(VideoDecoderMode), 0);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(VideoDecoderMode), 0, value);
                RiseSettingChanged(value, nameof(VideoDecoderMode));
            }
        }

        public string PlayerResumePath
        {
            get
            {
                return storageService.StringGetValueOrDefault(nameof(PlayerResumePath), string.Empty);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(PlayerResumePath), string.Empty, value);
                RiseSettingChanged(value, nameof(PlayerResumePath));
            }
        }


        [SettingDefaultValue(0)]
        public double MinimumSubtitleDuration
        {
            get
            {
                return storageService.DoubleGetValueOrDefault(nameof(MinimumSubtitleDuration), 0);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(MinimumSubtitleDuration), 0, value);
                RiseSettingChanged(value, nameof(MinimumSubtitleDuration));
            }
        }

        [SettingDefaultValue(true)]
        public bool PreventSubtitleOverlaps
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(PreventSubtitleOverlaps), true);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(PreventSubtitleOverlaps), true, value);
                RiseSettingChanged(value, nameof(PreventSubtitleOverlaps));
            }
        }


        [SettingDefaultValue(false)]
        public bool PlayToEnabled
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(PlayToEnabled), false);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(PlayToEnabled), false, value);
                RiseSettingChanged(value, nameof(PlayToEnabled));
            }
        }

        public int PlaybackIndexDlnaIntrerupt
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(PlaybackIndexDlnaIntrerupt), 0);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(PlaybackIndexDlnaIntrerupt), 0, value);
                RiseSettingChanged(value, nameof(PlaybackIndexDlnaIntrerupt));
            }
        }

        public double ResumePositionDlnaIntrerupt
        {
            get
            {
                return storageService.DoubleGetValueOrDefault(nameof(ResumePositionDlnaIntrerupt), 0);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(ResumePositionDlnaIntrerupt), 0, value);
                RiseSettingChanged(value, nameof(ResumePositionDlnaIntrerupt));
            }
        }

        [SettingDefaultValue(true)]
        public bool StereoDownMix
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(StereoDownMix), true);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(StereoDownMix), true, value);
                RiseSettingChanged(value, nameof(StereoDownMix));
            }
        }


        [SettingDefaultValue("MC Media Center")]
        public string EqualizerConfiguration
        {
            get
            {
                return storageService.StringGetValueOrDefault(nameof(EqualizerConfiguration), "MC Media Center");
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(EqualizerConfiguration), "MC Media Center", value);
                RiseSettingChanged(value, nameof(EqualizerConfiguration));
            }
        }

        [SettingDefaultValue(DefaultEqualizerPresetValue)]
        public string SelectedEqualizerPreset
        {
            get
            {
                return storageService.StringGetValueOrDefault(nameof(SelectedEqualizerPreset), "default");
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(SelectedEqualizerPreset), "default", value);
                RiseSettingChanged(value, nameof(SelectedEqualizerPreset));
            }
        }


        [SettingDefaultValue("folder.jpg;folder.png")]
        public string AlbumArtFolderCoverName
        {
            get
            {
                return storageService.StringGetValueOrDefault(nameof(AlbumArtFolderCoverName), "folder.jpg;folder.png");
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AlbumArtFolderCoverName), "folder.jpg;folder.png", value);
                RiseSettingChanged(value, nameof(AlbumArtFolderCoverName));
            }
        }

        [SettingDefaultValue(false)]
        public bool AutomaticPresetManagement
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutomaticPresetManagement), false);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutomaticPresetManagement), false, value);
                RiseSettingChanged(value, nameof(AutomaticPresetManagement));
            }
        }

        [SettingDefaultValue(0)]
        public int AlbumArtOptionIndex
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(AlbumArtOptionIndex), 0);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AlbumArtOptionIndex), 0, value);
                RiseSettingChanged(value, nameof(AlbumArtOptionIndex));
            }
        }

        [SettingDefaultValue(0)]
        public int FolderHierarchyMetadataIndex
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(FolderHierarchyMetadataIndex), 0);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(FolderHierarchyMetadataIndex), 0, value);
                RiseSettingChanged(value, nameof(FolderHierarchyMetadataIndex));
            }
        }

        [SettingDefaultValue("")]
        public string OpenSubtitlesApiKey
        {
            get
            {
                return storageService.StringGetValueOrDefault(nameof(OpenSubtitlesApiKey), string.Empty);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(OpenSubtitlesApiKey), string.Empty, value);
                RiseSettingChanged(value, nameof(OpenSubtitlesApiKey));
            }
        }
    }
}