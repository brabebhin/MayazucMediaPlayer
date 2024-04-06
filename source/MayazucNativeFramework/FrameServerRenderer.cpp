#include "pch.h"
#include "FrameServerRenderer.h"
#include "FrameServerRenderer.g.cpp"


namespace winrt::MayazucNativeFramework::implementation
{
	void FrameServerRenderer::RenderMediaPlayerFrame(winrt::Windows::Media::Playback::MediaPlayer const& player, float width, float height, float dpi, winrt::Windows::Graphics::DirectX::DirectXPixelFormat const& pixelFormat, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration const& effectConfiguration)
	{
		try {
			auto canvasDevice = CanvasDevice::GetSharedDevice();
			effectsPrcessor.EffectConfiguration = effectConfiguration;
			if (canvasSwapChain.Device().IsDeviceLost())
			{
				SwapChainAllocResources(this->swapChainPannel, width, height, dpi, pixelFormat, SubtitleSwapChainBufferCount, canvasSwapChain);
			}

			if (renderingTarget == nullptr || (renderingTarget.Bounds().Width != width) || (renderingTarget.Bounds().Height != height))
			{
				if (renderingTarget)
					renderingTarget.Close();
				
				//TODO: deal with HDR
				renderingTarget = CanvasRenderTarget(canvasDevice, (float)width, (float)height, dpi, pixelFormat, CanvasAlphaMode::Premultiplied);
			}
			if (canvasSwapChain.Format() != pixelFormat || canvasSwapChain.Size().Width != width || canvasSwapChain.Size().Height != height || canvasSwapChain.Dpi() != dpi)
			{
				canvasSwapChain.ResizeBuffers((float)width, (float)height, dpi, pixelFormat, SubtitleSwapChainBufferCount);
			}

			{
				CanvasDrawingSession outputDrawingSession = canvasSwapChain.CreateDrawingSession(winrt::Microsoft::UI::Colors::Transparent());
				player.CopyFrameToVideoSurface(renderingTarget);
				outputDrawingSession.DrawImage(effectsPrcessor.ProcessFrame(renderingTarget));
				outputDrawingSession.Flush();				
				canvasSwapChain.Present(0);
			}
		}
		catch (...)
		{

		}
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
