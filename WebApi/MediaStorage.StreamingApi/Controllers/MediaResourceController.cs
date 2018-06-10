using System;
using System.Net;
using MediaStorage.Core.Services;
using MediaStorage.Data.Media.Entities;
using MediaStorage.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MediaStorage.StreamingApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class MediaResourceController : BaseAuthController
    {
        private readonly IImageResourceService _imageResourceService;
        public MediaResourceController(IImageResourceService imageResourceService, IOptions<AuthSettings> authSettings) : base(authSettings)
        {
            _imageResourceService = imageResourceService;
        }

        [HttpGet, Route("image")]
        public IActionResult GetImage([FromQuery]string imageId, [FromQuery]string sizeType)
        {
            MediaImageSize size = MediaImageSize.Default;
            if(!string.IsNullOrEmpty(sizeType))
            {
                if (sizeType.ToLower() == MediaImageSize.Large.ToString().ToLower())
                    size = MediaImageSize.Large;
                else
                if (sizeType.ToLower() == MediaImageSize.Medium.ToString().ToLower())
                        size = MediaImageSize.Medium;
                    else
                if (sizeType.ToLower() == MediaImageSize.Small.ToString().ToLower())
                        size = MediaImageSize.Small;
            }

            string mimeType = string.Empty;
            byte[] imageData = _imageResourceService.ReadImageResource(imageId, size, out mimeType);
            if (imageData == null)
                return new StatusCodeResult((int)HttpStatusCode.NotFound);
            return File(imageData, mimeType);
        }
    }
}
