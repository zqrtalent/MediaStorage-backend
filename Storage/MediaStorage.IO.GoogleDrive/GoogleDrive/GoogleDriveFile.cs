using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Newtonsoft.Json;

namespace MediaStorage.IO.GoogleDrive
{
    public class GoogleDriveFile : IStorageFile
    {
        public GoogleDriveFile(DriveService service, string fileId, string fileName)
        {
            _service = service;
            _readBufferOffset = 0;
            _readBufferSize = 0;
            _buffer = null;

            _fileId = fileId;
            FileName = fileName;
            FilePath = $"gfileid://{fileId}";
        }

        public GoogleDriveFile(DriveService service, string stateJson)
        {
            var state = JsonConvert.DeserializeObject<GoogleDriveFileState>(stateJson);
            if(state == null)
                throw new InvalidOperationException($"Unable to initialize {nameof(GoogleDriveFile)} from state in json format!");

            _service = service;
            _readBufferOffset = 0;
            _readBufferSize = 0;
            _buffer = null;

            _offset = 0;
            _fileId = state.FileId;
            FileName = state.FileName;
            FilePath = state.FilePath;
            _length = state.Length;
            
        }

        private readonly string _fileId;
        private DriveService _service;
        
        /*
        Read buffer members.
         */
        private int _readBufferSize;
        private int _readBufferOffset;
        private byte[] _buffer;

        private int ReadBufferLength { get { return _buffer?.Length ?? 0; } }
        private int ReadBufferSizeUsed { get { return _readBufferOffset; } }
        private int ReadBufferSizeAvailable { get { return Math.Max(0, _readBufferSize - _readBufferOffset); } }
        private long OffsetStart_ReadBuffer { get { return _offset - ReadBufferSizeUsed; } }
        private long OffsetEnd_ReadBuffer { get { return _offset + ReadBufferSizeAvailable; } }

        public string FilePath { get; private set; }
        public string FileName { get; private set; }

        private long _length = -1;
        public long Length 
        {
            get
            {
                if(_length == -1)
                {
                    _length = GetFileLength();
                }
                return _length;
            }
        }

        private long _offset = 0; // Current read offset.

        public bool CanRead { get { return true; } }

        public bool CanWrite { get { return false; } }

        public bool CanSeek { get { return true; } }

        public bool UseReadBuffer(int bufferSize)
        {
            _readBufferOffset = 0;
            _readBufferSize = 0;
            _buffer = bufferSize > 0 ? new byte[bufferSize] : null;
            return true;
        }

        private bool ReadFromBuffer(long offset, uint bytesRead, out byte[] readData)
        {
            readData = null;
         
            if(ReadBufferSizeAvailable >= bytesRead)
            {
                if(offset < OffsetStart_ReadBuffer || offset > OffsetEnd_ReadBuffer - bytesRead)
                    return false;

                // Read data from buffer.
                readData = new byte[bytesRead];
                Array.Copy(_buffer, _readBufferOffset, readData, 0, bytesRead);

                // Advance read buffer offset.
                _readBufferOffset += (int)bytesRead;
            }
            return readData != null;
        }

        private void UpdateReadBuffer(Stream stream)
        {
            stream.Position = 0;
            stream.Read(_buffer, 0, (int)stream.Length);
            _readBufferOffset = 0;
            _readBufferSize = (int)stream.Length;
        }

        private void ClearReadBuffer()
        {
             _readBufferOffset = 0;
             _readBufferSize = 0;
        }

        public byte[] Read(long offset, uint bytesRead)
        {
            if(bytesRead == 0)
                return new byte[0];

            if(bytesRead == 5)
            {
                int n = 9;
            }

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Id = _fileId
            };

            byte[] readData = null;
            if(ReadFromBuffer(offset, bytesRead, out readData))
            {
                // Adjust offset variable.
                _offset = offset + readData?.Length ?? 0;
                return readData;
            }

            using(var stream = new MemoryStream())
            {
#if DEBUG
                var sw = new Stopwatch();
                sw.Start();
#endif

                long bytesDownloaded = 0;
                bool downloadFailed = false;
                FilesResource.GetRequest request = _service.Files.Get(_fileId);
                request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            {
                                bytesDownloaded += progress.BytesDownloaded;
                                break;
                            }
                        case DownloadStatus.Completed:
                            {
                                break;
                            }
                        case DownloadStatus.Failed:
                            {
                                downloadFailed = true;
                                break;
                            }
                    }
                };


