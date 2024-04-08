using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Media.Playback;
using CommunityToolkit.WinUI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class VolumeControlBar : BaseUserControl
    {
        public MediaPlayer Player { get; private set; }
        private double volumeBeforeMute = 1.0d;

        double CurrentPlayerVolume
        {
            get
            {
                if (Player != null)
                {
                    return Player.Volume * 100;
                }
                return 0;
            }
            set
            {
                if (Player != null)
                {
                    Player.Volume = value / 100;
                }
            }
        }

        public VolumeControlBar()
        {
            this.InitializeComponent();
            VolumeControlSlider.ValueChanged += VolumeControlSlider_ValueChanged;
        }

        private void VolumeControlSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            CurrentPlayerVolume = (double)e.NewValue;
        }

        public void SetMediaPlayer(MediaPlayer player)
        {
            Player = player;
            Player.VolumeChanged += Player_VolumeChanged;
            HandlePlayerVolumeChanged();
        }

        private void Player_VolumeChanged(MediaPlayer sender, object args)
        {
            DispatcherQueue.EnqueueAsync(() =>
            {
                HandlePlayerVolumeChanged();
            });
        }

        private void HandlePlayerVolumeChanged()
        {
            VolumeControlSlider.Value = CurrentPlayerVolume;
            SetVolumeIcon(CurrentPlayerVolume);
        }

        private void MuteUnmuteButton_click(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentPlayerVolume == 0)
            {
                CurrentPlayerVolume = volumeBeforeMute;
            }
            else
            {
                volumeBeforeMute = CurrentPlayerVolume;
                CurrentPlayerVolume = 0;
            }

            SetVolumeIcon(CurrentPlayerVolume);
        }

        private void SetVolumeIcon(double volume)
        {
            MuteUnmuteButton.Icon = new SymbolIcon(volume == 0 ? Symbol.Mute : Symbol.Volume);
        }
    }
}
