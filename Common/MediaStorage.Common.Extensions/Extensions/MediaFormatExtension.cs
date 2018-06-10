
namespace MediaStorage.Common.Extensions
{
    public static class MediaFormatExtension
    {
        public const string AudioMp3 = "mp3";
        public const string AudioAac = "aac";
        public const string VideoMp4 = "mp4";
        public const string ImageJpeg = "jpeg";

        public static string GetMimeType(string mediaFormat)
        {
            switch (mediaFormat)
            {
                case AudioMp3:
                    {
                        return "audio/mp3";
                    }
                case AudioAac:
                    {
                        return "audio/aac";
                    }
                case VideoMp4:
                    {
                        return "video/mp4";
                    }
                case ImageJpeg:
                    {
                        return "image/jpeg";
                    }
            }

            return string.Empty;
        }

        // public static IMediaMetadata MetadataFromJson(string mediaFormat, string metadataJson)
        // {
        //     switch (mediaFormat)
        //     {
        //         case AudioMp3:
        //             {
        //                 return JsonConvert.DeserializeObject<Mp3Metadata>(metadataJson);
        //             }
        //     }

        //     return null;
        // }
    }
}
