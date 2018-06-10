using System;

namespace MediaStorage.Settings
{
    public class AuthSettings
    {
        public string AccessTokenHeaderName { get; set; }
        public string SessionInfoContextKeyName { get; set; }
        public int ExpirationInMin { get; set; }
    }
}
