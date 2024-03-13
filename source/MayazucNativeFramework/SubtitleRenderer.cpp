#include "pch.h"
#include "SubtitleRenderer.h"
#include "SubtitleRenderer.g.cpp"
#include <vector>
#include <winrt/Windows.Media.Core.h>
#include <winrt/Windows.Media.Playback.h>
#include <winrt/Microsoft.UI.h>
#include <winrt/Microsoft.Graphics.Canvas.UI.Xaml.h>
#include <map>
using namespace std;

namespace winrt::MayazucNativeFramework::implementation
{
	using namespace winrt::Windows::Media::Core;
	using namespace winrt::Windows::Media::Playback;
	using namespace winrt::Microsoft::Graphics::Canvas;
	using namespace winrt::Microsoft::Graphics::Canvas::UI::Xaml;

	void SubtitleRenderer::RenderSubtitlesToFrame(winrt::Windows::Media::Playback::MediaPlaybackItem const& playbackItem, winrt::Microsoft::UI::Xaml::Controls::Image const& targetImage)
	{
		float width = (float)targetImage.Width();
		float height = (float)targetImage.Height();

		vector<ImageCue> imageCuesToRender;
		unordered_map<TimedTextRegion, vector<TimedTextCue>> timedTextCuesWithRegions;

		for (int i = 0; i < playbackItem.TimedMetadataTracks().Size(); i++)
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
		if (!renderTargetSurface || renderTargetSurface.SizeInPixels().Width != width || renderTargetSurface.SizeInPixels().Height != height)
		{
			renderTargetSurface = CanvasRenderTarget(canvasDevice, width, height, 96);
		}
		{
			//this is the directx surface we will be rendering all subtitles onto.
			auto renderSurfaceDrawingSession = renderTargetSurface.CreateDrawingSession();

			renderSurfaceDrawingSession.Clear(winrt::Microsoft::UI::Colors::Transparent());

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
						
					}
					renderSurfaceDrawingSession.DrawImage(CanvasBitmap::CreateFromSoftwareBitmap(canvasDevice, imageCue.SoftwareBitmap()), targetPosition);
				}
				catch(...) { }
			}

			renderSurfaceDrawingSession.Flush();
			if (!targetImageSource || targetImageSource.SizeInPixels().Width != width || targetImageSource.SizeInPixels().Height != height)
			{
				targetImageSource = CanvasImageSource(canvasDevice, width, height, 96);
				targetImage.Source(targetImageSource);
			}
			{
				auto finalSurfaceDS = targetImageSource.CreateDrawingSession(winrt::Microsoft::UI::Colors::Transparent());
				finalSurfaceDS.Clear(winrt::Microsoft::UI::Colors::Transparent());
				finalSurfaceDS.DrawImage(renderTargetSurface);
			}
		}
	}
}
