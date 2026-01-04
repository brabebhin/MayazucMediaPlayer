using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MayazucMediaPlayer.Controls
{
    public interface IPlaybackItemManagementUIService : INotifyPropertyChanged
    {
        IAsyncRelayCommand<object> AddSingleFileToPlaylistCommand { get; }
        IAsyncRelayCommand<object> AddToExistingPlaylistCommand { get; }
        IAsyncRelayCommand<object> AddToExistingPlaylistCommandOnlyMusic { get; }
        IAsyncRelayCommand<object> AddToExistingPlaylistCommandOnlySelected { get; }
        IAsyncRelayCommand<object> AddToExistingPlaylistCommandOnlyUnselected { get; }
        IAsyncRelayCommand<object> AddToExistingPlaylistCommandOnlyVideo { get; }
        IAsyncRelayCommand<object> AddToNowPlayingCommand { get; }
        IAsyncRelayCommand<object> AddToNowPlayingCommandOnlyAudio { get; }
        IAsyncRelayCommand<object> AddToNowPlayingCommandOnlySelected { get; }
        IAsyncRelayCommand<object> AddToNowPlayingCommandOnlyVideo { get; }
        bool CanReorderItems { get; set; }
        bool CanSearch { get; set; }
        IRelayCommand<object> ChangeSongOrderRequestCommand { get; }
        IAsyncRelayCommand<object> ClearAllCommand { get; }
        IRelayCommand<object> CopyAlbum { get; }
        IAsyncRelayCommand<object> CopyFileToClipboard { get; }
        IAsyncRelayCommand<object> CopySingleFileToFolder { get; }
        IRelayCommand EnableSelection { get; }
        bool EnqueueButtonIsEnabled { get; set; }
        IAsyncRelayCommand<object> EnqueueSingleFileCommand { get; }
        AdvancedCollectionView FilterCollectionView { get; }
        IAsyncRelayCommand<object> GoToNetworkPlaybackCommand { get; }
        IAsyncRelayCommand<object> GoToSingleItemPropertiesCommand { get; }
        bool IsChangingOrder { get; set; }
        bool IsReorderButtonEnabled { get; }
        Visibility NotSelectingPlayButtonsVisibility { get; }
        bool NpChangeOrderButton { get; set; }
        bool PlayButtonIsEnabled { get; set; }
        IAsyncRelayCommand<object> PlayCommand { get; }
        IAsyncRelayCommand<object> PlayCommandOnlyMusicFiles { get; }
        IAsyncRelayCommand<object> PlayCommandOnlySelected { get; }
        IAsyncRelayCommand<object> PlayCommandOnlyVideoFiles { get; }
        PlaylistsService PlaylistsService { get; }
        IAsyncRelayCommand<object> PlayNextCommand { get; }
        IAsyncRelayCommand<object> PlayNextSingleFileCommand { get; }
        IAsyncRelayCommand<object> PlaySingleFileCommand { get; }
        IAsyncRelayCommand<object> PlayStartingFromFileCommand { get; }
        IAsyncRelayCommand RemoveOnlyMusicCommand { get; }
        IAsyncRelayCommand RemoveOnlyVideoCommand { get; }
        IAsyncRelayCommand RemoveSelectedCommand { get; }
        IRelayCommand<object> RemoveSlidedItem { get; }
        ListViewReorderMode ReorderMode { get; set; }
        IAsyncRelayCommand<object> SaveAsPlaylistCommand { get; }
        IAsyncRelayCommand<object> SaveAsPlaylistCommandOnlyMusic { get; }
        IAsyncRelayCommand<object> SaveAsPlaylistCommandOnlySelected { get; }
        IAsyncRelayCommand<object> SaveAsPlaylistCommandOnlyunselected { get; }
        IAsyncRelayCommand<object> SaveAsPlaylistCommandOnlyVideo { get; }
        IRelayCommand<object> SelectAllCommandOnlyAudio { get; }
        IRelayCommand<object> SelectAllCommandOnlyVideo { get; }
        IRelayCommand<object> SelectAllCommandSelected { get; }
        Visibility SelectingPlayButtonsVisibility { get; }
        ListViewSelectionMode SelectionMode { get; set; }
        IRelayCommand<object> SingleItemCopyArtist { get; }
        IRelayCommand<object> SingleItemCopyFileName { get; }
        IRelayCommand<object> SingleItemCopyFilePath { get; }
        IRelayCommand<object> SingleItemCopyGenre { get; }
        IRelayCommand<object> UnselectAllCommand { get; }

        int ItemsCount { get; }
        bool CommandBarEnabled { get; }

        event EventHandler<List<object>> GetSelectedItemsRequest;
        event EventHandler<IEnumerable<ItemIndexRange>> SetSelectedItems;
    }
}