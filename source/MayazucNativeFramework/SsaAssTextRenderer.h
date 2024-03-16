#pragma once

using namespace winrt::Microsoft::Graphics::Canvas;
using namespace winrt::Windows::Media::Core;
using namespace winrt::Microsoft::Graphics::Canvas::Geometry;
using namespace winrt::Microsoft::Graphics::Canvas::Text;
using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::Foundation::Numerics;

namespace winrt::MayazucNativeFramework::implementation
{
    class SsaAssTextRenderer : public winrt::implements<SsaAssTextRenderer, ICanvasTextRenderer>
    {
        SsaAssTextRenderer() = default;
    public:
        SsaAssTextRenderer(CanvasDrawingSession const& resourceCreator, TimedTextStyle const& lineStyle);
        void DrawGlyphRun(float2 const& point, CanvasFontFace const& fontFace, float fontSize, array_view<CanvasGlyph const> glyphs, bool isSideways, uint32_t bidiLevel, IInspectable const& brush, CanvasTextMeasuringMode const& measuringMode, hstring const& localeName, hstring const& textString, array_view<int32_t const> clusterMapIndices, uint32_t characterIndex, CanvasGlyphOrientation const& glyphOrientation);
        void DrawStrikethrough(float2 const& point, float strikethroughWidth, float strikethroughThickness, float strikethroughOffset, CanvasTextDirection const& textDirection, IInspectable const& brush, CanvasTextMeasuringMode const& textMeasuringMode, hstring const& localeName, CanvasGlyphOrientation const& glyphOrientation);
        void DrawUnderline(float2 const& point, float underlineWidth, float underlineThickness, float underlineOffset, float runHeight, CanvasTextDirection const& textDirection, IInspectable const& brush, CanvasTextMeasuringMode const& textMeasuringMode, hstring const& localeName, CanvasGlyphOrientation const& glyphOrientation);
        void DrawInlineObject(float2 const& point, ICanvasTextInlineObject const& inlineObject, bool isSideways, bool isRightToLeft, IInspectable const& brush, CanvasGlyphOrientation const& glyphOrientation);
        bool PixelSnappingDisabled();
        float3x2 Transform();
        float Dpi();
    private:
        CanvasDrawingSession ResourceCreator = { nullptr };
        TimedTextStyle LineStyle = { nullptr };

    };
}

