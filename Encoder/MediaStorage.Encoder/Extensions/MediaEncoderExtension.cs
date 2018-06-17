using MediaStorage.Encoder;
using MediaStorage.Encoder.Mp3;
using MediaStorage.IO;

namespace MediaStorage.Encoder.Extensions
{
    public static class MediaEncoderExtension
    {
        public static IMediaEncoder EncoderByMediaType(string format)
        {
            IMediaEncoder encoder = null;
            switch (format)
            {
                //case MediaFormat.AudioMp3:
                case "mp3":
                    {
                        encoder = new Mp3Encoder();
                        break;
                    }
            }
            return encoder;
        }

        public static string GenerateEncoderState(string format, IStorageFile mediaFile, bool analyzeAllFrames)
        {
            string stateJson = string.Empty;
            using(var encoder = EncoderByMediaType(format))
            {
                if(encoder.Init(mediaFile, false))
                {
                    encoder.SaveStateIntoJson(analyzeAllFrames, out stateJson);
                }
            }
            return stateJson;
        }
    }
}
