using System;
using Newtonsoft.Json;

namespace MediaStorage.Core.Services
{
    internal class MediaFileStateInfos
    {
        [JsonProperty("mfs")]
        public string MediaFileStateJson { get; set; }
        
        [JsonProperty("enc")]
        public string EncoderFileStateJson { get; set; }
    }
}