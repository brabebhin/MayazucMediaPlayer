using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.Services
{
    internal static class FileCopyService
    {
        public static Task CopyFilesToFolderAsync(IEnumerable<string> sourcePaths, string destinationFolder)
        {
            return Task.Run(() =>
            {
                try
                {
                    foreach (string file in sourcePaths)
                    {
                        var destinationPath = Path.Combine(destinationFolder, Path.GetFileName(file));
                        Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(file, destinationPath, UIOption.AllDialogs, UICancelOption.ThrowException);
                    }
                }
                catch
                {
                    //user cancel, io issues etc.
                }
            });
        }
    }
}
