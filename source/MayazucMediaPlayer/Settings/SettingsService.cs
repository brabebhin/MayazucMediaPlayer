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
                lock (singletonLock)
                {
                    if (instance == null)
                    {
                        instance = new SettingsService();
                    }

                    return instance;
                }
            }
        }

        private SettingsStoreService settingsStoreService = new SettingsStoreService();

        private const string DefaultEqualizerPresetValue = "default";

        public event EventHandler<string> RepeatModeChanged;
        public event EventHandler<bool> ShuffleModeChanged;

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

        private void UnregisterAllCallbacks()
        {
            PropertyHandlerMap.Clear();
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

        readonly Dictionary<string, PropertyInfo> settingsProperties;

        SettingsService()
        {
            var props = typeof(SettingsService).GetProperties();
            settingsProperties = props.ToDictionary(x => x.Name);
        }

        public void SetProperty(string name, object value, object? sender)
        {
            if (settingsProperties.TryGetValue(name, out var property))
            {
                property.SetValue(this, value);
            }
        }

        public Object GetProperty(String name)
        {
            if (settingsProperties.TryGetValue(name, out var property))
            {
                return property.GetValue(this);
            }
            return null;
        }

        internal void SaveSettings()
        {
            settingsStoreService.SaveSettings();
        }

        [SettingDefaultValue(false)]
        public bool AutoPlayMusic
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoPlayMusic, false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoPlayMusic, false, value);
                RiseSettingChanged(value, nameof(AutoPlayMusic));
            }
        }

        [SettingDefaultValue(false)]
        public bool AutoPlayVideo
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoPlayVideo, false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoPlayVideo, false, value);
                RiseSettingChanged(value, nameof(AutoPlayVideo));
            }
        }

        public string RepeatMode
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<string>("playerstate", "repeat", Constants.RepeatAll);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<string>("playerstate", "repeat", Constants.RepeatAll, value);
                RepeatModeChanged?.Invoke(null, value);
            }
        }

        public int PlaybackIndex
        {
            set
            {
                settingsStoreService.SetValueOrDefault<int>("playerstate", "index", 0, value);
                RiseSettingChanged(value, nameof(PlaybackIndex));
            }
            get
            {
                return settingsStoreService.GetValueOrDefault("playerstate", "index", 0);
            }
        }

        public bool ShuffleMode
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>("playerstate", "shuffle", false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>("playerstate", "shuffle", false, value);
                ShuffleModeChanged?.Invoke(null, value);
                //RiseSettingChanged(value, nameof(ShuffleMode));
            }
        }

        [SettingDefaultValue(0)]
        public double AudioBalance
        {
            get
            {
                var userOption = settingsStoreService.GetValueOrDefault<double>(ContainerNames.PlayerState, ContainerKeys.AudioBalance, 0);
                return userOption;
            }
            set
            {
                settingsStoreService.SetValueOrDefault<double>(ContainerNames.PlayerState, ContainerKeys.AudioBalance, 0, value);
                RiseSettingChanged(value, nameof(AudioBalance));
            }
        }


        [SettingDefaultValue("")]
        public string NetworkInstanceName
        {
            get
            {
                var userOption = settingsStoreService.GetValueOrDefault<string>(ContainerNames.Networking, ContainerKeys.NetworkRole, "");
                if (string.IsNullOrWhiteSpace(userOption))
                {
                    var hostNames = NetworkInformation.GetHostNames();
                    var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));
                    var computerName = localName.DisplayName.Replace(".local", "");
                    settingsStoreService.SetValueOrDefault<string>(ContainerNames.Networking, ContainerKeys.NetworkRole, "", computerName);
                }
                return userOption;
            }
            set
            {
                settingsStoreService.SetValueOrDefault<string>(ContainerNames.Networking, ContainerKeys.NetworkRole, "", value);
                RiseSettingChanged(value, nameof(NetworkInstanceName));
            }
        }

        [SettingDefaultValue(false)]
        public bool AutoClearFilePicker
        {
            get
            {
                var userOption = settingsStoreService.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoClearFilePicker, false);
                return userOption;
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoClearFilePicker, false, value);
                RiseSettingChanged(value, nameof(AutoClearFilePicker));
            }
        }


        [SettingDefaultValue(true)]
        public bool MetadataOptionsUseDefault
        {
            get
            {
                var userOption = settingsStoreService.GetValueOrDefault<bool>(ContainerNames.MetadataOptions, ContainerKeys.metadataOptionsUseDefault, true);
                return userOption;
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.MetadataOptions, ContainerKeys.metadataOptionsUseDefault, true, value);
                RiseSettingChanged(value, nameof(MetadataOptionsUseDefault));
            }
        }


        [SettingDefaultValue(0)]
        public int MetadataAlbumIndex
        {
            get
            {
                var userOption = settingsStoreService.GetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsAlbumIndex, 0);
                return userOption;
            }
            set
            {
                settingsStoreService.SetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsAlbumIndex, 0, value);
                RiseSettingChanged(value, nameof(MetadataAlbumIndex));
            }
        }

        [SettingDefaultValue(0)]
        public int MetadataArtistIndex
        {
            get
            {
                var userOption = settingsStoreService.GetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsArtistIndex, 0);
                return userOption;
            }
            set
            {
                settingsStoreService.SetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsArtistIndex, 0, value);
                RiseSettingChanged(value, nameof(MetadataArtistIndex));
            }
        }

        [SettingDefaultValue(0)]
        public int MetadataGenreIndex
        {
            get
            {
                var userOption = settingsStoreService.GetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsGenreIndex, 0);
                return userOption;
            }
            set
            {
                settingsStoreService.SetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsGenreIndex, 0, value);
                RiseSettingChanged(value, nameof(MetadataGenreIndex));
            }
        }

        public TimeSpan StopMusicOnTimerPosition
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<TimeSpan>(ContainerNames.PlayerState, ContainerKeys.playerstateStopMusicTimePosition, TimeSpan.Zero);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<TimeSpan>(ContainerNames.PlayerState, ContainerKeys.playerstateStopMusicTimePosition, TimeSpan.Zero, value);
                RiseSettingChanged(value, nameof(StopMusicOnTimerPosition));
                AppState.Current.MediaServiceConnector.PlayerInstance.HandleStopMusicOnTimer();
            }
        }

        [SettingDefaultValue(false)]
        public bool StopMusicOnTimerEnabled
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.playerstateStopMusicEnabled, false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.playerstateStopMusicEnabled, false, value);
                RiseSettingChanged(value, nameof(StopMusicOnTimerEnabled));
                AppState.Current.MediaServiceConnector.PlayerInstance.HandleStopMusicOnTimer();
            }
        }

        [SettingDefaultValue(0)]
        public long PlayerResumePosition
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<long>(ContainerNames.PlayerState, ContainerKeys.ResumePosition, 0);

            }
            set
            {
                settingsStoreService.SetValueOrDefault<long>(ContainerNames.PlayerState, ContainerKeys.ResumePosition, 0, value);
                RiseSettingChanged(value, nameof(PlayerResumePosition));

            }
        }


        [SettingDefaultValue(true)]
        public bool AutoDetectExternalSubtitle
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.subtitleAutoDetect, true);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.subtitleAutoDetect, true, value);
                RiseSettingChanged(value, nameof(AutoDetectExternalSubtitle));
            }
        }

        [SettingDefaultValue(-1)]
        public int PreferredSubtitleLanguageIndex
        {
            get
            {
                var index = settingsStoreService.GetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.subtitleLanguage, -1);
                if (index == -1)
                {
                    index = LanguageCodesService.GetDefaultLanguageIndex();
                    settingsStoreService.SetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.subtitleLanguage, -1, index);
                }

                return index;
            }
            set
            {
                var index = value;
                settingsStoreService.SetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.subtitleLanguage, -1, index);
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
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.AudioDSP, ContainerKeys.equalizerEnabled, true);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.AudioDSP, ContainerKeys.equalizerEnabled, true, value);
                RiseSettingChanged(value, nameof(EqualizerEnabled));

            }
        }

        [SettingDefaultValue(false)]
        public bool VideoOutputAllowIyuv
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllowIyuv, false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllowIyuv, false, value);
                RiseSettingChanged(value, nameof(VideoOutputAllowIyuv));

            }
        }

        [SettingDefaultValue(false)]
        public bool VideoOutputAllow10bit
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllow10bit, false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllow10bit, false, value);
                RiseSettingChanged(value, nameof(VideoOutputAllow10bit));

            }
        }

        [SettingDefaultValue(false)]
        public bool VideoOutputAllowBgra8
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllowBgra8, false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllowBgra8, false, value);
                RiseSettingChanged(value, nameof(VideoOutputAllowBgra8));

            }
        }

        [SettingDefaultValue(false)]
        public bool StartPlaybackAfterSeek
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.StartPlaybackAfterSeek, false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.StartPlaybackAfterSeek, false, value);
                RiseSettingChanged(value, nameof(StartPlaybackAfterSeek));
            }
        }

        [SettingDefaultValue(2)]
        public int PlaybackTapGestureModeRaw
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.PlaybackTapGestureMode, 2);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.PlaybackTapGestureMode, 2, value);
                RiseSettingChanged(value, nameof(PlaybackTapGestureModeRaw));
            }
        }

        [SettingDefaultValue(true)]
        public bool ShowConfirmationMessages
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.BlockConfirmationMessages, true);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.BlockConfirmationMessages, true, value);
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
                return settingsStoreService.GetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.DefaultUITheme, 0);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.DefaultUITheme, 0, value);
                RiseSettingChanged(value, nameof(DefaultUITheme));
            }
        }

        public string CurrentPlaylist
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<string>(ContainerNames.PlayerState, ContainerKeys.CurrentPlaylist, string.Empty);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<string>(ContainerNames.PlayerState, ContainerKeys.CurrentPlaylist, string.Empty, value);
                RiseSettingChanged(value, nameof(CurrentPlaylist));
            }
        }


        [SettingDefaultValue(true)]
        public bool AutoloadInternalSubtitle
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.AutoloadInternalSubtitle, true);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.AutoloadInternalSubtitle, true, value);
                RiseSettingChanged(value, nameof(AutoloadInternalSubtitle));
            }
        }

        [SettingDefaultValue(0)]
        public int FFmpegCharacterEncodingIndex
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.FFmpegCharacterEncodingIndex, 0);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.FFmpegCharacterEncodingIndex, 0, value);
                RiseSettingChanged(value, nameof(FFmpegCharacterEncodingIndex));
            }
        }

        [SettingDefaultValue(true)]
        public bool KeepPlaybackRateBetweenTracks
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.KeepPlaybackRateBetweenTracks, true);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.KeepPlaybackRateBetweenTracks, true, value);
                RiseSettingChanged(value, nameof(KeepPlaybackRateBetweenTracks));
            }
        }

        [SettingDefaultValue(false)]
        public bool AutoloadForcedSubtitles
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.AutoloadForcedSubtitles, false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.AutoloadForcedSubtitles, false, value);
                RiseSettingChanged(value, nameof(AutoloadForcedSubtitles));
            }
        }

        [SettingDefaultValue(0)]
        public int VideoDecoderMode
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.VideoDecoderMode, 0);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.VideoDecoderMode, 0, value);
                RiseSettingChanged(value, nameof(VideoDecoderMode));
            }
        }

        public string PlayerResumePath
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<string>(ContainerNames.Networking, ContainerKeys.PlayerResumePath, string.Empty);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<string>(ContainerNames.Networking, ContainerKeys.PlayerResumePath, string.Empty, value);
                RiseSettingChanged(value, nameof(PlayerResumePath));
            }
        }


        [SettingDefaultValue(0)]
        public double MinimumSubtitleDuration
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<double>(ContainerNames.Customizations, ContainerKeys.MinimumSubtitleDuration, 0);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<double>(ContainerNames.Customizations, ContainerKeys.MinimumSubtitleDuration, 0, value);
                RiseSettingChanged(value, nameof(MinimumSubtitleDuration));
            }
        }

        [SettingDefaultValue(true)]
        public bool PreventSubtitleOverlaps
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.PreventSubtitleOverlaps, true);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.PreventSubtitleOverlaps, true, value);
                RiseSettingChanged(value, nameof(PreventSubtitleOverlaps));
            }
        }


        [SettingDefaultValue(false)]
        public bool PlayToEnabled
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.PlayToEnabled, false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.PlayToEnabled, false, value);
                RiseSettingChanged(value, nameof(PlayToEnabled));
            }
        }

        public int PlaybackIndexDlnaIntrerupt
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.PlaybackIndexDlnaIntrerupt, 0);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.PlaybackIndexDlnaIntrerupt, 0, value);
                RiseSettingChanged(value, nameof(PlaybackIndexDlnaIntrerupt));
            }
        }

        public long ResumePositionDlnaIntrerupt
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<long>(ContainerNames.PlayerState, ContainerKeys.ResumePositionDlnaIntrerupt, 0);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<long>(ContainerNames.PlayerState, ContainerKeys.ResumePositionDlnaIntrerupt, 0, value);
                RiseSettingChanged(value, nameof(ResumePositionDlnaIntrerupt));
            }
        }

        [SettingDefaultValue(true)]
        public bool StereoDownMix
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.StereoDownMix, true);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.StereoDownMix, true, value);
                RiseSettingChanged(value, nameof(StereoDownMix));
            }
        }


        [SettingDefaultValue("MC Media Center")]
        public string EqualizerConfiguration
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<string>(ContainerNames.Customizations, ContainerKeys.EqualizerConfiguration, "MC Media Center");
            }
            set
            {
                settingsStoreService.SetValueOrDefault<string>(ContainerNames.Customizations, ContainerKeys.EqualizerConfiguration, "MC Media Center", value);
                RiseSettingChanged(value, nameof(EqualizerConfiguration));
            }
        }

        [SettingDefaultValue(DefaultEqualizerPresetValue)]
        public string SelectedEqualizerPreset
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<string>("savedPresetsState", "selectedPresetName", "default");
            }
            set
            {
                settingsStoreService.SetValueOrDefault<string>("savedPresetsState", "selectedPresetName", "default", value);
                RiseSettingChanged(value, nameof(SelectedEqualizerPreset));
            }
        }

        [SettingDefaultValue(true)]
        public bool OnlyUseCacheInFilePicker
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<bool>(ContainerNames.MetadataOptions, ContainerKeys.OnlyUseCacheInFilePicker, true);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<bool>(ContainerNames.MetadataOptions, ContainerKeys.OnlyUseCacheInFilePicker, true, value);
                RiseSettingChanged(value, nameof(OnlyUseCacheInFilePicker));
            }
        }

        [SettingDefaultValue("folder.jpg;folder.png")]
        public string AlbumArtFolderCoverName
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<string>("metadataoptions", "foldercovers", "folder.jpg;folder.png");
            }
            set
            {
                settingsStoreService.SetValueOrDefault<string>("metadataoptions", "foldercovers", "folder.jpg;folder.png", value);
                RiseSettingChanged(value, nameof(AlbumArtFolderCoverName));
            }
        }

        [SettingDefaultValue(false)]
        public bool AutomaticPresetManagement
        {
            get
            {
                return settingsStoreService.GetValueOrDefault("playerstate", "AutomaticPresetManagement", false);
            }
            set
            {
                settingsStoreService.SetValueOrDefault("playerstate", "AutomaticPresetManagement", false, value);
                RiseSettingChanged(value, nameof(AutomaticPresetManagement));
            }
        }

        [SettingDefaultValue(0)]
        public int AlbumArtOptionIndex
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<int>("metadataoptions", "albumartindex", 0);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<int>("metadataoptions", "albumartindex", 0, value);
                RiseSettingChanged(value, nameof(AlbumArtOptionIndex));
            }
        }

        [SettingDefaultValue(0)]
        public int FolderHierarchyMetadataIndex
        {
            get
            {
                return settingsStoreService.GetValueOrDefault<int>("metadataoptions", "FolderHierarchyMetadataIndex", 0);
            }
            set
            {
                settingsStoreService.SetValueOrDefault<int>("metadataoptions", "FolderHierarchyMetadataIndex", 0, value);
                RiseSettingChanged(value, nameof(FolderHierarchyMetadataIndex));
            }
        }
    }
}