﻿using CommunityToolkit.WinUI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace MayazucMediaPlayer.Tests
{
    internal class Win2DSubtitleRenderer
    {
        CanvasImageSource targetImageSource;
        CanvasRenderTarget renderTargetSurface;
        public async void RenderSubtitlesToImage(Microsoft.UI.Xaml.Controls.Image targetImage,
            MediaPlaybackItem mediaPlaybackItem, DispatcherQueue dispatcher)
        {

            float width = (float)targetImage.Width;
            float height = (float)targetImage.Height;

            _ = Task.Run(async () =>
            {
                List<ImageCue> imageCuesToRender = new List<ImageCue>();
                Dictionary<TimedTextRegion, List<TimedTextCue>> timedTextCuesWithRegions = new Dictionary<TimedTextRegion, List<TimedTextCue>>();

                for (int i = 0; i < mediaPlaybackItem.TimedMetadataTracks.Count; i++)
                {
                    var currentTimedTrack = mediaPlaybackItem.TimedMetadataTracks[i];
                    //filter out the tracks we don't care about.

                    //only accept subtitles or imagesubtitles
                    if (currentTimedTrack.TimedMetadataKind != Windows.Media.Core.TimedMetadataKind.Subtitle && currentTimedTrack.TimedMetadataKind != Windows.Media.Core.TimedMetadataKind.ImageSubtitle)
                        continue;

                    var presentationMode = mediaPlaybackItem.TimedMetadataTracks.GetPresentationMode((uint)i);
                    //presentation mode must be set to ApplicationPresented
                    if (presentationMode != TimedMetadataTrackPresentationMode.ApplicationPresented)
                        continue;

                    //if the track contains ImageSubtitles, add them to the list to be rendered.
                    //image subtitles are rendered at absolute positioning in the output surface
                    if (currentTimedTrack.TimedMetadataKind == TimedMetadataKind.ImageSubtitle)
                    {
                        for (int j = 0; j < currentTimedTrack.ActiveCues.Count; j++)
                        {
                            var imageCue = currentTimedTrack.ActiveCues[j] as ImageCue;
                            if (imageCue != null)
                            {
                                imageCuesToRender.Add(imageCue);
                            }
                        }
                    }

                    //if the track contains text Subtitle(s), group them by region and add them to the list to render
                    //text lines in text subtitles are stacked based on the region they belong to
                    //regions are rendered similarly to image cues, with absolute positioning.
                    if (currentTimedTrack.TimedMetadataKind == TimedMetadataKind.Subtitle)
                    {
                        for (int j = 0; i < currentTimedTrack.ActiveCues.Count; j++)
                        {
                            var textCue = currentTimedTrack.ActiveCues[j] as TimedTextCue;
                            if (textCue != null)
                            {

                                if (timedTextCuesWithRegions.ContainsKey(textCue.CueRegion))
                                {
                                    timedTextCuesWithRegions[textCue.CueRegion].Add(textCue);
                                }
                                else
                                {
                                    timedTextCuesWithRegions.Add(textCue.CueRegion, new List<TimedTextCue>() { textCue });
                                }
                            }
                        }
                    }
                }

                using var canvasDevice = CanvasDevice.GetSharedDevice();
                if (renderTargetSurface == null || renderTargetSurface.SizeInPixels.Width != width || renderTargetSurface.SizeInPixels.Height != height)
                {
                    renderTargetSurface = new CanvasRenderTarget(canvasDevice, width, height, 96);
                }
                //this is the directx surface we will be rendering all subtitles onto.
                using (var renderSurfaceDrawingSession = renderTargetSurface.CreateDrawingSession())
                {
                    renderSurfaceDrawingSession.Clear(Colors.Transparent);

                    //start with image cues
                    for (int i = 0; i < imageCuesToRender.Count; i++)
                    {
                        try
                        {
                            var imageCue = imageCuesToRender[i];
                            var position = TimedTextPointRelativeToAbsolute(imageCue.Position, width, height);
                            var extent = TimedTextSizeRelativeToAbsolute(imageCue.Extent, width, height);

                            Windows.Foundation.Rect targetPosition = TimedTextPositionSizeToRect(position, extent);

                            using var canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(canvasDevice, imageCue.SoftwareBitmap);
                            renderSurfaceDrawingSession.DrawImage(canvasBitmap, targetPosition);
                        }
                        catch { }
                    }

                    //move to text cues
                }                            

                await dispatcher.EnqueueAsync(() =>
                {
                    if (targetImageSource == null || targetImageSource.SizeInPixels.Width != width || targetImageSource.SizeInPixels.Height != height)
                    {
                        targetImageSource = new CanvasImageSource(canvasDevice, width, height, 96);
                    }
                    using var finalSurfaceDS = targetImageSource.CreateDrawingSession(Microsoft.UI.Colors.Transparent);
                    finalSurfaceDS.Clear(Colors.Transparent);
                    finalSurfaceDS.DrawImage(renderTargetSurface);
                    targetImage.Source = targetImageSource;
                });
            });
        }

        private static TimedTextPoint TimedTextPointRelativeToAbsolute(TimedTextPoint point, float width, float height)
        {
            if (point.Unit == TimedTextUnit.Pixels) return point;

            TimedTextPoint returnValue = new TimedTextPoint();

            returnValue.X = point.X * width / 100;
            returnValue.Y = point.Y * height / 100;

            return returnValue;
        }

        public static Windows.Foundation.Rect TimedTextPositionSizeToRect(TimedTextPoint position, TimedTextSize size)
        {
            return new Windows.Foundation.Rect(position.X, position.Y, size.Width, size.Height);
        }

        private static TimedTextSize TimedTextSizeRelativeToAbsolute(TimedTextSize size, float width, float height)
        {
            if (size.Unit == TimedTextUnit.Pixels) return size;

            TimedTextSize returnValue = new TimedTextSize();

            returnValue.Width = size.Width * width / 100;
            returnValue.Height = size.Height * height / 100;

            return returnValue;
        }
    }
}