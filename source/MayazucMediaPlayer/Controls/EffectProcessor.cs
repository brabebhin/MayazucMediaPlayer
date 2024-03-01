using MayazucMediaPlayer.VideoEffects;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using GrayscaleEffect = Microsoft.Graphics.Canvas.Effects.GrayscaleEffect;
using TemperatureAndTintEffect = Microsoft.Graphics.Canvas.Effects.TemperatureAndTintEffect;

namespace MayazucMediaPlayer.Controls
{
    public class EffectProcessor
    {
        ICanvasEffect CreateColorEffect(ICanvasImage source, float c, float b, float s)
        {
            const float lumR = 0.2125f;
            const float lumG = 0.7154f;
            const float lumB = 0.0721f;

            float t = (1.0f - c) / 2.0f;
            float sr = (1.0f - s) * lumR;
            float sg = (1.0f - s) * lumG;
            float sb = (1.0f - s) * lumB;

            Matrix5x4 matrix = new Matrix5x4();
            matrix.M11 = c * (sr + s);
            matrix.M12 = c * (sr);
            matrix.M13 = c * (sr);
            matrix.M14 = 0;

            matrix.M21 = c * (sg);
            matrix.M22 = c * (sg + s);
            matrix.M23 = c * (sg);
            matrix.M24 = 0;

            matrix.M31 = c * (sb);
            matrix.M32 = c * (sb);
            matrix.M33 = c * (sb + s);
            matrix.M34 = 0;

            matrix.M41 = 0;
            matrix.M42 = 0;
            matrix.M43 = 0;
            matrix.M44 = 1;

            matrix.M51 = t + b;
            matrix.M52 = t + b;
            matrix.M53 = t + b;
            matrix.M54 = 0;
            //        {
            //            c * (sr + s),   c * (sr),       c * (sr),       0,
            //    c * (sg),       c * (sg + s),   c * (sg),       0,
            //    c * (sb),       c * (sb),       c * (sb + s),   0,
            //    0,              0,              0,              1,
            //    t + b,          t + b,          t + b,          0
            //};

            var colorMatrixEffect = new ColorMatrixEffect();
            colorMatrixEffect.ColorMatrix = matrix;
            colorMatrixEffect.Source = source;

            return colorMatrixEffect;
        }

        ICanvasEffect CreateSharpnessEffect(ICanvasImage source, float sharpness, float sharpnessThreshold)
        {
            var effect = new SharpenEffect();
            effect.Amount = sharpness;
            effect.Threshold = sharpnessThreshold;
            effect.Source = source;
            return effect;
        }

        ICanvasEffect CreateTermperatureAndTintEffect(ICanvasImage source, float temperature, float tint)
        {
            var effect = new TemperatureAndTintEffect();
            effect.Temperature = temperature;
            effect.Tint = tint;
            effect.Source = source;
            return effect;
        }

        ICanvasEffect CreateGrayscaleEffect(ICanvasImage source)
        {
            var effect = new GrayscaleEffect();
            effect.Source = source;
            return effect;
        }

        ICanvasEffect CreateBlurEffect(ICanvasImage source)
        {
            var gaussianBlurEffect = new GaussianBlurEffect();
            gaussianBlurEffect.Source = source;
            gaussianBlurEffect.BlurAmount = 5.0f;
            gaussianBlurEffect.Optimization = EffectOptimization.Speed;
            return gaussianBlurEffect;
        }

        public _VideoEffectProcessorConfiguration EffectConfiguration { get; set; }

        public EffectProcessor()
        {
        }

        public ICanvasImage ProcessFrame(ICanvasImage source)
        {
            if (EffectConfiguration.MasterSwitch)
            {
                var c = EffectConfiguration.Contrast;
                var b = EffectConfiguration.Brightness;
                var s = EffectConfiguration.Saturation;
                var temp = EffectConfiguration.Temperature;
                var tint = EffectConfiguration.Tint;
                var sharpness = EffectConfiguration.Sharpness;
                var sharpnessThreshold = EffectConfiguration.SharpnessThreshold;

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
                    source = CreateSharpnessEffect(source, sharpness, sharpnessThreshold);
                }

            }
            return source;
        }
    }
}
