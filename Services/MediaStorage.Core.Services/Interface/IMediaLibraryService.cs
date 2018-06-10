using System;
using MediaStorage.Common.Dtos.MediaLibrary;

namespace MediaStorage.Core.Services
{
    public interface IMediaLibraryService
    {
        MediaLibraryInfo GetLibraryInfo(DateTime? updatedSince = null);
    }
}
