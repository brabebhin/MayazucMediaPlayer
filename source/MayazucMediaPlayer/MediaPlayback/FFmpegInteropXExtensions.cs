using FFmpegInteropX;
using MayazucMediaPlayer.AudioEffects;
using MayazucMediaPlayer.LocalCache;
using MayazucMediaPlayer.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.MediaPlayback
{
    public static class FFmpegInteropXExtensions
    {
        public static bool IsVideo(this MediaPlaybackItem item)
        {
            return item != null && item.VideoTracks != null && item.VideoTracks.Count > 0;
        }

        public static bool HasSubtitles(this MediaPlaybackItem item)
        {
            return item != null && item.TimedMetadataTracks != null && item.TimedMetadataTracks.Any(x => x.IsSubtitle());
        }

        public static IReadOnlyList<AvEffectDefinition> GetEqualizerEffectDefinitions(EqualizerConfiguration configuration)
        {
            List<AvEffectDefinition> eqDefinitions = new List<AvEffectDefinition>();
            var Bands = configuration.Bands;

            for (int i = 0; i < Bands.Count; i++)
            {
                double octave = 2;
                int f1 = Bands[i].CutOffFrequency;
                int amplification = Bands[i].Amplification;
                //last band has hard coded octave of 2
                if (i < Bands.Count - 1)
                {
                    int f2 = Bands[i + 1].CutOffFrequency;
                    octave = AudioEffectsExtensions.GetOctaves(f1, f2 - 1);
                }

                Bands[i].Octave = octave;
                string adaptiveBand = string.Format(CultureInfo.InvariantCulture, ContainerKeys.EqualizerConfigurationStringFormat, f1, Math.Round(octave, 4).ToString(CultureInfo.InvariantCulture), amplification);
                eqDefinitions.Add(new AvEffectDefinition("equalizer", adaptiveBand));
            }

            return eqDefinitions.AsReadOnly();
        }

        public static IReadOnlyList<AvEffectDefinition> GetEchoFilters()
        {
            List<AvEffectDefinition> eqDefinitions = new List<AvEffectDefinition>();

            if(SettingsService.Instance.EchoInstrumentsEffectEnabled)
            {
                eqDefinitions.Add(new AvEffectDefinition("aecho", "0.8:0.88:60:0.4"));
            }
            if (SettingsService.Instance.EchoMountainsEffectEnabled)
            {
                eqDefinitions.Add(new AvEffectDefinition("aecho", "0.8:0.9:1000:0.3"));
            }
            if (SettingsService.Instance.EchoRoboticVoiceEffectEnabled)
            {
                eqDefinitions.Add(new AvEffectDefinition("aecho", "0.8:0.88:6:0.4"));
            }

            return eqDefinitions.AsReadOnly();
        }

        /// <summary>
        /// returns the TimedMetadataTracks from the given ffmpeginteropMss
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IReadOnlyCollection<TimedMetadataTrack> GetSubtitleTracks(this FFmpegMediaSource interop)
        {
            List<TimedMetadataTrack> retrunValue = new List<TimedMetadataTrack>();
            try
            {
                retrunValue.AddRange(interop.SubtitleStreams.Where(x => x.IsForced == false).Select(y => y.SubtitleTrack));
            }
            catch
            {

            }
            return retrunValue.AsReadOnly();

        }

        /// <summary>
        /// returns the TimedMetadataTracks from the given ffmpeginteropMss
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IReadOnlyCollection<CCSubtitleInfo> GetCCSubtitleTracks(this FFmpegMediaSource interop)
        {
            List<CCSubtitleInfo> retrunValue = new List<CCSubtitleInfo>();
            try
            {
                foreach (var t in interop.SubtitleStreams.Where(x => x.IsForced == false))
                {
                    var sub = new CCSubtitleInfo(t);
                    retrunValue.Add(sub);

                }
            }
            catch { }
            return retrunValue.AsReadOnly();
        }

        public static MediaSourceConfig GetFFmpegUserConfigs()
        {
            MediaSourceConfig config = new MediaSourceConfig();

            config.Video.VideoOutputAllow10bit = false;
            config.Video.VideoOutputAllowBgra8 = false;
            config.Video.VideoOutputAllowIyuv = false;
            config.General.FastSeek = true;
            config.Subtitles.UseLibassAsSubtitleRenderer = false;
            config.General.MaxSupportedPlaybackRate = 4;
            config.Audio.DownmixAudioStreamsToStereo = SettingsService.Instance.StereoDownMix;
            config.Video.HdrSupport = HdrSupport.Automatic;
            config.Video.VideoDecoderMode = FFmpegVideoModeInterop.DecoderModeMap[SettingsService.Instance.VideoDecoderMode];
            config.Subtitles.ExternalSubtitleAnsiEncoding = CharacterEncoding.AllEncodings[SettingsService.Instance.FFmpegCharacterEncodingIndex];
            config.Subtitles.AutoSelectForcedSubtitles = false;
            config.Subtitles.MinimumSubtitleDuration = TimeSpan.FromSeconds(SettingsService.Instance.MinimumSubtitleDuration);
            config.Subtitles.PreventModifiedSubtitleDurationOverlap = SettingsService.Instance.PreventSubtitleOverlaps;

            //TimedTextStyle configStyle = config.Subtitles.SubtitleStyle;
            //configStyle.OutlineColor = Microsoft.UI.Colors.Blue;
            //configStyle.OutlineRadius = new TimedTextDouble(80, TimedTextUnit.Percentage);
            //configStyle.OutlineThickness = new TimedTextDouble(80, TimedTextUnit.Percentage);
            //config.Subtitles.SubtitleStyle = configStyle;
            return config;
        }

        public static bool CheckSubtitlelanguage(TimedMetadataTrack sub, string language)
        {
            var subLanguage = sub.Language.ToLowerInvariant();
            var lowerLanguage = language.ToLowerInvariant();
            if (subLanguage.Contains(lowerLanguage) || sub.Name.ToLowerInvariant().Contains(lowerLanguage))
            {
                return true;
            }
            else return false;
        }

        public static double? GetCurrentVideoAspectRatio(this FFmpegMediaSource mediaSource)
        {
            if (mediaSource == null) return null;
            if (mediaSource.CurrentVideoStream == null) return null;
            return mediaSource.CurrentVideoStream.DisplayAspectRatio;
        }
    }
}
