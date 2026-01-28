using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace MayazucMediaPlayer
{
    public static class Program
    {
        const string SingleInstanceKey = "B9BA688B46CC431B93D07B9D48468C18";
        public static event EventHandler? ApplicationShutDownStarted;

        // Replaces the standard App.g.i.cs.
        // Note: We can't declare Main to be async because in a WinUI app
        // this prevents Narrator from reading XAML elements.
        [STAThread]
        static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();

            bool isRedirect = DecideRedirection();
            System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (!isRedirect)
            {
                App? app = null;
                Microsoft.UI.Xaml.Application.Start((p) =>
                {
                    var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    dispatcherQueue.ShutdownStarting += DispatcherQueue_ShutdownStarting;
                    var context = new DispatcherQueueSynchronizationContext(
                        dispatcherQueue);
                    SynchronizationContext.SetSynchronizationContext(context);
                    app = new App();
                });
                System.AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
                app?.Dispose();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static async void DispatcherQueue_ShutdownStarting(DispatcherQueue sender, DispatcherQueueShutdownStartingEventArgs args)
        {
            var deferal = args.GetDeferral();

            //signal UI elements it is time to get ready to be disposed
            ApplicationShutDownStarted?.Invoke(null, null);

            SettingsService.Instance.SaveSettings();

            deferal.Complete();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                SettingsService.Instance.SaveSettings();
            }
            catch { }
        }

        private static async void OnActivated(object? sender, AppActivationArguments args)
        {
            ExtendedActivationKind kind = args.Kind;
            if (kind == ExtendedActivationKind.Launch)
            {
                await App.CurrentInstance.LaunchActivateAsync(args.Data as ILaunchActivatedEventArgs);
            }
            else if (kind == ExtendedActivationKind.File)
            {
                await App.CurrentInstance.FileActivateAsync(args.Data as IFileActivatedEventArgs);
            }
        }


        // Decide if we want to redirect the incoming activation to another instance.
        private static bool DecideRedirection()
        {
            AppInstance keyInstance = AppInstance.FindOrRegisterForKey(SingleInstanceKey);
            bool isRedirect = false;
            AppActivationArguments args = keyInstance.GetActivatedEventArgs();

            if (keyInstance.IsCurrent)
            {
                // Hook up the Activated event, to allow for this instance of the app
                // getting reactivated as a result of multi-instance redirection.
                keyInstance.Activated += OnActivated;
                RedirectActivationTo(args, keyInstance);
            }
            else
            {
                isRedirect = true;
                RedirectActivationTo(args, keyInstance);
            }
            // Find out what kind of activation this is.

            return isRedirect;
        }

        // Do the redirection on another thread, and use a non-blocking
        // wait method to wait for the redirection to complete.
        public static void RedirectActivationTo(
            AppActivationArguments args, AppInstance keyInstance)
        {
            using var redirectSemaphore = new Semaphore(0, 1);
            Task.Run(() =>
            {
                keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
                redirectSemaphore.Release();
            });
            redirectSemaphore.WaitOne();
        }
    }
}
