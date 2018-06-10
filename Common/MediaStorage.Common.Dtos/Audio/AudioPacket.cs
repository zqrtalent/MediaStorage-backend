using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaStorage.Common.Dtos.Audio
{
    [DataContract]
    public class AudioPacket 
    {
        [DataMember]
        public byte[] Data { get; set; }

        [DataMember]
        public int SamplesPerFrame { get; set; } // Used only for VBR
    }
}
