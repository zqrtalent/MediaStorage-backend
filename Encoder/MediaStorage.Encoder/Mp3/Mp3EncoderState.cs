using System;
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
        public Frame frame { get; set; }
    }
}
