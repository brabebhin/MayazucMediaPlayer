#pragma once

using namespace winrt::Microsoft::Graphics::Canvas;
using namespace winrt::Windows::Foundation::Numerics;
using namespace winrt::Windows::Media::Core;

namespace winrt::MayazucNativeFramework::implementation
{
    class SsaAssTextRenderer : public winrt::implements<SsaAssTextRenderer, winrt::Microsoft::Graphics::Canvas::Text::ICanvasTextRenderer>
    {
        SsaAssTextRenderer() = default;
    public:
        SsaAssTextRenderer(winrt::Microsoft::Graphics::Canvas::CanvasDrawingSession const& resourceCreator, TimedTextStyle const& lineStyle);
        void DrawGlyphRun(winrt::Windows::Foundation::Numerics::float2 const& point, winrt::Microsoft::Graphics::Canvas::Text::CanvasFontFace const& fontFace, float fontSize, array_view<winrt::Microsoft::Graphics::Canvas::Text::CanvasGlyph const> glyphs, bool isSideways, uint32_t bidiLevel, winrt::Windows::Foundation::IInspectable const& brush, winrt::Microsoft::Graphics::Canvas::Text::CanvasTextMeasuringMode const& measuringMode, hstring const& localeName, hstring const& textString, array_view<int32_t const> clusterMapIndices, uint32_t characterIndex, winrt::Microsoft::Graphics::Canvas::Text::CanvasGlyphOrientation const& glyphOrientation);
        void DrawStrikethrough(winrt::Windows::Foundation::Numerics::float2 const& point, float strikethroughWidth, float strikethroughThickness, float strikethroughOffset, winrt::Microsoft::Graphics::Canvas::Text::CanvasTextDirection const& textDirection, winrt::Windows::Foundation::IInspectable const& brush, winrt::Microsoft::Graphics::Canvas::Text::CanvasTextMeasuringMode const& textMeasuringMode, hstring const& localeName, winrt::Microsoft::Graphics::Canvas::Text::CanvasGlyphOrientation const& glyphOrientation);
        void DrawUnderline(winrt::Windows::Foundation::Numerics::float2 const& point, float underlineWidth, float underlineThickness, float underlineOffset, float runHeight, winrt::Microsoft::Graphics::Canvas::Text::CanvasTextDirection const& textDirection, winrt::Windows::Foundation::IInspectable const& brush, winrt::Microsoft::Graphics::Canvas::Text::CanvasTextMeasuringMode const& textMeasuringMode, hstring const& localeName, winrt::Microsoft::Graphics::Canvas::Text::CanvasGlyphOrientation const& glyphOrientation);
        void DrawInlineObject(winrt::Windows::Foundation::Numerics::float2 const& point, winrt::Microsoft::Graphics::Canvas::Text::ICanvasTextInlineObject const& inlineObject, bool isSideways, bool isRightToLeft, winrt::Windows::Foundation::IInspectable const& brush, winrt::Microsoft::Graphics::Canvas::Text::CanvasGlyphOrientation const& glyphOrientation);
        bool PixelSnappingDisabled();
        winrt::Windows::Foundation::Numerics::float3x2 Transform();
        float Dpi();
    private:
        CanvasDrawingSession ResourceCreator = { nullptr };
        TimedTextStyle LineStyle = { nullptr };

    };
}

