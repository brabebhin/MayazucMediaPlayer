
// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

using MayazucMediaPlayer.AudioEffects;

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class AudioEffectsPicker : BaseDialog
    {
        public AudioEffectsViewModel EffectsModel
        {
            get;
            private set;
        }

        public AudioEffectsPicker()
        {
            InitializeComponent();
            EffectsModel = new AudioEffectsViewModel(DispatcherQueue);
            DataContext = EffectsModel;
        }


        protected override void OnPrimaryButtonClick()
        {
            EffectsModel.SaveEffectsCommand.Execute(this);
            Hide();
        }
    }
}
