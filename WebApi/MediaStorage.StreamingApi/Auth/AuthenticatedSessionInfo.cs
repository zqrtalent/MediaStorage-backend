using System;

namespace MediaStorage.StreamingApi.Auth
{
    public class AuthenticatedSessionInfo
    {
        public string SessionKey {get; set;}
        public Guid UserId {get; set;}
        public string UserName {get; set;}
        public string PlayingMediaId { get; set; }
        public int PlayingAtMSec {get; set;}
    }
}
