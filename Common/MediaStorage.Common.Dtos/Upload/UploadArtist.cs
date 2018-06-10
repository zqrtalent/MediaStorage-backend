using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaStorage.Common.Dtos.Upload
{
    [DataContract]
    public class UploadArtist
    {
        public UploadArtist()
        {
            Albums = new List<UploadAlbum>();
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid? ArtistImageId { get; set; }

        [DataMember]
        public IList<UploadAlbum> Albums { get; set; }
    }
}
