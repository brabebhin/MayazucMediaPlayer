using MayazucMediaPlayer.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows.Media.Animation;
using Windows.Networking.Connectivity;

namespace MayazucMediaPlayer.Settings
{
    public partial class SettingsService : IDisposable
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
        readonly static ReadOnlyDictionary<string, object> DefaultValuesMap = new ReadOnlyDictionary<string, object>(
            new Dictionary<string, object>
            {
                { nameof(AutoPlayMusic), false },
                { nameof(AutoPlayVideo), false },
                { nameof(RepeatMode), Constants.RepeatAll },
                { nameof(ShuffleMode), false },
                { nameof(AudioBalance), 0.0 },
                { nameof(MetadataOptionsUseDefault), true },
                { nameof(MetadataAlbumIndex), 0 },
                { nameof(MetadataArtistIndex), 0 },
                { nameof(MetadataGenreIndex), 0 },
                { nameof(StopMusicOnTimerPosition), TimeSpan.Zero },
                { nameof(StopMusicOnTimerEnabled), false },
                { nameof(PlayerResumePosition), 0.0 },
                { nameof(AutoDetectExternalSubtitle), true },
                { nameof(PreferredSubtitleLanguageIndex), -1 },
                { nameof(EqualizerEnabled), true },
                { nameof(DefaultUITheme), 0 },
                { nameof(AutoloadInternalSubtitle), true },
                { nameof(FFmpegCharacterEncodingIndex), 0 },
                { nameof(AutoloadForcedSubtitles), false },
                { nameof(VideoDecoderMode), 0 },
                { nameof(MinimumSubtitleDuration), 0.0 },
                { nameof(PreventSubtitleOverlaps), true },
                { nameof(PlayToEnabled), false },
                { nameof(StereoDownMix),true  },
                { nameof(EqualizerConfiguration), "MC Media Center" },
                { nameof(SelectedEqualizerPreset), DefaultEqualizerPresetValue },
                { nameof(AlbumArtFolderCoverName), "folder.jpg;folder.png" },
                { nameof(AutomaticPresetManagement), false },
                { nameof(AlbumArtOptionIndex), 0 },
                { nameof(FolderHierarchyMetadataIndex), 0 },
                { nameof(OpenSubtitlesApiKey), string.Empty },
                { nameof(EchoMountainsEffectEnabled), false },
                { nameof(EchoInstrumentsEffectEnabled), false },
                { nameof(EchoRoboticVoiceEffectEnabled), false },
                { nameof(PlaybackIndexDlnaIntrerupt), 0  },
                { nameof(ResumePositionDlnaIntrerupt), 0.0   },
                { nameof(PlayerResumePath), string.Empty },
            }
        );

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

        public void Dispose()
        {
            SaveSettings();
            autoSaveTimer.Stop();
            autoSaveTimer.Dispose();
        }

