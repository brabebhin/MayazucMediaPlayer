using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MayazucMediaPlayer
{
    public static class StreamExtensions
    {
        public static async Task<InMemoryRandomAccessStream> AsInMemoryRandomAccessStream(this Stream stream)
        {
            var imras = new InMemoryRandomAccessStream();

            await imras.WriteAsync((await stream.ReadToEndAsync()).AsBuffer());
            imras.Seek(0);
            return imras;
        }

        public static async Task<byte[]> ReadToEndAsync(this Stream stream)
        {
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
