using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaStorage.Common.Dtos.Encoder.Mp3
{
    [DataContract(Name = "mp3meta")]
    public class Mp3Metadata : IMediaMetadata
    {
        [DataMember(Name = "art")]
        public string Artist { get; set; }

        [DataMember(Name = "alb")]
        public string Album { get; set; }

        [DataMember(Name = "son")]
        public string Song { get; set; }

        [DataMember(Name = "gen")]
        public string Genre { get; set; }

        [DataMember(Name = "com")]
        public string Composer { get; set; }

        [DataMember(Name = "yer")]
        public string Year { get; set; }

        [DataMember(Name = "trk")]
        public string Track { get; set; }

        [DataMember(Name = "vbr")]
        public bool IsVBR { get; set; } // VBR or CBR

        [DataMember(Name = "btr")]
        public int Bitrate { get; set; }

        [DataMember(Name = "dur")]
        public int DurationSec { get; set; }

        [DataMember(Name = "albimg")]
        public IDictionary<AttachedPictureType, string> AlbumImagesUrl { get; set; }

        [DataMember(Name = "artimg")]
        public string ArtistImageUrl { get; set; }
    }
}
