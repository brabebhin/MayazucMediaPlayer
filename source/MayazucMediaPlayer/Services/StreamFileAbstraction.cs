using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace MayazucMediaPlayer.Services
{
    public class StreamFileAbstraction : TagLib.File.IFileAbstraction
    {
        public StreamFileAbstraction(string name, Stream stream)
        {
            Name = name;
            ReadStream = stream;
            WriteStream = stream;
        }

        public void CloseStream(Stream stream)
        {
            stream.Flush();
        }

        public string Name { get; private set; }
        public Stream ReadStream { get; private set; }
        public Stream WriteStream { get; private set; }


        public static async Task<TagLib.File> OpenTaglibSharpRead(StorageFile storageFile)
        {
            if (storageFile == null) throw new ArgumentNullException();
            using (var f = await storageFile.OpenStreamForReadAsync())
            {
                var file = TagLib.File.Create(new StreamFileAbstraction(storageFile.Name, f));

                return file;
            }
        }

        public static TagLib.File TaglibOpenWrite(Stream stream, StorageFile storageFile, bool dispose)
        {
            if (stream != null)
            {
                var file = TagLib.File.Create(new StreamFileAbstraction(storageFile.Name, stream));
                return file;
            }
            return null;
        }
    }
}
