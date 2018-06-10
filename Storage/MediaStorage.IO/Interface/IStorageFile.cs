namespace MediaStorage.IO
{
    using Stream = global::System.IO.Stream;
    using SeekOrigin = global::System.IO.SeekOrigin;
    using System;

    public interface IStorageFile : IDisposable
    {
        string FilePath { get; }
        string FileName { get; }
        long Length { get; }
        bool CanRead { get; }
        bool CanWrite { get; }
        bool CanSeek{ get; }

        bool UseReadBuffer(int bufferSize);

        // 
        // Reads entire data stores in byte array and returns as a result.
        //
        byte[] ReadAllBytes();
        
        // 
        // Reads data specifing file offset and numbers of bytes to read.
        //
        byte[] Read(long offset, uint bytesRead);

        // 
        // Reads data based on the current file offset (Seek position), specifing offset of array to write read bytes and numbers of bytes to read.
        //
        int Read(byte[] array, int offset, int count);

        //
        // Changes file read offset.
        //
        long Seek(long offset, SeekOrigin origin);

        //void Upload(byte[] bytesUpload, string contentType = "application/octet-stream");
        //void Upload(Stream streamUpload, string contentType = "application/octet-stream");

        void Write(byte[] array, int offset, int count);

        // Stores current file state in json format.
        string SaveStateAsJson();
    }
}