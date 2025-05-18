using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Graphics.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using MayazucMediaPlayer.Controls;
using MayazucMediaPlayer.VideoEffects;

namespace MayazucMediaPlayer.Rendering
{
    class VideoRenderer
    {
        private const int BufferCount = 2;
        private readonly CanvasSwapChainPanel canvasSwapChainPanel;
        private CanvasSwapChain? swapChain;
        private CanvasRenderTarget? canvasRenderTarget;
        private CanvasRenderTarget? subtitlesRenderTarget;

        private CanvasDevice? canvasDevice;
        VideoEffectProcessor videoProcessor = new VideoEffectProcessor();

        private uint height
        {
            get => swapChain!.SizeInPixels.Height;
        }

        private uint width
        {
            get => swapChain!.SizeInPixels.Width;
        }

        private DirectXPixelFormat pixelFormat
        {
            get => swapChain!.Format;
        }

        private float dpi
        {
            get => swapChain!.Dpi;
        }

        public CanvasSwapChainPanel SwapChainPanel
        {
            get;
            private set;
        }

        public VideoRenderer(CanvasSwapChainPanel canvasSwapChainPanel)
        {
            SwapChainPanel = canvasSwapChainPanel;
            canvasDevice = CanvasDevice.GetSharedDevice();
            SwapChainAllocResources(480, 800, 96, DirectXPixelFormat.R8G8B8A8UIntNormalized);
        }

        private void SwapChainAllocResources(uint width, uint height, float dpi, DirectXPixelFormat pixelformat)
        {
            if (swapChain == null)
            {
                swapChain = new CanvasSwapChain(canvasDevice, 480, 800, 96);
                SwapChainPanel.SwapChain = swapChain;
            }

            if (canvasDevice!.IsDeviceLost() || this.width != width || this.height != height || this.dpi != dpi || this.pixelFormat != pixelformat)
            {
                swapChain!.ResizeBuffers(width, height, dpi, pixelformat, BufferCount);
                canvasRenderTarget = new CanvasRenderTarget(swapChain, width, height, dpi);
                subtitlesRenderTarget = new CanvasRenderTarget(swapChain, width, height, dpi);
            }
        }

        public void ResizeSwapChain(uint width, uint height, float dpi, DirectXPixelFormat pixelformat)
        {
            SwapChainAllocResources(width, height, dpi, pixelformat);
        }

        public void RenderMediaPlayerFrame(MediaPlayer player, _VideoEffectProcessorConfiguration effectConfiguration)
        {
            if (canvasDevice!.IsDeviceLost())
            {
                SwapChainAllocResources(width, height, dpi, pixelFormat);
            }

            using (var drawingSession = swapChain!.CreateDrawingSession(Microsoft.UI.Colors.Transparent))
            {
                player.CopyFrameToVideoSurface(canvasRenderTarget);
                var processed = videoProcessor.ProcessFrame(canvasRenderTarget!, effectConfiguration);
                var rendered = player.RenderSubtitlesToSurface(canvasRenderTarget);
                drawingSession.DrawImage(canvasRenderTarget);
                swapChain.Present(1);
            }
        }
    }
}
