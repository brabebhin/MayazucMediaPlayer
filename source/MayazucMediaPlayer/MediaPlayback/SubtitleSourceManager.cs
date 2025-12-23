using FFmpegInteropX;
using MayazucMediaPlayer.FileSystemViews;
using MayazucMediaPlayer.Notifications;
using MayazucMediaPlayer.Settings;
using MayazucMediaPlayer.Subtitles.OnlineAPIs.OpenSubtitles;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

namespace MayazucMediaPlayer.MediaPlayback
{
    public class SubtitleSourceManager
    {
        private readonly AsyncLock operationLock = new AsyncLock();
        readonly TimedMetadataTrackPresentationMode defaultPresentationMode = Constants.DefaultSubtitlePresentationMode;
        public MediaPlaybackItem PlaybackItem
        {
            get;
            private set;
        }

        public SubtitleSourceManager(MediaPlaybackItem item)
        {
            PlaybackItem = item;
        }

        public async Task OpenSubtitleFile(IFileOpenPicker subtitlePicker)
        {
            using (await operationLock.LockAsync())
            {
                try
                {
                    var externalFile = await subtitlePicker.PickSingleFileAsync();
                    await ParseExternalSubtitleFile(externalFile, PlaybackItem);
                }
                catch { }
            }
        }

        public async Task<bool> DownloadSubtitleAsync(IOpenSubtitlesAgent SubtitlesAgent, SubtitleRequest request)
        {
            using (await operationLock.LockAsync())
            {
                if (PlaybackItem.IsVideo())
                {
                    var file = await SubtitlesAgent.AutoDownloadSubtitleAsync(request);
                    if (file != null)
                    {
                        try
                        {
                            await ParseAndSetSubtitleInternal(file);

                            return true;
                        }
                        catch
                        {
                            MiscNotificationGenerator.ShowToast("Could not load subtitle. Manual management of subtitles is required");
                        }
                    }
                }
                return false;
            }
        }

        public async Task ParseAndSetSubtitle(FileInfo subFile)
        {
            using (await operationLock.LockAsync())
            {
                await ParseAndSetSubtitleInternal(subFile);
            }
        }

        private async Task ParseAndSetSubtitleInternal(FileInfo subFile)
        {
            if (subFile != null)
            {
                await ParseExternalSubtitleFile(subFile, PlaybackItem);
            }
        }

