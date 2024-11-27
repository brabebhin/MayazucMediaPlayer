#include "pch.h"
#include "MediaFoundationSubtitleRenderer.h"
#include "MediaFoundationSubtitleRenderer.g.cpp"

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

	void MediaFoundationSubtitleRenderer::RenderSubtitlesToFrame(winrt::Windows::Media::Playback::MediaPlayer const& player, uint32_t width, uint32_t height, uint32_t dpi, winrt::Windows::Graphics::DirectX::DirectXPixelFormat const& pixelFormat)
	{
		try {
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
				auto renderSurfaceDS = renderTargetSurface.CreateDrawingSession();
				renderSurfaceDS.Clear(winrt::Microsoft::UI::Colors::Transparent());
				renderSurfaceDS.Close();
				CanvasDrawingSession outputDrawingSession = canvasSwapChain.CreateDrawingSession(winrt::Microsoft::UI::Colors::Transparent());
				player.RenderSubtitlesToSurface(renderTargetSurface);
				outputDrawingSession.DrawImage(renderTargetSurface);
				outputDrawingSession.Flush();
				canvasSwapChain.Present(0);
			}
		}
		catch (...)
		{

		}
	}
}
