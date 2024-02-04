using CommunityToolkit.WinUI;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Notifications;
using MayazucMediaPlayer.Runtime;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class MTCSubtitlesSelectionControl : UserControl
    {

        private readonly AsyncLock lockSync = new AsyncLock();

        MediaPlaybackItem m_item;
        private MediaPlaybackItem CurrentWrappedPlaybackItem
        {
            get => m_item;
            set
            {
                if (m_item != null)
                {
                    m_item.TimedMetadataTracksChanged -= Item_TimedMetadataTracksChanged;
                    m_item.TimedMetadataTracks.PresentationModeChanged -= TimedMetadataTracks_PresentationModeChanged;
                }

                m_item = value;

                if (m_item != null)
                {
                    m_item.TimedMetadataTracksChanged += Item_TimedMetadataTracksChanged;
                    m_item.TimedMetadataTracks.PresentationModeChanged += TimedMetadataTracks_PresentationModeChanged;
                }
            }
        }
        readonly IFileOpenPicker subtitlePicker = new MayazucFileOpenPicker();

        public IOpenSubtitlesAgent SubtitlesAgent
        {
            get;
            internal set;
        }

        public MTCSubtitlesSelectionControl()
        {
            InitializeComponent();
            DataContext = this;

            subtitlePicker.FileTypeFilter.Add(".srt");
            subtitlePicker.FileTypeFilter.Add(".sub");
            subtitlePicker.FileTypeFilter.Add(".ttml");
            subtitlePicker.FileTypeFilter.Add(".vtt");
        }

        public async void LoadMediaPlaybackItem(MediaPlaybackItem item)
        {
            using (await lockSync.LockAsync())
            {
                CurrentWrappedPlaybackItem = item;
                ProcessPlaybackItem(item);
            }
        }

        private void ProcessPlaybackItem(MediaPlaybackItem item)
        {
            var ItemsSource = new List<TimedMetadataTrackViewModelItem>();
            ItemsSource.Clear();
            for (uint i = 0; i < item.TimedMetadataTracks.Count; i++)
            {
                try
                {
                    if (item.TimedMetadataTracks[(int)i].TimedMetadataKind == TimedMetadataKind.Subtitle)
                    {
                        var presentationModeActive = item.TimedMetadataTracks.GetPresentationMode(i) == TimedMetadataTrackPresentationMode.PlatformPresented;
                        ItemsSource.Add(new TimedMetadataTrackViewModelItem(item.TimedMetadataTracks[(int)i], i)
                        {
                            IsActive = presentationModeActive
                        });
                    }
                }
                catch
                {
                    var extradata = item.GetExtradata();
                    if (extradata.Disposed) break;
                }
            }
            lsvSubtitles.ItemsSource = ItemsSource;
        }

        private async void TimedMetadataTracks_PresentationModeChanged(MediaPlaybackTimedMetadataTrackList sender, TimedMetadataPresentationModeChangedEventArgs args)
        {
            using (await lockSync.LockAsync())
            {
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    var itemsSource = lsvSubtitles.ItemsSource as List<TimedMetadataTrackViewModelItem>;
                    var track = itemsSource.FirstOrDefault(x => x.Track == args.Track);
                    if (track != null)
                    {
                        track.IsActive = args.NewPresentationMode == TimedMetadataTrackPresentationMode.PlatformPresented;
                    }
                });
            }
        }

        private async void Item_TimedMetadataTracksChanged(MediaPlaybackItem sender, IVectorChangedEventArgs args)
        {
            using (await lockSync.LockAsync())
            {
                await DispatcherQueue.EnqueueAsync(() =>
                {
                    ProcessPlaybackItem(sender);
                });
            }
        }

        private async void DisableAllSubtitles(object? sender, RoutedEventArgs e)
        {
            await DisableAllSubtitles();
        }

        public async Task DisableAllSubtitles()
        {
            using (await lockSync.LockAsync())
            {
                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    if (lsvSubtitles.ItemsSource != null)
                    {
                        var itemsSource = lsvSubtitles.ItemsSource as List<TimedMetadataTrackViewModelItem>;
                        uint i = 0;
                        foreach (var t in itemsSource)
                        {
                            var track = t.Track;

                            var playbackItem = track.PlaybackItem;
                            await AppState.Current.MediaServiceConnector.SetDisabledSubtitle(playbackItem, i);
                            i++;
                        }
                    }
                });
            }

        }

        private async void subtitleOnOff(object? sender, TappedRoutedEventArgs e)
        {
            using (await lockSync.LockAsync())
            {
                try
                {
                    var model = sender.GetDataContextObject<TimedMetadataTrackViewModelItem>();
                    var track = model.Track;
                    var index = model.Index;
                    var playbackItem = track.PlaybackItem;
                    var checkBox = sender as CheckBox;
                    if (checkBox.IsChecked.HasValue)
                    {
                        if (checkBox.IsChecked.Value)
                        {
                            await AppState.Current.MediaServiceConnector.SetActiveSubtitle(playbackItem, index);
                        }
                        else
                        {
                            await AppState.Current.MediaServiceConnector.SetDisabledSubtitle(playbackItem, index);
                        }
                    }
                    else
                    {
                        await AppState.Current.MediaServiceConnector.SetDisabledSubtitle(playbackItem, index);
                    }
                }
                catch
                {

                }
            }
        }

        private async void OpenLocalSubtitles(object? sender, RoutedEventArgs e)
        {
            using (await lockSync.LockAsync())
            {
                try
                {
                    await LoadLocalSubtitle(CurrentWrappedPlaybackItem);
                }
                catch { }
            }
        }

        private async void LookForSubtitlesOnline(object? sender, RoutedEventArgs e)
        {
            using (await lockSync.LockAsync())
            {
                try
                {
                    await LookForSubtitleOnline();
                }
                catch { }
            }
        }

        public async Task LoadLocalSubtitle(MediaPlaybackItem item)
        {
            using (await lockSync.LockAsync())
            {
                try
                {
                    var extraData = item.GetExtradata();
                    if (extraData != null)
                    {
                        await extraData.SubtitleService.OpenSubtitleFile(subtitlePicker);
                    }
                }
                catch { }
            }
        }

        public async Task LoadLocalSubtitleInternal(MediaPlaybackItem item)
        {
            try
            {
                var extraData = item.GetExtradata();
                if (extraData != null)
                {
                    await extraData.SubtitleService.OpenSubtitleFile(subtitlePicker);
                }
            }
            catch { }
        }

        public async Task LookForSubtitleOnline()
        {
            if (CurrentWrappedPlaybackItem != null)
            {
                var extraData = CurrentWrappedPlaybackItem.GetExtradata();
                var mds = extraData.MediaPlayerItemSource;

                var diag = new OnlineSubtitlePicker(await mds.PrepareSubtitles(), SubtitlesAgent);
                await ContentDialogService.Instance.ShowAsync(diag);

                if (diag.DownloadedSubtitleItem != null)
                {
                    try
                    {
                        var subFile = diag.DownloadedSubtitleItem;
                        await extraData.SubtitleService.ParseAndSetSubtitle(subFile);
                    }
                    catch
                    {
                        MiscNotificationGenerator.ShowToast("Could not load subtitle. Manual management of subtitles is required");
                    }
                }
            }
        }

        private async void DelaySubtitles(object? sender, RoutedEventArgs e)
        {
            using (await lockSync.LockAsync())
            {
                if (AppState.Current.MediaServiceConnector.HasActivePlaybackSession() && CurrentWrappedPlaybackItem != null)
                {
                    var nextDelay = await AppState.Current.MediaServiceConnector.DelaySubtitles();
                    //ShowNotification($"Subtitles {nextDelay.TotalSeconds} s");
                }
            }
        }

        private async void FastenSubtitles(object? sender, RoutedEventArgs e)
        {
            using (await lockSync.LockAsync())
            {
                if (AppState.Current.MediaServiceConnector.HasActivePlaybackSession() && CurrentWrappedPlaybackItem != null)
                {
                    var nextDelay = await AppState.Current.MediaServiceConnector.QuickenSubtitles();
                    //ShowNotification($"Subtitles {nextDelay.TotalSeconds} s");
                }
            }
        }
    }

    public class TimedMetadataTrackViewModelItem : ObservableObject
    {
        public TimedMetadataTrack Track
        {
            get;
            private set;
        }

        public string Name
        {
            get
            {
                return $"{Track.Language} {Track.Label}";
            }
        }

        bool _isActivel;
        public bool IsActive
        {
            get
            {
                return _isActivel;
            }
            set
            {
                if (_isActivel != value)
                {
                    _isActivel = value;
                    NotifyPropertyChanged(nameof(IsActive));
                }
            }
        }

        public uint Index
        {
            get;
            set;
        }

        public TimedMetadataTrackViewModelItem(TimedMetadataTrack track, uint index)
        {
            ReloadItem(track, index);
        }

        public void ReloadItem(TimedMetadataTrack track, uint index)
        {
            Track = track;
            IsActive = track.PlaybackItem.TimedMetadataTracks.GetPresentationMode(index) == TimedMetadataTrackPresentationMode.PlatformPresented;
            Index = index;
            NotifyPropertyChanged(nameof(Name));
        }
    }
}
