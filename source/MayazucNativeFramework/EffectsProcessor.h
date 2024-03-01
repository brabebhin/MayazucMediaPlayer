#include "pch.h"
#include <winrt/MayazucNativeFramework.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.Media.Effects.h>
#include <winrt/Windows.Media.MediaProperties.h>
#include <winrt/Microsoft.Graphics.Canvas.h>
#include "VideoEffectProcessorConfiguration.h"
#include <winrt/Microsoft.Graphics.Canvas.Effects.h>

using namespace winrt::Windows::Media::Effects;
using namespace winrt::Windows::Foundation::Collections;
using namespace winrt::Windows::Media::MediaProperties;

using namespace winrt::Microsoft::Graphics;
using namespace winrt::Microsoft::Graphics::Canvas;
using namespace winrt::Microsoft::Graphics::Canvas::Effects;
using namespace winrt::MayazucNativeFramework;

class EffectProcessor
{
public:

	VideoEffectProcessorConfiguration EffectConfiguration = { nullptr };

	EffectProcessor() { }

	ICanvasImage ProcessFrame(ICanvasImage source)
	{		
		if (EffectConfiguration.MasterSwitch())
		{
			auto c = EffectConfiguration.Contrast();
			auto b = EffectConfiguration.Brightness();
			auto s = EffectConfiguration.Saturation();
			auto temp = EffectConfiguration.Temperature();
			auto tint = EffectConfiguration.Tint();
			auto sharpness = EffectConfiguration.Sharpness();

			bool hasSharpness = sharpness > 0.0f;
			bool hasColor = c != 0.0f || b != 0.0f || s != 0.0f;
			bool hasTemperatureAndTint = tint != 0.0f || temp != 0.0f;

			if (hasColor)
			{
				source = CreateColorEffect(source, c + 1.0f, b, s + 1.0f);
			}

			if (hasTemperatureAndTint)
			{
				source = CreateTermperatureAndTintEffect(source, temp, tint);
			}

			if (hasSharpness)
			{
				source = CreateSharpnessEffect(source, sharpness);
			}

			if (EffectConfiguration.GrayscaleEffect())
			{
				source = CreateGrayscaleEffect(source);
			}

			if (EffectConfiguration.BlurEffect())
			{
				source = CreateBlurEffect(source);
			}
		}
		return source;
	}

	~EffectProcessor()
	{
		EffectConfiguration = { nullptr };
	}

private:

	Effects::ICanvasEffect CreateColorEffect(ICanvasImage source, float c, float b, float s)
	{
		const auto lumR = 0.2125f;
		const auto lumG = 0.7154f;
		const auto lumB = 0.0721f;

		auto t = (1.0f - c) / 2.0f;
		auto sr = (1.0f - s) * lumR;
		auto sg = (1.0f - s) * lumG;
		auto sb = (1.0f - s) * lumB;

		Effects::Matrix5x4 matrix =
		{
			c * (sr + s),	c * (sr),		c * (sr),		0,
			c * (sg),		c * (sg + s),	c * (sg),		0,
			c * (sb),		c * (sb),		c * (sb + s),	0,
			0,				0,				0,				1,
			t + b,			t + b,			t + b,			0
		};

		auto colorMatrixEffect = Effects::ColorMatrixEffect();
		colorMatrixEffect.ColorMatrix(matrix);
		colorMatrixEffect.Source(source);

		return colorMatrixEffect;
	}

	Effects::ICanvasEffect CreateSharpnessEffect(ICanvasImage source, float sharpness)
	{
		auto effect = Effects::SharpenEffect();
		effect.Amount(sharpness);
		effect.Source(source);
		return effect;
	}

	Effects::ICanvasEffect CreateTermperatureAndTintEffect(ICanvasImage source, float temperature, float tint)
	{
		auto effect = Effects::TemperatureAndTintEffect();
		effect.Temperature(temperature);
		effect.Tint(tint);
		effect.Source(source);
		return effect;
	}

	ICanvasEffect CreateGrayscaleEffect(ICanvasImage source)
	{
		auto effect = Effects::GrayscaleEffect();
		effect.Source(source);
		return effect;
	}

	ICanvasEffect CreateBlurEffect(ICanvasImage source)
	{
		auto gaussianBlurEffect = Effects::GaussianBlurEffect();
		gaussianBlurEffect.Source(source);
		gaussianBlurEffect.BlurAmount(5.0f);
		gaussianBlurEffect.Optimization(EffectOptimization::Speed);
		return gaussianBlurEffect;
	}
};

