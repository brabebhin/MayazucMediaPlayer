#pragma once
#include "pch.h"
#include "FrameServerRenderer.g.h"
#include "EffectsProcessor.h"
#include <winrt/Microsoft.UI.Xaml.h>
#include <winrt/Microsoft.UI.h>
#include <winrt/Microsoft.Graphics.Canvas.h>
#include <winrt/Microsoft.Graphics.Canvas.UI.Xaml.h>
#include <winrt/Windows.Graphics.Imaging.h>
#include <winrt/Windows.Media.Playback.h>
#include <winrt/Microsoft.UI.Xaml.Controls.h>

namespace winrt::MayazucNativeFramework::implementation
{
    using namespace winrt::Microsoft::Graphics;
    using namespace winrt::Microsoft::Graphics::Canvas;
    using namespace winrt::Microsoft::Graphics::Canvas::UI::Xaml;
    using namespace winrt::Windows::Graphics::Imaging;

    using namespace winrt::Windows::Foundation::Collections;
    using namespace winrt::Windows::Media::Effects;
    using namespace winrt::Windows::Foundation::Collections;
    using namespace winrt::Windows::Media::MediaProperties;

    using namespace winrt::Microsoft::Graphics;
    using namespace winrt::Microsoft::Graphics::Canvas;
    using namespace winrt::Microsoft::Graphics::Canvas::Effects;

    struct FrameServerRenderer : FrameServerRendererT<FrameServerRenderer>
    {
        FrameServerRenderer()
        {
        }

        void RenderMediaPlayerFrame(winrt::Windows::Media::Playback::MediaPlayer const& player, winrt::Microsoft::UI::Xaml::Controls::Image const& targetImage, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration const& effectConfiguration);
        winrt::Windows::Foundation::IAsyncAction RenderMediaPlayerFrameToStreamAsync(winrt::Windows::Media::Playback::MediaPlayer player, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration effectConfiguration, winrt::Windows::Storage::Streams::IRandomAccessStream outputStream);

    private:
        EffectProcessor effectsPrcessor;
        winrt::Microsoft::Graphics::Canvas::UI::Xaml::CanvasImageSource win2dImageSource = { nullptr };
        winrt::Microsoft::Graphics::Canvas::CanvasRenderTarget renderingTarget = { nullptr };
        winrt::Microsoft::Graphics::Canvas::CanvasRenderTarget subtitlesTarget = { nullptr };

    };
}
namespace winrt::MayazucNativeFramework::factory_implementation
{
    struct FrameServerRenderer : FrameServerRendererT<FrameServerRenderer, implementation::FrameServerRenderer>
    {
    };
}