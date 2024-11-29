using MayazucMediaPlayer.Runtime;
using MayazucMediaPlayer.Services;
using MayazucMediaPlayer.VideoEffects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Popups;
using Windows.UI.Text;
// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Dialogs
{
    public sealed partial class VideoColorProfileManagementDIalog : BaseDialog
    {
        readonly IEnumerable<SavedColorProfileUIWrapper> ItemsSource;

        public VideoEffectsPageViewModel Model
        {
            get;
            private set;
        }

        public VideoColorProfileManagementDIalog(VideoEffectsPageViewModel model)
        {
            InitializeComponent();
            DataContext = Model = model;
            lsvItems.ItemsSource = ItemsSource = model.SavedColorProfiles.Select(x => new SavedColorProfileUIWrapper(x)).ToArray();
        }

        protected override async void OnPrimaryButtonClick()
        {
            var profilesToDelete = ItemsSource.Where(x => x.MarkedForDeletion);
            var profileNames = string.Join(Environment.NewLine, profilesToDelete.Select(x => x.Title));
            await MessageDialogService.Instance.ShowMessageDialogAsync($"Are you sure you want to delete color profiles bellow{Environment.NewLine}{Environment.NewLine}{profileNames}?", "Confirm delete",
            new UICommand("Yes", async (a) =>
            {
                foreach (var profile in profilesToDelete)
                    await Model.DeleteColorProfileAsync(profile.Data);
            }),
            new UICommand("No", (a) => { }));
        }

        private void DeleteProfile(object? sender, RoutedEventArgs e)
        {
            var profileName = sender.GetDataContextObject<SavedColorProfileUIWrapper>();
            profileName.MarkedForDeletion = !profileName.MarkedForDeletion;
        }


    }

    public partial class SavedColorProfileUIWrapper : ObservableObject
    {
        public SavedColorProfile Data
        {
            get;
            private set;
        }

        bool _markedForDeletion;
        public bool MarkedForDeletion
        {
            get => _markedForDeletion;
            set
            {
                if (_markedForDeletion == value) return;

                _markedForDeletion = value;
                if (_markedForDeletion)
                {
                    ButtonIcon = Symbol.Refresh;
                    TextDecoration = Windows.UI.Text.TextDecorations.Strikethrough;
                }
                else
                {
                    ButtonIcon = Symbol.Delete;
                    TextDecoration = TextDecorations.None;
                }

                NotifyPropertyChanged(nameof(MarkedForDeletion));
                NotifyPropertyChanged(nameof(ButtonIcon));
                NotifyPropertyChanged(nameof(TextDecoration));
                NotifyPropertyChanged(nameof(Title));
            }
        }

        public Symbol ButtonIcon
        {
            get;
            private set;
        } = Symbol.Delete;

        public TextDecorations TextDecoration
        {
            get;
            private set;
        } = TextDecorations.None;

        public string Title
        {
            get;
            private set;
        }

        public SavedColorProfileUIWrapper(SavedColorProfile data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Title = data.Name;
        }
    }
}
