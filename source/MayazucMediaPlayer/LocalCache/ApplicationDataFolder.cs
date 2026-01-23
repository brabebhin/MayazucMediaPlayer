using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayazucMediaPlayer.LocalCache
{
    internal static class ApplicationDataFolder
    {
        public static string CurrentLocalFolderPath()
        {
            var rootPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var appFolderName = "MayazucMediaPlayer";
            var fullPath = Path.Combine(rootPath, appFolderName);

            return Directory.CreateDirectory(fullPath).FullName;
        }
    }
}
