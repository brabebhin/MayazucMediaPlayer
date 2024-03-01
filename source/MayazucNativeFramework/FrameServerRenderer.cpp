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
			effectsPrcessor->EffectConfiguration = effectConfiguration;

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
				ds.DrawImage(effectsPrcessor->ProcessFrame(inputBitmap));
				ds.Flush();

				targetImage.Source(win2dImageSource);
			}
		}
		catch (...)
		{

		}
	}
}
