using CommunityToolkit.Mvvm.Input;
using MayazucMediaPlayer.Navigation;
using Microsoft.UI.Xaml;
using System;

namespace MayazucMediaPlayer.Settings
{
    public partial class SettingsButton : SettingsItem
    {
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


        public SettingsButton(string settingsWrapperPropertyName, Action<object> setValueCallback, Func<object> getValueCallback) : base(settingsWrapperPropertyName, setValueCallback, getValueCallback)
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
        }

        protected override void RecheckValueInternal()
        {
            //no-op
        }
    }
}
