using System;
using System.Collections.Generic;
using MediaStorage.IO;
using MediaStorage.Common.Dtos.Encoder;
using MediaStorage.Common.Dtos.Audio;

namespace MediaStorage.Encoder
{
    public interface IMediaEncoder : IDisposable
    {
        bool Init(IStorageFile stream, bool readTags, string stateJson = null);
        bool SaveStateIntoJson(bool analyzeAllFrames, out string stateJson);
        IMediaMetadata GetMetadata();
        IEnumerable<AttachedPicture> AttachedPictures { get; }
        ulong GetMediaPacketsCount();
        AudioPackets ReadPacketsByTime(ulong milliSecond, int numPackets);
        AudioPackets ReadPackets(int offset, int numPackets);
        int TotalSec { get; }
        uint CurrentMs { get; }
        int SamplesPerFrame { get; }
    }
}
