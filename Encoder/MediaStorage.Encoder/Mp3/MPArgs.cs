using System;
using Newtonsoft.Json;

namespace MediaStorage.Encoder.Mp3
{
    internal class MPArgs
    {
        [JsonProperty("cpos")]
        public ulong CurrentPos { get; set; }            // Current player position in frames

        [JsonProperty("ffo")]
        public long FirstFrameOffset { get; set; }       // First frame file offset


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

         [JsonProperty("frfo")]
        public long[] FramesFileOffsets { get; set; }   // File offset of each frame offset.
     
        public MPArgs()
        {
            CurrentPos = 0;
            FirstFrameOffset = 0;
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
