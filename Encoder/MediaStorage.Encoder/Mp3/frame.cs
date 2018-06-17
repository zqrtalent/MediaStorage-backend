using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MediaStorage.IO;
using MediaStorage.Common.Dtos.Encoder;
using MediaStorage.Common.Dtos.Audio;
using MediaStorage.Encoder.Mp3.Tags;
using Newtonsoft.Json;

namespace MediaStorage.Encoder.Mp3
{
    internal class Frame
    {
        [JsonProperty("st")]
        public long stereo { get; set; }

        [JsonProperty("jsb")]
        public long jsbound { get; set; }

        [JsonProperty("sin")]
        public long single { get; set; }
        //public long II_sblimit;

        [JsonProperty("lsf")]
        public long lsf { get; set; }

        [JsonProperty("mpg")]
        public long mpeg25 { get; set; }
        //public long down_sample;

        [JsonProperty("hch")]
        public long header_change { get; set; }
        //public ulong block_size;

        [JsonProperty("lay")]
        public long lay { get; set; }

        [JsonProperty("wl")]
        public long WhatLayer { get; set; }

        [JsonProperty("ep")]
        public long error_protection { get; set; }

        [JsonProperty("bri")]
        public long bitrate_index { get; set; }

        [JsonProperty("sfr")]
        public long sampling_frequency { get; set; }

        [JsonProperty("pad")]
        public long padding { get; set; }

        [JsonProperty("ext")]
        public long extension { get; set; }

        [JsonProperty("md")]
        public long mode { get; set; }

        [JsonProperty("mdx")]
        public long mode_ext { get; set; }

        [JsonProperty("cp")]
        public long copyright { get; set; }

        [JsonProperty("or")]
        public long original { get; set; }

        [JsonProperty("em")]
        public long emphasis { get; set; }
    };
}
