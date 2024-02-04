using MayazucMediaPlayer.Playlists;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class PlaylistPicker : BaseDialog
    {
        public PlaylistItem SelectedPlaylist { get; set; }

        public PlaylistPicker()
        {
            InitializeComponent();
            IsPrimaryButtonEnabled = true;
        }

        public async Task<PlaylistItem> PickPlaylistAsync(IEnumerable<PlaylistItem> itemsToPickFrom)
        {
            cbPicker.ItemsSource = itemsToPickFrom;
            await ContentDialogService.Instance.ShowAsync(this);
            return SelectedPlaylist;
        }

        protected override void OnPrimaryButtonClick()
        {
            SelectedPlaylist = cbPicker.SelectedItem as PlaylistItem;
        }

        protected override void OnSecondaryButtonClick()
        {
            SelectedPlaylist = null;
        }

        private async void playlistPicker_textSubmited(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            var items = cbPicker.ItemsSource as IEnumerable<PlaylistItem>;
            var playlistName = args.Text.Trim();
            var matched = items.FirstOrDefault(x => Path.GetFileName(x.BackstorePath) == playlistName);
            if (matched == null)
            {
                var item = await AppState.Current.Services.GetService<PlaylistsService>().AddPlaylist(playlistName);
                if (item != null)
                {
                    SelectedPlaylist = item;
                    Hide();
                }
            }
        }
    }
}
