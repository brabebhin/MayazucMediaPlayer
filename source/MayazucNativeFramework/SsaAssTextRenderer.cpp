#include "pch.h"
#include "SsaAssTextRenderer.h"

namespace winrt::MayazucNativeFramework::implementation
{
	using namespace winrt::Microsoft::Graphics::Canvas::Geometry;
	using namespace winrt::Windows::Media::Core;

	SsaAssTextRenderer::SsaAssTextRenderer(winrt::Microsoft::Graphics::Canvas::CanvasDrawingSession const& resourceCreator, TimedTextStyle const& lineStyle) :
		ResourceCreator(resourceCreator),
		LineStyle(lineStyle)
	{
	}

	void SsaAssTextRenderer::DrawGlyphRun(winrt::Windows::Foundation::Numerics::float2 const& point, winrt::Microsoft::Graphics::Canvas::Text::CanvasFontFace const& fontFace, float fontSize, array_view<winrt::Microsoft::Graphics::Canvas::Text::CanvasGlyph const> glyphs, bool isSideways, uint32_t bidiLevel, winrt::Windows::Foundation::IInspectable const& brush, winrt::Microsoft::Graphics::Canvas::Text::CanvasTextMeasuringMode const& measuringMode, hstring const& localeName, hstring const& textString, array_view<int32_t const> clusterMapIndices, uint32_t characterIndex, winrt::Microsoft::Graphics::Canvas::Text::CanvasGlyphOrientation const& glyphOrientation)
	{
		auto glyphrun = winrt::Microsoft::Graphics::Canvas::Geometry::CanvasGeometry::CreateGlyphRun(ResourceCreator, point, fontFace, fontSize, glyphs, isSideways, bidiLevel, measuringMode, glyphOrientation);
		if (!brush)
			ResourceCreator.FillGeometry(glyphrun, LineStyle.Foreground());
		else if (auto color = brush.try_as<Windows::UI::Color>())
		{
			ResourceCreator.FillGeometry(glyphrun, color.value());
		}
		else if (auto cBrush = brush.try_as<winrt::Microsoft::Graphics::Canvas::Brushes::ICanvasBrush>())
		{
			ResourceCreator.FillGeometry(glyphrun, cBrush);
		}
		else
		{
			ResourceCreator.FillGeometry(glyphrun, LineStyle.Foreground());
		}
	}

	void SsaAssTextRenderer::DrawStrikethrough(winrt::Windows::Foundation::Numerics::float2 const& point, float strikethroughWidth, float strikethroughThickness, float strikethroughOffset, winrt::Microsoft::Graphics::Canvas::Text::CanvasTextDirection const& textDirection, winrt::Windows::Foundation::IInspectable const& brush, winrt::Microsoft::Graphics::Canvas::Text::CanvasTextMeasuringMode const& textMeasuringMode, hstring const& localeName, winrt::Microsoft::Graphics::Canvas::Text::CanvasGlyphOrientation const& glyphOrientation)
	{
		if (!brush)
			ResourceCreator.FillRectangle(point.x, point.y + strikethroughOffset, strikethroughWidth, strikethroughThickness, LineStyle.Foreground());
		else if (auto color = brush.try_as<Windows::UI::Color>())
		{
			ResourceCreator.FillRectangle(point.x, point.y + strikethroughOffset, strikethroughWidth, strikethroughThickness, color.value());
		}
		else if (auto cBrush = brush.try_as<winrt::Microsoft::Graphics::Canvas::Brushes::ICanvasBrush>())
		{
			ResourceCreator.FillRectangle(point.x, point.y + strikethroughOffset, strikethroughWidth, strikethroughThickness, cBrush);
		}
		else
		{
			ResourceCreator.FillRectangle(point.x, point.y + strikethroughOffset, strikethroughWidth, strikethroughThickness, color.value());
		}
	}

	void SsaAssTextRenderer::DrawUnderline(winrt::Windows::Foundation::Numerics::float2 const& point, float underlineWidth, float underlineThickness, float underlineOffset, float runHeight, winrt::Microsoft::Graphics::Canvas::Text::CanvasTextDirection const& textDirection, winrt::Windows::Foundation::IInspectable const& brush, winrt::Microsoft::Graphics::Canvas::Text::CanvasTextMeasuringMode const& textMeasuringMode, hstring const& localeName, winrt::Microsoft::Graphics::Canvas::Text::CanvasGlyphOrientation const& glyphOrientation)
	{
		ResourceCreator.FillRectangle(point.x, point.y, underlineWidth, underlineThickness, LineStyle.Foreground());
		if (!brush)
			ResourceCreator.FillRectangle(point.x, point.y, underlineWidth, underlineThickness, LineStyle.Foreground());
		else if (auto color = brush.try_as<winrt::Windows::UI::Color>())
		{
			ResourceCreator.FillRectangle(point.x, point.y, underlineWidth, underlineThickness, color.value());
		}
		else if (auto cBrush = brush.try_as<winrt::Microsoft::Graphics::Canvas::Brushes::ICanvasBrush>())
		{
			ResourceCreator.FillRectangle(point.x, point.y, underlineWidth, underlineThickness, cBrush);
		}
		else
		{
			ResourceCreator.FillRectangle(point.x, point.y, underlineWidth, underlineThickness, LineStyle.Foreground());
		}
	}

	void SsaAssTextRenderer::DrawInlineObject(winrt::Windows::Foundation::Numerics::float2 const& point, winrt::Microsoft::Graphics::Canvas::Text::ICanvasTextInlineObject const& inlineObject, bool isSideways, bool isRightToLeft, winrt::Windows::Foundation::IInspectable const& brush, winrt::Microsoft::Graphics::Canvas::Text::CanvasGlyphOrientation const& glyphOrientation)
	{

	}

	bool SsaAssTextRenderer::PixelSnappingDisabled()
	{
		return true;
	}

	winrt::Windows::Foundation::Numerics::float3x2 SsaAssTextRenderer::Transform()
	{
		return winrt::Windows::Foundation::Numerics::float3x2::identity();
	}

	float SsaAssTextRenderer::Dpi()
	{
		return 96;
	}
}
