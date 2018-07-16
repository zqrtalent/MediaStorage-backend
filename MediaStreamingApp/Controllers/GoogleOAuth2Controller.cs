using System;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MediaStreamingApp.Models;
using MediaStorage.Common.Dtos.Upload;
using MediaStorage.Data.WebApp.Entities;
using MediaStorage.Core.Services;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Web;
using MediaStorage.IO;

// Auth googledrive api.
/*
http://localhost:5000/oauth2/google/authorize
http://localhost:5000/oauth2/google/code
*/

// Note:
/*
https://myaccount.google.com/u/1/permissions?pageId=none&pli=1
 */

namespace MediaStreamingApp.Controllers
{
    [AllowAnonymous]
    [Route("oauth2/google")]
    public class GoogleOAuth2Controller : Controller
    {
        private readonly string _authApiUrl;
        private readonly string _authTokenApiUrl;
        private readonly IConfiguration _configuration;
        private readonly IOuth2CodeSynchronizer _synchronizer;
        public GoogleOAuth2Controller(IConfiguration configuration, IOuth2CodeSynchronizer synchronizer)
        {
            _configuration = configuration;
            _synchronizer = synchronizer;
            _authApiUrl = "https://accounts.google.com/o/oauth2/v2/auth";
            _authTokenApiUrl = "https://www.googleapis.com/oauth2/v4/token";
        }

        [HttpGet]
        [Route("authorize")]
        public ActionResult Authorize()
        {
            var defaultRedirectUrl = "http://localhost:5000/oauth2/google/callback";
            var redirectUrl = HttpUtility.UrlEncode(_configuration.GetValue<string>("MediaStorage:GoogleDrive:OAuth2RedirectUrl", defaultRedirectUrl));
            var clientId = _configuration["MediaStorage:GoogleDrive:ClientId"];
            string scope = HttpUtility.UrlEncode("https://www.googleapis.com/auth/drive.file");
            string accessType = "offline";
            string authRedirectUrl = $"{_authApiUrl}?scope={scope}&access_type={accessType}&include_granted_scopes=true&state=state_parameter_passthrough_value&redirect_uri={redirectUrl}&response_type=code&prompt=consent&client_id={clientId}";
            return Redirect(authRedirectUrl);
        }

        internal class AuthorizeResponseObject
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
            
            [JsonProperty("token_type")]
            public string TokenType { get; set; }
            
            [JsonProperty("refresh_token")]            
            public string RefreshToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }
        }

        [HttpGet]
        [Route("callback")]
        public ActionResult Callback(string code, string error)
        {
            var clientId = _configuration["MediaStorage:GoogleDrive:ClientId"];
            var clientSecret = _configuration["MediaStorage:GoogleDrive:ClientSecret"];
            var redirectUrl = _configuration["MediaStorage:GoogleDrive:OAuth2RedirectUrl"];
            AuthorizeResponseObject authResp = null;

            if(!string.IsNullOrEmpty(code))
            {
                using(var client = new HttpClient())
                {
                    var keyValuePairs = new List<KeyValuePair<string, string>>()
                    {
                        KeyValuePair.Create("code", code),
                        KeyValuePair.Create("client_id", clientId),
                        KeyValuePair.Create("client_secret", clientSecret),
                        KeyValuePair.Create("redirect_uri", redirectUrl),
                        KeyValuePair.Create("grant_type", "authorization_code"),
                    };
                    
                    var respMessage = client.PostAsync(_authTokenApiUrl, new FormUrlEncodedContent(keyValuePairs)).Result;
                    if(respMessage.IsSuccessStatusCode)
                    {
                        var responseString = respMessage.Content.ReadAsStringAsync().Result;
                        authResp = JsonConvert.DeserializeObject<AuthorizeResponseObject>(responseString);
                    }
                }
            }
            return Json( 
            new 
            { 
                AccessToken = authResp?.AccessToken ?? string.Empty, 
                RefreshToken = authResp?.RefreshToken ?? string.Empty, 
                Error = error 
            });
        }

        [HttpGet]
        [Route("code")]
        public ActionResult ReceiveCode(string code, string error)
        {
            _synchronizer.CodeReceived(code, error);
            return Json( new { Code = code, Error = error });
        }
    }
}