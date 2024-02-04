using CommunityToolkit.WinUI;
using MayazucMediaPlayer.Settings;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
            Loaded += ShuffleRadioGroup_Loaded;
            Unloaded += ShuffleRadioGroup_Unloaded;

            shuffleModes.Add(new ShuffleModeItem(Symbol.Shuffle, "Shuffle tracks", true));
            shuffleModes.Add(new ShuffleModeItem(Symbol.Switch, "Shuffle disabled", false));
            cbShuffleModes.ItemsSource = shuffleModes;
            cbShuffleModes.SelectionChanged += CbShuffleModes_SelectionChanged;
            SettingsWrapper.ShuffleModeChanged += SettingsWrapper_ShuffleModeChanged;
        }

        private async void CbShuffleModes_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (cbShuffleModes.SelectedIndex >= 0)
            {
                await (cbShuffleModes.SelectedItem as ShuffleModeItem).SetShuffleMode();
            }
        }

        private async void ShuffleRadioGroup_Unloaded(object? sender, RoutedEventArgs e)
        {
            SettingsWrapper.ShuffleModeChanged -= SettingsWrapper_ShuffleModeChanged;
            var playerInstance = AppState.Current.MediaServiceConnector.PlayerInstance;
            if (playerInstance.DataBusyFlag != null)
            {
                playerInstance.DataBusyFlag.StateChanged -= InternalDataBusyFlag_StateChanged;
            }

            Loaded -= ShuffleRadioGroup_Loaded;
            Unloaded -= ShuffleRadioGroup_Unloaded;
        }

        private async void ShuffleRadioGroup_Loaded(object? sender, RoutedEventArgs e)
        {
            var playerInstance = AppState.Current.MediaServiceConnector.PlayerInstance;
            if (playerInstance.DataBusyFlag != null)
            {
                playerInstance.DataBusyFlag.StateChanged += InternalDataBusyFlag_StateChanged;
            }
            SetSelectedButton();
        }

        private void SetSelectedButton()
        {
            cbShuffleModes.SelectionChanged -= CbShuffleModes_SelectionChanged;

            for (int i = 0; i < shuffleModes.Count; i++)
            {
                if (shuffleModes[i].ShuffleEnabled == SettingsWrapper.ShuffleMode)
                {
                    cbShuffleModes.SelectedIndex = i;
                    break;
                }
            }

            cbShuffleModes.SelectionChanged += CbShuffleModes_SelectionChanged;
        }

        private async void InternalDataBusyFlag_StateChanged(object? sender, bool e)
        {
            await DispatcherQueue.EnqueueAsync(() => { IsEnabled = !e; });
        }

        private async void SettingsWrapper_ShuffleModeChanged(object? sender, bool e)
        {
            await DispatcherQueue.EnqueueAsync(() => { SetSelectedButton(); });
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

            public ShuffleModeItem(Symbol modeSymbol, string modeText, bool shuffleMode)
            {
                ModeSymbol = modeSymbol;
                ModeText = modeText ?? throw new ArgumentNullException(nameof(modeText));
                ShuffleEnabled = shuffleMode;
            }

            public async Task SetShuffleMode()
            {
                await AppState.Current.MediaServiceConnector.SetShuffleMode(ShuffleEnabled);
            }
        }
    }
}
