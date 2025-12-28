using CommunityToolkit.WinUI;
using FluentResults;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.Services.MediaSources;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    /// <summary>
    /// UI wrapper for MediaDataStorage class
    /// </summary>
    public partial class MediaPlayerItemSourceUIWrapper : IMediaPlayerItemSourceProviderBase, IMediaPlayerItemSourceProvder
    {
        private Task<EmbeddedMetadata> metadataTask;

        [NotNull]
        public IMediaPlayerItemSource MediaData
        {
            get;
            private set;
        }

        public DispatcherQueue Dispatcher
        {
            get;
            private set;
        }

        public BitmapImage ThumbnailImage
        {
            get; private set;
        }

        public string Title
        {
            get
            {
                return MediaData.Title;
            }
        }

        public string Description { get; private set; }

        public string DisplayName => MediaData.Title;

        public string ImageUri
        {
            get
            {
                if (metadataTask.IsCompletedSuccessfully)
                    return metadataTask.Result.SavedThumbnailFile;
                else return EmbeddedMetadataResolver.GetDefaultMetadataForFile(MediaData.MediaPath).SavedThumbnailFile;
            }
        }

        public Guid ItemID => MediaData.ID;

        public EmbeddedMetadata Metadata
        {
            get
            {
                if (metadataTask.IsCompletedSuccessfully)
                    return metadataTask.Result;
                return EmbeddedMetadataResolver.GetDefaultMetadataForFile(MediaData.MediaPath);
            }
        }

        public string Path => MediaData.MediaPath;

        public bool SupportsMetadata => true;

        public event EventHandler<FileInfo> ImageFileChanged;
        public event EventHandler<EmbeddedMetadata> MetadataChanged;

        public Task<Result<ReadOnlyCollection<IMediaPlayerItemSource>>> GetMediaDataSourcesAsync()
        {
            return Task<Result<ReadOnlyCollection<IMediaPlayerItemSource>>>.FromResult(Result.Ok(new List<IMediaPlayerItemSource>() { MediaData }.AsReadOnly()));
        }

        public MediaPlayerItemSourceUIWrapper(IMediaPlayerItemSource mds, DispatcherQueue disp)
        {
            Dispatcher = disp;
            MediaData = mds;
            mds.MetadataChanged += Mds_MetadataChanged;
        }

        private async void Mds_MetadataChanged(object? sender, EmbeddedMetadata e)
        {
            await Checkmetadata();
        }

        public async Task LoadMediaThumbnailAsync(bool cacheOnly = false)
        {
            await Checkmetadata();
        }

        private async Task Checkmetadata()
        {
            metadataTask = MediaData.GetMetadataAsync();
            var metadata = await metadataTask;
            _ = Dispatcher.EnqueueAsync(async () =>
            {
                ThumbnailImage = new BitmapImage(new Uri(metadata.SavedThumbnailFile));

                NotifyPropertyChanged(nameof(ThumbnailImage));

                Description = string.Join(" -- ", metadata.Title, metadata.Artist, metadata.Album, metadata.Genre);
                NotifyPropertyChanged(nameof(Description));
                NotifyPropertyChanged(nameof(Title));

            });
        }

        public override string ToString()
        {
            return MediaData.ToString();
        }
    }

    public class MediaDataStorageUIWrapperCollection<T> : IMediaPlayerItemSourceProvderCollection<T> where T: MediaPlayerItemSourceUIWrapper
    {
        public MediaDataStorageUIWrapperCollection() : base()
        {
        }

        public MediaDataStorageUIWrapperCollection(IEnumerable<T> items) : base(items)
        {
        }

        public IReadOnlyCollection<IMediaPlayerItemSource> ToMediaDataList()
        {
            return this.Select(x => x.MediaData).ToList().AsReadOnly();
        }

        public int IndexOfMediaData(IMediaPlayerItemSource other)
        {
            if (other == null) return -1;

            for (int i = 0; i < Count; i++)
            {
                if (other.ID.Equals(this[i].MediaData.ID) || other.MediaPath == this[i].MediaData.MediaPath)
                    return i;
            }

            return -1;
        }

        public int IndexOfMediaData(MediaPlayerItemSourceUIWrapper other)
        {
            return IndexOfMediaData(other.MediaData);
        }
    }
}
