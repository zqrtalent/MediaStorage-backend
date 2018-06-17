using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MediaStorage.IO.FileStream
{
    public class LocalStorage : ILocalStorage
    {
        public LocalStorage(string baseDirectoryName)
        {
            //BaseUrl = @"C:\Users\ZqrTalent\Desktop\Dev\WEB\Asp.Net\MediaStreamingApp\MediaStreamingApp\" + baseDirectoryName;
            //BaseUrl = HostingEnvironment.ApplicationPhysicalPath + baseDirectoryName;
            BaseUrl = baseDirectoryName;
            DirectoryDelimiter = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/");

            if (BaseUrl.Substring(BaseUrl.Length - 1, 1) != DirectoryDelimiter)
                BaseUrl += DirectoryDelimiter;
        }

        private string DirectoryDelimiter { get; set; }

        public string BaseUrl { get; private set; }

        protected string GetFullPath(string path)
        {
            string fullPath = BaseUrl;
            if (string.IsNullOrEmpty(path))
                return fullPath;
            if (path.Substring(0, 1) == DirectoryDelimiter)
                fullPath += path.Substring(1);
            else
                fullPath += path;
            return fullPath;
        }

        public IStorageFile StoreAndGetFileStream(string itemPath, Stream stream, string contentType)
        {
            if(Store(itemPath, stream, contentType))
            {
                return new LocalFileStream(itemPath, FileMode.Open);
            }
            return null;
        }

        public bool Store(string itemPath, IStorageFile storageFile, string contentType)
        {
            // long fileLength = storageFile.Length;
            // long offset = 0;
            // int chunkSize = 100 * 10024;
            // var buffer = new byte[chunkSize];

            // while (offset < fileLength)
            // {
            //     int read = storageFile.Read(buffer, 0, chunkSize);
            //     if (read == 0)
            //         break;
            //     to.Store(pathTo, buffer, 0, read, mimeType);
            //     offset += read;
            // }

            throw new NotImplementedException();
        }

        public IStorageFile StoreAndGetFileStream(string itemPath, IStorageFile storageFile, string contentType)
        {
            throw new NotImplementedException();
        }

        public bool Store(string itemPath, Stream stream, string contentType)
        {
            bool success = true;
            try
            {
                var fullPath = GetFullPath(itemPath);
                CreateDirectoryTree(fullPath, true);

                using (var file = File.Open(fullPath, FileMode.OpenOrCreate))
                {
                    file.Seek(0, SeekOrigin.End);
                    stream.Position = 0;

                    long streamLength = stream.Length;
                    long copiedLength = 0;
                    long chunkSize = Math.Min(1024 * 1024, stream.Length);
                    byte[] data = new byte[chunkSize];

                    while(copiedLength < streamLength)
                    {
                        int read = stream.Read(data, 0, (int)Math.Min(chunkSize, streamLength - copiedLength));
                        if (read == 0)
                        {
                            success = false;
                            break;
                        }
                        file.Write(data, 0, read);
                        copiedLength += read;
                    }
                }
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        public long GetSize(string path)
        {
            long size = 0;
            using (var file = File.Open(GetFullPath(path), FileMode.Open))
            {
                size = file.Length;
            }
            return size;
        }

        // public int Read(string path, byte[] buffer, int bufferOffset, int readSize, long position = 0)
        // {
        //     int size = 0;
        //     using (var file = File.Open(GetFullPath(path), FileMode.Open))
        //     {
        //         if (position > 0)
        //             file.Seek(position, SeekOrigin.Current);
        //         size = file.Read(buffer, bufferOffset, readSize);
        //     }
        //     return size;
        // }

        private bool CreateDirectoryTree(string path, bool containsFileName)
        {
            if(containsFileName)
            {
                int index = path.LastIndexOf(DirectoryDelimiter);
                if(index != -1)
                    path = path.Substring(0, index);
            }
            Directory.CreateDirectory(path);
            return true;
        }

        // public bool Store(string path, byte[] buffer, int bufferOffset, int size, string mimeType)
        // {
        //     if (string.IsNullOrEmpty(path))
        //         return false;

        //     var ret = false;
        //     using (var mem = (bufferOffset == 0 && buffer.Length == size) ? 
        //         new MemoryStream(buffer, false) : 
        //         new MemoryStream(buffer, bufferOffset, size))
        //     {
        //         ret = Store(path, mem, mimeType);
        //     }
        //     return ret;
        // }

        public IStorageFile Open(string itemPath)
        {
            return new LocalFileStream(GetFullPath(itemPath), FileMode.Open);
        }

        public IStorageFile OpenAndRestoreState(string itemPath, string stateJson)
        {
            return Open(itemPath);
        }

        public bool Exists(string path)
        {
            return System.IO.File.Exists(GetFullPath(path));
        }

        public void Delete(string itemPath)
        {
            try
            {
                File.Delete(GetFullPath(itemPath));
            }
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
        }
    }
}
