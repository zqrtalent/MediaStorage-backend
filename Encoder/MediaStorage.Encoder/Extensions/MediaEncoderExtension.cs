using MediaStorage.Encoder;
using MediaStorage.Encoder.Mp3;

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
    }
}
