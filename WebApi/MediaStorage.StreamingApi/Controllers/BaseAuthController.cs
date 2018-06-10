using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using MediaStorage.Core.Services;
using MediaStorage.StreamingApi.ActionResults;
using MediaStorage.StreamingApi.Auth;
using Microsoft.AspNetCore.Authorization;
using MediaStorage.Settings;
using Microsoft.Extensions.Options;

namespace MediaStorage.StreamingApi.Controllers
{
    [Authorize]
    public abstract class BaseAuthController : Controller
    {
        protected readonly IOptions<AuthSettings> _authSettings;

        public BaseAuthController(IOptions<AuthSettings> authSettings)
        {
            _authSettings = authSettings;
        }

        protected AuthenticatedSessionInfo SessionInfo
        {
            get
            {
                object value = null;
                if(HttpContext.Items.TryGetValue(_authSettings.Value.SessionInfoContextKeyName, out value))
                    return value as AuthenticatedSessionInfo;
                return null;
            }
        }

        protected string SessionKey => SessionInfo?.SessionKey ?? string.Empty;
    }
}
