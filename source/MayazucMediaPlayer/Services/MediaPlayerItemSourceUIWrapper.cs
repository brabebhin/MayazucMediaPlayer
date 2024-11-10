using CommunityToolkit.WinUI;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.Services.MediaSources;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    /// <summary>
    /// UI wrapper for MediaDataStorage class
    /// </summary>
    public class MediaPlayerItemSourceUIWrapper : ObservableObject
    {
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

        int trackNumber;
        public int TrackNumber
        {
            get => trackNumber;
            set
            {
                if (trackNumber == value) return;

                trackNumber = value;
                NotifyPropertyChanged(nameof(TrackNumber));
            }
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
                }
            }
        }

        public MediaPlayerItemSourceUIWrapper(IMediaPlayerItemSource mds, DispatcherQueue disp)
        {
            Dispatcher = disp;
            MediaData = mds;
            mds.MetadataChanged += Mds_MetadataChanged;
        }

        private async void Mds_MetadataChanged(object? sender, EmbeddedMetadataResult e)
        {
            await Checkmetadata();
        }

        public async Task LoadMediaThumbnailAsync(bool cacheOnly = false)
        {
            await Checkmetadata();
        }

        private async Task Checkmetadata()
        {
            var metadata = await MediaData.GetMetadataAsync();

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

    public class MediaDataStorageUIWrapperCollection : ObservableCollection<MediaPlayerItemSourceUIWrapper>, IFilterableCollection<MediaPlayerItemSourceUIWrapper>
    {
        public MediaDataStorageUIWrapperCollection() : base()
        {
        }
        public MediaDataStorageUIWrapperCollection(IEnumerable<MediaPlayerItemSourceUIWrapper> items) : base(items)
        {
        }

        public IReadOnlyCollection<IMediaPlayerItemSource> ToMediaDataList()
        {
            return this.Select(x => x.MediaData).ToList().AsReadOnly();
        }

        public IEnumerable<MediaPlayerItemSourceUIWrapper> Filter(string filterParam)
        {
            if (string.IsNullOrWhiteSpace(filterParam))
            {
                return this;
            }
            else
            {
                return new ObservableCollection<MediaPlayerItemSourceUIWrapper>(this.Where(x => x.MediaData.Title.IndexOf(filterParam, StringComparison.CurrentCultureIgnoreCase) >= 0));
            }
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

    public class ReadOnlyMediaDataStorageUIWrapperCollection : MediaDataStorageUIWrapperCollection
    {
        public ReadOnlyMediaDataStorageUIWrapperCollection() : base()
        {
        }
        public ReadOnlyMediaDataStorageUIWrapperCollection(IEnumerable<MediaPlayerItemSourceUIWrapper> items) : base(items)
        {
        }
    }
}
