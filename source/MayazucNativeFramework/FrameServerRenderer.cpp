#include "pch.h"
#include "FrameServerRenderer.h"
#include "FrameServerRenderer.g.cpp"


namespace winrt::MayazucNativeFramework::implementation
{


	void FrameServerRenderer::RenderMediaPlayerFrame(winrt::Windows::Media::Playback::MediaPlayer const& player,
		winrt::Microsoft::UI::Xaml::Controls::Image const& targetImage,
		winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration const& effectConfiguration)
	{
		try {
			auto canvasDevice = CanvasDevice::GetSharedDevice();
			effectsPrcessor.EffectConfiguration = effectConfiguration;

			//it appears the entire rendering can be done with a CanvasRenderTarget
			//still need to test HDR, and if the frame comes too early or too late. 
			//the CanvasRenderTarget can also be cached so it is not recreated every frame

			if (renderingTarget == nullptr || (renderingTarget.Bounds().Width != targetImage.Width()) || (renderingTarget.Bounds().Height != targetImage.Height()))
			{
				if (renderingTarget)
					renderingTarget.Close();
				if (subtitlesTarget)
					subtitlesTarget.Close();
				//TODO: deal with HDR
				renderingTarget = CanvasRenderTarget(canvasDevice, (float)targetImage.Width(), (float)targetImage.Height(), 96);
			}
			if (win2dImageSource == nullptr
				|| (win2dImageSource.Size().Width != targetImage.Width())
				|| (win2dImageSource.Size().Height != targetImage.Height()))
			{
				win2dImageSource = CanvasImageSource(canvasDevice, (int)targetImage.Width(), (int)targetImage.Height(), 96);
			}
			if (targetImage.Source()!= win2dImageSource)
			{
				targetImage.Source(win2dImageSource);
			}

			{
				auto lock = canvasDevice.Lock();
				CanvasDrawingSession outputDrawingSession = win2dImageSource.CreateDrawingSession(winrt::Microsoft::UI::Colors::Transparent());
				CanvasDrawingSession videoDrawingSession = renderingTarget.CreateDrawingSession();
				videoDrawingSession.Clear(winrt::Microsoft::UI::Colors::Transparent());
				player.CopyFrameToVideoSurface(renderingTarget);
				outputDrawingSession.DrawImage(effectsPrcessor.ProcessFrame(renderingTarget));
				outputDrawingSession.Flush();
			}
		}
		catch (...)
		{

		}
	}

	void FrameServerRenderer::RenderMediaPlayerFrame(winrt::Windows::Media::Playback::MediaPlayer const& player, float width, float height, float dpi, winrt::Windows::Graphics::DirectX::DirectXPixelFormat const& pixelFormat, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration const& effectConfiguration)
	{
		try {
			auto canvasDevice = CanvasDevice::GetSharedDevice();
			effectsPrcessor.EffectConfiguration = effectConfiguration;

			if (renderingTarget == nullptr || (renderingTarget.Bounds().Width != width) || (renderingTarget.Bounds().Height != height))
			{
				if (renderingTarget)
					renderingTarget.Close();
				if (subtitlesTarget)
					subtitlesTarget.Close();
				//TODO: deal with HDR
				renderingTarget = CanvasRenderTarget(canvasDevice, (float)width, (float)height, dpi);
				canvasSwapChain.ResizeBuffers((float)width, (float)height, dpi, pixelFormat, 2);
			}			

			{
				auto lock = canvasDevice.Lock();
				CanvasDrawingSession outputDrawingSession = canvasSwapChain.CreateDrawingSession(winrt::Microsoft::UI::Colors::Transparent());
				player.CopyFrameToVideoSurface(renderingTarget);
				outputDrawingSession.DrawImage(effectsPrcessor.ProcessFrame(renderingTarget));
				outputDrawingSession.Flush();
				canvasSwapChain.Present();
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
