using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaStorage.Common.Dtos.Session;
using MediaStorage.Settings;
using MediaStorage.StreamingApi.ActionResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MediaStorage.StreamingApi.Controllers
{
    [Route("api/v1/[controller]")]
    public class AuthenticateController : BaseAuthController
    {
        public AuthenticateController(IOptions<AuthSettings> authSettings) : base(authSettings)
        {
        }

        [HttpGet, Route("")]
        public IActionResult Get()
        {
            SessionInfo sesInfo = new SessionInfo
            {
                SessionKey = SessionInfo.SessionKey,
                PlayingAtMSec = SessionInfo.PlayingAtMSec,
                PlayingMediaId = SessionInfo.PlayingMediaId    
            };
            return new SerializableObjectResult(sesInfo);
        }
    }
}
