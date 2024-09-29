using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.NowPlayingViews;
using MayazucMediaPlayer.Runtime;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class FileDetailsControl : BaseUserControl
    {
        public FileDetailsControl()
        {
            InitializeComponent();
            PlayCommand = new AsyncRelayCommand<object>(PlayCommandmethod);
            AddToNowPlayingCommand = new AsyncRelayCommand<object>(AddToNowPlayomgCommandmethod);
            SaveToPlaylistCommand = new AsyncRelayCommand<object>(SaveToPlaylistCommandmethod);
            DataContext = this;

            SkipToQueueItemCommand = new AsyncRelayCommand<object>(SkipToQueueItemMethod);
        }

        public Visibility PlaybackCommandBarVisibility
        {
            get
            {
                return (Visibility)GetValue(PlaybackCommandBarVisibilityProperty);
            }
            set
            {
                SetValue(PlaybackCommandBarVisibilityProperty, value);
            }
        }

        public static DependencyProperty PlaybackCommandBarVisibilityProperty = DependencyProperty.Register(nameof(PlaybackCommandBarVisibility), typeof(Visibility), typeof(FileDetailsControl), new PropertyMetadata(Visibility.Visible));

        private MediaPlaybackItemUIInformation? CurrentPlaybackItemInfo = null;

        bool _IsAudioFile;
        public bool IsAudioFile
        {
            get
            {
                return _IsAudioFile;
            }
            set
            {
                _IsAudioFile = value;
                NotifyPropertyChanged(nameof(IsAudioFile));
            }
        }

        IMediaPlayerItemSource _data;
        public IMediaPlayerItemSource MediaData
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                NotifyPropertyChanged(nameof(MediaData));
            }
        }

        public ObservableCollection<MediaPlaybackItemUIInformation.AudioStreamInfoWrapper> AudioStreams
        {
            get;
            private set;
        } = new ObservableCollection<MediaPlaybackItemUIInformation.AudioStreamInfoWrapper>();

        public ObservableCollection<MediaPlaybackItemUIInformation.VideoStreamInfoWrapper> VideoStreams
        {
            get;
            private set;
        } = new ObservableCollection<MediaPlaybackItemUIInformation.VideoStreamInfoWrapper>();

        public ObservableCollection<MediaPlaybackItemUIInformation.SubtitleStreamInfoWrapper> SubtitleStreams
        {
            get;
            private set;
        } = new ObservableCollection<MediaPlaybackItemUIInformation.SubtitleStreamInfoWrapper>();

        public ObservableCollection<MediaThumbnailPreviewData> VideoThumbnails
        {
            get;
            private set;
        } = new ObservableCollection<MediaThumbnailPreviewData>();

        public IRelayCommand<object> PlayCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> AddToNowPlayingCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> SaveToPlaylistCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> GoToSendToCommand
        {
            get;
            private set;
        }

        public IRelayCommand<object> SkipToQueueItemCommand
        {
            get;
            private set;
        }

        public bool ShowLibraryHelp
        {
            get;
            private set;
        } = false;

        public PlaylistsService PlaylistsService { get; private set; }

        private async Task SkipToQueueItemMethod(object arg)
        {
            await AppState.Current.MediaServiceConnector.SkipToQueueItem(MediaData);
        }

        public async Task<bool> CheckFileAvailability()
        {
            //var file = await MediaData.GetFileAsync();
            //await Task.Run(() =>
            //{
            //    ShowLibraryHelp = !(file).IsPartOf(StorageAccessHelpers.MusicLibrary, StorageAccessHelpers.VideosLibrary, StorageAccessHelpers.PicturesLibrary);
            //});
            //await Dispatcher.EnqueueAsync(() =>
            //{
            //    NotifyPropertyChanged(nameof(ShowLibraryHelp));
            //});

            return ShowLibraryHelp;
        }

        private async Task PlayCommandmethod(object? sender)
        {
            await AppState.Current.MediaServiceConnector.StartPlaybackFromBeginning(MediaData);
        }

        private async Task AddToNowPlayomgCommandmethod(object? sender)
        {
            await AppState.Current.MediaServiceConnector.AddToNowPlaying(MediaData);
        }

        private async Task SaveToPlaylistCommandmethod(object? sender)
        {
            PlaylistPicker picker = new PlaylistPicker();
            var target = await picker.PickPlaylistAsync(PlaylistsService.Playlists);
            if (target != null)
            {

                await PlaylistsService.AddToPlaylist(target, new IMediaPlayerItemSource[] { MediaData });
                await MessageDialogService.Instance.ShowMessageDialogAsync("Added to playlist", "Success");
            }
        }

        private async Task LoadVideoThumbnails(MediaPlaybackItemUIInformation? ffmpegData)
        {
            if (ffmpegData == null) return;
            var currentMediaDataStatus = ffmpegData.MediaData;
            var grabber = await currentMediaDataStatus.GetFrameGrabberAsync();
            if (grabber != null)
            {
                foreach (var v in VideoThumbnails)
                {
                    await v.LoadPreviewAsync(grabber);
                }
            }
        }

        public void LoadFileEmbeddedStreams(MediaPlaybackItemUIInformation ffmpegData)
        {
            try
            {
                if (ffmpegData != null)
                    IsAudioFile = !ffmpegData.VideoStreams.Any();
                if (ffmpegData != null)
                {
                    AudioStreams.Clear();
                    AudioStreams.AddRange(ffmpegData.AudioStreams);
                }
                if (ffmpegData != null)
                {
                    VideoStreams.Clear();
                    VideoStreams.AddRange(ffmpegData.VideoStreams);
                }
                if (ffmpegData != null)
                {
                    SubtitleStreams.Clear();
                    SubtitleStreams.AddRange(ffmpegData.SubtitleStreams);
                }
            }
            catch { }
        }

        public void PerformCleanUp()
        {
        }

        public async Task LoadStateInternal(MediaPlaybackItemUIInformation data,
            PlaybackSequenceService playbackDataModel,
            PlaylistsService playlistService)
        {
            try
            {
                PlaylistsService = playlistService;

                await LoadCurrentPlaybackItem(data);
            }
            catch { }
        }

        private async Task LoadFileInfo(MediaPlaybackItemUIInformation playbackItem)
        {
            CurrentPlaybackItemInfo = playbackItem;
            var data = this.MediaData = playbackItem.MediaData;
            var metadata = await data.GetMetadataAsync();
            AddToPlaylistButton.IsEnabled = true;
            lsvMetadataDisplay.ItemsSource = metadata.AdditionalMetadata;
            fdPlay.IsEnabled = true;
            fdEnqueue.IsEnabled = true;
            SkipToQueueItemButton.IsEnabled = true;
            MenuBar.IsEnabled = data.IsAvailable();

            if (metadata.HasSavedThumbnailFile())
            {
                var thumbnailStreamReference = await data.GetThumbnailStreamAsync();
                var stream = await thumbnailStreamReference.OpenReadAsync();
                if (stream.Size > 0)
                {
                    var albumArt = new BitmapImage();
                    await albumArt.SetSourceAsync(stream);
                    AlbumArtImage.Source = albumArt;
                }
                else
                {
                    stream.Dispose();
                }
            }


            if (!string.IsNullOrEmpty(metadata.Album))
            {
                TrackInfoAlbum.Text = metadata.Album;
            }

            if (!string.IsNullOrEmpty(metadata.Artist))
            {
                TrackInfoArtist.Text = metadata.Artist;
            }

            if (!string.IsNullOrEmpty(metadata.Genre))
            {
                TrackInfoGenre.Text = metadata.Genre;
            }

            TrackInfoTitle.Text = string.IsNullOrWhiteSpace(metadata.Title) ? data.Title : metadata.Title;

            this.LoadFileEmbeddedStreams(playbackItem);
            var mediaPath = data.MediaPath;

            if (!string.IsNullOrWhiteSpace(mediaPath))
            {
                FileExtensionTextBlock.Text = Path.GetExtension(mediaPath);
                FullPathTextBlock.Text = mediaPath;
                FileNameTextBlock.Text = Path.GetFileName(mediaPath);
            }

            if (!data.IsAvailable())
            {
                mediaNotAvailableStackPanel.Visibility = Visibility.Visible;
                if (string.IsNullOrWhiteSpace(mediaPath))
                {
                    FileExtensionTextBlock.Text = "";
                    FileNameTextBlock.Text = "Media not available";
                }
                return;
            }
            else
            {
                await this.CheckFileAvailability();
            }
        }

        public async Task HandleMediaOpened(MediaPlaybackItem item)
        {
            try
            {
                if (item != null)
                    await LoadCurrentPlaybackItem(item.GetExtradata().PlaybackDataStreamInfo);
            }
            finally
            {
            }
        }

        private async Task LoadCurrentPlaybackItem(MediaPlaybackItemUIInformation item)
        {
            try
            {
                IReadOnlyCollection<MediaThumbnailPreviewData> videothubnails = null;
                videothubnails = MediaThumbnailPreviewData.ProcessMediaPlaybackItem(item);
                await DispatcherQueue.EnqueueAsync(async () =>
                {
                    this.VideoThumbnails.Clear();
                    if (videothubnails != null)
                        this.VideoThumbnails.AddRange(videothubnails);

                    await LoadFileInfo(item);
                });
            }
            catch { }
        }

        private async void SeekToVideoPosition(object? sender, ItemClickEventArgs e)
        {
            var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
            var instance = AppState.Current.MediaServiceConnector.PlayerInstance;
            var x = e.ClickedItem as MediaThumbnailPreviewData;
            if (AppState.Current.MediaServiceConnector.CurrentPlaybackSession != null)
            {
                if (session.PlaybackState != MediaPlaybackState.None)
                {
                    session.Position = x.SeekPosition;
                }
                else
                {
                    SettingsService.Instance.PlayerResumePosition = x.SeekPosition.Ticks;
                    await instance.PlayPauseAsync();
                }
            }
        }

        private async void LoadViewPreviews_click(object? sender, RoutedEventArgs e)
        {
            try
            {
                (sender as Button).IsEnabled = false;
                await LoadVideoThumbnails(CurrentPlaybackItemInfo);
            }
            finally
            {
                (sender as Button).IsEnabled = true;
            }
        }
    }
}
