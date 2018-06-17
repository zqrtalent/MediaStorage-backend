using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MediaStorage.Encoder.Mp3
{
    public static class Mp3EncoderExtensions
    {
        public static void AnalyzeAllFrames(this Mp3Encoder encoder)
        {
            var offsets = new List<long>();
            long pos = 0, prevOffset = 0;
            while(encoder.Seek_StreamByPos(pos))
            {
                if(prevOffset == encoder.CurrentFrameFileOffset)
                    break;
                offsets.Add(encoder.CurrentFrameFileOffset - prevOffset); // Keep offsets related to previous packet offset.
                prevOffset = encoder.CurrentFrameFileOffset;
                pos ++;
            }
            encoder.SetFrameFileOffsets(offsets);
        }
    }
}
