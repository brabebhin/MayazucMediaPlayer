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
    public static class MediaHelperExtensions
    {
        public static bool IsVideo(this MediaPlaybackItem item)
        {
            return item != null && item.VideoTracks != null && item.VideoTracks.Count > 0;
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

        public static IReadOnlyList<AvEffectDefinition> GetAdditionalEffectsDefinitions()
        {
            List<AvEffectDefinition> eqDefinitions = new List<AvEffectDefinition>();

            var echoContainer = ApplicationDataContainers.EchoEffectsContainer;
            foreach (var kvp in echoContainer.Values)
            {
                eqDefinitions.Add(new AvEffectDefinition("aecho", (string)kvp.Value));
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

            config.Video.VideoOutputAllow10bit = SettingsWrapper.VideoOutputAllow10bit;
            config.Video.VideoOutputAllowBgra8 = SettingsWrapper.VideoOutputAllowBgra8;
            config.Video.VideoOutputAllowIyuv = SettingsWrapper.VideoOutputAllowIyuv;
            config.General.FastSeek = true;
            config.General.MaxSupportedPlaybackRate = 4;
            config.Audio.DownmixAudioStreamsToStereo = SettingsWrapper.StereoDownMix;
            config.Video.VideoDecoderMode = FFmpegVideoModeInterop.DecoderModeMap[SettingsWrapper.VideoDecoderMode];
            config.Subtitles.ExternalSubtitleAnsiEncoding = CharacterEncoding.AllEncodings[SettingsWrapper.FFmpegCharacterEncodingIndex];
            config.Subtitles.AutoSelectForcedSubtitles = false;
            config.Subtitles.MinimumSubtitleDuration = TimeSpan.FromSeconds(SettingsWrapper.MinimumSubtitleDuration);
            config.Subtitles.PreventModifiedSubtitleDurationOverlap = SettingsWrapper.PreventSubtitleOverlaps;

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
    }
}
