using MayazucMediaPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.Networking.Connectivity;

namespace MayazucMediaPlayer.Settings
{
    public static class SettingsWrapper
    {
        private const string DefaultEqualizerPresetValue = "default";

        public static event EventHandler<string> RepeatModeChanged;
        public static event EventHandler<bool> ShuffleModeChanged;

        static readonly Dictionary<string, List<Action<object, string>>> PropertyHandlerMap = new Dictionary<string, List<Action<object, string>>>();

        public static Action<object, string> RegisterSettingChangeCallback(string settingName, Action<object, string> callback)
        {
            if (!PropertyHandlerMap.ContainsKey(settingName))
            {
                PropertyHandlerMap.Add(settingName, new List<Action<object, string>>());
            }

            var callbacks = PropertyHandlerMap[settingName];
            callbacks.Add(callback);
            return callback;
        }

        public static void UnregisterSettingChangeCallback(string settingName, Action<object, string> callback)
        {
            var callbacks = PropertyHandlerMap[settingName];
            callbacks.Remove(callback);
        }

        public static void UnregisterAllCallbacks()
        {
            PropertyHandlerMap.Clear();
        }

        public static void RiseSettingChanged(object value, string settingName)
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

        static readonly Dictionary<string, PropertyInfo> settingsProperties;
        static SettingsWrapper()
        {
            var props = typeof(SettingsWrapper).GetProperties();
            settingsProperties = props.ToDictionary(x => x.Name);
        }

        public static void SetProperty(string name, object value, object? sender)
        {
            if (settingsProperties.TryGetValue(name, out var property))
            {
                property.SetValue(null, value);
            }
        }

        public static Object GetProperty(String name)
        {
            if (settingsProperties.TryGetValue(name, out var property))
            {
                return property.GetValue(null);
            }
            return null;
        }