        public void ResetToDefaultValues()
        {
            AutoPlayMusic = (bool)DefaultValuesMap[nameof(AutoPlayMusic)];
            AutoPlayVideo = (bool)DefaultValuesMap[nameof(AutoPlayVideo)];
            AudioBalance = (double)DefaultValuesMap[nameof(AudioBalance)];
            MetadataOptionsUseDefault = (bool)DefaultValuesMap[nameof(MetadataOptionsUseDefault)];
            MetadataAlbumIndex = (int)DefaultValuesMap[nameof(MetadataAlbumIndex)];
            MetadataArtistIndex = (int)DefaultValuesMap[nameof(MetadataArtistIndex)];
            MetadataGenreIndex = (int)DefaultValuesMap[nameof(MetadataGenreIndex)];
            StopMusicOnTimerEnabled = (bool)DefaultValuesMap[nameof(StopMusicOnTimerEnabled)];
            PlayerResumePosition = (double)DefaultValuesMap[nameof(PlayerResumePosition)];
            AutoDetectExternalSubtitle = (bool)DefaultValuesMap[nameof(AutoDetectExternalSubtitle)];
            PreferredSubtitleLanguageIndex = (int)DefaultValuesMap[nameof(PreferredSubtitleLanguageIndex)];
            EqualizerEnabled = (bool)DefaultValuesMap[nameof(EqualizerEnabled)];
            DefaultUITheme = (int)DefaultValuesMap[nameof(DefaultUITheme)];
            AutoloadInternalSubtitle = (bool)DefaultValuesMap[nameof(AutoloadInternalSubtitle)];
            FFmpegCharacterEncodingIndex = (int)DefaultValuesMap[nameof(FFmpegCharacterEncodingIndex)];
            AutoloadForcedSubtitles = (bool)DefaultValuesMap[nameof(AutoloadForcedSubtitles)];
            VideoDecoderMode = (int)DefaultValuesMap[nameof(VideoDecoderMode)];
            MinimumSubtitleDuration = (double)DefaultValuesMap[nameof(MinimumSubtitleDuration)];
            PreventSubtitleOverlaps = (bool)DefaultValuesMap[nameof(PreventSubtitleOverlaps)];
            StereoDownMix = (bool)DefaultValuesMap[nameof(StereoDownMix)];
            SelectedEqualizerPreset = (string)DefaultValuesMap[nameof(SelectedEqualizerPreset)];
            AlbumArtFolderCoverName = (string)DefaultValuesMap[nameof(AlbumArtFolderCoverName)];
            AutomaticPresetManagement = (bool)DefaultValuesMap[nameof(AutomaticPresetManagement)];
            AlbumArtOptionIndex = (int)DefaultValuesMap[nameof(AlbumArtOptionIndex)];
            FolderHierarchyMetadataIndex = (int)DefaultValuesMap[nameof(FolderHierarchyMetadataIndex)];
            OpenSubtitlesApiKey = (string)DefaultValuesMap[nameof(OpenSubtitlesApiKey)];
            EchoMountainsEffectEnabled = (bool)DefaultValuesMap[nameof(EchoMountainsEffectEnabled)];
            EchoInstrumentsEffectEnabled = (bool)DefaultValuesMap[nameof(EchoInstrumentsEffectEnabled)];
            EchoRoboticVoiceEffectEnabled = (bool)DefaultValuesMap[nameof(EchoRoboticVoiceEffectEnabled)];
            PlayerResumePath = (string)DefaultValuesMap[nameof(PlayerResumePath)];
        }


