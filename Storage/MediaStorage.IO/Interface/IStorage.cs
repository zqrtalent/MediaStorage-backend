namespace MediaStorage.IO
{
    using System;
    using Stream = global::System.IO.Stream;

    public interface IStorage : IDisposable
    {
        IStorageFile Open(string itemPath);
        IStorageFile OpenAndRestoreState(string itemPath, string stateJson);
        bool Store(string itemPath, Stream stream, string contentType);
        bool Store(string itemPath, IStorageFile storageFile, string contentType);
        void Delete(string itemPath);
        long GetSize(string itemPath);
        bool Exists(string itemPath);
    }
}