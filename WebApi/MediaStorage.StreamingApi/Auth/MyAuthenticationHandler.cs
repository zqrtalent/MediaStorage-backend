using System;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MediaStorage.Common.DI;
using MediaStorage.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace MediaStorage.StreamingApi.Auth
{
    public class MyAuthenticationHandler : AuthenticationHandler<MyAuthSchemeOptions>
    {
        private readonly IAbstractDependencyInjector _injector;
        public MyAuthenticationHandler(IOptionsMonitor<MyAuthSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IAbstractDependencyInjector injector) : 
            base(options, logger, encoder, clock)
        {
            _injector = injector;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if(string.IsNullOrEmpty(Options.Scheme) || string.IsNullOrEmpty(Options.AccessTokenKeyHeaderName))
                throw new ArgumentException($"Make sure to configure {nameof(MyAuthSchemeOptions)} options!");

            StringValues values;
            if(Request.Headers.TryGetValue("Authorization", out values) && values.Count == 1)
            {
                var arrValues = values[0].Split(" ");
                if(arrValues[0].ToLower() != "basic")
                {
                    return AuthenticateResult.Fail("Invalid authorization header!");
                }

                string userName, password, hash;
                if(!ParseCridentials(arrValues[1], out userName, out password, out hash))
                {
                    return AuthenticateResult.Fail("Invalid authorization header!");
                }

                AuthenticatedSessionInfo sesInfo = null;
                if(AuthenticateSession(userName, password, hash, ref sesInfo))
                {
                    Request.HttpContext.Items.Add(Options.SessionInfoContextKeyName, sesInfo);
                    SetAuthenticatedInfo(sesInfo.SessionKey, sesInfo);
                    
                    return await Task.FromResult(AuthenticateResult.Success(CreateAuthTicket(sesInfo)));
                }
            }
            else if(Request.Headers.TryGetValue(Options.AccessTokenKeyHeaderName, out values) && values.Count > 0)
            {
                AuthenticatedSessionInfo sesInfo = null;
                if(GetAthenticatedInfo(values[0], out sesInfo))
                {
                    Request.HttpContext.Items.Add(Options.SessionInfoContextKeyName, sesInfo);
                    return await Task.FromResult(AuthenticateResult.Success(CreateAuthTicket(sesInfo)));
                }
                else
                {
                    return await Task.FromResult(AuthenticateResult.Fail("Invalid auth token!"));
                }
            }

            return await Task.FromResult(AuthenticateResult.Fail("Unknown error occured while trying to authenticate!"));
        }

        #region Private functionality
        private bool ParseCridentials(string basicValueBase64, out string userName, out string password, out string hash)
        {
            var ret = false;
            var basicValue = UTF8Encoding.UTF8.GetString(Convert.FromBase64String(basicValueBase64));
            var arrItems = basicValue.Split(':');

            if(arrItems.Length >= 2)
            {
                userName = arrItems[0];
                password = arrItems[1];
                hash = arrItems.Length > 2 ? arrItems[2] : string.Empty;
                ret = true;
            }
            else
            {
                userName = string.Empty;
                password = string.Empty;
                hash = string.Empty;
            }
            
            return ret;
        }

        private bool AuthenticateSession(string userName, string password, string hash, ref AuthenticatedSessionInfo sesInfo)
        {
            sesInfo = null;
            var userService = _injector.Resolve<IStreamingUserService>();
            var sessionService = _injector.Resolve<IStreamingUserSessionService>();

            //userService.CreateUser("zqrtalent", "zqrtalentpass");

            Guid userId;
            if(userService.Authenticate(userName, password, hash, out userId))
            {
                var sInfo = sessionService.StartSession(userId, Request.Host.Value);
                if(sInfo != null)
                {
                    sesInfo = new AuthenticatedSessionInfo
                    {
                        SessionKey = sInfo.SessionKey,
                        UserId = userId,
                        UserName = userName,
                        PlayingMediaId = sInfo.PlayingMediaId,
                        PlayingAtMSec = sInfo.PlayingAtMSec
                    };
                }
            }
            return sesInfo != null;
        } 

        private AuthenticationTicket CreateAuthTicket(AuthenticatedSessionInfo sessionInfo)
        {
            var claims = new [] 
            { 
                new Claim(ClaimTypes.Name, sessionInfo.UserName),
                new Claim(ClaimTypes.NameIdentifier, sessionInfo.UserId.ToString()),
            };
            var claimsIdentity = new ClaimsIdentity(claims, Options.Scheme);
            var principal = new ClaimsPrincipal(new [] { claimsIdentity });
            return new AuthenticationTicket(principal, new AuthenticationProperties(), Options.Scheme);
        }

        private bool GetAthenticatedInfo(string sessionKey, out AuthenticatedSessionInfo sessInfo)
        {
            var cache = _injector.Resolve<IMemoryCache>();
            string infoJson = string.Empty;
            sessInfo = null;

            if(cache.TryGetValue(sessionKey, out infoJson))
            {
                sessInfo = JsonConvert.DeserializeObject<AuthenticatedSessionInfo>(infoJson);
            }

            return sessInfo != null;
        }

        private void SetAuthenticatedInfo(string sessionKey, AuthenticatedSessionInfo sessInfo)
        {
            string infoJson = JsonConvert.SerializeObject(sessInfo);
            var cache = _injector.Resolve<IMemoryCache>();

            if(Options.Expiration != null)
                cache.Set(sessionKey, infoJson, Options.Expiration);
            else
                cache.Set(sessionKey, infoJson);
        }
        #endregion
    }
}
