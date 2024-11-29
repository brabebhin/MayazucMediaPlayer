using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Common;
using MayazucMediaPlayer.Navigation;
using Microsoft.UI.Xaml;
using System;

namespace MayazucMediaPlayer.Settings
{
    public partial class SettingsButton : SettingsItem
    {
        private static void navigate(Object obj)
        {
            var button = (SettingsButton)obj;
            button.CurrentFrame.NavigateAsync(button.TargetPageType, button.NavigationArgument);
        }

        public override DataTemplate Template
        {
            get
            {
                return TemplatesDictionary.ButtonWithImageAndText;
            }
        }

        public string Label
        {
            get; set;
        }

        public string ButtonIcon
        {
            get; set;
        }


        public SettingsButton(string settingsWrapperPropertyName) : base(settingsWrapperPropertyName)
        {
        }

        public object NavigationArgument
        {
            get; set;
        }

        public Type TargetPageType
        {
            get; set;
        }

        public DependencyInjectionFrame CurrentFrame
        {
            get; set;
        }

        public virtual RelayCommand<object> Command
        {
            get; set;
        } = new RelayCommand<object>(navigate);

        protected override void RecheckValueInternal()
        {
            //no-op
        }
    }
}
