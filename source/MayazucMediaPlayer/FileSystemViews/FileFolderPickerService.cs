using Windows.Storage.Pickers;
using WinRT;

namespace MayazucMediaPlayer.FileSystemViews
{
    public static class FileFolderPickerService
    {
        public static FileOpenPicker GetFileOpenPicker()
        {
            var picker = new FileOpenPicker();
            var initializeWithWindowWrapper = picker.As<IInitializeWithWindow>();
            initializeWithWindowWrapper.Initialize(App.GetActiveWindow());
            return picker;
        }

        public static FolderPicker GetFolderPicker()
        {
            var picker = new FolderPicker();
            var initializeWithWindowWrapper = picker.As<IInitializeWithWindow>();
            initializeWithWindowWrapper.Initialize(App.GetActiveWindow());
            return picker;
        }
    }
}
