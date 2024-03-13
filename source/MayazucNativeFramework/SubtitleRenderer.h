#pragma once
#include "SubtitleRenderer.g.h"
#include <winrt/Microsoft.Graphics.Canvas.UI.Xaml.h>
#include <winrt/Windows.UI.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Media.Core.h>

using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Media::Core;

namespace winrt::MayazucNativeFramework::implementation
{
	struct SubtitleRenderer : SubtitleRendererT<SubtitleRenderer>
	{
		SubtitleRenderer() = default;

		void RenderSubtitlesToFrame(winrt::Windows::Media::Playback::MediaPlaybackItem const& playbackItem, winrt::Microsoft::UI::Xaml::Controls::Image const& targetImage);

		Microsoft::Graphics::Canvas::UI::Xaml::CanvasImageSource targetImageSource = { nullptr };
		Microsoft::Graphics::Canvas::CanvasRenderTarget renderTargetSurface = { nullptr };



		static TimedTextPoint TimedTextPointRelativeToAbsolute(TimedTextPoint point, float width, float height)
		{
			if (point.Unit == TimedTextUnit::Pixels) return point;

			TimedTextPoint returnValue = TimedTextPoint();
			returnValue.X = point.X * width / 100;
			returnValue.Y = point.Y * height / 100;

			return returnValue;
		}

		static Rect TimedTextPositionSizeToRect(TimedTextPoint position, TimedTextSize size)
		{
			return Rect(position.X, position.Y, size.Width, size.Height);
		}

		static TimedTextSize TimedTextSizeRelativeToAbsolute(TimedTextSize size, float width, float height)
		{
			if (size.Unit == TimedTextUnit::Pixels) return size;

			TimedTextSize returnValue = TimedTextSize();
			returnValue.Width = size.Width * width / 100,
				returnValue.Height = size.Height * height / 100;

			return returnValue;
		}
	};
}
namespace winrt::MayazucNativeFramework::factory_implementation
{
	struct SubtitleRenderer : SubtitleRendererT<SubtitleRenderer, implementation::SubtitleRenderer>
	{
	};
}
