using FFmpegInteropX;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using System;
using System.ComponentModel;

namespace MayazucMediaPlayer.Controls
{
    public interface IMinimumSubtitleLengthAdapter : INotifyPropertyChanged, IContentSettingsItem
    {
        double MinimumDuration { get; set; }

        bool PreventOverlaps { get; set; }

        string Subtitle { get; }

    }

    public class LocalFileMinimumSubtitleLengthAdapter : ObservableObject, IContentSettingsItem, IMinimumSubtitleLengthAdapter
    {
        public MediaSourceConfig StreamSourceConfig
        {
            get;
            private set;
        }

        public LocalFileMinimumSubtitleLengthAdapter(MediaSourceConfig mediaStreamSourceConfig)
        {
            MinimumDuration = SettingsService.Instance.MinimumSubtitleDuration;
            PreventOverlaps = SettingsService.Instance.PreventSubtitleOverlaps;
            StreamSourceConfig = mediaStreamSourceConfig;
        }

        double _duration;
        bool _preventOverlaps;
        public double MinimumDuration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    NotifyPropertyChanged(nameof(MinimumDuration));
                    if (StreamSourceConfig != null)
                        StreamSourceConfig.Subtitles.MinimumSubtitleDuration = TimeSpan.FromSeconds(_duration);
                }
            }
        }

        public bool PreventOverlaps
        {
            get => _preventOverlaps;
            set
            {
                if (_preventOverlaps != value)
                {
                    _preventOverlaps = value;
                    NotifyPropertyChanged(nameof(PreventOverlaps));
                    if (StreamSourceConfig != null)
                        StreamSourceConfig.Subtitles.PreventModifiedSubtitleDurationOverlap = _preventOverlaps;
                }
            }
        }
        public string Subtitle { get; private set; } = "Current file override";

        public void RecheckValue()
        {

        }
    }

    public class GlobalMinimumSubtitleLengthAdapter : ObservableObject, IContentSettingsItem, IMinimumSubtitleLengthAdapter
    {
        public double MinimumDuration
        {
            get => SettingsService.Instance.MinimumSubtitleDuration;
            set
            {
                if (SettingsService.Instance.MinimumSubtitleDuration == value) return;

                SettingsService.Instance.MinimumSubtitleDuration = value;
                NotifyPropertyChanged(nameof(MinimumDuration));
            }
        }
        public bool PreventOverlaps
        {
            get => SettingsService.Instance.PreventSubtitleOverlaps;
            set
            {
                if (SettingsService.Instance.PreventSubtitleOverlaps == value) return;

                SettingsService.Instance.PreventSubtitleOverlaps = value;
                NotifyPropertyChanged(nameof(PreventOverlaps));
            }
        }

        public string Subtitle { get; private set; } = "Global setting";

        public void RecheckValue()
        {

        }
    }
}
