using System;
using MediaStorage.Data.Media.Entities;

namespace MediaStorage.Core.Services
{
    public interface IImageResourceService
    {
        byte[] ReadImageResource(string imageGroupId, MediaImageSize imageSize, out string imageType);
    }
}
