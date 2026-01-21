using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class ShuffleRadioGroup : AppBarButton
    {
        readonly List<ShuffleModeItem> shuffleModes = new List<ShuffleModeItem>();

        public ShuffleRadioGroup()
        {
            InitializeComponent();
            Unloaded += ShuffleRadioGroup_Unloaded;

            shuffleModes.Add(new ShuffleModeItem(Symbol.Shuffle, "Shuffle tracks", true, new SolidColorBrush(Microsoft.UI.Colors.White)));
            shuffleModes.Add(new ShuffleModeItem(Symbol.Shuffle, "Shuffle disabled", false, new SolidColorBrush(Microsoft.UI.Colors.DarkGray)));
            cbShuffleModes.ItemsSource = shuffleModes;
            cbShuffleModes.SelectionChanged += CbShuffleModes_SelectionChanged;
            SettingsService.Instance.ShuffleModeChanged += SettingsWrapper_ShuffleModeChanged;
            LoadState();
        }

        private async void CbShuffleModes_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (cbShuffleModes.SelectedIndex >= 0)
            {
                try
                {
                    this.IsEnabled = false;
                    await (cbShuffleModes.SelectedItem as ShuffleModeItem).SetShuffleMode();
                }
                finally
                {
                    this.IsEnabled = true;
                }
            }
        }

        private async void ShuffleRadioGroup_Unloaded(object? sender, RoutedEventArgs e)
        {
            SettingsService.Instance.ShuffleModeChanged -= SettingsWrapper_ShuffleModeChanged;

            Unloaded -= ShuffleRadioGroup_Unloaded;
        }

        private void LoadState()
        {
            SetSelectedButton();
        }

        private void SetSelectedButton()
        {
            cbShuffleModes.SelectionChanged -= CbShuffleModes_SelectionChanged;

            for (int i = 0; i < shuffleModes.Count; i++)
            {
                if (shuffleModes[i].ShuffleEnabled == SettingsService.Instance.ShuffleMode)
                {
                    cbShuffleModes.SelectedIndex = i;
                    break;
                }
            }

            cbShuffleModes.SelectionChanged += CbShuffleModes_SelectionChanged;
        }

        private async void SettingsWrapper_ShuffleModeChanged(object? sender, bool e)
        {
            await DispatcherQueue.EnqueueAsync(() => { SetSelectedButton(); });
        }
    }

    internal class ShuffleModeItem
    {
        public Symbol ModeSymbol
        {
            get;
            private set;
        }

        public string ModeText
        {
            get;
            private set;
        }

        public bool ShuffleEnabled
        {
            get;
            private set;
        }

        public SolidColorBrush ShuffleOnOffColor
        {
            get;
            private set;
        }

        public ShuffleModeItem(Symbol modeSymbol, string modeText, bool shuffleMode, SolidColorBrush shuffleOnOffColor)
        {
            ModeSymbol = modeSymbol;
            ModeText = modeText ?? throw new ArgumentNullException(nameof(modeText));
            ShuffleEnabled = shuffleMode;
            ShuffleOnOffColor = shuffleOnOffColor;
        }

        public async Task SetShuffleMode()
        {
            await AppState.Current.MediaServiceConnector.SetShuffleMode(ShuffleEnabled);
        }
    }
}