        [SettingDefaultValue(false)]
        public static bool AutoPlayMusic
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoPlayMusic, false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoPlayMusic, false, value);
                RiseSettingChanged(value, nameof(AutoPlayMusic));
            }
        }


        [SettingDefaultValue(false)]
        public static bool AutoPlayVideo
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoPlayVideo, false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoPlayVideo, false, value);
                RiseSettingChanged(value, nameof(AutoPlayVideo));
            }
        }

        public static string RepeatMode
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<string>("playerstate", "repeat", Constants.RepeatAll);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<string>("playerstate", "repeat", Constants.RepeatAll, value);
                RepeatModeChanged?.Invoke(null, value);
            }
        }

        public static int PlaybackIndex
        {
            set
            {
                SettingsHelpers.SetValueOrDefault<int>("playerstate", "index", 0, value);
                RiseSettingChanged(value, nameof(PlaybackIndex));
            }
            get
            {
                return SettingsHelpers.GetValueOrDefault("playerstate", "index", 0);
            }
        }

        public static bool ShuffleMode
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>("playerstate", "shuffle", false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>("playerstate", "shuffle", false, value);
                ShuffleModeChanged?.Invoke(null, value);
                //RiseSettingChanged(value, nameof(ShuffleMode));
            }
        }

        [SettingDefaultValue(0)]
        public static double AudioBalance
        {
            get
            {
                var userOption = SettingsHelpers.GetValueOrDefault<double>(ContainerNames.PlayerState, ContainerKeys.AudioBalance, 0);
                return userOption;
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<double>(ContainerNames.PlayerState, ContainerKeys.AudioBalance, 0, value);
                RiseSettingChanged(value, nameof(AudioBalance));
            }
        }


        [SettingDefaultValue("")]
        public static string NetworkInstanceName
        {
            get
            {
                var userOption = SettingsHelpers.GetValueOrDefault<string>(ContainerNames.Networking, ContainerKeys.NetworkRole, "");
                if (string.IsNullOrWhiteSpace(userOption))
                {
                    var hostNames = NetworkInformation.GetHostNames();
                    var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));
                    var computerName = localName.DisplayName.Replace(".local", "");
                    SettingsHelpers.SetValueOrDefault<string>(ContainerNames.Networking, ContainerKeys.NetworkRole, "", computerName);
                }
                return userOption;
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<string>(ContainerNames.Networking, ContainerKeys.NetworkRole, "", value);
                RiseSettingChanged(value, nameof(NetworkInstanceName));
            }
        }

        [SettingDefaultValue(false)]
        public static bool AutoClearFilePicker
        {
            get
            {
                var userOption = SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoClearFilePicker, false);
                return userOption;
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.AutoClearFilePicker, false, value);
                RiseSettingChanged(value, nameof(AutoClearFilePicker));
            }
        }


        [SettingDefaultValue(true)]
        public static bool MetadataOptionsUseDefault
        {
            get
            {
                var userOption = SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.MetadataOptions, ContainerKeys.metadataOptionsUseDefault, true);
                return userOption;
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.MetadataOptions, ContainerKeys.metadataOptionsUseDefault, true, value);
                RiseSettingChanged(value, nameof(MetadataOptionsUseDefault));
            }
        }


        [SettingDefaultValue(0)]
        public static int MetadataAlbumIndex
        {
            get
            {
                var userOption = SettingsHelpers.GetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsAlbumIndex, 0);
                return userOption;
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsAlbumIndex, 0, value);
                RiseSettingChanged(value, nameof(MetadataAlbumIndex));
            }
        }

        [SettingDefaultValue(0)]
        public static int MetadataArtistIndex
        {
            get
            {
                var userOption = SettingsHelpers.GetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsArtistIndex, 0);
                return userOption;
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsArtistIndex, 0, value);
                RiseSettingChanged(value, nameof(MetadataArtistIndex));
            }
        }

        [SettingDefaultValue(0)]
        public static int MetadataGenreIndex
        {
            get
            {
                var userOption = SettingsHelpers.GetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsGenreIndex, 0);
                return userOption;
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<int>(ContainerNames.MetadataOptions, ContainerKeys.metadataoptionsGenreIndex, 0, value);
                RiseSettingChanged(value, nameof(MetadataGenreIndex));
            }
        }

        public static TimeSpan StopMusicOnTimerPosition
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<TimeSpan>(ContainerNames.PlayerState, ContainerKeys.playerstateStopMusicTimePosition, TimeSpan.Zero);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<TimeSpan>(ContainerNames.PlayerState, ContainerKeys.playerstateStopMusicTimePosition, TimeSpan.Zero, value);
                RiseSettingChanged(value, nameof(StopMusicOnTimerPosition));
                AppState.Current.MediaServiceConnector.PlayerInstance.HandleStopMusicOnTimer();
            }
        }

        [SettingDefaultValue(false)]
        public static bool StopMusicOnTimerEnabled
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.playerstateStopMusicEnabled, false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.playerstateStopMusicEnabled, false, value);
                RiseSettingChanged(value, nameof(StopMusicOnTimerEnabled));
                AppState.Current.MediaServiceConnector.PlayerInstance.HandleStopMusicOnTimer();
            }
        }

        [SettingDefaultValue(0)]
        public static long PlayerResumePosition
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<long>(ContainerNames.PlayerState, ContainerKeys.ResumePosition, 0);

            }
            set
            {
                SettingsHelpers.SetValueOrDefault<long>(ContainerNames.PlayerState, ContainerKeys.ResumePosition, 0, value);
                RiseSettingChanged(value, nameof(PlayerResumePosition));

            }
        }


        [SettingDefaultValue(true)]
        public static bool AutoDetectExternalSubtitle
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.subtitleAutoDetect, true);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.subtitleAutoDetect, true, value);
                RiseSettingChanged(value, nameof(AutoDetectExternalSubtitle));
            }
        }

        [SettingDefaultValue(-1)]
        public static int PreferredSubtitleLanguageIndex
        {
            get
            {
                var index = SettingsHelpers.GetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.subtitleLanguage, -1);
                if (index == -1)
                {
                    index = LanguageCodesService.GetDefaultLanguageIndex();
                    SettingsHelpers.SetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.subtitleLanguage, -1, index);
                }

                return index;
            }
            set
            {
                var index = value;
                SettingsHelpers.SetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.subtitleLanguage, -1, index);
                RiseSettingChanged(value, nameof(PreferredSubtitleLanguageIndex));
            }
        }

        public static LanguageCode PreferredSubtitleLanguage
        {
            get
            {
                return LanguageCodesService.Codes[SettingsWrapper.PreferredSubtitleLanguageIndex];
            }
        }

        [SettingDefaultValue(true)]
        public static bool EqualizerEnabled
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.AudioDSP, ContainerKeys.equalizerEnabled, true);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.AudioDSP, ContainerKeys.equalizerEnabled, true, value);
                RiseSettingChanged(value, nameof(EqualizerEnabled));

            }
        }

        [SettingDefaultValue(false)]
        public static bool VideoOutputAllowIyuv
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllowIyuv, false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllowIyuv, false, value);
                RiseSettingChanged(value, nameof(VideoOutputAllowIyuv));

            }
        }

        [SettingDefaultValue(false)]
        public static bool VideoOutputAllow10bit
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllow10bit, false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllow10bit, false, value);
                RiseSettingChanged(value, nameof(VideoOutputAllow10bit));

            }
        }

        [SettingDefaultValue(false)]
        public static bool VideoOutputAllowBgra8
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllowBgra8, false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.VideoOutputAllowBgra8, false, value);
                RiseSettingChanged(value, nameof(VideoOutputAllowBgra8));

            }
        }



        [SettingDefaultValue(false)]
        public static bool StartPlaybackAfterSeek
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.StartPlaybackAfterSeek, false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.StartPlaybackAfterSeek, false, value);
                RiseSettingChanged(value, nameof(StartPlaybackAfterSeek));
            }
        }

        [SettingDefaultValue(2)]
        public static int PlaybackTapGestureModeRaw
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.PlaybackTapGestureMode, 2);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.PlaybackTapGestureMode, 2, value);
                RiseSettingChanged(value, nameof(PlaybackTapGestureModeRaw));
            }
        }

        [SettingDefaultValue(true)]
        public static bool ShowConfirmationMessages
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.BlockConfirmationMessages, true);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.BlockConfirmationMessages, true, value);
                RiseSettingChanged(value, nameof(ShowConfirmationMessages));
            }
        }

        public static PlaybackTapGestureMode PlaybackTapGestureMode
        {
            get
            {
                return (PlaybackTapGestureMode)PlaybackTapGestureModeRaw;
            }
        }

        [SettingDefaultValue(0)]
        public static int DefaultUITheme
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.DefaultUITheme, 0);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<int>(ContainerNames.Customizations, ContainerKeys.DefaultUITheme, 0, value);
                RiseSettingChanged(value, nameof(DefaultUITheme));
            }
        }

        public static string CurrentPlaylist
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<string>(ContainerNames.PlayerState, ContainerKeys.CurrentPlaylist, string.Empty);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<string>(ContainerNames.PlayerState, ContainerKeys.CurrentPlaylist, string.Empty, value);
                RiseSettingChanged(value, nameof(CurrentPlaylist));
            }
        }


        [SettingDefaultValue(true)]
        public static bool AutoloadInternalSubtitle
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.AutoloadInternalSubtitle, true);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.AutoloadInternalSubtitle, true, value);
                RiseSettingChanged(value, nameof(AutoloadInternalSubtitle));
            }
        }

        [SettingDefaultValue(0)]
        public static int FFmpegCharacterEncodingIndex
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.FFmpegCharacterEncodingIndex, 0);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.FFmpegCharacterEncodingIndex, 0, value);
                RiseSettingChanged(value, nameof(FFmpegCharacterEncodingIndex));
            }
        }


        [SettingDefaultValue(true)]
        public static bool KeepPlaybackRateBetweenTracks
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.KeepPlaybackRateBetweenTracks, true);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.KeepPlaybackRateBetweenTracks, true, value);
                RiseSettingChanged(value, nameof(KeepPlaybackRateBetweenTracks));
            }
        }


        [SettingDefaultValue(false)]
        public static bool AutoloadForcedSubtitles
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.AutoloadForcedSubtitles, false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.AutoloadForcedSubtitles, false, value);
                RiseSettingChanged(value, nameof(AutoloadForcedSubtitles));
            }
        }

        [SettingDefaultValue(0)]
        public static int VideoDecoderMode
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.VideoDecoderMode, 0);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.VideoDecoderMode, 0, value);
                RiseSettingChanged(value, nameof(VideoDecoderMode));
            }
        }

        public static string PlayerResumePath
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<string>(ContainerNames.Networking, ContainerKeys.PlayerResumePath, string.Empty);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<string>(ContainerNames.Networking, ContainerKeys.PlayerResumePath, string.Empty, value);
                RiseSettingChanged(value, nameof(PlayerResumePath));
            }
        }


        [SettingDefaultValue(0)]
        public static double MinimumSubtitleDuration
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<double>(ContainerNames.Customizations, ContainerKeys.MinimumSubtitleDuration, 0);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<double>(ContainerNames.Customizations, ContainerKeys.MinimumSubtitleDuration, 0, value);
                RiseSettingChanged(value, nameof(MinimumSubtitleDuration));
            }
        }

        [SettingDefaultValue(true)]
        public static bool PreventSubtitleOverlaps
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.PreventSubtitleOverlaps, true);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.PreventSubtitleOverlaps, true, value);
                RiseSettingChanged(value, nameof(PreventSubtitleOverlaps));
            }
        }


        [SettingDefaultValue(false)]
        public static bool PlayToEnabled
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.PlayToEnabled, false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.Customizations, ContainerKeys.PlayToEnabled, false, value);
                RiseSettingChanged(value, nameof(PlayToEnabled));
            }
        }

        public static int PlaybackIndexDlnaIntrerupt
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.PlaybackIndexDlnaIntrerupt, 0);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<int>(ContainerNames.PlayerState, ContainerKeys.PlaybackIndexDlnaIntrerupt, 0, value);
                RiseSettingChanged(value, nameof(PlaybackIndexDlnaIntrerupt));
            }
        }

        public static long ResumePositionDlnaIntrerupt
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<long>(ContainerNames.PlayerState, ContainerKeys.ResumePositionDlnaIntrerupt, 0);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<long>(ContainerNames.PlayerState, ContainerKeys.ResumePositionDlnaIntrerupt, 0, value);
                RiseSettingChanged(value, nameof(ResumePositionDlnaIntrerupt));
            }
        }

        [SettingDefaultValue(true)]
        public static bool StereoDownMix
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.StereoDownMix, true);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.PlayerState, ContainerKeys.StereoDownMix, true, value);
                RiseSettingChanged(value, nameof(StereoDownMix));
            }
        }


        [SettingDefaultValue("MC Media Center")]
        public static string EqualizerConfiguration
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<string>(ContainerNames.Customizations, ContainerKeys.EqualizerConfiguration, "MC Media Center");
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<string>(ContainerNames.Customizations, ContainerKeys.EqualizerConfiguration, "MC Media Center", value);
                RiseSettingChanged(value, nameof(EqualizerConfiguration));
            }
        }

        [SettingDefaultValue(DefaultEqualizerPresetValue)]
        public static string SelectedEqualizerPreset
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<string>("savedPresetsState", "selectedPresetName", "default");
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<string>("savedPresetsState", "selectedPresetName", "default", value);
                RiseSettingChanged(value, nameof(SelectedEqualizerPreset));
            }
        }

        [SettingDefaultValue(true)]
        public static bool OnlyUseCacheInFilePicker
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<bool>(ContainerNames.MetadataOptions, ContainerKeys.OnlyUseCacheInFilePicker, true);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<bool>(ContainerNames.MetadataOptions, ContainerKeys.OnlyUseCacheInFilePicker, true, value);
                RiseSettingChanged(value, nameof(StereoDownMix));
            }
        }

        [SettingDefaultValue("folder.jpg;folder.png")]
        public static string AlbumArtFolderCoverName
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<string>("metadataoptions", "foldercovers", "folder.jpg;folder.png");
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<string>("metadataoptions", "foldercovers", "folder.jpg;folder.png", value);
                RiseSettingChanged(value, nameof(AlbumArtFolderCoverName));
            }
        }

        [SettingDefaultValue(false)]
        public static bool AutomaticPresetManagement
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault("playerstate", "AutomaticPresetManagement", false);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault("playerstate", "AutomaticPresetManagement", false, value);
                RiseSettingChanged(value, nameof(AutomaticPresetManagement));
            }
        }

        [SettingDefaultValue(0)]
        public static int AlbumArtOptionIndex
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<int>("metadataoptions", "albumartindex", 0);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<int>("metadataoptions", "albumartindex", 0, value);
                RiseSettingChanged(value, nameof(AlbumArtOptionIndex));
            }
        }

        [SettingDefaultValue(0)]
        public static int FolderHierarchyMetadataIndex
        {
            get
            {
                return SettingsHelpers.GetValueOrDefault<int>("metadataoptions", "FolderHierarchyMetadataIndex", 0);
            }
            set
            {
                SettingsHelpers.SetValueOrDefault<int>("metadataoptions", "FolderHierarchyMetadataIndex", 0, value);
                RiseSettingChanged(value, nameof(FolderHierarchyMetadataIndex));
            }
        }
    }
}