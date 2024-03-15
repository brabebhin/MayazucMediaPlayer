using CommunityToolkit.WinUI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
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
        public void RenderSubtitlesToImage(Microsoft.UI.Xaml.Controls.Image targetImage,
            MediaPlaybackItem mediaPlaybackItem, DispatcherQueue dispatcher)
        {

            float width = (float)targetImage.Width;
            float height = (float)targetImage.Height;


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
                    for (int j = 0; j < currentTimedTrack.ActiveCues.Count; j++)
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
                        if (targetPosition.Width == 0 || targetPosition.Height == 0)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                        renderSurfaceDrawingSession.DrawImage(CanvasBitmap.CreateFromSoftwareBitmap(canvasDevice, imageCue.SoftwareBitmap), targetPosition);
                    }
                    catch { }
                }

                //move to text cues
                //the default text format for now.

                var canvasTextFormat = new CanvasTextFormat()
                {
                    HorizontalAlignment = CanvasHorizontalAlignment.Center
                };
                //this should be ordered by the zIndex
                foreach (var regionWithCues in timedTextCuesWithRegions)
                {
                    //each region is a CanvasRenderTarget

                    var region = regionWithCues.Key;
                    var absoluteExtent = TimedTextSizeRelativeToAbsolute(region.Extent, width, height);
                    var regionW = (float)absoluteExtent.Width;
                    var regionH = (float)absoluteExtent.Height;

                    var regionDrawPoint = TimedTextPointRelativeToAbsolute(region.Position, width, height);
                    using var regionRenderTarget = new CanvasRenderTarget(canvasDevice, regionW, regionH, 96);

                    using var regionRenderTargetDs = regionRenderTarget.CreateDrawingSession();

                    regionRenderTargetDs.Clear(Colors.Transparent); //this should be region.Background
                    //each region has a bunch of cues
                    var cuesInRegion = regionWithCues.Value;
                    for (int cueIndex = 0; cueIndex < cuesInRegion.Count; cueIndex++)
                    {
                        var textCue = cuesInRegion[cueIndex];

                        //each cue has a bunch of lines

                        //the only supported case so far is alignment LeftRightTopBottom.
                        //the others shouldn't be too difficult to support, allignment calculations will be different.
                        //the last line is the first to render at the bottom of the region

                        //this variable is used to determine where the rendering of the line will take place.
                        //for LeftRightTopBottom this is initialized with the maximum Height of the region (i.e. after the last line)
                        //it will then be used to compute the actual rendering position of the line based on the line computed height
                        var renderingAvailableHeight = regionH;

                        for (int lineIndex = textCue.Lines.Count - 1; lineIndex >= 0; lineIndex--)
                        {
                            var line = textCue.Lines[lineIndex];
                            var text = line.Text;
                            //each line has a CanvasTextLayout.
                            //the requested size will be the full size of the region canvas. the text should not use the whole space.
                            var textLayout = new CanvasTextLayout(canvasDevice, text, canvasTextFormat, regionW, regionH);
                            textLayout.SetColor(0, text.Length, Microsoft.UI.Colors.White);
                            //apply subformats
                            for (int subformatIndex = 0; subformatIndex < line.Subformats.Count; subformatIndex++)
                            {
                                var subFormat = line.Subformats[subformatIndex];
                                //we do not fully support the TimedTextStyle specifications.
                                var subStyle = subFormat.SubformatStyle;
                                var startIndex = subFormat.StartIndex;
                                var length = subFormat.Length;
                                if (length == 0) continue;
                                textLayout.SetStrikethrough(startIndex, length, subStyle.IsLineThroughEnabled);
                                textLayout.SetUnderline(startIndex, length, subStyle.IsUnderlineEnabled);
                                textLayout.SetColor(startIndex, length, subStyle.Background);
                                //textLayout.SetFontFamily(startIndex, length, subStyle.FontFamily);
                                //textLayout.SetFontSize(startIndex, length, TimedTextDoubleRelativeToAbsolute(subStyle.FontSize, width, height));
                                textLayout.SetFontStyle(startIndex, length, (Windows.UI.Text.FontStyle)subStyle.FontStyle);
                                textLayout.SetFontWeight(startIndex, length, new Windows.UI.Text.FontWeight((ushort)subFormat.SubformatStyle.FontWeight));
                            }


                            var lineGeometry = Microsoft.Graphics.Canvas.Geometry.CanvasGeometry.CreateText(textLayout);
                            lineGeometry = lineGeometry.Simplify(Microsoft.Graphics.Canvas.Geometry.CanvasGeometrySimplification.Lines);
                            var lineRenderHeight = (float)(renderingAvailableHeight - lineGeometry.ComputeBounds().Height - 10); //the -10 makes the text look nicer
                            regionRenderTargetDs.FillGeometry(lineGeometry, new System.Numerics.Vector2(0, lineRenderHeight), Microsoft.UI.Colors.White);

                            renderingAvailableHeight = lineRenderHeight; //shrink the available space to render the next line on top of the current line

                        }
                    }

                    //done rendering all lines per region

                    //now it is time to draw the region to the main drawing surface
                    regionRenderTargetDs.Flush();
                    var regionDestinationRectangle = new Windows.Foundation.Rect(new Windows.Foundation.Point(regionDrawPoint.X, regionDrawPoint.Y), new Windows.Foundation.Size(regionW, regionH));
                    renderSurfaceDrawingSession.DrawImage(regionRenderTarget, regionDestinationRectangle);
                }

                if (targetImageSource == null || targetImageSource.SizeInPixels.Width != width || targetImageSource.SizeInPixels.Height != height)
                {
                    targetImageSource = new CanvasImageSource(canvasDevice, width, height, 96);
                    targetImage.Source = targetImageSource;
                }
                using var finalSurfaceDS = targetImageSource.CreateDrawingSession(Microsoft.UI.Colors.Transparent);
                finalSurfaceDS.Clear(Colors.Transparent);
                renderSurfaceDrawingSession.Flush();
                finalSurfaceDS.DrawImage(renderTargetSurface);
            }
        }

        private static TimedTextPoint TimedTextPointRelativeToAbsolute(TimedTextPoint point, float width, float height)
        {
            if (point.Unit == TimedTextUnit.Pixels) return point;

            TimedTextPoint returnValue = new TimedTextPoint
            {
                X = point.X * width / 100,
                Y = point.Y * height / 100
            };

            return returnValue;
        }

        public static Windows.Foundation.Rect TimedTextPositionSizeToRect(TimedTextPoint position, TimedTextSize size)
        {
            return new Windows.Foundation.Rect(position.X, position.Y, size.Width, size.Height);
        }

        private static TimedTextSize TimedTextSizeRelativeToAbsolute(TimedTextSize size, float width, float height)
        {
            if (size.Unit == TimedTextUnit.Pixels) return size;

            TimedTextSize returnValue = new TimedTextSize
            {
                Width = size.Width * width / 100,
                Height = size.Height * height / 100
            };

            return returnValue;
        }

        private static float TimedTextDoubleRelativeToAbsolute(TimedTextDouble value, float width, float height)
        {
            if (value.Unit == TimedTextUnit.Pixels) return (float)value.Value;

            return (float)value.Value * width / 100;
        }
    }
}
