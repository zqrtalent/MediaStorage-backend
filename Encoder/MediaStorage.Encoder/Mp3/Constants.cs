using System;
using System.IO;
using System.Collections.Generic;

namespace MediaStorage.Encoder.Mp3
{
    internal static class Constants
    {
        public const ulong ID3 = 0x49443300; // ID3
        public const ulong Xing = 0x58696E67; // Xing

       // Header mask definition
        public const long HDRCMPMASK = 0xfffffd00;
        public const long MAXFRAMESIZE = 1792;
        public const long MPG_MD_STEREO = 0;
        public const long MPG_MD_JOINT_STEREO = 1;
        public const long MPG_MD_DUAL_CHANNEL = 2;
        public const long MPG_MD_MONO = 3;
        public const int MAX_RESYNC = 0x20000;

        public static readonly int[] freqs = { 44100, 48000, 32000, 22050, 24000, 16000, 11025, 12000, 8000 };
        public static readonly int[,,] BitRateIndex = new int[2, 3, 15]
        {
            {
                {0,32,64,96,128,160,192,224,256,288,320,352,384,416,448},
                {0,32,48,56,64,80,96,112,128,160,192,224,256,320,384},
                {0,32,40,48,56,64,80,96,112,128,160,192,224,256,320}
            },
            {
                {0,32,48,56,64,80,96,112,128,144,160,176,192,224,256},
                {0,8,16,24,32,40,48,56,64,80,96,112,128,144,160},
                {0,8,16,24,32,40,48,56,64,80,96,112,128,144,160}
            }
        };

        public  static readonly double[,] ms_per_frame_array = new double[3, 3] 
        { 
            { 8.707483f, 8.0f, 12.0f }, 
            { 26.12245f, 24.0f, 36.0f }, 
            { 26.12245f, 24.0f, 36.0f } 
        };
        
        // Samples per Frame: 1. index = LSF (MPEG 1 - MPEG 2, 2.5 ), 2. index = Layer (Layer1, Layer2, Layer3)
        public static readonly int[,] samplesperframes = new int[2, 3] 
        { 
            { 384, 1152, 1152 }, 
            { 384, 1152, 576 }
        };
    };
}
