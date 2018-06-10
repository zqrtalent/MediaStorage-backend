using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaStorage.Common.Dtos.Audio
{
    [DataContract]
    public class AudioPackets
    {
        public AudioPackets()
        {
            Packets = new List<AudioPacket>();
        }

        [DataMember]
        public ulong Offset { get; set; }

        [DataMember]
        public int NumPackets { get; set; }

        [DataMember]
        public int SamplesPerFrame { get; set; } // Samples per frame for CBR

        [DataMember]
        public IList<AudioPacket> Packets { get; set; }

        [DataMember]
        public bool IsEof { get; set; }

        [DataMember]
        public ulong FramesCt { get; set; } // Frames count in entire media file

        [DataMember]
        public bool IsVBR { get; set; }
    }
}
