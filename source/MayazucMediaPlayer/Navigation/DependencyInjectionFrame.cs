using MayazucMediaPlayer.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Navigation
{
    public partial class DependencyInjectionFrame : ContentControl
    {
        public BasePage CurrentPage
        {
            get
            {
                return Content as BasePage;
            }
        }

        public DependencyInjectionFrame()
        {
        }

        public event EventHandler<BasePage> AsyncNavigated;

        public async Task NavigateAsync(BasePage page)
        {
            if (this.Content != page)
                this.Content = page;
            AsyncNavigated?.Invoke(this, page);
        }

        public async Task InitializeAndNavigateAsync(BasePage page, object args)
        {
            await page.InitializeStateAsync(args);

            this.Content = page;
            AsyncNavigated?.Invoke(this, page);
        }

        public Type CurrentSourcePageType
        {
            get
            {
                if (Content != null) return Content.GetType();
                return GetType();
            }
        }
    }
}
