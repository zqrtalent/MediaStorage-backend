using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaStorage.Common.Dtos.Upload
{
    [DataContract]
    public class UploadAlbum
    {
        public UploadAlbum()
        {
            Songs = new List<UploadSong>();
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid? AlbumImageId { get; set; }

        [DataMember]
        public string Year { get; set; }

        [DataMember]
        public IList<UploadSong> Songs { get; set; }
    }
}
