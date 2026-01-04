using MayazucMediaPlayer.Converters;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Media.Core;
using Windows.Media.Playback;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{

    public class ChapterCueDto
    {
        public ChapterCue Cue { get; private set; }

        public string Title => Cue.Title;

        public string StartTime => new ConverterLocator().TimespanToString(Cue.StartTime);

        public ChapterCueDto(ChapterCue cue)
        {
            Cue = cue;
        }
    }

    public sealed partial class ChapterSelectionControl : BaseUserControl
    {
        ObservableCollection<ChapterCueDto> chapters = new ObservableCollection<ChapterCueDto>();
        public ChapterSelectionControl()
        {
            this.InitializeComponent();
        }

        public void LoadMediaPlaybackItem(MediaPlaybackItem item)
        {
            var chapterTrack = item.TimedMetadataTracks.FirstOrDefault(x => x.TimedMetadataKind == Windows.Media.Core.TimedMetadataKind.Chapter);
            chapters.Clear();
            if (chapterTrack != null)
                chapters.AddRange(chapterTrack.Cues.Cast<ChapterCue>().Select(x => new ChapterCueDto(x)));
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
