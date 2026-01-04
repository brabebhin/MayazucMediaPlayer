using FluentResults;
using MayazucMediaPlayer.BackgroundServices;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.FileSystemViews
{
    public partial class IMediaPlayerItemSourceProviderBase : ObservableObject
    {
        int trackNumber;
        public int ExpectedPlaybackIndex
        {
            get => trackNumber;
            set
            {
                if (trackNumber == value) return;

                trackNumber = value;
                NotifyPropertyChanged(nameof(ExpectedPlaybackIndex));
                NotifyPropertyChanged(nameof(UIDisplayedIndex));
            }
        }

        public int UIDisplayedIndex
        {
            get => trackNumber + 1;
        }

        bool isInPlayback;

        public bool IsInPlayback
        {
            get => isInPlayback;
            set
            {
                if (isInPlayback != value)
                {
                    isInPlayback = value;
                    NotifyPropertyChanged(nameof(IsInPlayback));
                    NotifyPropertyChanged(nameof(BackgroundColor));
                }
            }
        }

        public SolidColorBrush BackgroundColor
        {
            get
            {
                if (IsInPlayback)
                {
                    return new SolidColorBrush(Colors.Coral);
                }
                return new SolidColorBrush(Colors.Transparent);
            }
        }
    }

    public partial class PickedFileItem : IMediaPlayerItemSourceProviderBase, IMediaPlayerItemSourceProvder
    {
        public EmbeddedMetadata Metadata { get; private set; }

        public event EventHandler<EmbeddedMetadata?> MetadataChanged;
        public event EventHandler<FileInfo?> ImageFileChanged;
        private readonly Task MetadataInitializationTask;

        
        public FileInfo File
        {
            get;
            private set;
        }

        /// <summary>
        /// Used for UI databinding
        /// </summary>
        public string DisplayName
        {
            get;
            private set;
        }

        public string Path
        {
            get;
            private set;
        }

        public bool SupportsMetadata { get; private set; }

        public PickedFileItem(FileInfo file)
        {
            File = file;
            Path = file.FullName;
            DisplayName = File.Name;
            Metadata = EmbeddedMetadataResolver.GetDefaultMetadataForFile(file.FullName);
            SupportsMetadata = EmbeddedMetadataResolver.IsSupportedExtension(File);
            MetadataInitializationTask = RetrieveThumbnailImageAsync();
        }

        public async Task<Result<ReadOnlyCollection<IMediaPlayerItemSource>>> GetMediaDataSourcesAsync()
        {
            var result = new List<IMediaPlayerItemSource>();
            result.Clear();
            try
            {
                result.AddRange(await File.GetMediaPlayerItemSources());
            }
            catch
            {
                return Result.Fail("An error occured while parsing the file");
            }
            return Result.Ok(result.AsReadOnly());
        }

        /// <summary>
        /// Used for UI databinding
        /// </summary>
        public string ImageUri
        {
            get;
            private set;
        } = AssetsPaths.PlaceholderAlbumArt;


        FileInfo imageFile;
        private FileInfo ImageFile
        {
            get
            {
                return imageFile;
            }
            set
            {
                if (imageFile == value) return;

                if (value != null)
                {
                    imageFile = value;
                    ImageUri = imageFile.FullName;
                }

                NotifyPropertyChanged(nameof(ImageUri));

            }
        }

        private async Task TryGetMetadataAsync()
        {
            if (SupportsMetadata)
            {
                var expectedThumbnailFiles = await ThumbnailProvider.GetExpectedAlbumArtFileNames(File);
                var metadataFile = await EmbeddedMetadataResolver.GetMetadataDocumentForFile(File.FullName);
                GetImageFileAsync(expectedThumbnailFiles);
                if (ImageFile == null || !metadataFile.Exists)
                {
                    //we didn't find the image file, so queue it on the thumbnail provider
                    _ = AppState.Current.FileMetadataService.ProcessFileAsync(File, true).ContinueWith(async (metadata) =>
                    {
                        GetImageFileAsync(expectedThumbnailFiles);
                        await SetMetadata(metadataFile);
                    });
                }
                else if (metadataFile.Exists)
                {
                    await SetMetadata(metadataFile);
                }
            }

            void GetImageFileAsync(IReadOnlyCollection<string> expectedThumbnailFiles)
            {
                foreach (var expectedFile in expectedThumbnailFiles)
                {
                    var expectedFileInfo = new FileInfo(expectedFile);
                    if (expectedFileInfo.Exists)
                    {
                        ImageFile = expectedFileInfo;
                        ImageFileChanged?.Invoke(this, ImageFile);
                        break;
                    }
                }
            }

            async Task SetMetadata(FileInfo metadataFile)
            {
                var asyncLock = FileMetadataLockManager.GetLock(Path);
                using (await asyncLock.LockAsync())
                {
                    Metadata = EmbeddedMetadataResolver.ReadMetadataDocumentForFile(metadataFile, File);
                    MetadataChanged?.Invoke(this, Metadata);

                    NotifyPropertyChanged(nameof(Metadata));

                }
            }
        }

        private async Task RetrieveThumbnailImageAsync()
        {
            await TryGetMetadataAsync();
        }

        public override string ToString()
        {
            return File.Name;
        }
    }

    public partial class InternetStreamMediaPlayerItemSourceProvder : IMediaPlayerItemSourceProviderBase, IMediaPlayerItemSourceProvder
    {
        public string DisplayName
        {
            get;
            private set;
        }

        public string ImageUri
        {
            get;
            private set;
        } = AssetsPaths.PlaceholderAlbumArt;

        public Guid ItemID
        {
            get;
            private set;
        }

        public EmbeddedMetadata Metadata
        {
            get;
            private set;
        }

        public string Path
        {
            get;
            private set;
        }

        public bool SupportsMetadata => false;

        public event EventHandler<FileInfo> ImageFileChanged;
        public event EventHandler<EmbeddedMetadata> MetadataChanged;

        public InternetStreamMediaPlayerItemSourceProvder(string path, string title, EmbeddedMetadata metadata)
        {
            Path = path;
            DisplayName = title;
            Metadata = metadata;
        }

        public InternetStreamMediaPlayerItemSourceProvder(string path)
        {
            Path = path;
            Metadata = EmbeddedMetadataResolver.GetDefaultMetadataForFile(path);

            DisplayName = Metadata.Title;
        }

        public Task<Result<ReadOnlyCollection<IMediaPlayerItemSource>>> GetMediaDataSourcesAsync()
        {
            var result = IMediaPlayerItemSourceFactory.Get(Path);
            return Task.FromResult(Result.Ok(new List<IMediaPlayerItemSource>() { result }.AsReadOnly()));
        }
    }
}
