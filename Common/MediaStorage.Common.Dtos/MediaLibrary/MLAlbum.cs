using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaStorage.Common.Dtos.MediaLibrary
{
    [DataContract]
    public class MLAlbum
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Year { get; set; }

        [DataMember]
        public string ArtworkImageId { get; set; }

        [DataMember]
        public List<MLSong> Songs { get; set; }
    }
}
