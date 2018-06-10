using System;
using MediaStorage.Common.Dtos.Audio;

namespace MediaStorage.Core.Services
{
    public interface IMediaStreamingService
    {
        AudioPackets ReadAudioPacketsByOffset(string sessionKey, string songId, int offset, int numPackets);
        AudioPackets ReadAudioPacketsByTime(string sessionKey, string songId, int msecOffset, int numPackets);
    }
}