                // Download entire file.
                if(offset == 0 && bytesRead == UInt32.MaxValue)
                    request.Download(stream);
                else
                    request.DownloadRange(stream, new RangeHeaderValue(offset, offset + Math.Max(bytesRead, ReadBufferLength) - 1 ));
                
                if(downloadFailed)
                    throw new InvalidOperationException($"File(fileId = '{_fileId}') download has failed!");
#if DEBUG
                sw.Stop();
                Console.WriteLine($"{_offset}: File size download: {stream.Length} Duration: {sw.ElapsedMilliseconds}ms");
#endif
                if(stream.Length > 0)
                {
                    // Keep read bytes in buffer.
                    if(bytesRead < ReadBufferLength)
                    {
                        UpdateReadBuffer(stream);
                        ReadFromBuffer(offset, bytesRead, out readData);
                    }
                    else
                    {
                        stream.Position = 0;
                        readData = stream.ToArray();
                        ClearReadBuffer();
                    }

                    // Adjust offset variable.
                    _offset = offset + readData?.Length ?? 0;
                }
            }
        
            return readData;
        }

        public byte[] ReadAllBytes()
        {
            // Read entire file.
            return Read(0, UInt32.MaxValue);
        }

        public int Read(byte[] array, int offset, int count)
        {
            byte[] readData = Read(_offset, (uint)count);
            if(readData == null || readData.Length == 0)
                return 0;
            Array.Copy(readData, 0, array, offset, count);
            return count;
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            long offsetNew = 0;
            switch(origin)
            {
                case SeekOrigin.Begin:
                {
                    if(offset < 0)
                        throw new ArgumentException($"Invalid offset ({offset}) paameter!");
                    offsetNew = offset;    
                    break;
                }
                case SeekOrigin.End:
                {
                    if(offset > 0) 
                        throw new ArgumentException($"Invalid offset ({offset}) paameter!");
                    offsetNew = Length < offset ? 0 : (Length - offset);
                    break;
                }
                case SeekOrigin.Current:
                {
                    offsetNew = (_offset + offset);
                    if(offsetNew >= Length)
                        offsetNew = Length - 1;
                    else
                    if(offsetNew < 0)
                        offsetNew = 0;
                    break;
                }
            }

            // Adjust read buffer.
            if( (ReadBufferSizeAvailable + ReadBufferSizeUsed) > 0)
            {
                if(offsetNew >= OffsetStart_ReadBuffer && offsetNew < OffsetEnd_ReadBuffer)
                    _readBufferOffset = (int)(offsetNew - OffsetStart_ReadBuffer);
                else
                    ClearReadBuffer();
            }

            _offset = offsetNew;
            return _offset;
        }

        public string SaveStateAsJson()
        {
            var state = new GoogleDriveFileState
            {
                FileId = _fileId,
                FileName = FileName,
                FilePath = FilePath,
                Offset = _offset,
                Length = _length
            };

            return JsonConvert.SerializeObject(state);
        }

        public void Write(byte[] array, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            _service = null;
        }
        
        private long GetFileLength()
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Id = _fileId,
            };

            var req = _service.Files.Get(_fileId);
            req.Fields = "id,size";
            var file = req.Execute();
            if(file != null)
                return file.Size.HasValue ? file.Size.Value : 0;
            return 0;
        }
/* 
        public void Upload(byte[] bytesUpload, string contentType = "application/octet-stream")
        {
            throw new NotSupportedException("Google drive api doesn't support upload functionality!");
            // using(var mem = new MemoryStream(bytesUpload))
            // {
            //     Upload(mem, contentType);
            // }
        }

        public void Upload(Stream streamUpload, string contentType = "application/octet-stream")
        {
            throw new NotSupportedException("Google drive api doesn't support upload functionality!");
            // var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            // {
            //     Id = _fileId
            // };

            // FilesResource.UpdateMediaUpload request = _service.Files.Update(fileMetadata, _fileId, 
            //         streamUpload, string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType);
            // request.Fields = "id";
            // request.Upload();

            // var file = request.ResponseBody;
            // if(file != null)
            // {
            // }
        }
*/
    }

}