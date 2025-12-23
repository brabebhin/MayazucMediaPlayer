using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace MayazucMediaPlayer.FileSystemViews
{
    public class MayazucFileOpenPicker : IFileOpenPicker
    {
        readonly FileOpenPicker picker;

        public MayazucFileOpenPicker(FileOpenPicker _picker)
        {
            picker = _picker;
        }

        public PickerViewMode ViewMode
        {
            get => picker.ViewMode;
            set => picker.ViewMode = value;
        }
        public PickerLocationId SuggestedStartLocation
        {
            get => picker.SuggestedStartLocation;
            set => picker.SuggestedStartLocation = value;
        }
        public string SettingsIdentifier
        {
            get => picker.SettingsIdentifier;
            set => picker.SettingsIdentifier = value;
        }
        public string CommitButtonText
        {
            get => picker.CommitButtonText;
            set => picker.CommitButtonText = value;
        }

        public IList<string> FileTypeFilter
        {
            get => picker.FileTypeFilter;
        }


        public async Task<IReadOnlyList<FileInfo>> PickMultipleFilesAsync()
        {
            var storageFiles = await picker.PickMultipleFilesAsync();
            if (storageFiles != null)
            {
                List<FileInfo> files = new List<FileInfo>();
                files.AddRange(storageFiles.Select(f => f.ToFileInfo()));
            }

            return null;
        }

        public async Task<FileInfo> PickSingleFileAsync(string pickerOperationId)
        {
            var file = await picker.PickSingleFileAsync(pickerOperationId);
            if (file != null)
            {
                return file.ToFileInfo();
            }
            return null;
        }

        public async Task<FileInfo> PickSingleFileAsync()
        {
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                return file.ToFileInfo();
            }
            return null;
        }
    }
}
