using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace MayazucMediaPlayer.FileSystemViews
{
    public interface IFileOpenPicker
    {
        //
        // Summary:
        //     Shows the file picker so that the user can pick one file.
        //
        // Parameters:
        //   pickerOperationId:
        //     This argument is ignored and has no effect.
        //
        // Returns:
        //     When the call to this method completes successfully, it returns a StorageFile
        //     object that represents the file that the user picked.

        Task<FileInfo> PickSingleFileAsync(string pickerOperationId);
        //
        // Summary:
        //     Shows the file picker so that the user can pick one file.
        //
        // Returns:
        //     When the call to this method completes successfully, it returns a StorageFile
        //     object that represents the file that the user picked.
        Task<FileInfo> PickSingleFileAsync();
        //
        // Summary:
        //     Shows the file picker so that the user can pick multiple files. (UWP app)
        //
        // Returns:
        //     When the call to this method completes successfully, it returns a filePickerSelectedFilesArray
        //     object that contains all the files that were picked by the user. Picked files
        //     in this array are represented by storageFile objects.
        Task<IReadOnlyList<FileInfo>> PickMultipleFilesAsync();


        //
        // Summary:
        //     Gets or sets the view mode that the file open picker uses to display items.
        //
        // Returns:
        //     The view mode.
        PickerViewMode ViewMode { get; set; }
        //
        // Summary:
        //     Gets or sets the initial location where the file open picker looks for files
        //     to present to the user.
        //
        // Returns:
        //     The identifier of the starting location.
        PickerLocationId SuggestedStartLocation { get; set; }
        //
        // Summary:
        //     Gets or sets the settings identifier associated with the state of the file open
        //     picker.
        //
        // Returns:
        //     The settings identifier.
        string SettingsIdentifier { get; set; }
        //
        // Summary:
        //     Gets or sets the label text of the file open picker's commit button.
        //
        // Returns:
        //     The label text.
        string CommitButtonText { get; set; }
        //
        // Summary:
        //     Gets the collection of file types that the file open picker displays.
        //
        // Returns:
        //     A fileExtensionVector object that contains a collection of file types (file name
        //     extensions) , such as ".doc" and ".png". File name extensions are stored in this
        //     array as string objects.
        IList<string> FileTypeFilter { get; }


    }
}
