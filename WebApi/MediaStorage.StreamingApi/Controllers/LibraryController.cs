using System;
using MediaStorage.Core.Services;
using MediaStorage.Settings;
using MediaStorage.StreamingApi.ActionResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MediaStorage.StreamingApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class MediaLibraryMetadataController : BaseAuthController
    {
        private readonly IMediaLibraryService _mediaLibraryService;
        public MediaLibraryMetadataController(IMediaLibraryService mediaLibraryService, IOptions<AuthSettings> authSettings) : base(authSettings)
        {
            _mediaLibraryService = mediaLibraryService;
        }

        // GET api/values
        [HttpGet, Route("all")]
        public IActionResult GetEntireLibraryInfo()
        {
            return new SerializableObjectResult(_mediaLibraryService.GetLibraryInfo());
        }
    }
}
