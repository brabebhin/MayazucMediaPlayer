using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace MayazucMediaPlayer.Services
{
    internal class FileInfoPathComparer : IEqualityComparer<FileInfo>
    {
        public bool Equals(FileInfo x, FileInfo y)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(x.FullName, y.FullName);
        }

        public int GetHashCode([DisallowNull] FileInfo obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}