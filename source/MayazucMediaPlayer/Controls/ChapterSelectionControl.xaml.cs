using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Media.Playback;
using System.Collections.ObjectModel;
using Windows.Media.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class ChapterSelectionControl : BaseUserControl
    {
        ObservableCollection<ChapterCue> chapters = new ObservableCollection<ChapterCue>();
        public ChapterSelectionControl()
        {
            this.InitializeComponent();
        }

        public void LoadMediaPlaybackItem(MediaPlaybackItem item)
        {
            var chapterTrack = item.TimedMetadataTracks.FirstOrDefault(x => x.TimedMetadataKind == Windows.Media.Core.TimedMetadataKind.Chapter);
            chapters.Clear();
            if (chapterTrack != null)
                chapters.AddRange(chapterTrack.Cues.Cast<ChapterCue>());
            lsvChapterCues.ItemsSource = chapters;
        }

        private async void SeekMediaToChapter_click(object sender, ItemClickEventArgs e)
        {
            var cue = e.ClickedItem as ChapterCue;
            if (cue != null)
            {
                await AppState.Current.MediaServiceConnector.PlayerInstance.Seek(cue.StartTime, true);
            }
        }
    }
}
