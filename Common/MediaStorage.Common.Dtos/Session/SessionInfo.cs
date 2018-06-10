using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaStorage.Common.Dtos.Session
{
    [DataContract]
    public class SessionInfo
    {
        [DataMember]
        public string SessionKey { get; set; }

        [DataMember]
        public string PlayingMediaId { get; set; }

        [DataMember]
        public int PlayingAtMSec { get; set; }
    }
}
