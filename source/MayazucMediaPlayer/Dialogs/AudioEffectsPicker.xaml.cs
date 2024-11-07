
// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

using MayazucMediaPlayer.AudioEffects;

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class AudioEffectsPicker : BaseDialog
    {
        public AudioEffectsViewModel DataService
        {
            get;
            private set;
        }

        public AudioEffectsPicker()
        {
            InitializeComponent();
            DataService = new AudioEffectsViewModel(DispatcherQueue);
            DataContext = DataService;
        }


        protected override void OnPrimaryButtonClick()
        {
            DataService.SaveEffectsCommand.Execute(this);
            Hide();
        }
    }
}
