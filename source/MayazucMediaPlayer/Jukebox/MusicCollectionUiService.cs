using MayazucMediaPlayer.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MayazucMediaPlayer.Jukebox
{
    public class MusicCollectionUiService : ServiceBase
    {
        readonly AsyncLock collectionlock = new AsyncLock();

        public IEnumerable<FileInfo> MusicCollection
        {
            get;
            private set;
        }

        public MusicCollectionUiService(DispatcherQueue dp) : base(dp)
        {
        }

        public async Task LoadMusicCollectionAsync()
        {
            using (await collectionlock.LockAsync())
            {
                MusicCollection = (await KnownFoldersExtensions.GetFilesAsync(LibraryFolderId.MusicFolder)).Where(x => SupportedFileFormats.IsAudioFile(x.Extension));
            }
        }
    }

    public class StorageFileWithThumbnail : ObservableObject
    {
        public StorageFile File { get; private set; }

        public BitmapImage ThumbnailImage
        {
            get;
            private set;
        }

        public StorageFileWithThumbnail(StorageFile filefile)
        {
            File = filefile;
        }
    }
}
