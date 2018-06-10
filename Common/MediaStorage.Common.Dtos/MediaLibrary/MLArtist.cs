using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaStorage.Common.Dtos.MediaLibrary
{
    [DataContract]
    public class MLArtist
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Genre { get; set; }

        [DataMember]
        public string ArtworkImageId { get; set; }

        [DataMember]
        public List<MLAlbum> Albums { get; set; }
    }
}
