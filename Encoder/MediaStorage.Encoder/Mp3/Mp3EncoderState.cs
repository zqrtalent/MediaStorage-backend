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
    internal class Mp3EncoderState
    {
        [JsonProperty("fo")]
        public long FileOffset { get; set; }

        [JsonProperty("frs")]
        public int framesize { get; set; }

        [JsonProperty("fs")]
        public int fsize { get; set; }

        [JsonProperty("fsz")]
        public int fsizeold { get; set; }

        [JsonProperty("ss")]
        public int ssize { get; set; }

        [JsonProperty("bsn")]
        public int bsnum { get; set; }

        [JsonProperty("oh")]
        public ulong oldhead { get; set; }

        [JsonProperty("fh")]
        public ulong firsthead { get; set; }

        [JsonProperty("sy")]
        public ulong syncword { get; set; }

        [JsonProperty("args")]
        public MPArgs Args { get; set; }

        [JsonProperty("frm")]
        public frame frame { get; set; }
    }
}
