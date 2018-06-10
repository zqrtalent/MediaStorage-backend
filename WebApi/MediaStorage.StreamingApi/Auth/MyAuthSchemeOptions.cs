using System;
using Microsoft.AspNetCore.Authentication;

namespace MediaStorage.StreamingApi.Auth
{
    public class MyAuthSchemeOptions : AuthenticationSchemeOptions
    {
        public string Scheme { get; set; }
        public string AccessTokenKeyHeaderName { get; set; }
        public string SessionInfoContextKeyName { get; set; }
        public TimeSpan Expiration { get; set; }
    }
}
