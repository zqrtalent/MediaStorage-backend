﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaStorage.Common.Dtos.MediaLibrary
{
    [DataContract]
    public class MLSong
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int DurationSec { get; set; }

        [DataMember]
        public int Track { get; set; }
    }
}
