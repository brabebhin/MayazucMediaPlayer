using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Help
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LicensesPage : BasePage
    {
        public override string Title => "Open source licenses";

        private ObservableCollection<LicenseFile> LicenseFiles
        {
            get;
            set;
        } = new ObservableCollection<LicenseFile>();


        public LicensesPage()
        {
            InitializeComponent();
        }

        protected override async Task OnInitializeStateAsync(object? parameter)
        {
            await base.OnInitializeStateAsync(parameter);
            LicenseFiles.Clear();
            string[] files = new string[] { "bzip2.txt", "ffmpeg.txt", "iconv.txt", "liblzma.txt", "libxml2.txt", "zlib.txt", "CueSharp.txt", "Taglib-Sharp.txt", "Shell API Code Pack.txt" };
            foreach (var l in files)
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Help/Licenses/{l}"));

                LicenseFiles.Add(new LicenseFile(Path.GetFileNameWithoutExtension(l), file));
            }

            lsvLicenses.ItemsSource = LicenseFiles;
        }

        public class LicenseFile
        {
            public string Name { get; private set; }

            public StorageFile File { get; private set; }

            public LicenseFile(string name, StorageFile file)
            {
                Name = name;
                File = file;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private async void OpenLicenseFile(object? sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var wnd = new LicenseViewWindow();
            await wnd.ShowAsync(sender.GetDataContextObject<LicenseFile>());
        }
    }
}