        public bool AutoPlayMusic
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutoPlayMusic), (bool)DefaultValuesMap[nameof(AutoPlayMusic)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutoPlayMusic), (bool)DefaultValuesMap[nameof(AutoPlayMusic)], value);
                RiseSettingChanged(value, nameof(AutoPlayMusic));
            }
        }

        public bool AutoPlayVideo
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutoPlayVideo), (bool)DefaultValuesMap[nameof(AutoPlayVideo)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutoPlayVideo), (bool)DefaultValuesMap[nameof(AutoPlayVideo)], value);
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

        public double AudioBalance
        {
            get
            {
                var userOption = storageService.DoubleGetValueOrDefault(nameof(AudioBalance), (double)DefaultValuesMap[nameof(AudioBalance)]);
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AudioBalance), ((double)DefaultValuesMap[nameof(AudioBalance)]), value);
                RiseSettingChanged(value, nameof(AudioBalance));
            }
        }


        public string NetworkInstanceName
        {
            get
            {
                var userOption = storageService.StringGetValueOrDefault(nameof(NetworkInstanceName), (string)DefaultValuesMap[nameof(NetworkInstanceName)]);
                if (string.IsNullOrWhiteSpace(userOption))
                {
                    var hostNames = NetworkInformation.GetHostNames();
                    var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));
                    var computerName = localName.DisplayName.Replace(".local", (string)DefaultValuesMap[nameof(NetworkInstanceName)]);
                    storageService.SetValueOrDefault2(nameof(NetworkInstanceName), (string)DefaultValuesMap[nameof(NetworkInstanceName)], computerName);
                }
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(NetworkInstanceName), (string)DefaultValuesMap[nameof(NetworkInstanceName)], value);
                RiseSettingChanged(value, nameof(NetworkInstanceName));
            }
        }

        public bool MetadataOptionsUseDefault
        {
            get
            {
                var userOption = storageService.BoolGetValueOrDefault(nameof(MetadataOptionsUseDefault), (bool)DefaultValuesMap[nameof(MetadataOptionsUseDefault)]);
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(MetadataOptionsUseDefault), (bool)DefaultValuesMap[nameof(MetadataOptionsUseDefault)], value);
                RiseSettingChanged(value, nameof(MetadataOptionsUseDefault));
            }
        }

        public int MetadataAlbumIndex
        {
            get
            {
                var userOption = storageService.IntGetValueOrDefault(nameof(MetadataAlbumIndex), (int)DefaultValuesMap[nameof(MetadataAlbumIndex)]);
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(MetadataAlbumIndex), (int)DefaultValuesMap[nameof(MetadataAlbumIndex)], value);
                RiseSettingChanged(value, nameof(MetadataAlbumIndex));
            }
        }

        public int MetadataArtistIndex
        {
            get
            {
                var userOption = storageService.IntGetValueOrDefault(nameof(MetadataArtistIndex), (int)DefaultValuesMap[nameof(MetadataArtistIndex)]);
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(MetadataArtistIndex), (int)DefaultValuesMap[nameof(MetadataArtistIndex)], value);
                RiseSettingChanged(value, nameof(MetadataArtistIndex));
            }
        }

        public int MetadataGenreIndex
        {
            get
            {
                var userOption = storageService.IntGetValueOrDefault(nameof(MetadataGenreIndex), (int)DefaultValuesMap[nameof(MetadataGenreIndex)]);
                return userOption;
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(MetadataGenreIndex), (int)DefaultValuesMap[nameof(MetadataGenreIndex)], value);
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

        public bool StopMusicOnTimerEnabled
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(StopMusicOnTimerEnabled), (bool)DefaultValuesMap[nameof(StopMusicOnTimerEnabled)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(StopMusicOnTimerEnabled), (bool)DefaultValuesMap[nameof(StopMusicOnTimerEnabled)], value);
                RiseSettingChanged(value, nameof(StopMusicOnTimerEnabled));
                AppState.Current.MediaServiceConnector.PlayerInstance.HandleStopMusicOnTimer();
            }
        }

        public double PlayerResumePosition
        {
            get
            {
                return storageService.DoubleGetValueOrDefault(nameof(PlayerResumePosition), (double)DefaultValuesMap[nameof(PlayerResumePosition)]);

            }
            set
            {
                storageService.SetValueOrDefault2(nameof(PlayerResumePosition), (double)DefaultValuesMap[nameof(PlayerResumePosition)], value);
                RiseSettingChanged(value, nameof(PlayerResumePosition));
            }
        }

        public bool AutoDetectExternalSubtitle
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutoDetectExternalSubtitle), (bool)DefaultValuesMap[nameof(AutoDetectExternalSubtitle)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutoDetectExternalSubtitle), (bool)DefaultValuesMap[nameof(AutoDetectExternalSubtitle)], value);
                RiseSettingChanged(value, nameof(AutoDetectExternalSubtitle));
            }
        }

        public int PreferredSubtitleLanguageIndex
        {
            get
            {
                var index = storageService.IntGetValueOrDefault(nameof(PreferredSubtitleLanguageIndex), (int)DefaultValuesMap[nameof(PreferredSubtitleLanguageIndex)]);
                if (index == (int)DefaultValuesMap[nameof(PreferredSubtitleLanguageIndex)])
                {
                    index = LanguageCodesService.GetDefaultLanguageIndex();
                    storageService.SetValueOrDefault2(nameof(PreferredSubtitleLanguageIndex), (int)DefaultValuesMap[nameof(PreferredSubtitleLanguageIndex)], index);
                }

                return index;
            }
            set
            {
                var index = value;
                storageService.SetValueOrDefault2(nameof(PreferredSubtitleLanguageIndex), (int)DefaultValuesMap[nameof(PreferredSubtitleLanguageIndex)], index);
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

        public bool EqualizerEnabled
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(EqualizerEnabled), (bool)DefaultValuesMap[nameof(EqualizerEnabled)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(EqualizerEnabled), (bool)DefaultValuesMap[nameof(EqualizerEnabled)], value);
                RiseSettingChanged(value, nameof(EqualizerEnabled));

            }
        }

        public int DefaultUITheme
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(DefaultUITheme), (int)DefaultValuesMap[nameof(DefaultUITheme)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(DefaultUITheme), (int)DefaultValuesMap[nameof(DefaultUITheme)], value);
                RiseSettingChanged(value, nameof(DefaultUITheme));
            }
        }


        public bool AutoloadInternalSubtitle
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutoloadInternalSubtitle), (bool)DefaultValuesMap[nameof(AutoloadInternalSubtitle)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutoloadInternalSubtitle), (bool)DefaultValuesMap[nameof(AutoloadInternalSubtitle)], value);
                RiseSettingChanged(value, nameof(AutoloadInternalSubtitle));
            }
        }

        public int FFmpegCharacterEncodingIndex
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(FFmpegCharacterEncodingIndex), (int)DefaultValuesMap[nameof(FFmpegCharacterEncodingIndex)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(FFmpegCharacterEncodingIndex), (int)DefaultValuesMap[nameof(FFmpegCharacterEncodingIndex)], value);
                RiseSettingChanged(value, nameof(FFmpegCharacterEncodingIndex));
            }
        }

        public bool AutoloadForcedSubtitles
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutoloadForcedSubtitles), (bool)DefaultValuesMap[nameof(AutoloadForcedSubtitles)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutoloadForcedSubtitles), (bool)DefaultValuesMap[nameof(AutoloadForcedSubtitles)], value);
                RiseSettingChanged(value, nameof(AutoloadForcedSubtitles));
            }
        }

        public int VideoDecoderMode
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(VideoDecoderMode), (int)DefaultValuesMap[nameof(VideoDecoderMode)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(VideoDecoderMode), (int)DefaultValuesMap[nameof(VideoDecoderMode)], value);
                RiseSettingChanged(value, nameof(VideoDecoderMode));
            }
        }

        public string PlayerResumePath
        {
            get
            {
                return storageService.StringGetValueOrDefault(nameof(PlayerResumePath), (string)DefaultValuesMap[nameof(PlayerResumePath)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(PlayerResumePath), (string)DefaultValuesMap[nameof(PlayerResumePath)], value);
                RiseSettingChanged(value, nameof(PlayerResumePath));
            }
        }

        public double MinimumSubtitleDuration
        {
            get
            {
                return storageService.DoubleGetValueOrDefault(nameof(MinimumSubtitleDuration), (double)DefaultValuesMap[nameof(MinimumSubtitleDuration)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(MinimumSubtitleDuration), (double)DefaultValuesMap[nameof(MinimumSubtitleDuration)], value);
                RiseSettingChanged(value, nameof(MinimumSubtitleDuration));
            }
        }

        public bool PreventSubtitleOverlaps
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(PreventSubtitleOverlaps), (bool)DefaultValuesMap[nameof(PreventSubtitleOverlaps)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(PreventSubtitleOverlaps), (bool)DefaultValuesMap[nameof(PreventSubtitleOverlaps)], value);
                RiseSettingChanged(value, nameof(PreventSubtitleOverlaps));
            }
        }

        public bool PlayToEnabled
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(PlayToEnabled), (bool)DefaultValuesMap[nameof(PlayToEnabled)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(PlayToEnabled), (bool)DefaultValuesMap[nameof(PlayToEnabled)], value);
                RiseSettingChanged(value, nameof(PlayToEnabled));
            }
        }

        public int PlaybackIndexDlnaIntrerupt
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(PlaybackIndexDlnaIntrerupt), (int)DefaultValuesMap[nameof(PlaybackIndexDlnaIntrerupt)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(PlaybackIndexDlnaIntrerupt), (int)DefaultValuesMap[nameof(PlaybackIndexDlnaIntrerupt)], value);
                RiseSettingChanged(value, nameof(PlaybackIndexDlnaIntrerupt));
            }
        }

        public double ResumePositionDlnaIntrerupt
        {
            get
            {
                return storageService.DoubleGetValueOrDefault(nameof(ResumePositionDlnaIntrerupt), (int)DefaultValuesMap[nameof(ResumePositionDlnaIntrerupt)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(ResumePositionDlnaIntrerupt), (int)DefaultValuesMap[nameof(ResumePositionDlnaIntrerupt)], value);
                RiseSettingChanged(value, nameof(ResumePositionDlnaIntrerupt));
            }
        }

        public bool StereoDownMix
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(StereoDownMix), (bool)DefaultValuesMap[nameof(StereoDownMix)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(StereoDownMix), (bool)DefaultValuesMap[nameof(StereoDownMix)], value);
                RiseSettingChanged(value, nameof(StereoDownMix));
            }
        }

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

        public string SelectedEqualizerPreset
        {
            get
            {
                return storageService.StringGetValueOrDefault(nameof(SelectedEqualizerPreset), (string)DefaultValuesMap[nameof(SelectedEqualizerPreset)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(SelectedEqualizerPreset), (string)DefaultValuesMap[nameof(SelectedEqualizerPreset)], value);
                RiseSettingChanged(value, nameof(SelectedEqualizerPreset));
            }
        }

        public string AlbumArtFolderCoverName
        {
            get
            {
                return storageService.StringGetValueOrDefault(nameof(AlbumArtFolderCoverName), (string)DefaultValuesMap[nameof(AlbumArtFolderCoverName)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AlbumArtFolderCoverName), (string)DefaultValuesMap[nameof(AlbumArtFolderCoverName)], value);
                RiseSettingChanged(value, nameof(AlbumArtFolderCoverName));
            }
        }

        public bool AutomaticPresetManagement
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(AutomaticPresetManagement), (bool)DefaultValuesMap[nameof(AutomaticPresetManagement)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AutomaticPresetManagement), (bool)DefaultValuesMap[nameof(AutomaticPresetManagement)], value);
                RiseSettingChanged(value, nameof(AutomaticPresetManagement));
            }
        }

        public int AlbumArtOptionIndex
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(AlbumArtOptionIndex), (int)DefaultValuesMap[nameof(AlbumArtOptionIndex)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(AlbumArtOptionIndex), (int)DefaultValuesMap[nameof(AlbumArtOptionIndex)], value);
                RiseSettingChanged(value, nameof(AlbumArtOptionIndex));
            }
        }

        public int FolderHierarchyMetadataIndex
        {
            get
            {
                return storageService.IntGetValueOrDefault(nameof(FolderHierarchyMetadataIndex), (int)DefaultValuesMap[nameof(FolderHierarchyMetadataIndex)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(FolderHierarchyMetadataIndex), (int)DefaultValuesMap[nameof(FolderHierarchyMetadataIndex)], value);
                RiseSettingChanged(value, nameof(FolderHierarchyMetadataIndex));
            }
        }

        public string OpenSubtitlesApiKey
        {
            get
            {
                return storageService.StringGetValueOrDefault(nameof(OpenSubtitlesApiKey), (string)DefaultValuesMap[nameof(OpenSubtitlesApiKey)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(OpenSubtitlesApiKey), (string)DefaultValuesMap[nameof(OpenSubtitlesApiKey)], value);
                RiseSettingChanged(value, nameof(OpenSubtitlesApiKey));
            }
        }

        public bool EchoMountainsEffectEnabled
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(EchoMountainsEffectEnabled), (bool)DefaultValuesMap[nameof(EchoMountainsEffectEnabled)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(EchoMountainsEffectEnabled), (bool)DefaultValuesMap[nameof(EchoMountainsEffectEnabled)], value);
                RiseSettingChanged(value, nameof(EchoMountainsEffectEnabled));
            }
        }

        public bool EchoInstrumentsEffectEnabled
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(EchoInstrumentsEffectEnabled), (bool)DefaultValuesMap[nameof(EchoInstrumentsEffectEnabled)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(EchoInstrumentsEffectEnabled), (bool)DefaultValuesMap[nameof(EchoInstrumentsEffectEnabled)], value);
                RiseSettingChanged(value, nameof(EchoInstrumentsEffectEnabled));
            }
        }

        public bool EchoRoboticVoiceEffectEnabled
        {
            get
            {
                return storageService.BoolGetValueOrDefault(nameof(EchoRoboticVoiceEffectEnabled), (bool)DefaultValuesMap[nameof(EchoRoboticVoiceEffectEnabled)]);
            }
            set
            {
                storageService.SetValueOrDefault2(nameof(EchoRoboticVoiceEffectEnabled), (bool)DefaultValuesMap[nameof(EchoRoboticVoiceEffectEnabled)], value);
                RiseSettingChanged(value, nameof(EchoRoboticVoiceEffectEnabled));
            }
        }
    }
}
