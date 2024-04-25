#pragma once
#include "SubtitleRenderer.g.h"
#include <winrt/Microsoft.UI.Xaml.h>
#include <winrt/Microsoft.UI.h>
#include <winrt/Microsoft.Graphics.Canvas.UI.Xaml.h>
#include <winrt/Windows.Graphics.Imaging.h>
#include <winrt/Windows.Media.Playback.h>
#include <winrt/Microsoft.UI.Xaml.Controls.h>
#include <winrt/Microsoft.Graphics.Canvas.h> //This defines the C++/WinRT interfaces for the Win2D Windows Runtime Components
#include <d2d1_3.h>
#include <winrt/Windows.UI.h>
#include <dxgi1_2.h>
#include <microsoft.ui.xaml.media.dxinterop.h>
#include "SwapChainPanelHelper.h"

namespace winrt::MayazucNativeFramework::implementation
{	
	using namespace winrt::Microsoft::Graphics;
	using namespace winrt::Microsoft::Graphics::Canvas;
	using namespace winrt::Microsoft::Graphics::Canvas::UI::Xaml;
	using namespace winrt::Windows::Graphics::Imaging;

	using namespace winrt::Windows::Foundation::Collections;
	using namespace winrt::Windows::Media::Effects;
	using namespace winrt::Windows::Foundation;
	using namespace winrt::Windows::Media::MediaProperties;

	using namespace winrt::Microsoft::Graphics;
	using namespace winrt::Microsoft::Graphics::Canvas;
	using namespace winrt::Microsoft::Graphics::Canvas::Effects;

	using namespace winrt::Windows::Foundation;
	using namespace winrt::Windows::Media::Core;

	struct SubtitleRenderer : SubtitleRendererT<SubtitleRenderer>
	{
		SubtitleRenderer(winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& swapChainPannel)
		{
			SwapChainAllocResources(swapChainPannel, 800, 480, 96, winrt::Windows::Graphics::DirectX::DirectXPixelFormat::B8G8R8A8UIntNormalized, SubtitleSwapChainBufferCount, canvasSwapChain);
			this->swapChainPannel = swapChainPannel;
		}

		void RenderSubtitlesToFrame(winrt::Windows::Media::Playback::MediaPlaybackItem const& playbackItem, float width, float height, float dpi, winrt::Windows::Graphics::DirectX::DirectXPixelFormat const& pixelFormat);

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

	private:
		winrt::Microsoft::Graphics::Canvas::CanvasSwapChain canvasSwapChain = { nullptr };
		winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel swapChainPannel = { nullptr };
		Microsoft::Graphics::Canvas::CanvasRenderTarget renderTargetSurface = { nullptr };

	};
}
namespace winrt::MayazucNativeFramework::factory_implementation
{
	struct SubtitleRenderer : SubtitleRendererT<SubtitleRenderer, implementation::SubtitleRenderer>
	{
	};
}
