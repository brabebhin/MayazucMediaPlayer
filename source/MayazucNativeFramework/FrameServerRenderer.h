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
            canvasSwapChain = CanvasSwapChain(CanvasDevice::GetSharedDevice(), 480, 470, 96);
            com_ptr<abi::ICanvasResourceWrapperNative> nativeDeviceWrapper = canvasSwapChain.as<abi::ICanvasResourceWrapperNative>();
            com_ptr<IDXGISwapChain1> pNativeSwapChain{ nullptr };
            check_hresult(nativeDeviceWrapper->GetNativeResource(nullptr, 0.0f, guid_of<IDXGISwapChain1>(), pNativeSwapChain.put_void()));

            auto nativeSwapChainPanel = swapChainPannel.as< ISwapChainPanelNative>();
            nativeSwapChainPanel->SetSwapChain(pNativeSwapChain.get());
        }

        void RenderMediaPlayerFrame(winrt::Windows::Media::Playback::MediaPlayer const& player, winrt::Microsoft::UI::Xaml::Controls::Image const& targetImage, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration const& effectConfiguration);
        void RenderMediaPlayerFrame(winrt::Windows::Media::Playback::MediaPlayer const& player, winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel const& swapChainPannel, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration const& effectConfiguration);
        IAsyncAction RenderMediaPlayerFrameToStreamAsync(winrt::Windows::Media::Playback::MediaPlayer player, winrt::MayazucNativeFramework::VideoEffectProcessorConfiguration effectConfiguration, winrt::Windows::Storage::Streams::IRandomAccessStream outputStream);

    private:
        EffectProcessor effectsPrcessor;
        winrt::Microsoft::Graphics::Canvas::UI::Xaml::CanvasImageSource win2dImageSource = { nullptr };
        winrt::Microsoft::Graphics::Canvas::CanvasRenderTarget renderingTarget = { nullptr };
        winrt::Microsoft::Graphics::Canvas::CanvasRenderTarget subtitlesTarget = { nullptr };
        winrt::Microsoft::Graphics::Canvas::CanvasSwapChain canvasSwapChain = { nullptr };

    };
}
namespace winrt::MayazucNativeFramework::factory_implementation
{
    struct FrameServerRenderer : FrameServerRendererT<FrameServerRenderer, implementation::FrameServerRenderer>
    {
    };
}
