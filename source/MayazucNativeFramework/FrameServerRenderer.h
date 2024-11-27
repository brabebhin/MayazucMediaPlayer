#pragma once
#include "pch.h"
#include "FrameServerRenderer.g.h"
#include "EffectsProcessor.h"
#include <winrt/Microsoft.UI.Xaml.h>
#include <winrt/Microsoft.UI.h>
#include <winrt/Microsoft.Graphics.Canvas.UI.Xaml.h>
#include <winrt/Windows.Graphics.Imaging.h>
#include <winrt/Windows.Media.Playback.h>
#include <winrt/Microsoft.UI.Xaml.Controls.h>
#include <winrt/Microsoft.Graphics.Canvas.h> //This defines the C++/WinRT interfaces for the Win2D Windows Runtime Components
#include <Microsoft.Graphics.Canvas.native.h> //This is for interop
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

    namespace abi {
        using namespace ABI::Microsoft::Graphics::Canvas;
    }

    struct FrameServerRenderer : FrameServerRendererT<FrameServerRenderer>
    {
        FrameServerRenderer(winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& swapChainPannel)
        {
            this->swapChainPannel = swapChainPannel;
            SwapChainAllocResources(swapChainPannel, 800, 480, 96, winrt::Windows::Graphics::DirectX::DirectXPixelFormat::B8G8R8A8UIntNormalized, SubtitleSwapChainBufferCount, canvasSwapChain);
        }

        void RefreshSubtitle(winrt::Windows::Media::Playback::MediaPlayer const& player, uint32_t width, uint32_t height, uint32_t dpi);
        void RenderMediaPlayerFrame(winrt::Windows::Media::Playback::MediaPlayer const& player, uint32_t width, uint32_t height, uint32_t dpi, winrt::Windows::Graphics::DirectX::DirectXPixelFormat const& pixelFormat, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration const& effectConfiguration);
        IAsyncAction RenderMediaPlayerFrameToStreamAsync(winrt::Windows::Media::Playback::MediaPlayer player, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration effectConfiguration, winrt::Windows::Storage::Streams::IRandomAccessStream outputStream);

    private:

        void RefreshSubtitleInternal(winrt::Windows::Media::Playback::MediaPlayer const& player, uint32_t width, uint32_t height, uint32_t dpi, CanvasDevice const& device)
        {
            //if (subtitleRenderingTarget == nullptr || subtitleRenderingTarget.Device().IsDeviceLost() || (subtitleRenderingTarget.Bounds().Width != width) || (subtitleRenderingTarget.Bounds().Height != height))
            {
                if (subtitleRenderingTarget)
                    subtitleRenderingTarget.Close();

                subtitleRenderingTarget = CanvasRenderTarget(device, (uint32_t)width, (uint32_t)height, dpi, winrt::Windows::Graphics::DirectX::DirectXPixelFormat::B8G8R8A8UIntNormalized, CanvasAlphaMode::Premultiplied);
            }
            player.RenderSubtitlesToSurface(subtitleRenderingTarget);
        }

        EffectProcessor effectsPrcessor;
        winrt::Microsoft::Graphics::Canvas::CanvasRenderTarget subtitleRenderingTarget = { nullptr };
        winrt::Microsoft::Graphics::Canvas::CanvasRenderTarget renderingTarget = { nullptr };
        winrt::Microsoft::Graphics::Canvas::CanvasSwapChain canvasSwapChain = { nullptr };
        winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel swapChainPannel = { nullptr };

    };
}
namespace winrt::MayazucNativeFramework::factory_implementation
{
    struct FrameServerRenderer : FrameServerRendererT<FrameServerRenderer, implementation::FrameServerRenderer>
    {
    };
}
