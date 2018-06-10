using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaStorage.Common.Extensions;
using MediaStorage.Data.Media.Context;
using MediaStorage.Data.Media.Entities;
using MediaStorage.IO;

namespace MediaStorage.Core.Services
{
    internal class ImageResourceService : IImageResourceService
    {
        private readonly IMediaDataContext _mediaDataContext;
        private readonly IStorage _storage;

        public ImageResourceService(IStorage mediaStorage, IMediaDataContext dataContext)
        {
            _mediaDataContext = dataContext;
            _storage = mediaStorage;
        }
       
        public byte[] ReadImageResource(string imageGroupId, MediaImageSize imageSize, out string imageType)
        {
            imageType = string.Empty;

            Guid imageGroupGuid;
            if (!Guid.TryParse(imageGroupId, out imageGroupGuid))
                return null;

            var mediaImage = (from mi in _mediaDataContext.Get<MediaImage>()
                            where mi.ImageGroup.Id == imageGroupGuid &&
                            mi.ImageSize == imageSize && mi.IsArchived == false
                            select new { Format = mi.Format, Url = mi.Url })
                            .FirstOrDefault();
                            
            if (mediaImage == null || string.IsNullOrEmpty(mediaImage.Url))
                return null;

            byte[] imageData = null;
            imageType = MediaFormatExtension.GetMimeType(mediaImage.Format);

            using (var stream = _storage.Open(mediaImage.Url))
            {
                imageData = new byte[stream.Length];
                if (stream.Read(imageData, 0, imageData.Length) != imageData.Length)
                {
                    imageData = null;
                }
            }
            return imageData;
        }

    }
}
