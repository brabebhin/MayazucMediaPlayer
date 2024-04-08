using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Nito.AsyncEx;
using System;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.NowPlayingViews
{
    public sealed partial class NowPlayingControlLite : BaseUserControl
    {
        readonly AsyncLock mediaOpenedLock = new AsyncLock();
        readonly AsyncLock skipToIndexLock = new AsyncLock();
        public event EventHandler<RoutedEventArgs> OpenInMainViewRequest;

        public PlaybackSequenceService PlaybackModel
        {
            get;
            private set;
        }

        public NowPlayingControlLite()
        {
            InitializeComponent();
            PlaybackModel = AppState.Current.Services.GetService<PlaybackSequenceService>();
            lsvNowPlayingList.ItemsSource = PlaybackModel.NowPlayingBackStore;
        }

        private async void SkipToItem(object? sender, ItemClickEventArgs e)
        {
            using (await skipToIndexLock.LockAsync())
            {
                var index = PlaybackModel.NowPlayingBackStore.IndexOf(e.ClickedItem as MediaPlayerItemSourceUIWrapper);
                if (index >= 0)
                {
                    await AppState.Current.MediaServiceConnector.SkipToIndex(index);
                }
            }
        }
    }
}
