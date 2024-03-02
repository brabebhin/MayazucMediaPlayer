#include "pch.h"
#include "FrameServerRenderer.h"
#include "FrameServerRenderer.g.cpp"
#include <winrt/Microsoft.Graphics.Canvas.UI.Xaml.h>
#include <winrt/Windows.UI.h>

namespace winrt::MayazucNativeFramework::implementation
{
	void FrameServerRenderer::RenderMediaPlayerFrame(winrt::Windows::Media::Playback::MediaPlayer const& player,
		winrt::Microsoft::UI::Xaml::Controls::Image const& targetImage,
		winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration const& effectConfiguration)
	{
		try {
			auto canvasDevice = CanvasDevice::GetSharedDevice();
			effectsPrcessor.EffectConfiguration = effectConfiguration;

			if (frameServerImageSource == nullptr || (frameServerImageSource.PixelWidth() != targetImage.Width()) || (frameServerImageSource.PixelHeight() != targetImage.Height()))
			{
				if (frameServerImageSource)
					frameServerImageSource.Close();
				frameServerImageSource = SoftwareBitmap(BitmapPixelFormat::Bgra8, (int)targetImage.Width(), (int)targetImage.Height(), BitmapAlphaMode::Ignore);
			}
			if (win2dImageSource == nullptr
				|| (win2dImageSource.Size().Width != targetImage.Width())
				|| (win2dImageSource.Size().Height != targetImage.Height()))
			{
				win2dImageSource = CanvasImageSource(canvasDevice, (int)targetImage.Width(), (int)targetImage.Height(), 96);
			}

			{
				auto lock = canvasDevice.Lock();
				CanvasBitmap inputBitmap = CanvasBitmap::CreateFromSoftwareBitmap(canvasDevice, frameServerImageSource);
				CanvasDrawingSession ds = win2dImageSource.CreateDrawingSession(winrt::Microsoft::UI::Colors::Transparent());
				player.CopyFrameToVideoSurface(inputBitmap);
				ds.DrawImage(effectsPrcessor.ProcessFrame(inputBitmap));
				ds.Flush();

				targetImage.Source(win2dImageSource);
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
