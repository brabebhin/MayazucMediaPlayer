using Microsoft.UI.Xaml;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class ConfirmationDialog : BaseDialog
    {
        public bool Result
        {
            get; private set;
        }


        public string SubTitle
        {
            get; set;
        }

        public ConfirmationDialog()
        {
            InitializeComponent();
            IsPrimaryButtonEnabled = true;
        }

        public ConfirmationDialog(string title, string subtitle) : this()
        {
            Title = title;
            body.Text = subtitle;
        }

        protected override void OnSecondaryButtonClick()
        {
            Result = false;
            Hide();
        }

        protected override void OnPrimaryButtonClick()
        {
            Result = true;
            Hide();
        }

        private void OK_Click(object? sender, RoutedEventArgs e)
        {

        }
    }

}
