using FFmpegInteropX;
using MayazucMediaPlayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Media.Core;

namespace MayazucMediaPlayer.MediaPlayback
{
    public class CCSubtitleInfo
    {
        public string DisplayString
        {
            get;
            private set;
        }

        public TimedMetadataTrack MetadataTrack
        {
            get;
            private set;
        }

        public bool IsDefault
        {
            get;
            private set;
        }

        public LanguageCode? Language
        {
            get;
            private set;
        }

        public CCSubtitleInfo(TimedMetadataTrack track)
        {
            MetadataTrack = track;
            DisplayString = !string.IsNullOrWhiteSpace($"{track.Label} {track.Language}") ? $"{track.Label} {track.Language}" : "Untitled";
        }

        public CCSubtitleInfo(SubtitleStreamInfo info) : this(info.SubtitleTrack)
        {
            Language = LanguageCodesService.Codes.FirstOrDefault(x => x.ThreeLetterIsoCode.Equals(info.Language, StringComparison.OrdinalIgnoreCase) || x.TwoLetterIsoCode.Equals(info.Language, StringComparison.OrdinalIgnoreCase) || x.LanguageName.Equals(info.Language, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object? obj)
        {
            var info = obj as CCSubtitleInfo;
            return info != null &&
                   DisplayString == info.DisplayString &&
                   EqualityComparer<TimedMetadataTrack>.Default.Equals(MetadataTrack, info.MetadataTrack) &&
                   IsDefault == info.IsDefault &&
                   Language == info.Language;
        }

        public override int GetHashCode()
        {
            var hashCode = -1789113448;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisplayString);
            hashCode = hashCode * -1521134295 + EqualityComparer<TimedMetadataTrack>.Default.GetHashCode(MetadataTrack);
            hashCode = hashCode * -1521134295 + IsDefault.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<LanguageCode>.Default.GetHashCode(Language);
            return hashCode;
        }
    }
}
