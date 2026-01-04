using System;
using System.Collections.Generic;
using System.IO;

namespace MayazucMediaPlayer.Services
{
    public partial class FileInfoCollection : List<FileInfo>
    {
        readonly Func<string, bool> Filter;

        public FileInfoCollection() : this((s) => { return SupportedFileFormats.IsSupportedMedia(s); })
        {
        }

        public FileInfoCollection(Func<string, bool> filter)
        {
            Filter = filter;
        }

        public new bool Add(FileInfo fileInfo)
        {
            if (Filter(fileInfo.FullName.ToLowerInvariant()))
            {
                base.Add(fileInfo);
                return true;
            }
            return false;
        }


        public new int AddRange(IEnumerable<FileInfo> fileInfo)
        {
            int added = 0;
            foreach (var f in fileInfo)
                if (Add(f)) added++;
            return added;
        }
    }
}
