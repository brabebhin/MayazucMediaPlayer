using System.Threading;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.BackgroundServices
{
    public class MusicLibraryIndexingService
    {
        public Task WorkerTask = null;
        readonly CancellationTokenSource cancelSignal = new CancellationTokenSource();

        public MusicLibraryIndexingService() { }

        public async Task StopAsync()
        {
            cancelSignal.Cancel();
            await WorkerTask;
        }

        public void Start()
        {
            WorkerTask = Task.Run(async () =>
            {
                try
                {
                    var files = await KnownFoldersExtensions.GetFilesAsync(LibraryFolderId.MusicFolder);
                    foreach (var file in files)
                    {
                        if (cancelSignal.IsCancellationRequested)
                            await AppState.Current.FileMetadataService.ProcessFileAsync(file, false);
                    }
                }
                catch
                {

                }
            });
        }
    }
}
