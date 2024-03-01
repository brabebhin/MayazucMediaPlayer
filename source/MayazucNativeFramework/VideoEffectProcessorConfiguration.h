#pragma once
#include "pch.h"
#include "VideoEffectProcessorConfiguration.g.h"

namespace winrt::MayazucNativeFramework::implementation
{
    struct VideoEffectProcessorConfiguration : VideoEffectProcessorConfigurationT<VideoEffectProcessorConfiguration>
    {
        VideoEffectProcessorConfiguration() = default;

        winrt::event_token ConfigurationChanged(winrt::Windows::Foundation::EventHandler<hstring> const& handler);
        void ConfigurationChanged(winrt::event_token const& token) noexcept;
        bool MasterSwitch();
        void MasterSwitch(bool value);
        float Brightness();
        void Brightness(float value);
        float Contrast();
        void Contrast(float value);
        float Saturation();
        void Saturation(float value);
        float Temperature();
        void Temperature(float value);
        float Tint();
        void Tint(float value);
        float Sharpness();
        void Sharpness(float value);
        bool GrayscaleEffect();
        void GrayscaleEffect(bool value);
        bool BlurEffect();
        void BlurEffect(bool value);

    private:
        winrt::event<Windows::Foundation::EventHandler<hstring>> m_ConfigurationChangedEvent;


    private:
        float _Brightness = 0.0f;
        float _Contrast = 0.0f;
        float _Saturation = 0.0f;
        float _Temperature = 0.0f;
        float _Tint = 0.0f;
        float _Sharpness = 0.0f;
        bool _GrayscaleEffect = false;
        bool _BlurEffect = false;
        bool _masterSwitch = false;
    };
}
namespace winrt::MayazucNativeFramework::factory_implementation
{
    struct VideoEffectProcessorConfiguration : VideoEffectProcessorConfigurationT<VideoEffectProcessorConfiguration, implementation::VideoEffectProcessorConfiguration>
    {
    };
}