        private async Task<FileInfo> GetExternalSubtitleFileAsync(string filePath)
        {
            try
            {
                var file = new FileInfo(filePath);
                if (file != null)
                {
                    var parent = file.Directory;
                    if (parent != null)
                    {
                        var thisFileName = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        List<Task<IStorageItem>> subLookupTasks = new List<Task<IStorageItem>>();
                        foreach (var ssf in SupportedFileFormats.SupportedSubtitleFormats)
                        {
                            var fileEnum = parent.EnumerateFiles($"{thisFileName}.*").FirstOrDefault(x => x.Extension.Equals(ssf, StringComparison.InvariantCultureIgnoreCase));
                            if (fileEnum != null)
                            {
                                return fileEnum;
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            return null;
        }

        public async Task PrepareSubtitles(SubtitleRequest request)
        {
            if (!request.Supported) return;
            using (await operationLock.LockAsync())
            {
                var playbackItemExtradata = PlaybackItem.GetExtradata();

                try
                {
                    if (SettingsService.Instance.AutoDetectExternalSubtitle)
                    {
                        var subFile = await GetExternalSubtitleFileAsync(request.FullMediaLocation);
                        if (subFile != null)
                        {
                            await ParseAndSetSubtitleInternal(subFile);
                        }
                        else
                        {
                            //we have no external subtitle file, look for an internal one.
                            if (SettingsService.Instance.AutoloadInternalSubtitle)
                            {
                                await LookupInternalSubtitle(PlaybackItem);
                            }
                        }
                    }
                    else if (SettingsService.Instance.AutoloadInternalSubtitle)
                    {
                        await LookupInternalSubtitle(PlaybackItem);
                    }
                }
                catch
                {
                    if (!playbackItemExtradata.Disposed)
                        MiscNotificationGenerator.ShowToast("Could not auto load subtitle. Make sure your file is part of the videos library and that subtitle files are valid");
                }
            }
        }

        private async Task<bool> LookupInternalSubtitle(MediaPlaybackItem PlaybackItem)
        {
            var subs = PlaybackItem.TimedMetadataTracks.Where(x => x.IsSubtitle()).ToList();
            var subsDictionary = PlaybackItem.GetExtradata().FFmpegMediaSource.SubtitleStreams.ToDictionary(x => x.SubtitleTrack, y => y.IsForced);
            int index = 0;
            TimedMetadataTrack internalSub = null;
            if (subs.Count == 1)
            {
                internalSub = subs[0];
            }
            else
            if (subs.Count > 1)
            {
                for (index = 0; index < subs.Count; index++)
                {
                    if (FFmpegInteropXExtensions.CheckSubtitlelanguage(subs[index], SettingsService.Instance.PreferredSubtitleLanguage.LanguageName))
                    {
                        if (subsDictionary[subs[index]] == false)
                        {
                            internalSub = subs[index];
                            break;
                        }
                    }
                }
            }

            if (internalSub != null)
            {
                if (PlaybackItem.TimedMetadataTracks.Count > index)
                    PlaybackItem.TimedMetadataTracks.SetPresentationMode((uint)index, defaultPresentationMode);

                //mediaPlaybackItem.TimedMetadataTracks.SetPresentationMode((uint)index, TimedMetadataTrackPresentationMode.PlatformPresented);
                return true;
            }
            else
            {
                return false;
                //no internal one found, look online if allowed
            }
        }

        private async Task ParseExternalSubtitleFile(FileInfo externalFile, MediaPlaybackItem CurrentPlaybackItem)
        {
            if (externalFile == null)
            {
                return;
            }
            var stream = externalFile.OpenRead().AsRandomAccessStream();

            try
            {
                var extradata = CurrentPlaybackItem.GetExtradata();

                if (extradata.FFmpegMediaSource != null)
                {
                    extradata.FFmpegMediaSource.Configuration.Subtitles.ExternalSubtitleEncoding = CharacterEncoding.AllEncodings[SettingsService.Instance.FFmpegCharacterEncodingIndex];
                    extradata.FFmpegMediaSource.Configuration.Subtitles.ExternalSubtitleAnsiEncoding = CharacterEncoding.AllEncodings[SettingsService.Instance.FFmpegCharacterEncodingIndex];
                    var externalTracks = await extradata.FFmpegMediaSource.AddExternalSubtitleAsync(stream, externalFile.Name);
                    if (externalTracks != null)
                    {
                        var index = CurrentPlaybackItem.TimedMetadataTracks.IndexOf(externalTracks.FirstOrDefault().SubtitleTrack);
                        if (index != -1)
                        {
                            if (CurrentPlaybackItem.TimedMetadataTracks.Count > index)
                                CurrentPlaybackItem.TimedMetadataTracks.SetPresentationMode((uint)index, defaultPresentationMode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MiscNotificationGenerator.ShowToast("Could not load subtitle. Manual management of subtitles is required. " + ex.Message);
            }
        }
    }

    public class SubtitleRequest
    {
        public bool Supported { get; private set; }
        public string FullMediaLocation { get; private set; }
        public string FileHash { get; private set; }

        public string FileName { get; private set; }

        public SubtitleRequest(bool supported, string fullMediaLocation, string fileHash)
        {
            Supported = supported;
            FullMediaLocation = fullMediaLocation;
            FileHash = fileHash;
            FileName = Path.GetFileName(fullMediaLocation);
        }
    }
}
