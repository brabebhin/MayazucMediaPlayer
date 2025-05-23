﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.MediaCollections
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NetworkStreamsCollectionPage : BasePage
    {
        public override string Title => "Network streams";

        public NetworkStreamsCollectionUiService DataService { get; private set; }

        public NetworkStreamsCollectionPage()
        {
            this.InitializeComponent();
            DataService = new NetworkStreamsCollectionUiService(DispatcherQueue);
            DataContext = DataService;
        }

        protected override async Task OnInitializeStateAsync(object? parameter)
        {
            await DataService.LoadHistory();
            await base.OnInitializeStateAsync(parameter);
        }

        private async void PlayUrlHistory_click(object sender, RoutedEventArgs e)
        {
            await DataService.PlayUrl(sender.GetDataContextObject<NetworkStreamHistoryEntry>().Url);
        }
    }
}
