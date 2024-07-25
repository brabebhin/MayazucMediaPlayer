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
    public sealed partial class RepeatRadioGroup : AppBarButton
    {
        readonly List<RepeatModeItem> repeatModes = new List<RepeatModeItem>();
        public RepeatRadioGroup()
        {
            InitializeComponent();
            Unloaded += RepeatRadioGroup_Unloaded;

            repeatModes.Add(new RepeatModeItem(Symbol.RepeatAll, "Repeat entire list", Constants.RepeatAll));
            repeatModes.Add(new RepeatModeItem(Symbol.RepeatOne, "Repeat current item", Constants.RepeatOne));
            repeatModes.Add(new RepeatModeItem(Symbol.DisableUpdates, "Repeat disabled", Constants.RepeatNone));

            cbRepeatModes.ItemsSource = repeatModes;
            cbRepeatModes.SelectionChanged += CbRepeatModes_SelectionChanged;

            SettingsService.Instance.RepeatModeChanged += SettingsWrapper_RepeatModeChanged;
            LoadState();
        }

        private async void CbRepeatModes_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (cbRepeatModes.SelectedIndex >= 0)
            {
                try
                {
                    this.IsEnabled = false;
                    await (cbRepeatModes.SelectedItem as RepeatModeItem).SetRepeatMode();
                }
                finally
                {
                    this.IsEnabled = true;
                }
            }
        }

        private void LoadState()
        {
            SetSelectedButton();
        }   

        private async void SettingsWrapper_RepeatModeChanged(object? sender, string e)
        {
            await DispatcherQueue.EnqueueAsync(() =>
            {
                SetSelectedButton();
            });
        }

        private async void RepeatRadioGroup_Unloaded(object? sender, RoutedEventArgs e)
        {
            SettingsService.Instance.RepeatModeChanged -= SettingsWrapper_RepeatModeChanged;      

            Unloaded -= RepeatRadioGroup_Unloaded;
        }

        private void SetSelectedButton()
        {
            cbRepeatModes.SelectionChanged -= CbRepeatModes_SelectionChanged;


            for (int i = 0; i < repeatModes.Count; i++)
            {
                if (repeatModes[i].RepeatMode == SettingsService.Instance.RepeatMode)
                {
                    cbRepeatModes.SelectedIndex = i;
                    this.Icon = new SymbolIcon(repeatModes[i].ModeSymbol);

                    break;
                }
            }


            cbRepeatModes.SelectionChanged += CbRepeatModes_SelectionChanged;
        }

        internal class RepeatModeItem
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

            public string RepeatMode
            {
                get;
                private set;
            }

            public RepeatModeItem(Symbol modeSymbol, string modeText, string repeatMode)
            {
                ModeSymbol = modeSymbol;
                ModeText = modeText ?? throw new ArgumentNullException(nameof(modeText));
                RepeatMode = repeatMode ?? throw new ArgumentNullException(nameof(repeatMode));
            }

            public async Task SetRepeatMode()
            {
                await AppState.Current.MediaServiceConnector.SetRepeatMode(RepeatMode);
            }
        }
    }
}
