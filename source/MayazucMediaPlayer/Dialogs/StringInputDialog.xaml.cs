using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Linq;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class StringInputDialog : BaseDialog
    {
        public string Result
        {
            get; private set;
        }
        public string SubTitle
        {
            get; set;
        }

        public StringInputDialog()
        {
            InitializeComponent();
            textInput.TextChanged += TextInput_TextChanged;
            IsPrimaryButtonEnabled = false;
        }

        private void TextInput_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (Validator == null)
            {
                IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textInput.Text);
            }
            else
            {
                IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textInput.Text) && Validator(GetResultInternal());
            }
        }

        public StringInputDialog(string title, string subtitle) : this()
        {
            Title = title;
            textInput.Header = subtitle;
        }

        /// <summary>
        /// a Prefix to attach to the input string
        /// </summary>
        public string Prefix
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// A subfix to attach to the input string
        /// </summary>
        public string Subfix { get; set; } = string.Empty;

        public Func<string, bool> Validator
        {
            get;
            set;
        }

        public static bool FileNameValidator(string input)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var i in input)
            {
                if (invalid.Contains(i))
                {
                    return false;
                }
            }

            return true;
        }

        protected override void OnSecondaryButtonClick()
        {
            Result = null;
            Hide();
        }

        protected override void OnPrimaryButtonClick()
        {
            Result = GetResultInternal();
            Hide();
        }

        private string GetResultInternal()
        {
            return Prefix + textInput.Text + Subfix;
        }
    }
}
