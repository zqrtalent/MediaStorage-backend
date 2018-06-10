using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Http;
using Google.Apis.Services;

namespace MediaStorage.IO.GoogleDrive
{
    public class GoogleDriveStorage : IStorage
    {
        public GoogleDriveStorage(string appName, string clientId, string clientSecret)
        {
            _appName = appName;
            _clientId = clientId;
            _clientSecret = clientSecret;
            //_cancelToken = new CancellationToken();
            Authenticate();
        }

        public GoogleDriveStorage(string appName, string serviceAccountCredentialJsonFilePath)
        {
            _appName = appName;
            _serviceAccountCredentialJsonFilePath = serviceAccountCredentialJsonFilePath;
            //_cancelToken = new CancellationToken();
            Authenticate();
        }

        #region Variables
        private string _appName;
        private string _clientId;
        private string _clientSecret;
        private string _serviceAccountCredentialJsonFilePath;
        private DriveService _service;
        //private CancellationToken _cancelToken;

        protected static string _mimeTypeFolder = @"application/vnd.google-apps.folder";
        protected static string _mimeTypeAudio = @"application/vnd.google-apps.audio";
        protected static string _mimeTypeJpg = @"application/vnd.google-apps.photo";
        protected static string _mimeTypeFile = @"application/vnd.google-apps.file";
        #endregion

        protected void Authenticate()
        {
            IConfigurableHttpClientInitializer credentials = null;
            string[] arrScopes = new string[] { DriveService.Scope.Drive };
            if(!string.IsNullOrEmpty(_serviceAccountCredentialJsonFilePath))
            {
                credentials = GoogleCredential.FromStream(System.IO.File.OpenRead(_serviceAccountCredentialJsonFilePath))
                    .CreateScoped(arrScopes);
            }
            else
            {
                credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                                    new ClientSecrets 
                                    { 
                                        ClientId = _clientId, 
                                        ClientSecret = _clientSecret 
                                    }, arrScopes, "user", CancellationToken.None).Result;
            }

