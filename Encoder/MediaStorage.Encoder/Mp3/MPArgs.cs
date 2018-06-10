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
    internal class MPArgs
    {
        [JsonProperty("cpos")]
        public ulong CurrentPos { get; set; }            // Current player position in frames

        [JsonProperty("ffo")]
        public long FirstFrameOffset { get; set; }       // First frame file offset

        [JsonProperty("sf")]
        public ulong StartFrame { get; set; }

        [JsonProperty("sp")]
        public ulong StartPos { get; set; }

        [JsonProperty("ef")]
        public ulong EndFrame { get; set; }

        [JsonProperty("ep")]
        public ulong EndPos { get; set; }

        [JsonProperty("sr")]
        public int SampleRate { get; set; }              // Samplerate

        [JsonProperty("bs")]
        public ulong BufferSize { get; set; }            // The size of the output buffer

        [JsonProperty("b")]
        public ulong Buffers { get; set; }               // The number of buffers 

        [JsonProperty("ch")]
        public long Channels { get; set; }               // Channels(1=mono;2=stereo)

        [JsonProperty("ab")]
        public long AudioBits { get; set; }              // Bits per sample(ex. 16 bit)

        [JsonProperty("ff")]
        public long ForceFreq { get; set; }              // Force frequency?

        [JsonProperty("tr")]
        public bool TryResync { get; set; }              // Resync on bad data?

        [JsonProperty("sk")]
        public bool Seekable { get; set; }               // Is the stream seekable?

        [JsonProperty("vbr")]
        public bool IsVBR { get; set; }                  // Used to determine stream length

        [JsonProperty("nf")]
        public uint NumFrames { get; set; }             // Xing - Number of frames.

        [JsonProperty("fl")]
        public uint FileLength { get; set; }            // Xing - File length in bytes

        [JsonProperty("toc")]
        public byte[] TOC { get; set; }                 // Xing - Table of contents
     
        public MPArgs()
        {
            CurrentPos = 0;
            FirstFrameOffset = 0;
            StartPos = 0;
            EndPos = 0;
            AudioBits = 16;
            SampleRate = 44100;
            Channels = 2;
            BufferSize = 16384;
            Buffers = 8;
            //BufferCB = NULL;
            TryResync = true;
            Seekable = true;
            ForceFreq = -1;
        }
    }
}
