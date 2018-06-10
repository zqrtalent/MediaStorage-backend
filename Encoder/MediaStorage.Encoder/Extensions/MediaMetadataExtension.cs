using System.Collections.Generic;
using Newtonsoft.Json;
using MediaStorage.Encoder;
using MediaStorage.IO;
using MediaStorage.Common.Dtos.Encoder;
using MediaStorage.Common.Dtos.Encoder.Mp3;

namespace MediaStorage.Encoder.Extensions
{
    public static class MediaMetadataExtension
    {
        public static IMediaMetadata ReadMetadata(this IStorageFile file, string format, out IEnumerable<AttachedPicture> pictures)
        {
            IMediaEncoder encoder = MediaEncoderExtension.EncoderByMediaType(format);
            pictures = null;

            if(encoder != null)
            {
                if (encoder.Init(file, true))
                {
                    pictures = encoder.AttachedPictures;
                    return encoder.GetMetadata();
                }
            }
            return null;
        }

        public static IMediaMetadata MetadataFromJson(string mediaFormat, string metadataJson)
        {
            switch (mediaFormat)
            {
//              case AudioMp3:
                case "mp3":
                    {
                        return JsonConvert.DeserializeObject<Mp3Metadata>(metadataJson);
                    }
            }

            return null;
        }
    }
}