            // Create Drive API service.
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = _appName,
            });
        }

        public IStorageFile Open(string itemPath)
        {
           return OpenAndUpload(itemPath, false, null, string.Empty);
        }

        public IStorageFile OpenAndRestoreState(string itemPath, string stateJson)
        {
           return new GoogleDriveFile(_service, stateJson);
        }

        private bool ExtractFileId(string itemPath, out string fileId)
        {
            fileId = itemPath.StartsWith("gfileid://") ? itemPath.Substring("gfileid://".Length) : string.Empty;
            return !string.IsNullOrEmpty(fileId);
        }

        private IStorageFile OpenAndUpload(string itemPath, bool createIfNotExists, Stream uploadStream, string uploadContentType)
        {
            IStorageFile file = null;
            string fileId = null;
            ExtractFileId(itemPath, out fileId);
            string fileName = string.Empty;

            if(string.IsNullOrEmpty(fileId))
            {
                var subFolders = itemPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if(subFolders == null || subFolders.Length == 0)
                    throw new ArgumentException("Invalid file path parameter!");

                var parentId = string.Empty;
                if(subFolders.Length > 1)
                {
                    var parentIds = GetFolders(subFolders, subFolders.Length-1, createIfNotExists);
                    if(parentIds == null || !parentIds.Any())
                        return null;
                    parentId = parentIds.Last();
                }

                fileId = GetFileId(subFolders.Last(), _mimeTypeFile, parentId, false);
                if(string.IsNullOrEmpty(fileId))
                {
                    fileName = subFolders.Last();
                    if(createIfNotExists)
                    {
                        fileId = CreateFile(subFolders.Last(), _mimeTypeFile, parentId, uploadStream, uploadContentType);
                        if(string.IsNullOrEmpty(fileId))
                            throw new InvalidOperationException($"Unable to store file ({itemPath}) !");
                    }
                }
            }

            if(!string.IsNullOrEmpty(fileId))
            {
                if(uploadStream != null)
                    throw new NotSupportedException("Overwrite of existing file operation is not supported !");
                file = new GoogleDriveFile(_service, fileId, fileName);
            }
            return file;
        }

        public bool Store(string itemPath, Stream stream, string contentType = "application/octet-stream")
        {
            var file = OpenAndUpload(itemPath, true, stream, contentType);
            return (file != null);
        }

        public bool Store(string itemPath, IStorageFile storageFile, string contentType = "application/octet-stream")
        {
            bool ret = false;
            using(var mem = new MemoryStream(storageFile.ReadAllBytes()))
            {
                ret = Store(itemPath, mem, contentType);
            }
            return ret;
        }

        public void Delete(string itemPath)
        {
            var fileId = GetFileId(itemPath, _mimeTypeFile, false);
            if(!string.IsNullOrEmpty(fileId))
            {
                var request = _service.Files.Delete(fileId);
                request.Execute();
            }
        }

        private IEnumerable<string> GetFolders(string[] folderNames, int length = -1, bool createIfNotExists = false)
        {
            if(folderNames == null || folderNames.Length == 0)
                throw new ArgumentException("Invalid folder path parameter!");
            
            IList<string> folderIds = new List<string>();
            string parentId = string.Empty;

            if(length == -1)
                length = folderNames.Length;
            for(int i=0; i<length; i++)
            {
                parentId = GetFolderId(folderNames[i], parentId, createIfNotExists);
                if(string.IsNullOrEmpty(parentId))
                    break;
                folderIds.Add(parentId);
            }

            return folderIds;
        }

        private string GetFileId(string filePath, string mimeType, bool createIfNotExists)
        {
            var folders = filePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if(folders == null || folders.Length == 0)
                throw new ArgumentException("Invalid file path parameter!");
            
            string parentId = string.Empty;
            for(int i=0; i<folders.Length-1; i++)
            {
                parentId = GetFolderId(folders[i], parentId, createIfNotExists);
                if(string.IsNullOrEmpty(parentId))
                    return string.Empty;
            }

            return GetFileId(folders[folders.Length-1], mimeType, parentId, createIfNotExists);
        }

        private string GetFolderId(string folderName, string parentId, bool createIfNotExists)
        {
            return GetFileId(folderName, _mimeTypeFolder, parentId, createIfNotExists);
        }

        private string GetFileId(string fileName, string mimeType, string parentId, bool createIfNotExists)
        {
            var request = _service.Files.List();
            request.Q = string.IsNullOrEmpty(parentId) ? $"name='{fileName}'" : 
                 $"parents in '{parentId}' and name='{fileName}'";
            request.Spaces = "drive";
            request.Fields = "files(id)";
            
            var result = request.Execute();
            var file = result.Files.FirstOrDefault();
            var fileId = file?.Id ?? string.Empty;
            if(string.IsNullOrEmpty(fileId) && createIfNotExists)
            {
                if(mimeType == _mimeTypeFolder)
                    fileId = CreateFolder(fileName, parentId);
                else
                    fileId = CreateFile(fileName, mimeType, parentId);
            }
            return fileId;
        }

        public string CreateFolder(string folderName, string parentId)
        {
            if(string.IsNullOrEmpty(folderName))
                throw new ArgumentException("folderName must not be empty or null!");

            var folderMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = _mimeTypeFolder,
                Parents = !string.IsNullOrEmpty(parentId) ? new string[] { parentId }.ToList() : null,
            };

            var request = _service.Files.Create(folderMetadata);
            request.Fields = "id";
            var file = request.Execute();
            return file?.Id ?? string.Empty;
        } 

        private string CreateFile(string fileName, string mimeType, string parentId, Stream uploadStream = null, string contentType = null)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = !string.IsNullOrEmpty(parentId) ? new string[] { parentId }.ToList() : null,
            };

            // Upload file.
            FilesResource.CreateMediaUpload request = _service.Files.Create(fileMetadata, 
                uploadStream ?? new MemoryStream(), 
                string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType);
            request.Fields = "id";
            request.Upload();

            var file = request.ResponseBody;
            return file?.Id ?? string.Empty;
        }

        // private string CreateFileAndUploadPartial(string fileName, string mimeType, string parentId, string contentType, Func<byte[]> funcPartialUpload)
        // {
        //     var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        //     {
        //         Name = fileName,
        //         Parents = !string.IsNullOrEmpty(parentId) ? new string[] { parentId }.ToList() : null,
        //     };

        //     // Upload file.
        //     FilesResource.CreateMediaUpload request = _service.Files.Create(fileMetadata, 
        //         uploadStream ?? new MemoryStream(), 
        //         string.IsNullOrEmpty(contentType) ? "application/octet-stream" : contentType);
        //     request.Fields = "id";
        //     request.Upload();

            

        //     var file = request.ResponseBody;
        //     return file?.Id ?? string.Empty;
        // }

        private string[] GetParentIds(string fileId)
        {
            // Retrieve the existing parents.
            var getRequest = _service.Files.Get(fileId);
            getRequest.Fields = "parents";
            var file = getRequest.Execute();
            return file.Parents != null ? file.Parents.ToArray() : null;
        }

        public long GetSize(string itemPath)
        {
            long size = 0;
            using(var file = Open(itemPath))
            {
                size = file.Length;
            }
            return size;
        }

        public bool Exists(string itemPath)
        {
            var fileId = GetFileId(itemPath, _mimeTypeFile, false);
            return !string.IsNullOrEmpty(fileId);
        }

        public void Dispose()
        {
            if(_service != null)
                _service.Dispose();
        }
    }
}