// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using static MayazucMediaPlayer.Help.LicensesPage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MayazucMediaPlayer.Help
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LicenseViewWindow : Window
    {
        public LicenseViewWindow()
        {
            InitializeComponent();
        }

        public async Task ShowAsync(LicenseFile file)
        {
            LicenseText.Text = await FileIO.ReadTextAsync(file.File);
            Title = file.Name;
            Activate();
        }
    }
}
