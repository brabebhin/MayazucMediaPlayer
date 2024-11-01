using CommunityToolkit.Mvvm.Input;
using FluentResults;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.MediaMetadata;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.Services.MediaSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.FileSystemViews
{
    public class IMediaPlayerItemSourceProvderCollection : ObservableCollection<IMediaPlayerItemSourceProvder>, IFilterableCollection<IMediaPlayerItemSourceProvder>
    {
        public IEnumerable<IMediaPlayerItemSourceProvder> Filter(string filterParam)
        {
            if (string.IsNullOrWhiteSpace(filterParam))
            {
                return this;
            }
            else
            {
                return new ObservableCollection<IMediaPlayerItemSourceProvder>(this.Where(x => x.Path.IndexOf(filterParam, StringComparison.CurrentCultureIgnoreCase) >= 0));
            }
        }
    }

    public class IMediaPlayerItemSourceProviderWithCommands : IMediaPlayerItemSourceProvder, BindingCommandDecorator<FilePickerUiService>
    {
        public IMediaPlayerItemSourceProvder MediaSourceProvider { get; private set; }

        public string DisplayName => MediaSourceProvider.DisplayName;

        public string ImageUri => MediaSourceProvider.ImageUri;

        public Guid ItemID => MediaSourceProvider.ItemID;

        public EmbeddedMetadataResult Metadata => MediaSourceProvider.Metadata;

        public string Path => MediaSourceProvider.Path;

        public bool SupportsMetadata => MediaSourceProvider.SupportsMetadata;

        public FilePickerUiService Commands { get; private set; }

        public event EventHandler<FileInfo> ImageFileChanged
        {
            add
            {
                MediaSourceProvider.ImageFileChanged += value;
            }
            remove
            {
                MediaSourceProvider.ImageFileChanged -= value;
            }
        }

        public event EventHandler<EmbeddedMetadataResult> MetadataChanged
        {
            add
            {
                MediaSourceProvider.MetadataChanged += value;
            }
            remove
            {
                MediaSourceProvider.MetadataChanged -= value;
            }
        }

        public Task<Result<ReadOnlyCollection<IMediaPlayerItemSource>>> GetMediaDataSourcesAsync()
        {
            return MediaSourceProvider.GetMediaDataSourcesAsync();
        }

        public IMediaPlayerItemSourceProviderWithCommands(IMediaPlayerItemSourceProvder mediaSourceProvider, FilePickerUiService commands)
        {
            MediaSourceProvider = mediaSourceProvider;
            Commands = commands;
        }
    }
}
