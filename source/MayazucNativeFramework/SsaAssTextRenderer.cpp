#include "pch.h"
#include "SsaAssTextRenderer.h"

namespace winrt::MayazucNativeFramework::implementation
{
	using namespace winrt::Microsoft::Graphics::Canvas::Geometry;
	using namespace winrt::Windows::Media::Core;
	using namespace Numerics;
	using namespace winrt::Microsoft::Graphics::Canvas::Text;
	using namespace winrt::Windows::Foundation;

	SsaAssTextRenderer::SsaAssTextRenderer(CanvasDrawingSession const& resourceCreator, TimedTextStyle const& lineStyle) :
		ResourceCreator(resourceCreator),
		LineStyle(lineStyle)
	{
	}

	void SsaAssTextRenderer::DrawGlyphRun(float2 const& point, CanvasFontFace const& fontFace, float fontSize, array_view<CanvasGlyph const> glyphs, bool isSideways, uint32_t bidiLevel, IInspectable const& brush, CanvasTextMeasuringMode const& measuringMode, hstring const& localeName, hstring const& textString, array_view<int32_t const> clusterMapIndices, uint32_t characterIndex, CanvasGlyphOrientation const& glyphOrientation)
	{
		auto glyphrun = CanvasGeometry::CreateGlyphRun(ResourceCreator, point, fontFace, fontSize, glyphs, isSideways, bidiLevel, measuringMode, glyphOrientation);
		if (!brush)
			ResourceCreator.FillGeometry(glyphrun, LineStyle.Foreground());
		else if (auto color = brush.try_as<Windows::UI::Color>())
		{
			ResourceCreator.FillGeometry(glyphrun, color.value());
		}
		else if (auto cBrush = brush.try_as<Brushes::ICanvasBrush>())
		{
			ResourceCreator.FillGeometry(glyphrun, cBrush);
		}
		else
		{
			ResourceCreator.FillGeometry(glyphrun, LineStyle.Foreground());
		}
	}

	void SsaAssTextRenderer::DrawStrikethrough(float2 const& point, float strikethroughWidth, float strikethroughThickness, float strikethroughOffset, CanvasTextDirection const& textDirection, IInspectable const& brush, CanvasTextMeasuringMode const& textMeasuringMode, hstring const& localeName, CanvasGlyphOrientation const& glyphOrientation)
	{
		if (!brush)
			ResourceCreator.FillRectangle(point.x, point.y + strikethroughOffset, strikethroughWidth, strikethroughThickness, LineStyle.Foreground());
		else if (auto color = brush.try_as<Windows::UI::Color>())
		{
			ResourceCreator.FillRectangle(point.x, point.y + strikethroughOffset, strikethroughWidth, strikethroughThickness, color.value());
		}
		else if (auto cBrush = brush.try_as<Brushes::ICanvasBrush>())
		{
			ResourceCreator.FillRectangle(point.x, point.y + strikethroughOffset, strikethroughWidth, strikethroughThickness, cBrush);
		}
		else
		{
			ResourceCreator.FillRectangle(point.x, point.y + strikethroughOffset, strikethroughWidth, strikethroughThickness, LineStyle.Foreground());
		}
	}

	void SsaAssTextRenderer::DrawUnderline(float2 const& point, float underlineWidth, float underlineThickness, float underlineOffset, float runHeight, CanvasTextDirection const& textDirection, IInspectable const& brush, CanvasTextMeasuringMode const& textMeasuringMode, hstring const& localeName, CanvasGlyphOrientation const& glyphOrientation)
	{
		ResourceCreator.FillRectangle(point.x, point.y, underlineWidth, underlineThickness, LineStyle.Foreground());
		if (!brush)
			ResourceCreator.FillRectangle(point.x, point.y, underlineWidth, underlineThickness, LineStyle.Foreground());
		else if (auto color = brush.try_as<winrt::Windows::UI::Color>())
		{
			ResourceCreator.FillRectangle(point.x, point.y, underlineWidth, underlineThickness, color.value());
		}
		else if (auto cBrush = brush.try_as<Brushes::ICanvasBrush>())
		{
			ResourceCreator.FillRectangle(point.x, point.y, underlineWidth, underlineThickness, cBrush);
		}
		else
		{
			ResourceCreator.FillRectangle(point.x, point.y, underlineWidth, underlineThickness, LineStyle.Foreground());
		}
	}

	void SsaAssTextRenderer::DrawInlineObject(float2 const& point, ICanvasTextInlineObject const& inlineObject, bool isSideways, bool isRightToLeft, IInspectable const& brush, CanvasGlyphOrientation const& glyphOrientation)
	{

	}

	bool SsaAssTextRenderer::PixelSnappingDisabled()
	{
		return false;
	}

	float3x2 SsaAssTextRenderer::Transform()
	{
		return float3x2::identity();
	}

	float SsaAssTextRenderer::Dpi()
	{
		return 96;
	}
}
