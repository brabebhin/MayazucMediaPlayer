#pragma once
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
	namespace abi {
		using namespace ABI::Microsoft::Graphics::Canvas;
	}

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

	static void SwapChainAllocResources(const winrt::Microsoft::UI::Xaml::Controls::SwapChainPanel& swapChainPannel,
		float width,
		float height,
		float dpi, winrt::Windows::Graphics::DirectX::DirectXPixelFormat const& pixelFormat,
		int bufferCount,
		CanvasSwapChain& canvasSwapChain)
	{
		canvasSwapChain = CanvasSwapChain(CanvasDevice::GetSharedDevice(), width, height, dpi, pixelFormat, bufferCount, CanvasAlphaMode::Premultiplied);
		com_ptr<abi::ICanvasResourceWrapperNative> nativeDeviceWrapper = canvasSwapChain.as<abi::ICanvasResourceWrapperNative>();
		com_ptr<IDXGISwapChain1> pNativeSwapChain{ nullptr };
		check_hresult(nativeDeviceWrapper->GetNativeResource(nullptr, 0.0f, guid_of<IDXGISwapChain1>(), pNativeSwapChain.put_void()));

		auto nativeSwapChainPanel = swapChainPannel.as< ISwapChainPanelNative>();
		nativeSwapChainPanel->SetSwapChain(pNativeSwapChain.get());
	}
}
#pragma once
