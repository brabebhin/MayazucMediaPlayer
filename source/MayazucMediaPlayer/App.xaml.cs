using MayazucMediaPlayer.AudioEffects;
using MayazucMediaPlayer.BackgroundServices;
using MayazucMediaPlayer.Dialogs;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaPlayback;
using MayazucMediaPlayer.NowPlayingViews;
using MayazucMediaPlayer.Playlists;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;
using MayazucMediaPlayer.Themes;
using MayazucMediaPlayer.UserInput;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer
{

    [ComImport, System.Runtime.InteropServices.Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitializeWithWindow
    {
        void Initialize([In] IntPtr hwnd);
    }

    public partial class AppState : IDisposable
    {
        static readonly object lockState = new object();
        static AppState _current;
        private bool disposedValue;

        public static AppState Current
        {
            get
            {
                lock (lockState)
                {
                    if (_current == null)
                        _current = new AppState();
                    return _current;
                }
            }
        }

        private AppState()
        {
        }

        public FileMetadataService FileMetadataService { get; private set; } = new FileMetadataService();

        public DispatcherQueue Dispatcher
        {
            get
            {
                return Services.GetService<DispatcherQueue>();
            }
        }

        public ServiceProvider Services { get; private set; }

        public HotKeyManager KeyboardInputManager
        {
            get
            {
                return Services.GetService<HotKeyManager>();
            }
        }

        public BackgroundMediaServiceConnector MediaServiceConnector { get; private set; } = new BackgroundMediaServiceConnector();

        public MusicLibraryIndexingService MusicLibraryIndexService { get; private set; } = new MusicLibraryIndexingService();

        public ServiceProvider ConfigureDependencyInjection(Func<DispatcherQueue> dispatcherQueueFactory)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<DispatcherQueue>((sp) =>
            {
                return dispatcherQueueFactory();
            });

            serviceCollection.AddSingleton<IPlaybackSequenceProviderFactory, JsonPlaybackSequenceFactory>();

            serviceCollection.AddSingleton<PlaybackSequenceService>();
            serviceCollection.AddSingleton<PlaylistsService>();

            serviceCollection.AddSingleton<IBackgroundPlayer, BackgroundMediaPlayer>();

            serviceCollection.AddSingleton<NowPlayingUiService>();


            serviceCollection.AddSingleton<EqualizerService>();

            serviceCollection.AddSingleton<AudioEnhancementsPageViewModel>();

            serviceCollection.AddSingleton<IOpenSubtitlesAgent, OpenSubtitlesRestAgent>();


            serviceCollection.AddSingleton<ApplicationDataModel>();

            serviceCollection.AddTransient<FileManagementService>();
            serviceCollection.AddTransient<PlaylistViewerModel>();
            serviceCollection.AddTransient<PlaylistsDetailsService>();

            serviceCollection.AddSingleton<HotKeyManager>();

            ContentDialogService.Initialize();

            return Services = serviceCollection.BuildServiceProvider();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                Services.Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~AppState()
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
    }


    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application, IDisposable
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
            AppState.Current.ConfigureDependencyInjection(DispatcherQueue.GetForCurrentThread);
            SetupApplication();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            SettingsService.Instance.SaveSettings();
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, PreserveSig = true, SetLastError = false)]
        public static extern IntPtr GetActiveWindow();

        private void SetupApplication()
        {
            var requestedUserTheme = ThemesManager.GetRequestedTheme();
            if (requestedUserTheme != null)
            {
                RequestedTheme = requestedUserTheme.Value;
            }

        }

        private void InitBackgroundMediaPlayer(IntPtr hwnd, ulong windowId)
        {
            AppState.Current.MediaServiceConnector.Initialize(ApplicationState.Services, hwnd, windowId);
        }

        private AppState ApplicationState
        {
            get
            {
                return AppState.Current;
            }
        }

        public static App CurrentInstance
        {
            get
            {
                return App.Current as App;
            }
        }

        public static object LaunchConstants { get; internal set; }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file (that's a lie .
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();

            InitBackgroundMediaPlayer(GetActiveWindow(), m_window.AppWindow.Id.Value);

            await m_window.LoadAsync();
            AppState.Current.MusicLibraryIndexService.Start();

            var launchArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (launchArgs.Kind == ExtendedActivationKind.File)
            {
                await FileActivateAsync(launchArgs.Data as IFileActivatedEventArgs);
            }
        }

        internal async Task FileActivateAsync(IFileActivatedEventArgs args)
        {
            await m_window.FileActivate(args.Files, args.Verb);
        }

        internal Task LaunchActivateAsync(ILaunchActivatedEventArgs args)
        {
            return Task.CompletedTask;
        }


        public XamlRoot CurrentXamlRoot()
        {
            return m_window.Content.XamlRoot;
        }

        private MainWindow m_window;
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //m_window?.Dispose();
                    // TODO: dispose managed state (managed objects)
                }
                AppState.Current?.Dispose();

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~App()
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
    }
}
