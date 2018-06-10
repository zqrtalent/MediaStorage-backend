using System;
using System.IO;

namespace MediaStorage.IO
{
    public interface IFileStream : IDisposable
    {
        int Read(byte[] array, int offset, int count);
        void Write(byte[] array, int offset, int count);
        long Seek(long offset, SeekOrigin origin);
        bool CanRead { get; }
        bool CanWrite { get; }
        bool CanSeek { get; }
        long Length { get; }
    }
}
