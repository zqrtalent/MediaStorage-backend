using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaStorage.Common.Dtos.MediaLibrary
{
    [DataContract]
    public class MLPlaylistSong
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Song { get; set; }

        [DataMember]
        public int Track { get; set; }

        [DataMember]
        public string Artist { get; set; }

        [DataMember]
        public string Album { get; set; }

        [DataMember]
        public int Year { get; set; }

        [DataMember]
        public string Genre { get; set; }
    }
}
