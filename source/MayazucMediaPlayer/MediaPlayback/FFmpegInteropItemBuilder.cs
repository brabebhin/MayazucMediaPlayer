﻿using FFmpegInteropX;
using MayazucMediaPlayer.AudioEffects;
using MayazucMediaPlayer.Services.MediaSources;
using MayazucMediaPlayer.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.MediaPlayback
{
    internal sealed class FFmpegInteropItemBuilder : IFFmpegInteropMediaSourceProvider<IMediaPlayerItemSource>
    {
        public EqualizerService EqualizerServiceInstance
        {
            get;
            private set;
        }

        public FFmpegInteropItemBuilder(EqualizerService equalizerServiceInstance)
        {
            EqualizerServiceInstance = equalizerServiceInstance;
        }

        public async Task<FFmpegMediaSource> GetFFmpegInteropMssAsync(IMediaPlayerItemSource source, bool createPlaybackItem = true)
        {
            var config = MediaHelperExtensions.GetFFmpegUserConfigs();

            if (source != null)
            {
                FFmpegMediaSource ffmpegInterop = await source.GetFFmpegMediaSourceAsync();

                if (ffmpegInterop.VideoStreams.Count > 0)
                {
                    config.General.FastSeek = true;
                }

                if (ffmpegInterop != null && createPlaybackItem)
                {
                    ffmpegInterop.CreateMediaPlaybackItem();

                    if (SettingsWrapper.EqualizerEnabled && EqualizerServiceInstance != null)
                    {
                        List<AvEffectDefinition> definitions = new List<AvEffectDefinition>(MediaHelperExtensions.GetEqualizerEffectDefinitions(EqualizerServiceInstance.GetCurrentEqualizerConfig()));
                        definitions.AddRange(MediaHelperExtensions.GetAdditionalEffectsDefinitions());
                        ffmpegInterop.SetFFmpegAudioFilters(definitions.GetFFmpegFilterJoinedFilterDef());
                    }
                    return ffmpegInterop;
                }
            }
            return null;
        }

        public static async Task<FFmpegMediaSource> CreateFFmpegInteropMediaSourceFromFileAsync(MediaSourceConfig config, FileInfo file)
        {
            var stream = file.OpenRead().AsRandomAccessStream();
            //don't bother opening a stream with 0 length

            var ffmpegInterop = await FFmpegMediaSource.CreateFromStreamAsync(stream, config);
            //TODO: check equalizer on or off


            return ffmpegInterop;
        }
    }
}
