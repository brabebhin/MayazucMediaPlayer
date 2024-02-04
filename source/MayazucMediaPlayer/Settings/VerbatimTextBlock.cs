using Microsoft.UI.Xaml;

namespace MayazucMediaPlayer.Settings
{
    public class VerbatimTextBlock : SettingsItem
    {
        public override DataTemplate Template
        {
            get
            {
                return TemplatesDictionary.PlainTextBlock;
            }
        }

        public string TextDescription
        {
            get
            {
                return textDescription;
            }
            set
            {
                textDescription = value;
                NotifyPropertyChanged(nameof(TextDescription));
            }
        }



        string textDescription;

        public VerbatimTextBlock() : base(string.Empty)
        {

        }

        protected override void RecheckValueInternal()
        {
            //NOOP
        }
    }
}
