using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Dialogs
{
    public partial class BaseDialog : ContentDialog
    {
        public BaseDialog()
        {
            DefaultStyleKey = typeof(BaseDialog);
            PrimaryButtonStyle = (Style)App.CurrentInstance.Resources["DialogPrimaryButtonStyle"];

            SecondaryButtonStyle = (Style)App.CurrentInstance.Resources["DialogSecondaryButtonStyle"];
            Loaded += McBaseDialog_Loaded;
            Unloaded += McBaseDialog_Unloaded;
            base.PrimaryButtonClick += McBaseDialog_PrimaryButtonClick;
            base.SecondaryButtonClick += McBaseDialog_SecondaryButtonClick;
        }

        private void McBaseDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            OnSecondaryButtonClick();
        }

        private void McBaseDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            OnPrimaryButtonClick();
        }

        protected virtual void OnPrimaryButtonClick()
        {

        }

        protected virtual void OnSecondaryButtonClick() { }

        private void McBaseDialog_Unloaded(object? sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            KeyDown -= McBaseDialog_KeyDown;
        }

        private void McBaseDialog_Loaded(object? sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            KeyDown += McBaseDialog_KeyDown;
        }

        private void McBaseDialog_KeyDown(object? sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Enter:

                    if (IsPrimaryButtonEnabled)
                    {
                        OnPrimaryButtonClick();
                        Hide();
                    }
                    e.Handled = true;


                    break;
                case Windows.System.VirtualKey.Escape:
                    if (IsSecondaryButtonEnabled)
                    {
                        OnSecondaryButtonClick();
                    }
                    Hide();
                    e.Handled = true;

                    break;
            }
        }

        public async Task ShowAsync(XamlRoot root)
        {
            XamlRoot = root;
            await ShowAsync();
        }
    }
}
