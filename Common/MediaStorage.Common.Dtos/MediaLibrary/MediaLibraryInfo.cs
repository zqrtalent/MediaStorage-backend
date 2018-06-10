using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaStorage.Common.Dtos.MediaLibrary
{
    [DataContract]
    public class MediaLibraryInfo
    {
        [DataMember]
        public int LastUpdated { get; set; }

        [DataMember]
        public IList<MLArtist> Artists { get; set; }

        [DataMember]
        public IList<MLPlaylist> Playlists { get; set; }
    }
}
