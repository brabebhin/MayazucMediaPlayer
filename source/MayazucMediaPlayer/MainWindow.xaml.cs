using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using MayazucMediaPlayer.AudioEffects;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaCollections;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.Navigation;
using MayazucMediaPlayer.NowPlayingViews;
using MayazucMediaPlayer.Playlists;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.Transcoding;
using MayazucMediaPlayer.UserInput;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer
{
    public class MainWindowingService
    {
        private MainWindow HostWindow
        {
            get;
            set;
        }

        public MainWindowingService(MainWindow HostWindow)
        {
            this.HostWindow = HostWindow;
        }

        public WindowId Id
        {
            get
            {
                return HostWindow.AppWindow.Id;
            }
        }


        public async Task RequestAlwaysOnTopOverlayMode(bool shouldOverlayOnTop)
        {
            await HostWindow.SetWindowOnTopOverlayMode(shouldOverlayOnTop);
        }

        public async Task RequestFullScreenMode(bool shouldFullScreen)
        {
            await HostWindow.GoToFullScreenMode(shouldFullScreen);
            MediaPlayerElementFullScreenModeChanged?.Invoke(this, shouldFullScreen);
        }

        public bool IsAlwaysOnTopWindowOverlayMode()
        {
            return HostWindow.IsInCompactOverlayMode();
        }

        public bool IsInFullScreenMode()
        {
            return HostWindow.IsInFullScreenMode();
        }

        public static MainWindowingService Instance { get; private set; }

        public static void InitializeInstanceAsync(MainWindow window)
        {
            Instance = new MainWindowingService(window);
        }

        public Task<ContentDialogServiceResult> ShowContentDialog(FrameworkElement element)
        {
            return HostWindow.ShowDialogAsync(element);
        }

        public event EventHandler<bool> MediaPlayerElementFullScreenModeChanged;
    }

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WindowBase, IDisposable, IContentDialogService
    {
        private bool disposedValue;
        public const int CompactNowPlayingHeightThreshold = 450;
        public const int SplitViewerForceOverlayHeightThreshold = 720;
        public const int CompactNowPlayingHight = 124;
        private bool WasNowPlayingMaximziedSomeTime = false;
        private double NowPlatyingCompactSize
        {
            get
            {
                return ((GridLength)App.CurrentInstance.Resources["CompactNowPlayingWidth"]).Value;
            }
        }

        string NowPlayingPageTitle = "Now Playing";

        private NowPlayingHome NowPlayingPage { get; set; }
        private bool NowPlayingWasJustMinimized { get; set; }

        public NowPlayingCommandBarViewModel PlaybackCommandsModel
        {
            get;
            private set;
        }

        public IServiceProvider ServiceProvider
        {
            get
            {
                return AppState.Current.Services;
            }
        }

        public bool NowPlayingMaximized
        {
            get
            {
                return IsNowPlayingMaximized();
            }
            set
            {
                if (IsNowPlayingMaximized() != value)
                {
                    NowPlayingPage.IsTabStop = value;
                    if (!value)
                    {
                        MinimizeNowPlaying();
                    }
                    else
                    {
                        MaximizeNowPlaying();
                    }
                }
            }
        }

        public MainWindow()
        {
            MainWindowingService.InitializeInstanceAsync(this);
            InitializeComponent();
            MinHeight = 420;
            MinWidth = 860;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBarGrid);

            Title = "Mayazuc Media Player";
            RootFrame.ServiceProvider = ServiceProvider;
            MainWindowingService.Instance.MediaPlayerElementFullScreenModeChanged += BackgroundMediaService_MediaPlayerElementFullScreenModeChanged;
            PopupHelper.NotificationRequest += PopupHelper_NotificationRequest;

            PlaybackCommandsModel = new NowPlayingCommandBarViewModel(DispatcherQueue);
            RootFrame.AsyncNavigated += RootFrame_AsyncNavigated;
            SizeChanged += MCMediaCenterRootApplication_SizeChanged;

            OverayModeButtonIcon.Glyph = !IsAlwaysOnTop ? EnterOverlayModeIcon : ExitOverlayModeIcon;
            GetAppWindowForCurrentWindow().Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {            
            var children = WindowLayoutRoot.FindVisualChildrenDeep<Control>().Select(x => x as IDisposable);
            foreach (var c in children)
            {
                c?.Dispose();
            }
        }

        private async void PlayerInstance_OnMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            await HandleMediaOpened(sender, args);
        }

        private void RootFrame_AsyncNavigated(object? sender, Type e)
        {
            if (!IsNowPlayingMaximized())
            {
                CurrentViewTitle.Text = RootFrame.CurrentPage.Title;
            }
        }

        private void MCMediaCenterRootApplication_SizeChanged(object? sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs e)
        {
            var width = e.Size.Width;
            var height = e.Size.Height;

            if (!IsNowPlayingMaximized())
            {
                MinimizeNowPlaying();
            }
        }

        private async void BackgroundMediaService_MediaPlayerElementFullScreenModeChanged(object? sender, bool e)
        {
            if (e)
                await MaximizeNowPlayingIfCollapsed();
        }

        private async void PopupHelper_NotificationRequest(object? sender, PopupRequestData e)
        {
            await ShowNotification(e);
        }

        private void FreeMemory()
        {
            RootFrame.Dispose();

            NowPlayingPage.ExternalNavigationRequest -= NowPlayingSplitFrame_ExternalNavigationRequest;

            PopupHelper.NotificationRequest -= PopupHelper_NotificationRequest;

            NowPlayingPage.MediaPlayerElementInstance.DoubleTapped -= MediaPlayerElementInstance_DoubleTapped;
            NowPlayingPage.Dispose();
        }

        public async Task LoadAsync()
        {
            await InitializeNowPlayingViewAsync();

            await FirstNavigate();

            AppState.Current.MediaServiceConnector.PlayerInstance.OnMediaOpened += PlayerInstance_OnMediaOpened;

            await AppState.Current.KeyboardInputManager.RegisterAcceleratorsAsync(Content);
        }

        private async Task InitializeNowPlayingViewAsync()
        {
            NowPlayingPage = (NowPlayingHome)await PageFactory.GetPage(typeof(NowPlayingHome), null);
            NowPlayingPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            NowPlayingPage.VerticalAlignment = VerticalAlignment.Stretch;
            NowPlayingPage.VerticalContentAlignment = VerticalAlignment.Stretch;
            NowPlayingPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            NowPlayingPage.ExternalNavigationRequest += NowPlayingSplitFrame_ExternalNavigationRequest;
            NowPlayingContainer.Children.Add(NowPlayingPage);
            await NowPlayingPage.InitializeStateAsync(null);
            NowPlayingPage.MediaPlayerElementInstance.DoubleTapped += MediaPlayerElementInstance_DoubleTapped;
            NowPlayingPage.Tapped += NowPlayingSplitFrame_Tapped;
        }

        private async void NowPlayingSplitFrame_Tapped(object? sender, TappedRoutedEventArgs e)
        {
            if (!IsNowPlayingMaximized())
                await NowPlayingHomeTapped();
        }

        private async Task NowPlayingHomeTapped()
        {
            await MaximizeNowPlayingIfCollapsed();
        }

        private void MediaPlayerElementInstance_DoubleTapped(object? sender, DoubleTappedRoutedEventArgs e)
        {
            if (IsNowPlayingMaximized())
                NowPlayingPage.MediaPlayerElementInstance.IsFullWindow = !NowPlayingPage.MediaPlayerElementInstance.IsFullWindow;
            else MaximizeNowPlaying();
        }


        public async Task HandleMediaOpened(MediaPlayer sender, MediaOpenedEventArgs args)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                if (args.Reason != MediaOpenedEventReason.MediaPlayerObjectRequested)
                {
                    if (args.Data.PlaybackItem.IsVideo() && !WasNowPlayingMaximziedSomeTime)
                    {
                        NowPlayingMaximized = true;
                    }

                    NowPlayingPageTitle = args.Data.ExtraData.MediaPlayerItemSource.Title;
                    if (IsNowPlayingMaximized())
                    {
                        SetWindowTitle();
                    }
                }
            });
        }

        private async void NowPlayingSplitFrame_ExternalNavigationRequest(object? sender, NavigationRequestEventArgs e)
        {
            await RootFrame.NavigateAsync(e.PageType, e.Parameter);
            if (IsNowPlayingMaximized()) MinimizeNowPlaying();
        }

        public async Task FileActivate(IReadOnlyList<IStorageItem> files, string verb)
        {
            List<IMediaPlayerItemSource> mediaDataSources = new List<IMediaPlayerItemSource>();
            foreach (var f in files.Where(x => x.IsOfType(StorageItemTypes.File) && x.IsSupportedExtension()))
            {
                if (f is StorageFile)
                {
                    mediaDataSources.AddRange(await (f as StorageFile).ToFileInfo().GetMediaPlayerItemSources());
                }
            }

            if (verb == FileFolderPickerPage.PlayCommandString || verb == "open")
            {
                await AppState.Current.MediaServiceConnector.StartPlaybackFromBeginning(mediaDataSources);
            }
            else if (verb == FileFolderPickerPage.AddToNowPlayingString)
            {
                await AppState.Current.MediaServiceConnector.EnqueueAtEnd(mediaDataSources);
            }
            else if (verb == FileFolderPickerPage.PlayNextCommandString)
            {
                await AppState.Current.MediaServiceConnector.EnqueueNext(mediaDataSources);
            }
        }

        private async Task FirstNavigate()
        {
            var firstPageNavigationType = typeof(FileSystemViews.FileFolderPickerPage);
            await RootFrame.NavigateAsync(firstPageNavigationType);
            var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
            if (session != null)
            {
                if (session.PlaybackState != Windows.Media.Playback.MediaPlaybackState.None)
                {
                    NowPlayingMaximized = false;
                }
            }
        }

        private async Task NavigateCommandInternal(Type xPageNavigationType)
        {
            if (RootFrame.CurrentSourcePageType == null || (xPageNavigationType != null && xPageNavigationType != RootFrame.CurrentSourcePageType))
            {
                await RootFrame.NavigateAsync(xPageNavigationType);
            }

            NowPlayingWasJustMinimized = NowPlayingMaximized;

            NowPlayingMaximized = false;
        }

        private async Task ShowNotification(PopupRequestData content, int duration = 2500)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                try
                {
                    notificationRoot.Content = content;

                    notificationRoot.Show(duration);
                }
                catch { }
            });
        }

        private async void FileDropped_dropped(object? sender, DragEventArgs e)
        {
            var deferal = e.GetDeferral();
            try
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await e.DataView.GetStorageItemsAsync();

                    if (items.Count > 0)
                    {
                        string commandName = (sender as FrameworkElement).Tag.ToString();

                        switch (commandName)
                        {
                            case "Play":
                                await new QuickPlayService().PlayItems(items);
                                break;
                            case "Enqueue":
                                await new QuickPlayService().EnqueueItems(items);
                                break;
                            case "Playlist":
                                await new QuickPlayService().AddItemsToPlaylist(items);
                                break;
                        }
                    }
                }
                else
                {

                }
            }
            catch { }
            finally
            {
                deferal.Complete();
            }
        }

        private void FileDrag_dragover(object? sender, DragEventArgs e)
        {
            try
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    e.DragUIOverride.Caption = ToolTipService.GetToolTip(sender as DependencyObject).ToString();
                    //if (e.Modifiers == (DragDropModifiers.LeftButton))
                    //{
                    //    e.DragUIOverride.Caption = "Open...";
                    //}
                    //else if (e.Modifiers == (DragDropModifiers.Shift | DragDropModifiers.LeftButton))
                    //{
                    //    e.DragUIOverride.Caption = "Play...";
                    //}
                    //else if (e.Modifiers == (DragDropModifiers.Control | DragDropModifiers.LeftButton))
                    //{
                    //    e.DragUIOverride.Caption = "Add to now playing...";
                    //}
                    //else if (e.Modifiers == (DragDropModifiers.Alt | DragDropModifiers.LeftButton))
                    //{
                    //    e.DragUIOverride.Caption = "Add to playlist...";
                    //}

                    e.DragUIOverride.IsGlyphVisible = false;
                }
                else
                {
                    e.AcceptedOperation = DataPackageOperation.Move;
                }
            }
            catch { }
        }

        private void CheckNowPlayingSlitterSize()
        {
            if (!IsNowPlayingMaximized())
            {
                NowPlayingMaximized = true;
            }
            else
            {
                NowPlayingMaximized = false;
            }
        }

        private async Task MaximizeNowPlayingIfCollapsed()
        {
            if (!IsNowPlayingMaximized())
            {
                await MiniMaximizeNowPlayingAndWait(true);
            }
        }

        private async Task MiniMaximizeNowPlayingAndWait(bool maximize)
        {
            if (NowPlayingMaximized != maximize)
            {
                await NowPlayingPage.WaitForElementSizeChangedAsync(() =>
                {
                    NowPlayingMaximized = maximize;
                });
            }
        }

        private bool IsNowPlayingMaximized()
        {
            return LayoutRoot.Visibility == Visibility.Collapsed;
        }

        private void MaximizeNowPlaying()
        {
            WasNowPlayingMaximziedSomeTime = true;
            NowPlayingSwitchIcon.Symbol = Microsoft.UI.Xaml.Controls.Symbol.Back;
            ToolTipService.SetToolTip(NowPlayingSwitchButton, RootFrame.CurrentPage.Title);
            LayoutRoot.Visibility = Visibility.Collapsed;

            nowPlayingRow.Height = new GridLength(1, GridUnitType.Star);
            contentRow.Height = new GridLength(1, GridUnitType.Auto);

            BufferColumn.Width = new GridLength(0, GridUnitType.Pixel);
            BufferColumn2.Width = new GridLength(0, GridUnitType.Pixel);

            DeviceBindingsColumn.Width = new GridLength(0, GridUnitType.Pixel);
            CompactNowPlayingColumn.Width = new GridLength(1, GridUnitType.Star);
            SettingsColumn.Width = new GridLength(0, GridUnitType.Pixel);
            SetWindowTitle();
        }

        private void SetWindowTitle()
        {
            if (IsNowPlayingMaximized())
            {
                CurrentViewTitle.Text = NowPlayingPageTitle;
            }
            else
            {
                CurrentViewTitle.Text = RootFrame.CurrentPage.Title;
            }
        }

        private void MinimizeNowPlaying()
        {
            NowPlayingSwitchIcon.Symbol = Symbol.MusicInfo;
            ToolTipService.SetToolTip(NowPlayingSwitchButton, "Show now playing");

            LayoutRoot.Visibility = Visibility.Visible;
            nowPlayingRow.Height = new GridLength(GetMinimizedNowPlayingViewHieght(), GridUnitType.Pixel);
            contentRow.Height = new GridLength(1, GridUnitType.Star);

            BufferColumn2.Width = new GridLength(1, GridUnitType.Auto);
            BufferColumn.Width = new GridLength(1, GridUnitType.Star);

            DeviceBindingsColumn.Width = new GridLength(1, GridUnitType.Auto);
            CompactNowPlayingColumn.Width = new GridLength(NowPlatyingCompactSize, GridUnitType.Pixel);
            SettingsColumn.Width = new GridLength(1, GridUnitType.Auto);
            SetWindowTitle();
        }

        double GetMinimizedNowPlayingViewHieght()
        {
            return Bounds.Height > CompactNowPlayingHeightThreshold ? CompactNowPlayingHight : 0;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                FreeMemory();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~MainWindow()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        internal Task GoToFullScreenMode(bool shouldFullScreen)
        {
            var appView = GetAppWindowForCurrentWindow();
            var mode = AppWindowPresenterKind.Default;
            if (shouldFullScreen)
            {
                mode = AppWindowPresenterKind.FullScreen;
                ExtendsContentIntoTitleBar = false;
                RootTitleBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                ExtendsContentIntoTitleBar = true;
                RootTitleBar.Visibility = Visibility.Visible;
            }
            appView.SetPresenter(mode);
            return Task.CompletedTask;
        }

        internal Task SetWindowOnTopOverlayMode(bool miniPlayer)
        {
            var appView = GetAppWindowForCurrentWindow();
            var mode = AppWindowPresenterKind.Default;
            if (miniPlayer) mode = AppWindowPresenterKind.CompactOverlay;
            appView.SetPresenter(mode);
            return Task.CompletedTask;
        }

        public bool IsInCompactOverlayMode()
        {
            var appView = GetAppWindowForCurrentWindow();
            return appView.Presenter.Kind == AppWindowPresenterKind.CompactOverlay;
        }

        public bool IsInFullScreenMode()
        {
            var appView = GetAppWindowForCurrentWindow();
            return appView.Presenter.Kind == AppWindowPresenterKind.FullScreen;
        }

        private async void GoToSomeScreen(object? sender, RoutedEventArgs e)
        {
            var obj = sender as FrameworkElement;
            switch (obj.Tag.ToString())
            {
                case "Settings":
                    await NavigateCommandInternal(typeof(SettingsPage));
                    break;
                case "MusicLibrary":
                    await NavigateCommandInternal(typeof(MusicCollectionPage));
                    break;
                case "VideoLibrary":
                    await NavigateCommandInternal(typeof(VideoCollectionPage));
                    break;
                case "PlaylistsLibrary":
                    await NavigateCommandInternal(typeof(PlaylistViewerPage));
                    break;
                case "FilesFolders":
                    await NavigateCommandInternal(typeof(FileFolderPickerPage));
                    break;
                case "NetworkStreamHistory":
                    await NavigateCommandInternal(typeof(NetworkStreamsCollectionPage));
                    break;
            }
        }

        private async void PlayShallowFolder(object? sender, RoutedEventArgs e)
        {
            var service = new QuickPlayService();
            await service.PlayShallowFolderAsync();
        }

        private async void PlayDeepFolder(object? sender, RoutedEventArgs e)
        {
            var service = new QuickPlayService();
            await service.PlayDeepFolderAsync();
        }

        private async void PlayFiles(object? sender, RoutedEventArgs e)
        {
            var service = new QuickPlayService();
            await service.PlayFilesAsync();
        }

        private async void PlayRandomPlaylist(object? sender, RoutedEventArgs e)
        {
            MediaPlayerItemSourceList data = new MediaPlayerItemSourceList();
            var files = await RandomPlaylist.GetRandomPlaylist();
            foreach (var f in files)
            {
                data.AddRange(await f.GetMediaPlayerItemSources());
            }

            await AppState.Current.MediaServiceConnector.StartPlaybackFromBeginning(data);
        }

        private async void ShowHideNowPlaying(object? sender, TappedRoutedEventArgs e)
        {
            if (IsNowPlayingMaximized())
            {
                if (!await NowPlayingPage.GoBack())
                    MinimizeNowPlaying();

            }
            else MaximizeNowPlaying();
        }

        private async void GoToConvertFilesPage(object sender, TappedRoutedEventArgs e)
        {
            await NavigateCommandInternal(typeof(TranscodingRootPage));
        }

        private async void GoToQueueManagement(object sender, RoutedEventArgs e)
        {
            await OpenPageOrNowPlaying(typeof(NowPlayingManagementPage));
        }

        private async void GoToMediaEffects(object sender, RoutedEventArgs e)
        {
            await OpenPageOrNowPlaying(typeof(MediaEffectsPage));
        }

        private async Task OpenPageOrNowPlaying(Type pageType)
        {
            if (!NowPlayingMaximized &&
                RootFrame.CurrentSourcePageType == pageType)
            {
                MaximizeNowPlaying();
            }
            else
            {
                await NavigateCommandInternal(pageType);
            }
        }

        public Task<ContentDialogServiceResult> ShowDialogAsync(FrameworkElement content)
        {
            return contentDialogService.ShowDialogAsync(content);
        }

        private async void GoToMediaEqualizerConfigs(object sender, RoutedEventArgs e)
        {
            await OpenPageOrNowPlaying(typeof(EQConfigurationManagementPage));
        }

        private async void EnterExitOverlayMode(object sender, TappedRoutedEventArgs e)
        {
            var shouldOverlay = !MainWindowingService.Instance.IsAlwaysOnTopWindowOverlayMode();
            await MainWindowingService.Instance.RequestAlwaysOnTopOverlayMode(shouldOverlay);
            OverayModeButtonIcon.Glyph = shouldOverlay ? ExitOverlayModeIcon : EnterOverlayModeIcon;
        }

        private readonly string EnterOverlayModeIcon = "\uEE49";
        private readonly string ExitOverlayModeIcon = "\uEE47";

    }
}
