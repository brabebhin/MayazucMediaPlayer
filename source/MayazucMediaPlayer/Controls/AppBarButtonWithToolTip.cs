using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MayazucMediaPlayer.Controls
{
    public partial class AppBarButtonWithToolTip : AppBarButton
    {
        private long labelChangedToken;

        public AppBarButtonWithToolTip()
        {
            labelChangedToken = RegisterPropertyChangedCallback(AppBarButton.LabelProperty, ToolTipChangedCallback);
        }

        private void ToolTipChangedCallback(Microsoft.UI.Xaml.DependencyObject sender, Microsoft.UI.Xaml.DependencyProperty dp)
        {
            ToolTipService.SetToolTip(this, this.Label);
        }
    }

    public partial class AppBarToggleButtonWithToolTip: AppBarToggleButton
    {
        private long labelChangedToken;

        public AppBarToggleButtonWithToolTip()
        {
            labelChangedToken = RegisterPropertyChangedCallback(AppBarButton.LabelProperty, ToolTipChangedCallback);
        }

        private void ToolTipChangedCallback(Microsoft.UI.Xaml.DependencyObject sender, Microsoft.UI.Xaml.DependencyProperty dp)
        {
            ToolTipService.SetToolTip(this, this.Label);
        }
    }
}
