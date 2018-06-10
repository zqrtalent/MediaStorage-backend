using System;
using System.Collections.Generic;
using System.IO;
using MediaStorage.Common.Extensions;
using MediaStorage.Common.Dtos.Upload;

namespace MediaStorage.Core.Services
{
    public interface IMediaUploadService
    {
        bool UploadMedia(Stream media, string fileName, Guid userId, string mediaFormat = MediaFormatExtension.AudioMp3);
        bool SyncUploadMedia(Guid userId, UploadSync uploadInfo);
        IEnumerable<UploadSync> GetSyncUpload(Guid userId);
    }
}
