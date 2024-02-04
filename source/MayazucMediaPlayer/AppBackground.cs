using MayazucMediaPlayer.BackgroundServices;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Settings;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Media.Playback;
using Microsoft.UI.Dispatching;
using Windows.UI.Notifications;
using Microsoft.UI.Xaml;

namespace MayazucMediaPlayer
{
    sealed partial class App : Application
    {
        /// <summary>
        /// Called from App.xaml.cs when the application is constructed.
        /// </summary>
        void Construct()
        {
            // Subscribe to key lifecyle events to know when the app
            // transitions to and from foreground and background.
            // Leaving the background is an important transition
            // because the app may need to restore UI.
            EnteredBackground += App_EnteredBackground;
            LeavingBackground += App_LeavingBackground;

            // Subscribe to regular lifecycle events to display a toast notification
            Suspending += App_Suspending;

            //this.OnFileActivatedEventArgs += App_OnFileActivatedEventArgs;

            //var musicLib = StorageLibrary.GetLibraryAsync(KnownLibraryId.Music).AsTask().ContinueWith((x) =>
            //{
            //    BackgroundTaskHelper.Register("MusicLibraryChangeListener", StorageLibraryContentChangedTrigger.Create(x.Result));
            //});
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void App_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                var session = AppState.Current.MediaServiceConnector.CurrentPlaybackSession;
                var playerInstance = AppState.Current.MediaServiceConnector.PlayerInstance;
                if (session != null
                    && (session.PlaybackState == MediaPlaybackState.Playing ||
                    session.PlaybackState == MediaPlaybackState.Paused))
                {
                    SettingsWrapper.PlayerResumePosition = session.Position.Ticks;
                    if (playerInstance.CurrentPlaybackData != null)
                    {
                        SettingsWrapper.PlayerResumePath = playerInstance.CurrentPlaybackData.MediaPath;
                    }
                    else
                    {
                        SettingsWrapper.PlayerResumePath = string.Empty;
                    }
                }

                if (playerInstance != null)
                {
                    var currentConfig = await playerInstance.GetCurrentEqualizerConfiguration();
                    if (currentConfig != null)
                        currentConfig.SaveToFileAsync();
                }
            }
            catch { }
            deferral.Complete();
        }

        /// <summary>
        /// The application entered the background.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            // Place the application into "background mode" and note the
            // transition with a flag.
            var def = e.GetDeferral();
            ApplicationState.IsInBackgroundMode = true;

            //await ApplicationSessionState.Current.SavePresets();
            ScreenSessionManager.Current.RequestScreenOff();
            //if (SettingsValues.DisposeInterfacesWhenGoingBackground)
            //ReduceMemoryUsage();
            // An application may wish to release views and view data
            // at this point since the UI is no longer visible.
            //
            // As a performance optimization, here we note instead that 
            // the app has entered background mode with a boolean and
            // defer unloading views until AppMemoryUsageLimitChanging or 
            // AppMemoryUsageIncreased is raised with an indication that
            // the application is under memory pressure.
            def.Complete();
        }

        /// <summary>
        /// The application is leaving the background.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void App_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            // Mark the transition out of background mode.
            ApplicationState.IsInBackgroundMode = false;
            using (var deferal = e.GetDeferral())
            {
                await LoadWindowUI();
            }
        }

        private async Task LoadWindowUI()
        {
            ActivateWindow();
            if (Window.Current.Content == null)
            {
                await ResetMainWindowContent();
            }
        }

        private static async Task ResetMainWindowContent()
        {
            var content = new MCMediaCenterRootApplication();
            Window.Current.Content = content;
            await content.LoadState(null);
        }

        private void InitBackgroundMediaPlayer()
        {
            AppState.Current.MediaServiceConnector.InitializeAsync(ApplicationState.Services);
        }
    }
}
