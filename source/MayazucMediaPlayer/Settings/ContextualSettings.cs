using FFmpegInteropX;
using MayazucMediaPlayer.AudioEffects;
using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MayazucMediaPlayer.Settings
{
    public static class ContextualSettings
    {
        public static SettingsItemGroup GetAutoPlaySettings()
        {
            var Title = "Auto play";
            SettingsItemGroup items = new SettingsItemGroup(groupImage: FontIconPaths.AutoPlaySettings, groupName: Title);

            items.Add(new VerbatimTextBlock()
            {
                TextDescription = "Automatically enqueue the next music or video file in the last track's folder"
            });

            items.Add(new CheckBoxItem(nameof(SettingsService.Instance.AutoPlayMusic),
                (value) => { SettingsService.Instance.AutoPlayMusic = (bool)value; },
                () => { return SettingsService.Instance.AutoPlayMusic; })
            {
                Description = "Auto play music",
            });

            items.Add(new CheckBoxItem(nameof(SettingsService.Instance.AutoPlayVideo),
                (value) => { SettingsService.Instance.AutoPlayVideo = (bool)value; },
                () => { return SettingsService.Instance.AutoPlayVideo; })
            {
                Description = "Auto play video",
            });


            return items;
        }


        public static SettingsItemGroup GetPlaybackControlSettings()
        {
            var Title = "Playback Control";

            SettingsItemGroup items = new SettingsItemGroup(groupImage: FontIconPaths.PlaybackControlImagePath, groupName: Title)
            {

                new CheckBoxItem( nameof(SettingsService.Instance.StopMusicOnTimerEnabled),
                (value) => { SettingsService.Instance.StopMusicOnTimerEnabled =(bool) value; },
                () => { return SettingsService.Instance.StopMusicOnTimerEnabled; })
                {
                    Description = "Stop media playback at a specified time",
                },

                new TimePickerSettingsItem(nameof(SettingsService.Instance.StopMusicOnTimerPosition),
                (value)=>{ SettingsService.Instance.StopMusicOnTimerPosition = (TimeSpan)value; },
                ()=>{
                    return SettingsService.Instance.StopMusicOnTimerPosition;
                })
                {
                    TimePickerDescription = "Stop media time stamp"
                },

              
                new CheckBoxItem(nameof(SettingsService.Instance.PlayToEnabled),
                (value) => { SettingsService.Instance.PlayToEnabled =(bool) value; },
                () => { return SettingsService.Instance.PlayToEnabled; })
                {
                    Description = "DLNA sink",
                }
            };

            items.Add(new VerbatimTextBlock()
            {
                TextDescription = "Automatically enqueue the next music or video file in the last track's folder"
            });

            items.Add(new CheckBoxItem(nameof(SettingsService.Instance.AutoPlayMusic),
                (value) => { SettingsService.Instance.AutoPlayMusic = (bool)value; },
                () => { return SettingsService.Instance.AutoPlayMusic; })
            {
                Description = "Auto play music",
            });

            items.Add(new CheckBoxItem(nameof(SettingsService.Instance.AutoPlayVideo),
                (value) => { SettingsService.Instance.AutoPlayVideo = (bool)value; }
            , () => { return SettingsService.Instance.AutoPlayVideo; })
            {
                Description = "Auto play video",
            });

            return items;
        }


        public static SettingsItemGroup GetSubtitleSettings()
        {
            var settingsItemRoot = new SubtitlesSettingsItem();


            SettingsItemGroup items = new SettingsItemGroup(groupImage: FontIconPaths.SubtitlesEncodings, groupName: "CC + Subtitles")
            {
                new SystemSettingsHyperlink()
                {
                    Description = "Change CC options in system settings",
                    SystemSettingsLink = "ms-settings:easeofaccess-closedcaptioning"
                },

                new ComboboxWithHeader(CharacterEncoding.AllEncodings.Select(x => GetEncodingName(x)),
                nameof(SettingsService.Instance.FFmpegCharacterEncodingIndex),
                (value) => { SettingsService.Instance.FFmpegCharacterEncodingIndex = (int)value; },
                ()=>{ return SettingsService.Instance.FFmpegCharacterEncodingIndex; })
                {
                    ImagePath = FontIconPaths.EncodingsGroupImage,
                    ComboboxHeader = "Subtitle encoding for ASCII text."
                },

                new VerbatimTextBlock()
                {
                    TextDescription = "Auto detection of subtitles only works with files that are part of music, video or pictures libraries. "
                },

                new VerbatimTextBlock()
                {
                    TextDescription = "Forced subtitles are usually subtitles which present translations or other capitons inside movies"
                },

                new CheckBoxItem(nameof(SettingsService.Instance.AutoloadForcedSubtitles), (value) => { SettingsService.Instance.AutoloadForcedSubtitles =(bool) value; }, () => { return SettingsService.Instance.AutoloadForcedSubtitles; })
                {
                    Description = "Auto-load forced subtitles.",
                },

                new CheckBoxItem(nameof(SettingsService.Instance.AutoDetectExternalSubtitle), (value) => { SettingsService.Instance.AutoDetectExternalSubtitle =(bool) value; }, () => { return SettingsService.Instance.AutoDetectExternalSubtitle; })
                {
                    Description = "Auto-detect local (external) subtitle.",
                },

                new CheckBoxItem(nameof(SettingsService.Instance.AutoloadInternalSubtitle), (value) => { SettingsService.Instance.AutoloadInternalSubtitle =(bool) value; }, () => { return SettingsService.Instance.AutoloadInternalSubtitle; })
                {
                    Description = "Auto-detect embedded (internal) subtitle if languages match.",
                },

                new VerbatimTextBlock()
                {
                    TextDescription = "The app will first look for local subtitles. External subtitles have priority over embedded ones. If no external subtitles are found, the app will look for internal ones with matching language. Finally, if none are found, it will look online if allowed. "
                },

                new VerbatimTextBlock()
                {
                    TextDescription = "You must supply an account with user name and password to continue recieveing subtitles Visit settings > Application > accounts+logins."
                },

                new ContentSettingsItem(MinimumSubtitleDurationControl.CreateGlobal())
                {
                }
            };
            items.SecondarySettings.AddRange(new List<SettingsItem>()
            {
                new ContentSettingsItem(new OpenSubtitlesAccountForm()),

                new ComboboxWithHeader(LanguageCodesService.Codes, nameof(SettingsService.Instance.PreferredSubtitleLanguageIndex), (value) => { SettingsService.Instance.PreferredSubtitleLanguageIndex = (int)value; }, ()=>{return SettingsService.Instance.PreferredSubtitleLanguageIndex; })
                {
                    ImagePath = FontIconPaths.SubtitlesEncodings,
                    DefaultValue = LanguageCodesService.GetDefaultLanguageIndex(),
                    ComboboxHeader = "OpenSubtitle download language.",
                },
            });

            return items;
        }

        static string GetEncodingName(CharacterEncoding x)
        {
            if (!string.IsNullOrWhiteSpace(x.Name))
            {
                return x.Description + " --- " + x.Name;
            }
            else
            {
                return x.Description;
            }
        }

        public static SettingsItemGroup GetUISettings()
        {
            SettingsItemGroup items = new SettingsItemGroup(groupImage: FontIconPaths.UIThemesGroup, groupName: "UI General");

            items.Add(new ComboboxWithHeader(nameof(SettingsService.Instance.DefaultUITheme), (value) => { SettingsService.Instance.DefaultUITheme = (int)value; }, () => { return SettingsService.Instance.DefaultUITheme; }, "System default", "Dark", "Light")
            {
                ComboboxHeader = "UI theme. Requires restart to take full effect",
            });
                                    
            return items;
        }

        public static SettingsItemGroup GetVideoSettings()
        {
            SettingsItemGroup items = new SettingsItemGroup(groupImage: FontIconPaths.VideoSettings, groupName: "Video");
            items.Add(new VerbatimTextBlock()
            {
                TextDescription = "Video decoder mode. Choose between automatic hardware-software switch, force software decoding or using system decoders"
            });

            items.Add(new ComboboxWithHeader(FFmpegVideoModeInterop.DecoderModeNames, nameof(SettingsService.Instance.VideoDecoderMode), (value) => { SettingsService.Instance.VideoDecoderMode = (int)value; }, () => { return SettingsService.Instance.VideoDecoderMode; })
            {
                ComboboxHeader = "Video decoder mode"
            });
                        
            items.Add(new CheckBoxItem(nameof(SettingsService.Instance.StereoDownMix), (value) => { SettingsService.Instance.StereoDownMix = (bool)value; }, () => { return SettingsService.Instance.StereoDownMix; })
            {
                Description = "Stereo downmix",
            });

            return items;
        }

        public static SettingsItemGroup GetTrackMetadataSettings()
        {
            SettingsItemGroup items = new SettingsItemGroup(groupImage: FontIconPaths.metadataGroupImage, groupName: "Track Metadata");

            items.Add(new VerbatimTextBlock()
            {
                TextDescription = "Folder cover relative path. Example: for the input COVER.JPG, the application will look for a file called COVER.JPG in the same folder with the music file. You can add more than one file name separated by ;. Be careful, however, using many files may reduce performance significantly.",
            });

            items.Add(new VerbatimTextBlock()
            {
                TextDescription = "External covers must be in same folder, share same file name and formats must be png or jpg. "
            });

            items.Add(new ComboBoxWithTextBlockAndButton(nameof(SettingsService.Instance.AlbumArtOptionIndex), (value) => { SettingsService.Instance.AlbumArtOptionIndex = (int)value; }, () => { return SettingsService.Instance.AlbumArtOptionIndex; }, "Extract from files", "Use file covers (same name as file)", "Folder covers")
            {
                ComboboxHeader = "File metadata source",
                DefaultValue = 0,
                TextBoxHeader = "File cover names",
                ImageTextBoxPath = FontIconPaths.metadataFileCovers,
                ImageComboBoxPath = FontIconPaths.metadataFolderCovers,
                Label = "Save folder covers",
                ButtonIcon = FontIconPaths.saveButton
            });

            items.Add(new ContentSettingsItem(new AdvancedTrackMetadataSettingsControl()));
            return items;
        }

        public static SettingsItemGroup GetEqualizerManagamenetGroup()
        {
            SettingsItemGroup items = new SettingsItemGroup(groupImage: "\uF8A6", groupName: "Equalizer configs");

            items.Add(new ContentSettingsItem(new EQConfigurationManagementPage()));

            return items;
        }
    }
}
