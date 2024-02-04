using MayazucMediaPlayer.Services.MediaSources;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace MayazucMediaPlayer.MediaPlayback
{
    public static class MediaPlaybackItemDisplayPropertiesHelper
    {
        public static async Task SetPlaybackItemMediaProperties(
            MediaPlaybackItem playbackItem,
            IMediaPlayerItemSource currentPlaybackData)
        {
            var metadata = await currentPlaybackData.GetMetadataAsync();

            var mediaProperties = playbackItem.GetDisplayProperties();

            try
            {
                if (playbackItem.VideoTracks.Any())
                {
                    if (mediaProperties.Type != MediaPlaybackType.Video)
                        mediaProperties.Type = MediaPlaybackType.Video;

                    if (mediaProperties.VideoProperties.Title != metadata.Title)
                        mediaProperties.VideoProperties.Title = metadata.Title;

                    mediaProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(FontIconPaths.PlaceholderAlbumArt, UriKind.Absolute));

                    if (playbackItem.AutoLoadedDisplayProperties != AutoLoadedDisplayPropertyKind.Video)
                        playbackItem.AutoLoadedDisplayProperties = AutoLoadedDisplayPropertyKind.Video;
                }
                else
                {
                    if (mediaProperties.Type != MediaPlaybackType.Music)
                        mediaProperties.Type = MediaPlaybackType.Music;

                    if (playbackItem.AutoLoadedDisplayProperties != AutoLoadedDisplayPropertyKind.Music)
                        playbackItem.AutoLoadedDisplayProperties = AutoLoadedDisplayPropertyKind.Music;

                    if (mediaProperties.MusicProperties.AlbumTitle != metadata.Album)
                        mediaProperties.MusicProperties.AlbumTitle = metadata.Album;

                    if (mediaProperties.MusicProperties.Artist != metadata.Artist)
                        mediaProperties.MusicProperties.Artist = metadata.Artist;

                    if (mediaProperties.MusicProperties.Title != metadata.Title)
                        mediaProperties.MusicProperties.Title = metadata.Title;

                    mediaProperties.Thumbnail = await currentPlaybackData.GetThumbnailStreamAsync();
                }
            }
            catch { }
            finally
            {
                playbackItem.ApplyDisplayProperties(mediaProperties);
            }
        }

        public static void CopyPropertiesFrom(this SystemMediaTransportControlsDisplayUpdater target, MediaPlaybackItem sourceItem)
        {
            var source = sourceItem.GetDisplayProperties();

            target.Type = source.Type;
            if (sourceItem.AutoLoadedDisplayProperties == AutoLoadedDisplayPropertyKind.Music)
            {
                target.Type = MediaPlaybackType.Music;
                target.MusicProperties.AlbumTitle = source.MusicProperties.AlbumTitle;
                target.MusicProperties.Artist = source.MusicProperties.Artist;
                target.MusicProperties.Title = source.MusicProperties.Title;
            }
            else if (sourceItem.AutoLoadedDisplayProperties == AutoLoadedDisplayPropertyKind.Video)
            {
                target.Type = MediaPlaybackType.Video;
                target.VideoProperties.Title = source.VideoProperties.Title;
            }
            target.Thumbnail = source.Thumbnail;

        }
    }
}
