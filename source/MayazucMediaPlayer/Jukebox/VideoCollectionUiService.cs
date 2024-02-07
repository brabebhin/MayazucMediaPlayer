using MayazucMediaPlayer.Services;
using Microsoft.UI.Dispatching;
using Nito.AsyncEx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Jukebox
{
    public class VideoCollectionUiService : ServiceBase
    {
        readonly AsyncLock collectionlock = new AsyncLock();

        public IEnumerable<FileInfo> VideoCollection
        {
            get;
            private set;
        }

        public VideoCollectionUiService(DispatcherQueue dp) : base(dp)
        {
        }

        public async Task LoadVideoCollectionAsync()
        {
            using (await collectionlock.LockAsync())
            {
                VideoCollection = (await KnownFoldersExtensions.GetFilesAsync(LibraryFolderId.VideosFolder, SupportedFileFormats.AllVideoFormats));
            }
        }
    }
}
