using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using MediaStorage.Core.Services;
using MediaStorage.StreamingApi.ActionResults;
using MediaStorage.Settings;
using Microsoft.Extensions.Options;

namespace MediaStorage.StreamingApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class AudioPacketsController : BaseAuthController
    {
        private readonly IMediaStreamingService _streamingService;
        public AudioPacketsController(IMediaStreamingService streamingService, IOptions<AuthSettings> authSettings) : base(authSettings)
        {
            _streamingService = streamingService;
        }

        [HttpGet, Route("offset")]
        public IActionResult GetPacketsByOffset([FromQuery]string mediaId, [FromQuery]int offset, [FromQuery]int numPackets)
        {
            string sessionKey = SessionKey;
            var result = _streamingService.ReadAudioPacketsByOffset(sessionKey, mediaId, offset, numPackets);
            if (result == null)
                return new StatusCodeResult((int)HttpStatusCode.NotFound);
            return new SerializableObjectResult(result);
        }

        [HttpGet, Route("time")]
        public IActionResult GetPacketsByTime([FromQuery]string mediaId, [FromQuery]int tsec, [FromQuery]int numPackets)
        {
            string sessionKey = SessionKey;
            var result = _streamingService.ReadAudioPacketsByTime(sessionKey, mediaId, tsec, numPackets);
            if (result == null)
                return new StatusCodeResult((int)HttpStatusCode.NotFound);
            return new SerializableObjectResult(result);
        }
    }
}
