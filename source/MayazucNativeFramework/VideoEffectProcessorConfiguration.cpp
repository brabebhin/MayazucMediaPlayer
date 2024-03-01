#include "pch.h"
#include "VideoEffectProcessorConfiguration.h"
#include "VideoEffectProcessorConfiguration.g.cpp"

namespace winrt::MayazucNativeFramework::implementation
{
    bool VideoEffectProcessorConfiguration::MasterSwitch()
    {
        return _masterSwitch;
    }

    void VideoEffectProcessorConfiguration::MasterSwitch(bool value)
    {
        _masterSwitch = value;
        m_ConfigurationChangedEvent(*this, L"MasterSwitch");
    }

    float VideoEffectProcessorConfiguration::Brightness()
    {
        return _Brightness;
    }
    
    void VideoEffectProcessorConfiguration::Brightness(float value)
    {
        _Brightness = value;
        m_ConfigurationChangedEvent(*this, L"Brightness");
    }

    float VideoEffectProcessorConfiguration::Contrast()
    {
        return _Contrast;
    }

    void VideoEffectProcessorConfiguration::Contrast(float value)
    {
        _Contrast = value;
        m_ConfigurationChangedEvent(*this, L"Contrast");
    }

    float VideoEffectProcessorConfiguration::Saturation()
    {
        return _Saturation;
    }

    void VideoEffectProcessorConfiguration::Saturation(float value)
    {
        _Saturation = value;
        m_ConfigurationChangedEvent(*this, L"Saturation");
    }

    float VideoEffectProcessorConfiguration::Temperature()
    {
        return _Temperature;
    }

    void VideoEffectProcessorConfiguration::Temperature(float value)
    {
        _Temperature = value;
        m_ConfigurationChangedEvent(*this, L"Temperature");
    }

    float VideoEffectProcessorConfiguration::Tint()
    {
        return _Tint;
    }

    void VideoEffectProcessorConfiguration::Tint(float value)
    {
        _Tint = value;
        m_ConfigurationChangedEvent(*this, L"Tint");
    }

    float VideoEffectProcessorConfiguration::Sharpness()
    {
        return _Sharpness;
    }
    void VideoEffectProcessorConfiguration::Sharpness(float value)
    {
        _Sharpness = value;
        m_ConfigurationChangedEvent(*this, L"Sharpness");
    }

    bool VideoEffectProcessorConfiguration::GrayscaleEffect()
    {
        return _GrayscaleEffect;
    }

    void VideoEffectProcessorConfiguration::GrayscaleEffect(bool value)
    {
        _GrayscaleEffect = value;
        m_ConfigurationChangedEvent(*this, L"GrayscaleEffect");
    }

    bool VideoEffectProcessorConfiguration::BlurEffect()
    {
        return _BlurEffect;
    }

    void VideoEffectProcessorConfiguration::BlurEffect(bool value)
    {
        _BlurEffect = value;
        m_ConfigurationChangedEvent(*this, L"BlurEffect");
    }

    winrt::event_token VideoEffectProcessorConfiguration::ConfigurationChanged(winrt::Windows::Foundation::EventHandler<hstring> const& handler)
    {
        return m_ConfigurationChangedEvent.add(handler);
    }

    void VideoEffectProcessorConfiguration::ConfigurationChanged(winrt::event_token const& token) noexcept
    {
        m_ConfigurationChangedEvent.remove(token);
    }
}
