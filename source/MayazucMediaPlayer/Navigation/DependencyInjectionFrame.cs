using MayazucMediaPlayer.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Navigation
{
    public partial class DependencyInjectionFrame : ContentPresenter, IDisposable
    {
        public event EventHandler<NavigationRequestEventArgs> ExternalNavigationRequest;

        public void NotifyExternalNavigationRequest(object? sender, NavigationRequestEventArgs args)
        {
            ExternalNavigationRequest?.Invoke(sender, args);
        }

        public BasePage CurrentPage
        {
            get
            {
                return Content as BasePage;
            }
        }

        protected virtual void SubmitExternalNavigation(NavigationRequestEventArgs args)
        {
            ExternalNavigationRequest?.Invoke(this, args);
        }

        MemoryCache PageCache { get; set; } = new MemoryCache(new MemoryCacheOptions());

        public bool AllowsNestedNavigation
        {
            get; set;
        } = false;

        public bool UseCache { get; set; } = true;

        public DependencyInjectionFrame()
        {
        }


        private readonly Stack<BasePage> PageBackStack = new Stack<BasePage>();

        public IServiceProvider ServiceProvider
        {
            get;
            set;
        }

        private bool disposedValue;

        public event EventHandler<Type> AsyncNavigated;

        public Task<BasePage> NavigateAsync(Type pageType)
        {
            return NavigateAsync(pageType, null);
        }

        public Type CurrentSourcePageType
        {
            get
            {
                if (Content != null) return Content.GetType();
                return GetType();
            }
        }

        public Task<BasePage> NavigateAsync(Type pageType, object args)
        {
            return NavigateAsyncInternal(pageType, args);
        }

        private async Task<BasePage> NavigateAsyncInternal(Type pageType, object args)
        {
            if (ShouldNavigatePage())
            {
                return await CreateAndActivatePage(pageType, args);
            }
            else
            {
                SubmitExternalNavigation(new NavigationRequestEventArgs(pageType, args));
            }
            return CurrentPage;
        }

        protected async Task<BasePage> CreateAndActivatePage(Type pageType, object args)
        {
            var cacheHit = PageCache.TryGetValue(pageType, out BasePage page);
            if (!cacheHit || !UseCache)
            {
                page = await PageFactory.GetPage(pageType, args);

                PageCache.Set(pageType, page, DateTimeOffset.Now.AddMinutes(30));
            }

            Content = page;

            AsyncNavigated?.Invoke(this, pageType);
            return page;
        }

        private bool ShouldNavigatePage()
        {
            return Content == null || AllowsNestedNavigation;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }
                foreach (var backEntry in PageBackStack)
                    backEntry.Dispose();
                PageBackStack.Clear();
                //var currentPage = (BasePage)Content;
                //currentPage?.Dispose();
                //Content = null;
                ServiceProvider = null;
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~DependencyInjectionFrame()
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

    public static class PageFactory
    {
        public static async Task<BasePage> GetPage(Type pageType, object args)
        {
            var page = (BasePage)Activator.CreateInstance(pageType);
            await LoadPage(page, args);
            return page;
        }

        private static async Task LoadPage(BasePage page, object args)
        {
            await page.InitializeStateAsync(args);
        }
    }
}
