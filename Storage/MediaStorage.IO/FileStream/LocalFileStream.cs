using System;
using System.IO;
using IO = System.IO;

namespace MediaStorage.IO.FileStream
{
    public class LocalFileStream : IStorageFile, IDisposable
    {
        public LocalFileStream(string filePath, System.IO.FileMode mode)
        {
            _fileStream = System.IO.File.Open(filePath, mode);
            FilePath = filePath;
        }

        private System.IO.FileStream _fileStream;

        public bool CanRead { get { return _fileStream.CanRead; } }
        public bool CanSeek { get { return _fileStream.CanSeek; } }
        public bool CanWrite { get { return _fileStream.CanWrite; } }
        public long Length { get { return _fileStream.Length; } }

        public string FilePath { get; private set; }
        public string FileName => throw new NotImplementedException();

        public void Dispose()
        {
            //_fileStream.Close();
            _fileStream.Dispose();
        }

        public bool UseReadBuffer(int bufferSize)
        {
            return false;
        }

        public byte[] Read(long offset, uint bytesRead)
        {
            throw new NotImplementedException();
        }

        public int Read(byte[] array, int offset, int count)
        {
            return _fileStream.Read(array, offset, count);
        }

        public long Seek(long offset, System.IO.SeekOrigin origin)
        {
            return _fileStream.Seek(offset, origin);
        }

        public void Write(byte[] array, int offset, int count)
        {
            _fileStream.Write(array, offset, count);
        }

        public byte[] ReadAllBytes()
        {
            var arrContent = new byte[_fileStream.Length];
            if(_fileStream.CanSeek)
                _fileStream.Seek(0, SeekOrigin.Begin);
            return (_fileStream.Read(arrContent, 0, arrContent.Length) == arrContent.Length) ? arrContent : null;
        }

        public string SaveStateAsJson()
        {
            throw new NotImplementedException();
        }
    }
}
