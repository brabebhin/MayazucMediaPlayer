#include "pch.h"
#include "FrameServerRenderer.h"
#include "FrameServerRenderer.g.cpp"
#include <mfmediaengine.h>

namespace winrt::MayazucNativeFramework::implementation
{
	void FrameServerRenderer::RenderMediaPlayerFrame(winrt::Windows::Media::Playback::MediaPlayer const& player, uint32_t width, uint32_t height, uint32_t dpi, winrt::Windows::Graphics::DirectX::DirectXPixelFormat const& pixelFormat, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration const& effectConfiguration)
	{
		try {
			auto canvasDevice = CanvasDevice::GetSharedDevice();
			effectsPrcessor.EffectConfiguration = effectConfiguration;
			if (canvasSwapChain.Device().IsDeviceLost())
			{
				SwapChainAllocResources(this->swapChainPannel, width, height, dpi, pixelFormat, SubtitleSwapChainBufferCount, canvasSwapChain);
			}

		/*	if (renderingTarget == nullptr || (renderingTarget.Bounds().Width != width) || (renderingTarget.Bounds().Height != height))
			{
				if (renderingTarget)
					renderingTarget.Close();

			}*/

			if (canvasSwapChain.Format() != pixelFormat || (uint32_t)canvasSwapChain.Size().Width != (uint32_t)width || (uint32_t)canvasSwapChain.Size().Height != (uint32_t)height || (uint32_t)canvasSwapChain.Dpi() != (uint32_t)dpi)
			{
				canvasSwapChain.ResizeBuffers(width, height, dpi, pixelFormat, SubtitleSwapChainBufferCount);
			}
			{
			
				//renderSurfaceDS.Close();
				
				CanvasDrawingSession outputDrawingSession = canvasSwapChain.CreateDrawingSession(winrt::Microsoft::UI::Colors::Transparent());
				
				renderingTarget = CanvasRenderTarget(outputDrawingSession, width, height, dpi, pixelFormat, CanvasAlphaMode::Premultiplied);
				//auto renderSurfaceDS = renderingTarget.CreateDrawingSession();
				//renderSurfaceDS.Clear(winrt::Microsoft::UI::Colors::Transparent());

				
				player.CopyFrameToVideoSurface(renderingTarget);
				
				
				auto effectImage = effectsPrcessor.ProcessFrame(renderingTarget);
				outputDrawingSession.DrawImage(effectImage);

				//RefreshSubtitleInternal(player, width, height, dpi, canvasDevice);

				if (subtitleRenderingTarget)
				{
					outputDrawingSession.DrawImage(subtitleRenderingTarget);
				}
				//outputDrawingSession.Flush();
				canvasSwapChain.Present();
			}
		}
		catch (...)
		{

		}
	}

	void FrameServerRenderer::RefreshSubtitle(winrt::Windows::Media::Playback::MediaPlayer const& player, uint32_t width, uint32_t height, uint32_t dpi)
	{
		RefreshSubtitleInternal(player, width, height, dpi, CanvasDevice::GetSharedDevice());
	}

	winrt::Windows::Foundation::IAsyncAction FrameServerRenderer::RenderMediaPlayerFrameToStreamAsync(winrt::Windows::Media::Playback::MediaPlayer player, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration effectConfiguration, winrt::Windows::Storage::Streams::IRandomAccessStream outputStream)
	{
		winrt::apartment_context caller; // Capture calling context.

		CanvasDevice canvasDevice = CanvasDevice::GetSharedDevice();

		//TODO: deal with HDR
		auto frameServerDest = SoftwareBitmap(BitmapPixelFormat::Bgra8, player.PlaybackSession().NaturalVideoWidth(), player.PlaybackSession().NaturalVideoHeight(), BitmapAlphaMode::Ignore);
		auto canvasImageSource = Microsoft::Graphics::Canvas::CanvasRenderTarget(canvasDevice, player.PlaybackSession().NaturalVideoWidth(), player.PlaybackSession().NaturalVideoHeight(), 96);

		auto inputBitmap = CanvasBitmap::CreateFromSoftwareBitmap(canvasDevice, frameServerDest);
		auto ds = canvasImageSource.CreateDrawingSession();

		player.CopyFrameToVideoSurface(inputBitmap);

		ds.DrawImage(effectsPrcessor.ProcessFrame(inputBitmap));
		ds.Flush();

		co_await canvasImageSource.SaveAsync(outputStream, CanvasBitmapFileFormat::Png);
	}
}
