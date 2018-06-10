using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaStorage.Common.Dtos.Upload
{
    [DataContract]
    public class UploadSong
    {
        [DataMember]
        public Guid MediaId { get; set; } // Song media identifier.

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Track { get; set; }

        [DataMember]
        public string Genre { get; set; }
    }
}
