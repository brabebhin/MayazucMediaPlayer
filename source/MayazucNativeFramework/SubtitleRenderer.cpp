#include "pch.h"
#include "SubtitleRenderer.h"
#include "SubtitleRenderer.g.cpp"
#include <SsaAssTextRenderer.h>

namespace winrt::MayazucNativeFramework::implementation
{
	using namespace winrt::Windows::Media::Core;
	using namespace winrt::Windows::Media::Playback;
	using namespace winrt::Microsoft::Graphics::Canvas;
	using namespace winrt::Microsoft::Graphics::Canvas::UI::Xaml;
	using namespace winrt::Microsoft::Graphics::Canvas::Text;
	using namespace winrt::Microsoft::Graphics::Canvas::Geometry;
	using namespace winrt::Windows::Media::ClosedCaptioning;
	using namespace winrt::Windows::Media;
	using namespace winrt::Microsoft::Graphics::Canvas::Brushes;
	using namespace winrt::Windows::Foundation;
	using namespace winrt::Microsoft::UI;
	using namespace std;

	void SubtitleRenderer::RenderSubtitlesToFrame(winrt::Windows::Media::Playback::MediaPlaybackItem const& playbackItem, uint32_t width, uint32_t height, uint32_t dpi, winrt::Windows::Graphics::DirectX::DirectXPixelFormat const& pixelFormat)
	{
		vector<ImageCue> imageCuesToRender;
		unordered_map<TimedTextRegion, vector<TimedTextCue>> timedTextCuesWithRegions;

		if (width == 0 || height == 0)
			return;

		for (int i = 0; playbackItem && i < playbackItem.TimedMetadataTracks().Size(); i++)
		{
			auto currentTimedTrack = playbackItem.TimedMetadataTracks().GetAt(i);
			//filter out the tracks we don't care about.

			//only accept subtitles or imagesubtitles
			if (currentTimedTrack.TimedMetadataKind() != TimedMetadataKind::Subtitle && currentTimedTrack.TimedMetadataKind() != TimedMetadataKind::ImageSubtitle)
				continue;

			auto presentationMode = playbackItem.TimedMetadataTracks().GetPresentationMode((unsigned int)i);
			//presentation mode must be set to ApplicationPresented
			if (presentationMode != TimedMetadataTrackPresentationMode::ApplicationPresented)
				continue;

			//if the track contains ImageSubtitles, add them to the list to be rendered.
			//image subtitles are rendered at absolute positioning in the output surface
			if (currentTimedTrack.TimedMetadataKind() == TimedMetadataKind::ImageSubtitle)
			{
				for (int j = 0; j < currentTimedTrack.ActiveCues().Size(); j++)
				{
					auto imageCue = currentTimedTrack.ActiveCues().GetAt(j).as<ImageCue>();
					if (imageCue)
					{
						imageCuesToRender.push_back(imageCue);
					}
				}
			}

			//if the track contains text Subtitle(s), group them by region and add them to the list to render
			//text lines in text subtitles are stacked based on the region they belong to
			//regions are rendered similarly to image cues, with absolute positioning.
			if (currentTimedTrack.TimedMetadataKind() == TimedMetadataKind::Subtitle)
			{
				for (int j = 0; j < currentTimedTrack.ActiveCues().Size(); j++)
				{
					auto textCue = currentTimedTrack.ActiveCues().GetAt(j).as<TimedTextCue>();
					if (textCue)
					{
						auto existing = timedTextCuesWithRegions.find(textCue.CueRegion());
						if (existing != timedTextCuesWithRegions.end())
						{
							existing->second.push_back(textCue);
						}
						else
						{
							vector<TimedTextCue> newItem;
							newItem.push_back(textCue);
							timedTextCuesWithRegions.insert_or_assign(textCue.CueRegion(), newItem);
						}
					}
				}
			}
		}

		auto canvasDevice = CanvasDevice::GetSharedDevice();
		if (canvasSwapChain.Device().IsDeviceLost())
		{
			SwapChainAllocResources(this->swapChainPannel, width, height, dpi, pixelFormat, SubtitleSwapChainBufferCount, canvasSwapChain);
		}

		if (canvasSwapChain.Format() != pixelFormat || canvasSwapChain.Size().Width != width || canvasSwapChain.Size().Height != height || canvasSwapChain.Dpi() != dpi)
		{
			canvasSwapChain.ResizeBuffers((float)width, (float)height, dpi, pixelFormat, SubtitleSwapChainBufferCount);
		}

		if (!renderTargetSurface || renderTargetSurface.SizeInPixels().Width != width || renderTargetSurface.SizeInPixels().Height != height)
		{
			renderTargetSurface = CanvasRenderTarget(canvasDevice, width, height, 96, pixelFormat, CanvasAlphaMode::Premultiplied);
		}
		{
			//this is the directx surface we will be rendering all subtitles onto.
			auto renderSurfaceDrawingSession = renderTargetSurface.CreateDrawingSession();
			renderSurfaceDrawingSession.Clear(winrt::Microsoft::UI::Colors::Transparent());

			ICanvasSpriteBatch subtitleOutputSpriteBatch = nullptr;
			if (imageCuesToRender.size() > 0) {
				subtitleOutputSpriteBatch = renderSurfaceDrawingSession.CreateSpriteBatch();
				//start with image cues
				for (int i = 0; i < imageCuesToRender.size(); i++)
				{
					try
					{
						auto imageCue = imageCuesToRender[i];
						auto position = TimedTextPointRelativeToAbsolute(imageCue.Position(), width, height);
						auto extent = TimedTextSizeRelativeToAbsolute(imageCue.Extent(), width, height);

						auto targetPosition = TimedTextPositionSizeToRect(position, extent);
						if (targetPosition.Width == 0 || targetPosition.Height == 0)
						{
							continue;
						}
						subtitleOutputSpriteBatch.Draw(CanvasBitmap::CreateFromSoftwareBitmap(canvasDevice, imageCue.SoftwareBitmap()), targetPosition);
					}
					catch (...) {}
				}
			}

			//move on to textCues
			auto canvasTextFormat = CanvasTextFormat();
			canvasTextFormat.HorizontalAlignment(CanvasHorizontalAlignment::Center);
			canvasTextFormat.FontSize(height * 0.05);

			for (auto regionWithCues : timedTextCuesWithRegions)
			{
				//text can be drawn directly onto the base surface render target

				auto region = regionWithCues.first;
				auto absoluteExtent = TimedTextSizeRelativeToAbsolute(region.Extent(), width, height);
				auto regionW = (float)absoluteExtent.Width;
				auto regionH = (float)absoluteExtent.Height;

				auto regionDrawPoint = TimedTextPointRelativeToAbsolute(region.Position(), width, height);

				//each region has a bunch of cues
				auto cuesInRegion = regionWithCues.second;
				for (int cueIndex = 0; cueIndex < cuesInRegion.size(); cueIndex++)
				{
					auto textCue = cuesInRegion[cueIndex];

					//each cue has a bunch of lines

					//the only supported case so far is alignment LeftRightTopBottom.
					//the others shouldn't be too difficult to support, allignment calculations will be different.
					//the last line is the first to render at the bottom of the region

					//this variable is used to determine where the rendering of the line will take place.
					//for LeftRightTopBottom this is initialized with the maximum Height of the region (i.e. after the last line)
					//it will then be used to compute the actual rendering position of the line based on the line computed height
					auto renderingAvailableHeight = regionH;

					for (int lineIndex = textCue.Lines().Size() - 1; lineIndex >= 0; lineIndex--)
					{
						auto line = textCue.Lines().GetAt(lineIndex);
						auto text = line.Text();
						//each line has a CanvasTextLayout.
						//the requested size will be the full size of the region canvas. the text should not use the whole space.
						auto textLayout = CanvasTextLayout(canvasDevice, text, canvasTextFormat, regionW, regionH);
						textLayout.SetColor(0, text.size(), Microsoft::UI::Colors::White());


						//apply subformats
						for (int subformatIndex = 0; subformatIndex < line.Subformats().Size(); subformatIndex++)
						{
							auto subFormat = line.Subformats().GetAt(subformatIndex);
							//we do not fully support the TimedTextStyle specifications.
							auto subStyle = subFormat.SubformatStyle();
							auto startIndex = subFormat.StartIndex();
							auto length = subFormat.Length();
							if (length == 0) continue;

							textLayout.SetStrikethrough(startIndex, length, subStyle.IsLineThroughEnabled());
							textLayout.SetUnderline(startIndex, length, subStyle.IsUnderlineEnabled());
							textLayout.SetFontStyle(startIndex, length, (Windows::UI::Text::FontStyle)subStyle.FontStyle());

							auto fontWeight = winrt::Windows::UI::Text::FontWeight();
							fontWeight.Weight = (uint16_t)subFormat.SubformatStyle().FontWeight();

							textLayout.SetFontWeight(startIndex, length, fontWeight);
						}

						auto lineRenderHeight = (float)(renderingAvailableHeight - textLayout.DrawBounds().Height - canvasTextFormat.FontSize());
						if (lineRenderHeight <= 0) break;

						auto textRenderer = winrt::make<SsaAssTextRenderer>(renderSurfaceDrawingSession, textCue.CueStyle());
						textLayout.DrawToTextRenderer(textRenderer, 0, lineRenderHeight);

						renderingAvailableHeight = lineRenderHeight; //shrink the available space to render the next line on top of the current line
					}
				}
			}

			if (subtitleOutputSpriteBatch)
				subtitleOutputSpriteBatch.Close();
			renderSurfaceDrawingSession.Flush();

			{
				auto swapChainDrawingSession = canvasSwapChain.CreateDrawingSession(Colors::Transparent());
				swapChainDrawingSession.Clear(winrt::Microsoft::UI::Colors::Transparent());
				swapChainDrawingSession.DrawImage(renderTargetSurface);
				swapChainDrawingSession.Flush();
				canvasSwapChain.Present(0);
			}
		}
	}
}


